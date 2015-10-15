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

namespace FolderBackup.Server
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.PerCall)]
    public class Server : IBackupService
    {

        static private Dictionary<string, Session> sessions = new Dictionary<string, Session>();

        public Server()
        {
            
        }

        
        
        public string auth(string username, string password)
        {
            Session s;
            try
            {
                s = Session.auth(username, password);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.AUTHENTICATIONFAILED));
            }

            String token = Server.GetUniqueKey(20);
            s.token = token;
            Server.sessions.Add(token, s);

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

            StreamReader sr = new StreamReader(fileStream, Encoding.Default);
            sr.Read(chars, 0, 20);
            foreach (char c in chars)
            {
                token += c;
            }
            return Server.getSessionByToken(token).uploadFile(fileStream);
        }

        public byte[][] getFilesToUpload(string token)
        {
            return Server.getSessionByToken(token).getFilesToUpload();
        }
    }
}