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
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.PerCall)]
    public class Server : IBackupService
    {
        static private Dictionary<string, Session> sessions = new Dictionary<string, Session>();

        public Server()
        {
            
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
            s.token = token;
            Server.sessions.Add(token, s);

            return new AuthenticationData(salt, token);
        }

        public string authStep2(string token, string username, string password)
        {
            User u;

            Session s = getSessionByToken(token);
            try
            {
                u = User.authUser(username, password, token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.GetType());
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.AUTHENTICATIONFAILED));
            }

            s.user = u;
            Server.sessions.Remove(token);
            token = GetUniqueKey(20);
            s.token = token;
            Server.sessions.Add(token, s);
            s.initializeUser();
            
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

        public static Session getSessionByToken(string token)
        {
            Session session = Server.sessions[token];
            if (session == null)
            {
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.PERMISSIONDENIED));
            }
            return session;
        }

        public SerializedVersion getCurrentVersion(string token)
        {
            Session session = Server.getSessionByToken(token);

            return session.getCurrentVersion();
        }

        public bool newTransaction(string token, SerializedVersion newVersion)
        {
            return Server.getSessionByToken(token).newTransaction(newVersion);
        }

        public bool commit(string  token)
        {
            return Server.getSessionByToken(token).commit();
        }

        public bool rollback(string token)
        {
            return Server.getSessionByToken(token).rollback();
        }

        public string uploadFile(Stream fileStream)
        {
            string token = "";
            char[] chars = new char[20];

            if(!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");
            SaveStreamToFile(fileStream, "tmp\\prova");
            fileStream.Close();

            FileStream fs = new FileStream("tmp\\prova", FileMode.Open, FileAccess.Read);

            StreamReader sr = new StreamReader(fs, Encoding.Default);
            sr.Read(chars, 0, 20);
            foreach (char c in chars)
            {
                token += c;
            }
            return Server.getSessionByToken(token).uploadFile(fs);
        }

        private static void SaveStreamToFile(System.IO.Stream stream, string filePath)
        {
            FileStream outstream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            CopyStream(stream, outstream);
            outstream.Close();
            stream.Close();
        }

        private static void CopyStream(System.IO.Stream instream, System.IO.Stream outstream)
        {
            const int bufferLen = 4096;
            byte[] buffer = new byte[bufferLen];
            int count = 0;
            int bytecount = 0;
            while ((count = instream.Read(buffer, 0, bufferLen)) > 0)
            {
                outstream.Write(buffer, 0, count);
                bytecount += count;
            }
        }

        public byte[][] getFilesToUpload(string token)
        {
            return Server.getSessionByToken(token).getFilesToUpload();
        }

        public SerializedVersion[] getOldVersions(string token)
        {
            LinkedList<FBVersion> versions = Server.getSessionByToken(token).getOldVersions();
            SerializedVersion[] svers = new SerializedVersion[versions.Count];
            int i = 0;
            foreach (FBVersion ver in versions)
            {
                svers[i] = new SerializedVersion();
                svers[i++].encodedVersion = ver.serialize();
            }

            return svers;
        }

        public Stream resetToPreviousVersion(string token, int versionAgo)
        {
            return Server.getSessionByToken(token).revertVersion(versionAgo);
        }
    }
}