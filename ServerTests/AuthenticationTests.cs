using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.Server;
using FolderBackup.CommunicationProtocol;
using System.ServiceModel;
using System.Security.Cryptography;
using System.Text;
using FolderBackup.Shared;

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
            AuthenticationData ad = serv.authStep1("test1");
            token = ad.token;
            Assert.IsNotNull(ad);
            token = serv.authStep2(ad.token, "test1", AuthenticationPrimitives.hashPassword("test1", ad.salt, ad.token));
            Assert.AreEqual(FolderBackup.Server.Server.getSessionByToken(token).user.rootDirectory.FullName, @"c:\folderBackup\test1\");
        }
        [TestMethod]
        [ExpectedException(typeof(FaultException<ServiceErrorMessage>))]
        public void InvalidUserAuthenticationTest()
        {
            FolderBackup.Server.Server serv = new FolderBackup.Server.Server();
            AuthenticationData ad = serv.authStep1("test");
        }


        [TestMethod]
        [ExpectedException(typeof(FaultException<ServiceErrorMessage>))]
        public void BadPasswordAuthenticationTest()
        {
            FolderBackup.Server.Server serv = new FolderBackup.Server.Server();
            AuthenticationData ad = serv.authStep1("test1");
            token = ad.token;
            Assert.IsNotNull(ad);
            token = serv.authStep2(ad.token, "test1", AuthenticationPrimitives.hashPassword("asd", ad.salt, ad.token));
        }
        
        
    }
}
