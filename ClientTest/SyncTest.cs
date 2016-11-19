using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.Client;


namespace ClientTest
{
    [TestClass]
    public class SyncTest
    {
        BackupServiceClient server;
        [TestInitialize]
        public void TestInitialize()
        {
            String token;
            server = MainWindow.logIn("asd","123",out token);
            Const<BackupServiceClient>.Instance().set(server);
            Config.Instance().targetPath.set("C:\\Users\\Andrea\\sincronizzami");
        }
        [TestMethod]
        public void SyncFolder()
        {
            SyncEngine sync = new SyncEngine();
            sync.StartSync();
        }
        [TestMethod]
        public void ControlViewTest()
        {
            ControlView cv = new ControlView();
            cv.Show();
            cv.Activate();
        }
    }
}
