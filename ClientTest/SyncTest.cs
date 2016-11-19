using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.Client;
using System.IO;
using FolderBackup.Shared;
using FolderBackup.CommunicationProtocol;

namespace ClientTest
{
    [TestClass]
    public class SyncTest
    {
        BackupServiceClient server;
        String path = "C:\\syncMe";
        Config conf = Config.Instance();
            
        [TestInitialize]
        public void TestInitialize()
        {
            CleanUp();
            String token;
            String username = "testUser";
            String password = "ciao";
            server = new BackupServiceClient();
            string salt = server.registerStep1(username);
            server.registerStep2(username, AuthenticationPrimitives.hashPassword(password, salt), salt);
            server = MainWindow.logIn(username,password,out token);
            Const<BackupServiceClient>.Instance().set(server);
            if (!Directory.Exists(path))
            {
                conf.targetPath.set(path);
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(path+"\\ciao");
                Directory.CreateDirectory(path + "\\ciao\\ciao");
                string[] lines = { "First line", "Second line", "Third line" };
                string[] lines1 = { "First line", "Second line", "Third lines" };
                System.IO.File.WriteAllLines(path+"\\ciao\\uno.txt", lines);
                System.IO.File.WriteAllLines(path+"\\ciao\\due.txt", lines1);
                System.IO.File.WriteAllLines(path+"\\ciao\\ciao\\due.txt", lines);
            }
        }
        [TestMethod]
        public void SyncFolder()
        {
            SyncEngine sync = SyncEngine.Instance();
            sync.StartSync();
            sync.WaitSync();
            SerializedVersion[] sversions = server.getOldVersions();
            Assert.IsTrue(sversions.Length == 1);
            FBVersion version = FBVersion.deserialize(sversions[0].encodedVersion);
            FBVersionBuilder vb = new FBVersionBuilder(path);
            FBVersion actVersion = (FBVersion) vb.generate();
            Assert.IsTrue(actVersion.Equals(version));

            sync.StartSync();
            sync.WaitSync();
            Assert.IsTrue(sversions.Length == 1);

            CleanUp();
        }
        [TestMethod]
        public void ControlViewTest()
        {
            ControlView cv = new ControlView();
            cv.Show();
            cv.Activate();
            CleanUp();
        }

        private void CleanUp()
        {
            if (Directory.Exists(@"C:\folderBackup\testUser")) {
                Directory.Delete(@"C:\folderBackup\testUser");
            }
        }
    }
}
