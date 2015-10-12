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
        public Boolean auth(string username, string password) { return true; }
        public SerializedVersion getCurrentVersion() { return null; }
        public Boolean newTransaction(SerializedVersion newVersion) { return true; }
        public Boolean commit() { return true; }

        public Boolean rollback() { return false; }

        public string uploadFile(Stream file) { return ""; }

        public byte[][] getFilesToUpload() { return null; }
    }
}
