using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.Server;
using FolderBackup.CommunicationProtocol;
using System.ServiceModel;

namespace ServerTests
{
    [TestClass]
    public class AuthenticationTests
    {
        string token;
        [TestMethod]
        public void CorrectAuthenticationTest()
        {
            FolderBackup.Server.Server serv = new FolderBackup.Server.Server();
            token = serv.auth("test1", "test1");
            Assert.IsNotNull(token);
            Assert.AreEqual(FolderBackup.Server.Server.getSessionByToken(token).user.rootDirectory.FullName, @"c:\folderBackup\test1");
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
