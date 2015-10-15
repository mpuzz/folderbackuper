using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;

namespace FolderBackup.CommunicationProtocol
{
    // NOTA: è possibile utilizzare il comando "Rinomina" del menu "Refactoring" per modificare il nome di interfaccia "IBackupService" nel codice e nel file di configurazione contemporaneamente.
    [ServiceContract]
    public interface IBackupService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceErrorMessage))]
        string auth(string username, string password);

        [OperationContract]
        SerializedVersion getCurrentVersion(string token);

        [OperationContract]
        Boolean newTransaction(string token, SerializedVersion newVersion);

        [OperationContract]
        Boolean commit(string token);

        [OperationContract]
        Boolean rollback(string token);

        [OperationContract]
        string uploadFile(Stream file);

        [OperationContract]
        byte[][] getFilesToUpload(string token);

    }

    [DataContract]
    public class SerializedVersion
    {
        [DataMember]
        public byte[] encodedVersion;
    }

    [DataContract]
    public class ServiceErrorMessage
    {
        public const int AUTHENTICATIONFAILED = 1;
        public const int PERMISSIONDENIED = 2;
        public const int ROOTDIRECTORYNOTFOUND = 3;
        public const int TRANSACTIONNOTENABLED = 4;
        public const int CREATEVERSIONDIRECTORYFAILED = 5;
        public const int TRANSACTIONALREADYENABLED = 6;
        public const int SYNCNOTTERMINATED = 7;
        public const int TRANSACTIONENABLED = 8;
        public const int FILENOTNECESSARY = 9;
        public ServiceErrorMessage(int type)
        {
            this.type = type;
        }

        [DataMember]
        int type;
    }
}
