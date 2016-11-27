using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.CommunicationProtocol;
using System.IO;
using FolderBackup.Shared;
using System.Net.Sockets;
using System.Net.Security;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace FolderBackup.ServerTests
{
    [TestClass]
    public class RevertTests
    {
        Server.Server server;
        DirectoryInfo rinfo;
        string token;

        [TestInitialize]
        private void TestInitialize()
        {
            server = new Server.Server();
            AuthenticationData ad = server.authStep1("test1");
            token = server.authStep2(ad.token, "test1", AuthenticationPrimitives.hashPassword("test1", ad.salt, ad.token));
            CleanUp();

            server = new Server.Server();
            ad = server.authStep1("test1");
            token = server.authStep2(ad.token, "test1", AuthenticationPrimitives.hashPassword("test1", ad.salt, ad.token));

            string[] lines = { "First line", "Second line", "Third line" };
            string[] lines1 = { "First line", "Second line", "Third lines" };
            System.IO.Directory.CreateDirectory("asd");
            System.IO.Directory.CreateDirectory(@"asd\ciao");
            rinfo = new DirectoryInfo("asd");
            System.IO.File.WriteAllLines(@"asd\uno.txt", lines);
            System.IO.File.WriteAllLines(@"asd\due.txt", lines1);
            System.IO.File.WriteAllLines(@"asd\ciao\due.txt", lines);

            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v = (FolderBackup.Shared.FBVersion)vb.generate();

            SerializedVersion serV = new SerializedVersion(v.serialize());

            server.newTransaction(serV);

            byte[][] bfiles = server.getFilesToUpload();
            
            FBFile file = (FBFile)new FBFileBuilder(@"asd\uno.txt").generate();
            FileStream fstream = new FileStream(@"asd\uno.txt", FileMode.Open, FileAccess.Read);

            UploadData credential = server.uploadFile();
            UsefullMethods.SendFile(credential.ip, credential.port, credential.token, fstream);

            fstream.Close();

            file = (FBFile)new FBFileBuilder(@"asd\due.txt").generate();
            fstream = new FileStream(@"asd\due.txt", FileMode.Open, FileAccess.Read);

            credential = server.uploadFile();
            UsefullMethods.SendFile(credential.ip, credential.port, credential.token, fstream);

            Assert.IsTrue(server.commit());
        }


        [TestMethod]
        public void RevertWithFileDeletion()
        {
            this.TestInitialize();
            string[] lines = { "First line", "Second line", "Third line"};
            System.IO.File.WriteAllLines(@"asd\ciao\tre.txt", lines);

            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v = (FolderBackup.Shared.FBVersion)vb.generate();

            SerializedVersion serV = new SerializedVersion(v.serialize());

            server.newTransaction(serV);

            server.commit();

            UploadData ud = server.resetToPreviousVersion(1);
            UsefullMethods.ReceiveFile(ud.ip, ud.port, ud.token, @"asd\asd.zip");

            String ignore;
            List<Instruction> instructionList = UsefullMethods.ExtractInstructions(@"asd\asd.zip", out ignore);

            Assert.IsTrue(instructionList.Count == 1);
            Assert.IsTrue(instructionList[0].cmd == InstructionType.DELETE);
            Assert.IsTrue(instructionList[0].src == @"ciao\tre.txt");
        }

        [TestMethod]
        public void RevertWithFileCopy()
        {
            this.TestInitialize();
            string[] lines = { "First line", "Second line", "Third line" };
            System.IO.File.WriteAllLines(@"asd\ciao\tre.txt", lines);

            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v = (FolderBackup.Shared.FBVersion)vb.generate();

            SerializedVersion serV = new SerializedVersion(v.serialize());

            server.newTransaction(serV);

            server.commit();

            System.IO.File.Delete(@"asd\ciao\tre.txt");
            vb = new FBVersionBuilder(rinfo.FullName);
            v = (FolderBackup.Shared.FBVersion)vb.generate();
            serV = new SerializedVersion(v.serialize());

            server.newTransaction(serV);

            server.commit();

            UploadData ud = server.resetToPreviousVersion(1);
            UsefullMethods.ReceiveFile(ud.ip, ud.port, ud.token, @"asd\asd.zip");

            String ignore;
            List<Instruction> instructionList = UsefullMethods.ExtractInstructions(@"asd\asd.zip", out ignore);
            
            Assert.IsTrue(instructionList.Count == 1);
            Assert.IsTrue(instructionList[0].cmd == InstructionType.COPY);
            Assert.IsTrue(instructionList[0].dst == @"ciao\tre.txt");
        }

        [TestMethod]
        public void RevertWithNewFile()
        {
            this.TestInitialize();
            string[] lines = { "First line", "Second line", "Third line", "Fourth line" };
            System.IO.File.WriteAllLines(@"asd\ciao\tre.txt", lines);

            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v = (FolderBackup.Shared.FBVersion)vb.generate();

            SerializedVersion serV = new SerializedVersion(v.serialize());

            server.newTransaction(serV);
            FileStream fstream = new FileStream(@"asd\ciao\tre.txt", FileMode.Open, FileAccess.Read);
            server.getFilesToUpload();
            UploadData credential = server.uploadFile();
            UsefullMethods.SendFile(credential.ip, credential.port, credential.token, fstream);
            server.commit();

            System.IO.File.Delete(@"asd\ciao\tre.txt");
            vb = new FBVersionBuilder(rinfo.FullName);
            v = (FolderBackup.Shared.FBVersion)vb.generate();
            serV = new SerializedVersion(v.serialize());

            server.newTransaction(serV);

            server.commit();

            UploadData ud = server.resetToPreviousVersion(1);
            UsefullMethods.ReceiveFile(ud.ip, ud.port, ud.token, @"asd\asd.zip");

            String ignore;
            List<Instruction> instructionList = UsefullMethods.ExtractInstructions(@"asd\asd.zip", out ignore);

            Assert.IsTrue(instructionList.Count == 1);
            Assert.IsTrue(instructionList[0].cmd == InstructionType.NEW);
            Assert.IsTrue(instructionList[0].dst == @"ciao\tre.txt");
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (System.IO.Directory.Exists("asd"))
                System.IO.Directory.Delete("asd", true);
            if (server.user.rootDirectory.GetFiles() != null)
                foreach (FileInfo f in server.user.rootDirectory.GetFiles())
                {
                    f.Delete();
                }
            if (server.user.rootDirectory.GetDirectories() != null)
                foreach (DirectoryInfo d in server.user.rootDirectory.GetDirectories())
                {
                    d.Delete(true);
                }
        }
    }
}
