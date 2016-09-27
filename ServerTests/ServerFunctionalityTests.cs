using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.Server;
using FolderBackup.Shared;
using FolderBackup.CommunicationProtocol;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace FolderBackup.ServerTests
{
    [TestClass]
    public class TransactionTests
    {
        Server.Server server;
        DirectoryInfo rinfo;
        string token;

        [TestInitialize]
        public void TestInitialize()
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
        }

        [TestMethod]
        public void TransactionCommitTest()
        {
            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v = (FolderBackup.Shared.FBVersion)vb.generate();

            SerializedVersion serV = new SerializedVersion(v.serialize());

            Assert.IsTrue(server.newTransaction(serV));

            byte[][] bfiles = server.getFilesToUpload();
            foreach (byte[] bf in bfiles)
            {
                FBFile f = FBFile.deserialize(bf);
                Assert.IsTrue(v.fileList.Contains(f));
            }

            FBFile file = (FBFile)new FBFileBuilder(@"asd\uno.txt").generate();
            string[] lines = {"First line", "Second line", "Third line" };
            System.IO.File.WriteAllLines(@"asd\uno.txt", lines);
            FileStream fstream = new FileStream(@"asd\uno.txt", FileMode.Open, FileAccess.Read);

            UploadData credential = server.uploadFile(new SerializedFile(file.serialize()));
            this.SendFile(credential, fstream);

            //Assert.AreEqual(server.uploadFile(fstream), file.hash);
            //fstream.Close();
            
            file = (FBFile)new FBFileBuilder(@"asd\due.txt").generate();
            string[] lines1 = {"First line", "Second line", "Third lines" };
            System.IO.File.WriteAllLines(@"asd\due.txt", lines1);
            fstream = new FileStream(@"asd\due.txt", FileMode.Open, FileAccess.Read);

            credential = server.uploadFile(new SerializedFile(file.serialize()));
            this.SendFile(credential, fstream);
            System.Threading.Thread.Sleep(1000);
            Assert.IsTrue(server.commit());
        }

        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            X509Certificate cert = new X509Certificate("certificate\\certificate.cer");
            if (cert.Equals(cert))
                return true;
            return false;
        }

        private void SendFile(UploadData credential, FileStream fstream)
        {
            System.Threading.Thread.Sleep(100);
            TcpClient client = new TcpClient("127.0.0.1", credential.port);
            SslStream ssl = new SslStream(
                client.GetStream(), false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null, EncryptionPolicy.RequireEncryption);
            try
            {
                ssl.AuthenticateAsClient("127.0.0.1", null, System.Security.Authentication.SslProtocols.Tls12, false);
                ssl.Write(UsefullMethods.GetBytesFromString(credential.token));
                fstream.CopyTo(ssl);
                ssl.Close();
                fstream.Close();
            }
            catch {  }
        }

        [TestMethod]
        public void PersistenceTest()
        {
            TransactionCommitTest();
            server = null;

            server = new Server.Server();
            AuthenticationData ad = server.authStep1("test1");
            token = server.authStep2(ad.token, "test1", AuthenticationPrimitives.hashPassword("test1", ad.salt, ad.token));
            
            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v = (FolderBackup.Shared.FBVersion)vb.generate();

            SerializedVersion serV = new SerializedVersion(v.serialize());

            Assert.IsTrue(server.newTransaction(serV));
        }

        [TestMethod]
        public void RollbackTest()
        {
            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v = (FolderBackup.Shared.FBVersion)vb.generate();

            SerializedVersion serV = new SerializedVersion(v.serialize());


            Assert.IsTrue(server.newTransaction(serV));

            byte[][] bfiles = server.getFilesToUpload();
            foreach (byte[] bf in bfiles)
            {
                FBFile f = FBFile.deserialize(bf);
                Assert.IsTrue(v.fileList.Contains(f));
            }

            FBFile file = (FBFile)new FBFileBuilder(@"asd\uno.txt").generate();
            string[] lines = {"First line", "Second line", "Third line" };
            System.IO.File.WriteAllLines(@"asd\uno.txt", lines);
            FileStream fstream = new FileStream(@"asd\uno.txt", FileMode.Open, FileAccess.Read);
            //Assert.AreEqual(server.uploadFile(fstream), file.hash);
            //fstream.Close();
            UploadData credential = server.uploadFile(new SerializedFile(file.serialize()));
            this.SendFile(credential, fstream);

            file = (FBFile)new FBFileBuilder(@"asd\due.txt").generate();
            string[] lines1 = {"First line", "Second line", "Third lines" };
            System.IO.File.WriteAllLines(@"asd\due.txt", lines1);
            fstream = new FileStream(@"asd\due.txt", FileMode.Open, FileAccess.Read);
            //Assert.AreEqual(server.uploadFile(fstream), file.hash);
            //fstream.Close();
            credential = server.uploadFile(new SerializedFile(file.serialize()));
            this.SendFile(credential, fstream);
            
            Assert.IsTrue(server.rollback());

            Assert.IsTrue(server.newTransaction(serV));

            bfiles = server.getFilesToUpload();
            foreach (byte[] bf in bfiles)
            {
                FBFile f = FBFile.deserialize(bf);
                Assert.IsTrue(v.fileList.Contains(f));
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            if(System.IO.Directory.Exists("asd"))
                System.IO.Directory.Delete("asd", true);
            if(server.session.user.rootDirectory.GetFiles() != null)
                foreach (FileInfo f in server.session.user.rootDirectory.GetFiles())
                {
                    f.Delete();
                }
            if(server.session.user.rootDirectory.GetDirectories() != null)
                foreach (DirectoryInfo d in server.session.user.rootDirectory.GetDirectories())
                {
                    d.Delete(true);
                }
        }
    }
}
