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
        Boolean auth(string username, string password);

        [OperationContract]
        SerializedVersion getCurrentVersion();

        [OperationContract]
        Boolean newTransaction(SerializedVersion newVersion);

        [OperationContract]
        Boolean commit();

        [OperationContract]
        Boolean rollback();

        [OperationContract]
        string uploadFile(Stream file);

        [OperationContract]
        byte[][] getFilesToUpload();

    }

    // Per aggiungere tipi compositi alle operazioni del servizio utilizzare un contratto di dati come descritto nell'esempio seguente.
    // È possibile aggiungere file XSD nel progetto. Dopo la compilazione del progetto è possibile utilizzare direttamente i tipi di dati definiti qui con lo spazio dei nomi "CommunicationProtocol.ContractType".
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
