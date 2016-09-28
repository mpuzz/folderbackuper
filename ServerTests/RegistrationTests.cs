using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderBackup.Server;
using FolderBackup.CommunicationProtocol;
using System.ServiceModel;
using System.Security.Cryptography;
using System.Text;
using FolderBackup.Shared;

namespace FolderBackup.ServerTests
{
    [TestClass]
    public class RegistrationTests
    {
        private const string username = "registration_test";
        private const string password = "asdasdasd";
        private Server.Server server;

        [TestInitialize]
        public void TestInitialize()
        {
            CleanUp();
            server = new Server.Server();
            
        }

        private bool Register()
        {
            string salt = server.registerStep1(username);
            string newPassword = AuthenticationPrimitives.hashPassword(password, salt);
            return server.registerStep2(username, newPassword, salt);
        }

        [TestMethod]
        public void CorrectRegistrationTest()
        {
            Assert.IsTrue(Register());

            CleanUp();
        }

        [TestMethod]
        public void DoubleRegistration()
        {
            Register();

            Assert.IsNull(server.registerStep1(username));

            CleanUp();
        }

        public void CleanUp()
        {
            DatabaseManager.getInstance().Delete(username);   
        }
    }
}
