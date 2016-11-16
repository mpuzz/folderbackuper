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
            Config.Instance().targetPath.set("C:\\Users\\Andrea\\sincronizzami");
        }
        [TestMethod]
        public void SyncFolder()
        {
            SyncEngine sync = new SyncEngine();
            sync.sync();
        }
    }
}
