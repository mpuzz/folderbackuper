namespace FolderBackup.CommunicationProtocol
{
    using System.Runtime.Serialization;


    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "AuthenticationData", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    public partial class AuthenticationData : object, System.Runtime.Serialization.IExtensibleDataObject
    {

        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        private string saltField;

        private string tokenField;

        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string salt
        {
            get
            {
                return this.saltField;
            }
            set
            {
                this.saltField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string token
        {
            get
            {
                return this.tokenField;
            }
            set
            {
                this.tokenField = value;
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "SerializedVersion", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    public partial class SerializedVersion : object, System.Runtime.Serialization.IExtensibleDataObject
    {

        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        private byte[] encodedVersionField;

        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte[] encodedVersion
        {
            get
            {
                return this.encodedVersionField;
            }
            set
            {
                this.encodedVersionField = value;
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name = "ServiceErrorMessage", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    public partial class ServiceErrorMessage : object, System.Runtime.Serialization.IExtensibleDataObject
    {

        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;

        private int typeField;

        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }
}


[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
[System.ServiceModel.ServiceContractAttribute(ConfigurationName = "IBackupService")]
public interface IBackupService
{

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/register", ReplyAction = "http://tempuri.org/IBackupService/registerResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(FolderBackup.CommunicationProtocol.ServiceErrorMessage), Action = "http://tempuri.org/IBackupService/registerServiceErrorMessageFault", Name = "ServiceErrorMessage", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    bool register(string username, string password);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/register", ReplyAction = "http://tempuri.org/IBackupService/registerResponse")]
    System.Threading.Tasks.Task<bool> registerAsync(string username, string password);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/authStep1", ReplyAction = "http://tempuri.org/IBackupService/authStep1Response")]
    [System.ServiceModel.FaultContractAttribute(typeof(FolderBackup.CommunicationProtocol.ServiceErrorMessage), Action = "http://tempuri.org/IBackupService/authStep1ServiceErrorMessageFault", Name = "ServiceErrorMessage", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    FolderBackup.CommunicationProtocol.AuthenticationData authStep1(string username);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/authStep1", ReplyAction = "http://tempuri.org/IBackupService/authStep1Response")]
    System.Threading.Tasks.Task<FolderBackup.CommunicationProtocol.AuthenticationData> authStep1Async(string username);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/authStep2", ReplyAction = "http://tempuri.org/IBackupService/authStep2Response")]
    [System.ServiceModel.FaultContractAttribute(typeof(FolderBackup.CommunicationProtocol.ServiceErrorMessage), Action = "http://tempuri.org/IBackupService/authStep2ServiceErrorMessageFault", Name = "ServiceErrorMessage", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    string authStep2(string token, string username, string password);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/authStep2", ReplyAction = "http://tempuri.org/IBackupService/authStep2Response")]
    System.Threading.Tasks.Task<string> authStep2Async(string token, string username, string password);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/getCurrentVersion", ReplyAction = "http://tempuri.org/IBackupService/getCurrentVersionResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(FolderBackup.CommunicationProtocol.ServiceErrorMessage), Action = "http://tempuri.org/IBackupService/getCurrentVersionServiceErrorMessageFault", Name = "ServiceErrorMessage", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    FolderBackup.CommunicationProtocol.SerializedVersion getCurrentVersion(string token);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/getCurrentVersion", ReplyAction = "http://tempuri.org/IBackupService/getCurrentVersionResponse")]
    System.Threading.Tasks.Task<FolderBackup.CommunicationProtocol.SerializedVersion> getCurrentVersionAsync(string token);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/newTransaction", ReplyAction = "http://tempuri.org/IBackupService/newTransactionResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(FolderBackup.CommunicationProtocol.ServiceErrorMessage), Action = "http://tempuri.org/IBackupService/newTransactionServiceErrorMessageFault", Name = "ServiceErrorMessage", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    bool newTransaction(string token, FolderBackup.CommunicationProtocol.SerializedVersion newVersion);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/newTransaction", ReplyAction = "http://tempuri.org/IBackupService/newTransactionResponse")]
    System.Threading.Tasks.Task<bool> newTransactionAsync(string token, FolderBackup.CommunicationProtocol.SerializedVersion newVersion);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/commit", ReplyAction = "http://tempuri.org/IBackupService/commitResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(FolderBackup.CommunicationProtocol.ServiceErrorMessage), Action = "http://tempuri.org/IBackupService/commitServiceErrorMessageFault", Name = "ServiceErrorMessage", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    bool commit(string token);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/commit", ReplyAction = "http://tempuri.org/IBackupService/commitResponse")]
    System.Threading.Tasks.Task<bool> commitAsync(string token);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/rollback", ReplyAction = "http://tempuri.org/IBackupService/rollbackResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(FolderBackup.CommunicationProtocol.ServiceErrorMessage), Action = "http://tempuri.org/IBackupService/rollbackServiceErrorMessageFault", Name = "ServiceErrorMessage", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    bool rollback(string token);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/rollback", ReplyAction = "http://tempuri.org/IBackupService/rollbackResponse")]
    System.Threading.Tasks.Task<bool> rollbackAsync(string token);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/uploadFile", ReplyAction = "http://tempuri.org/IBackupService/uploadFileResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(FolderBackup.CommunicationProtocol.ServiceErrorMessage), Action = "http://tempuri.org/IBackupService/uploadFileServiceErrorMessageFault", Name = "ServiceErrorMessage", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    string uploadFile(System.IO.Stream file);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/uploadFile", ReplyAction = "http://tempuri.org/IBackupService/uploadFileResponse")]
    System.Threading.Tasks.Task<string> uploadFileAsync(System.IO.Stream file);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/getFilesToUpload", ReplyAction = "http://tempuri.org/IBackupService/getFilesToUploadResponse")]
    [System.ServiceModel.FaultContractAttribute(typeof(FolderBackup.CommunicationProtocol.ServiceErrorMessage), Action = "http://tempuri.org/IBackupService/getFilesToUploadServiceErrorMessageFault", Name = "ServiceErrorMessage", Namespace = "http://schemas.datacontract.org/2004/07/FolderBackup.CommunicationProtocol")]
    byte[][] getFilesToUpload(string token);

    [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IBackupService/getFilesToUpload", ReplyAction = "http://tempuri.org/IBackupService/getFilesToUploadResponse")]
    System.Threading.Tasks.Task<byte[][]> getFilesToUploadAsync(string token);
}

[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public interface IBackupServiceChannel : IBackupService, System.ServiceModel.IClientChannel
{
}

[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
public partial class BackupServiceClient : System.ServiceModel.ClientBase<IBackupService>, IBackupService
{

    public BackupServiceClient()
    {
    }

    public BackupServiceClient(string endpointConfigurationName) :
        base(endpointConfigurationName)
    {
    }

    public BackupServiceClient(string endpointConfigurationName, string remoteAddress) :
        base(endpointConfigurationName, remoteAddress)
    {
    }

    public BackupServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
        base(endpointConfigurationName, remoteAddress)
    {
    }

    public BackupServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
        base(binding, remoteAddress)
    {
    }

    public bool register(string username, string password)
    {
        return base.Channel.register(username, password);
    }

    public System.Threading.Tasks.Task<bool> registerAsync(string username, string password)
    {
        return base.Channel.registerAsync(username, password);
    }

    public FolderBackup.CommunicationProtocol.AuthenticationData authStep1(string username)
    {
        return base.Channel.authStep1(username);
    }

    public System.Threading.Tasks.Task<FolderBackup.CommunicationProtocol.AuthenticationData> authStep1Async(string username)
    {
        return base.Channel.authStep1Async(username);
    }

    public string authStep2(string token, string username, string password)
    {
        return base.Channel.authStep2(token, username, password);
    }

    public System.Threading.Tasks.Task<string> authStep2Async(string token, string username, string password)
    {
        return base.Channel.authStep2Async(token, username, password);
    }

    public FolderBackup.CommunicationProtocol.SerializedVersion getCurrentVersion(string token)
    {
        return base.Channel.getCurrentVersion(token);
    }

    public System.Threading.Tasks.Task<FolderBackup.CommunicationProtocol.SerializedVersion> getCurrentVersionAsync(string token)
    {
        return base.Channel.getCurrentVersionAsync(token);
    }

    public bool newTransaction(string token, FolderBackup.CommunicationProtocol.SerializedVersion newVersion)
    {
        return base.Channel.newTransaction(token, newVersion);
    }

    public System.Threading.Tasks.Task<bool> newTransactionAsync(string token, FolderBackup.CommunicationProtocol.SerializedVersion newVersion)
    {
        return base.Channel.newTransactionAsync(token, newVersion);
    }

    public bool commit(string token)
    {
        return base.Channel.commit(token);
    }

    public System.Threading.Tasks.Task<bool> commitAsync(string token)
    {
        return base.Channel.commitAsync(token);
    }

    public bool rollback(string token)
    {
        return base.Channel.rollback(token);
    }

    public System.Threading.Tasks.Task<bool> rollbackAsync(string token)
    {
        return base.Channel.rollbackAsync(token);
    }

    public string uploadFile(System.IO.Stream file)
    {
        return base.Channel.uploadFile(file);
    }

    public System.Threading.Tasks.Task<string> uploadFileAsync(System.IO.Stream file)
    {
        return base.Channel.uploadFileAsync(file);
    }

    public byte[][] getFilesToUpload(string token)
    {
        return base.Channel.getFilesToUpload(token);
    }

    public System.Threading.Tasks.Task<byte[][]> getFilesToUploadAsync(string token)
    {
        return base.Channel.getFilesToUploadAsync(token);
    }
}
