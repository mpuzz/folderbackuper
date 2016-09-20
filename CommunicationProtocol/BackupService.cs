using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;

namespace FolderBackup.CommunicationProtocol
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession)] 
    public class BackupService : IBackupService
    {
        public string registerStep1(string username) { return ""; }
        public bool registerStep2(string username, string password, string salt) { return true; }
        public AuthenticationData authStep1(string username) { return null; }
        public string authStep2(string token, string username, string password) { return ""; }
        public SerializedVersion getCurrentVersion(string token) { return null; }
        public Boolean newTransaction(string token, SerializedVersion newVersion) { return true; }
        public Boolean commit(string token) { return true; }

        public Boolean rollback(string token) { return false; }

        public string uploadFile(Stream file) { return ""; }

        public byte[][] getFilesToUpload(string token) { return null; }

        public Stream resetToPreviousVersion(string token, int versionAgo) { return null; }

        public SerializedVersion[] getOldVersions(string token) { return null; }
    }
}
