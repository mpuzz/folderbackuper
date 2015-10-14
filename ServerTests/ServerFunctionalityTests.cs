using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.Server;
using FolderBackup.Shared;
using FolderBackup.CommunicationProtocol;
using System.IO;

namespace FolderBackup.ServerTests
{
    [TestClass]
    public class TransactionTests
    {
        Server.Server server;
        DirectoryInfo rinfo;

        [TestInitialize]
        public void TestInitialize()
        {
            server = new Server.Server();
            server.auth("test1", "b444ac06613fc8d63795be9ad0beaf55011936ac");

            string[] lines = { "First line", "Second line", "Third line" };
            string[] lines1 = { "First line", "Second line", "Third lines" };
            System.IO.Directory.CreateDirectory("asd");
            System.IO.Directory.CreateDirectory(@"asd\ciao");
            rinfo = new DirectoryInfo("asd");
            System.IO.File.WriteAllLines(@"asd\uno.txt", lines);
            System.IO.File.WriteAllLines(@"asd\due.txt", lines1);
            System.IO.File.WriteAllLines(@"asd\ciao\due.txt", lines);
        }

       /* [TestMethod]
        public void CorrectCurrentVersion()
        {
            string[] lines = { "First line", "Second line", "Third line" };
            System.IO.Directory.CreateDirectory(server.user.rootDirectory.FullName + @"\asd");
            System.IO.Directory.CreateDirectory(server.user.rootDirectory.FullName + @"\asd\ciao");
            DirectoryInfo rinfo = new DirectoryInfo(server.user.rootDirectory.FullName + @"\asd");
            System.IO.File.WriteAllLines(server.user.rootDirectory.FullName + @"\asd\uno.txt", lines);
            System.IO.File.WriteAllLines(server.user.rootDirectory.FullName + @"\asd\ciao\due.txt", lines);
            FileInfo finfo = new FileInfo(server.user.rootDirectory.FullName + @"\asd\ciao\due.txt");

            FBVersionBuilder vb = new FBVersionBuilder(server.user.rootDirectory.FullName);
            FBVersion correct = (FBVersion) vb.generate();

            SerializedVersion vers = server.getCurrentVersion();
            FBVersion returned = FBVersion.deserialize(vers.encodedVersion);

            Assert.AreEqual(correct, returned);
        }*/

        [TestMethod]
        public void TransactionCommitTest()
        {
            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v = (FolderBackup.Shared.FBVersion)vb.generate();

            SerializedVersion serV = new SerializedVersion();
            serV.encodedVersion = v.serialize();


            Assert.IsTrue(server.newTransaction(serV));

            byte[][] bfiles = server.getFilesToUpload();
            foreach (byte[] bf in bfiles)
            {
                FBFile f = FBFile.deserialize(bf);
                Assert.IsTrue(v.fileList.Contains(f));
            }

            FBFile file = (FBFile)new FBFileBuilder(@"asd\uno.txt").generate();
            FileStream fstream = new FileStream(@"asd\uno.txt", FileMode.Open, FileAccess.Read);
            Assert.AreEqual(server.uploadFile(fstream), file.hash);
            fstream.Close();

            file = (FBFile)new FBFileBuilder(@"asd\due.txt").generate();
            fstream = new FileStream(@"asd\due.txt", FileMode.Open, FileAccess.Read);
            Assert.AreEqual(server.uploadFile(fstream), file.hash);
            fstream.Close();

            Assert.IsTrue(server.commit());
        }

        [TestCleanup]
        public void CleanUp()
        {
            System.IO.Directory.Delete("asd", true);
            foreach(FileInfo f in server.user.rootDirectory.GetFiles())
            {
                f.Delete();
            }
            foreach (DirectoryInfo d in server.user.rootDirectory.GetDirectories())
            {
                d.Delete(true);
            }
        }
    }
}
