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
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IBackupService
    {
        [OperationContract(IsInitiating = true)]
        [FaultContract(typeof(ServiceErrorMessage))]
        string registerStep1(string username);
        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        bool registerStep2(string username, string password, string salt);

        [OperationContract(IsInitiating = true)]
        [FaultContract(typeof(ServiceErrorMessage))]
        AuthenticationData authStep1(string username);

        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        string authStep2(string token, string username, string password);

        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        SerializedVersion getCurrentVersion();

        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        Boolean newTransaction(SerializedVersion newVersion);

        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        Boolean commit();

        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        Boolean rollback();

        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        UploadData uploadFile();

        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        byte[][] getFilesToUpload();

        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        UploadData resetToPreviousVersion(int versionAgo);

        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        SerializedVersion[] getOldVersions();

        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        UploadData getFile(SerializedVersion serV);

        [OperationContract(IsInitiating = false)]
        [FaultContract(typeof(ServiceErrorMessage))]
        UploadData resetToCraftedVersion(SerializedVersion serV);
    }

    [DataContract]
    public class SerializedVersion
    {
        [DataMember]
        public byte[] encodedVersion;
        public SerializedVersion(byte[] serV)
        {
            encodedVersion = serV;
        }
    }

    [DataContract]
    public class SerializedFile
    {
        [DataMember]
        public byte[] encodedFile;
        public SerializedFile(byte[] serF)
        {
            encodedFile = serF;
        }
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

    [DataContract]
    public class AuthenticationData
    {
        [DataMember]
        public string salt;
        [DataMember]
        public string token;

        public AuthenticationData(string s, string t)
        {
            this.token = t;
            this.salt = s;
        }
    }

    [DataContract]
    public class UploadData
    {
        [DataMember]
        public String ip;
        [DataMember]
        public UInt16 port;
        [DataMember]
        public string token;

        public UploadData(String ip, UInt16 p, string t)
        {
            this.ip = ip;
            this.token = t;
            this.port = p;
        }
    }
}
