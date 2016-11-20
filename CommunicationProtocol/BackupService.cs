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
        public SerializedVersion getCurrentVersion() { return null; }
        public Boolean newTransaction(SerializedVersion newVersion) { return true; }
        public Boolean commit() { return true; }

        public Boolean rollback() { return false; }

        public UploadData uploadFile() { return null; }

        public byte[][] getFilesToUpload() { return null; }

        public UploadData resetToPreviousVersion(int versionAgo) { return null; }

        public SerializedVersion[] getOldVersions() { return null; }

        public UploadData getFile(SerializedVersion serV) { return null; }
    }
}
