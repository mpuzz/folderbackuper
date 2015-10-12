using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.Server;
using FolderBackup.CommunicationProtocol;
using System.ServiceModel;

namespace ServerTests
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        public void CorrectAuthenticationTest()
        {
            FolderBackup.Server.Server serv = new FolderBackup.Server.Server();
            Assert.IsNotNull(serv.auth("test1", "b444ac06613fc8d63795be9ad0beaf55011936ac"));
            Assert.AreEqual(serv.user.rootDirectory.FullName, @"c:\folderBackup\test1");
        }
        [TestMethod]
        [ExpectedException(typeof(FaultException<ServiceErrorMessage>))]
        public void InvalidUserAuthenticationTest()
        {
            FolderBackup.Server.Server serv = new FolderBackup.Server.Server();
            serv.auth("test", "asd");
        }


        [TestMethod]
        [ExpectedException(typeof(FaultException<ServiceErrorMessage>))]
        public void BadPasswordAuthenticationTest()
        {
            FolderBackup.Server.Server serv = new FolderBackup.Server.Server();
            serv.auth("test1", "asd");
        }

        
    }
}
