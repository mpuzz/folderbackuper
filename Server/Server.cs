using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using FolderBackup.CommunicationProtocol;
using FolderBackup.Shared;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.IO.Compression;

namespace FolderBackup.Server
{
    public delegate void NotifyErrorReceiving(string token);
    public delegate void NotifyReceiveComplete(FBFile file, PhysicFile pf);

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.PerSession)]
    public class Server : IBackupService
    {
        public Session session;
        private List<SecureChannel> channels = new List<SecureChannel>();

        public Server()
        {
            this.session = new Session();
        }

        public string registerStep1(string username)
        {
            if (User.getSalt(username) != null) return null;
            return Server.GetUniqueKey(10);
        }

        public bool registerStep2(string username, string password, string salt)
        {
            if (User.register(username, password, salt))
            {
                if (Directory.Exists(@"c:\folderBackup\" + username + "\\"))
                {
                    Directory.Delete(@"c:\folderBackup\" + username + "\\", true);
                }
                Directory.CreateDirectory(@"c:\folderBackup\" + username + "\\");
                return true;
            }

            return false;
        }

        public AuthenticationData authStep1(string username)
        {
            string salt = User.getSalt(username);

            if(salt == null)
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.AUTHENTICATIONFAILED));

            Session s = new Session();
            String token = Server.GetUniqueKey(20);

            return new AuthenticationData(salt, token);
        }

        public string authStep2(string token, string username, string password)
        {
            User u;

            try
            {
                u = User.authUser(username, password, token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.GetType());
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.AUTHENTICATIONFAILED));
            }

            session.user = u;
            token = GetUniqueKey(20);
            session.initializeUser();
            
            return token;
        }

        public static string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public SerializedVersion getCurrentVersion()
        {
            return session.getCurrentVersion();
        }

        public bool newTransaction(SerializedVersion newVersion)
        {
            return session.newTransaction(newVersion);
        }

        public bool commit()
        {
            return session.commit();
        }

        public bool rollback()
        {
            return session.rollback();
        }

        public void ManageCompleteUpload(FBFile f, PhysicFile pf)
        {
            this.session.uploadedFiles.add(pf);

            this.session.necessaryFiles.Remove(f);
        }

        public void ManageFailedUpload(string token)
        {

        }

        public UploadData uploadFile(SerializedFile fileStream)
        {
            Console.WriteLine("ASD");
            string token = Server.GetUniqueKey(20);
            SecureChannel channel = new SecureChannel(this, token, this.ManageCompleteUpload, this.ManageFailedUpload);
            UInt16 port = channel.port;
            this.channels.Add(channel);

            return new UploadData(port, token);
        }

        public byte[][] getFilesToUpload()
        {
            return session.getFilesToUpload();
        }

        public SerializedVersion[] getOldVersions()
        {
            LinkedList<FBVersion> versions = session.getOldVersions();
            SerializedVersion[] svers = new SerializedVersion[versions.Count];
            int i = 0;
            foreach (FBVersion ver in versions)
            {
                svers[i++] = new SerializedVersion(ver.serialize());
            }

            return svers;
        }

        public UInt16 resetToPreviousVersion(int versionAgo)
        {
            return session.revertVersion(versionAgo);
        }
    }
}