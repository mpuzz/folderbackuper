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

namespace FolderBackup.Server
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerSession)]
    public class Server : IBackupService
    {
        private int instanceNumber;
        static private int nInstance = 0;
        private List<FBFile> necessaryFiles;
        private PhysicFilesList realFiles;
        private PhysicFilesList uploadedFiles;

        public User user;
        public FBVersion inSyncVersion { get; set; }

        private DirectoryInfo ptransactDir;

        public DirectoryInfo transactDir
        {

            get
            {
                return this.transactionEnabled ? this.ptransactDir : null;
            }
            set
            {
                this.ptransactDir = this.transactionEnabled ? value : null;
            }
        }

        private Boolean ptransactEnabled;
        public Boolean transactionEnabled
        {
            get
            {
                return (user == null ? false : ptransactEnabled);
            }
            set
            {
                ptransactEnabled = (user == null ? false : value);
            }
        }

        public Server()
        {
            this.instanceNumber = Server.newInstance();
            this.user = null;
        }

        static private int newInstance()
        {
            Server.nInstance++;
            return Server.nInstance;
        }

        private void checkAuthentication()
        {
            if (this.user == null)
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.PERMISSIONDENIED));
        }

        public bool auth(string username, string password)
        {
            try
            {
                this.user = User.authUser(username, password);

                if (!File.Exists(this.user.rootDirectory + @"\files.bin"))
                {
                    this.realFiles = new PhysicFilesList();
                    Stream FilesStream = File.OpenWrite(this.user.rootDirectory + @"\files.bin");
                    BinaryFormatter serializer = new BinaryFormatter();
                    serializer.Serialize(FilesStream, realFiles);
                    FilesStream.Close();
                }
                else
                {
                    Stream FilesStream1 = File.OpenRead(this.user.rootDirectory + @"\files.bin");
                    BinaryFormatter deserializer = new BinaryFormatter();
                    this.realFiles = (PhysicFilesList)deserializer.Deserialize(FilesStream1);
                    FilesStream1.Close();
                }
            }
            catch
            {
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.AUTHENTICATIONFAILED));
            }
            
            return true;
        }

        public SerializedVersion getCurrentVersion()
        {
            this.checkAuthentication();

            this.checkTransactionIsNotEnabled();
            
            try
            {
                FBVersion version = currentVersion();
                SerializedVersion serVersion = new SerializedVersion();
                serVersion.encodedVersion = version.serialize();

                return serVersion;
            }
            catch (Exception e)
            {
                throw e;
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.ROOTDIRECTORYNOTFOUND));
            }
        }

        private FBVersion currentVersion()
        {

            DirectoryInfo[] versionDirs = user.rootDirectory.GetDirectories();
            if (versionDirs == null) throw new Exception();

            if (versionDirs.Length == 0)
            {
                Directory.CreateDirectory(user.rootDirectory.FullName + @"\1970_01_01__00_00_00");
                versionDirs = user.rootDirectory.GetDirectories();
            }

            DirectoryInfo last = versionDirs[0];
            
            foreach (DirectoryInfo di in versionDirs)
            {
                if (last.Equals(di)) continue;

                DateTime dtl = DateTime.ParseExact(last.Name, "yyyy_MM_dd__HH_mm_ss", CultureInfo.InvariantCulture);
                DateTime dt2 = DateTime.ParseExact(di.Name, "yyyy_MM_dd__HH_mm_ss", CultureInfo.InvariantCulture);
                if (dt2 > dtl) last = di;
            }

            Stream TestFileStream = File.OpenRead(last.FullName + @"\version.bin");
            BinaryFormatter deserializer = new BinaryFormatter();
            FBVersion version = (FBVersion)deserializer.Deserialize(TestFileStream);
            TestFileStream.Close();

            return version;
        }

        public bool newTransaction(SerializedVersion newVersion)
        {
            this.checkAuthentication();
            
            if (this.transactionEnabled)
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.TRANSACTIONALREADYENABLED));

            this.transactionEnabled = true;
            this.inSyncVersion = FBVersion.deserialize(newVersion.encodedVersion);

            String newDirPath = this.user.rootDirectory.FullName;
            newDirPath += "\\" + this.inSyncVersion.timestamp.ToString("yyyy_MM_dd__HH_mm_ss", CultureInfo.InvariantCulture);
            this.transactDir = Directory.CreateDirectory(newDirPath);
            if (this.transactDir == null)
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.CREATEVERSIONDIRECTORYFAILED));

            necessaryFiles = FBVersion.getNecessaryFilesToUpgrade(this.inSyncVersion, this.realFiles.filesAlreadyRepresented());

            return true;
        }

        private void checkTransactionIsNotEnabled()
        {
            if (this.transactionEnabled)
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.TRANSACTIONENABLED));
        }
        private void checkTransactionIsEnabled()
        {
            if (!this.transactionEnabled)
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.TRANSACTIONNOTENABLED));
        }

        public bool commit()
        {
            this.checkAuthentication();
            this.checkTransactionIsEnabled();

            FBVersionBuilder fvb = new FBVersionBuilder(this.ptransactDir.FullName);
            FBVersion actualVersion = (FBVersion)fvb.generate();
            actualVersion.timestamp = inSyncVersion.timestamp;

            if (this.necessaryFiles.Count == 0)
            {
                Stream FileStream = File.Create(this.transactDir.FullName + @"\version.bin");
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(FileStream, this.inSyncVersion);
                FileStream.Close();

                realFiles.add(this.uploadedFiles);

                FileStream = File.OpenWrite(this.user.rootDirectory + @"\files.bin");
                serializer.Serialize(FileStream, realFiles);
                FileStream.Close();

                this.uploadedFiles = null;
                this.transactDir = null;
                this.inSyncVersion = null;
                this.transactionEnabled = false;
                return true;
            }
            else
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.SYNCNOTTERMINATED));
            
        }

        public bool rollback()
        {
            this.checkAuthentication();
            this.checkTransactionIsEnabled();

            if (this.transactDir != null)
            {
                this.transactDir.Delete(true);
                this.transactDir = null;
            }

            this.inSyncVersion = null;
            this.transactionEnabled = false;
            foreach (PhysicFile pf in this.uploadedFiles.list)
            {
                File.Delete(pf.getRealFileInfo().FullName);
            }
            this.uploadedFiles = null;

            return true;
        }

        public string uploadFile(Stream fileStream)
        {
            string path = this.user.rootDirectory.FullName + "\\" + DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss_fff", CultureInfo.InvariantCulture);
            FBFile newFile;
            FBFileBuilder fb;

            SaveStreamToFile(fileStream, path);
            fb = new FBFileBuilder(path);
            newFile = (FBFile) fb.generate();

            if (!this.necessaryFiles.Contains(newFile))
            {
                File.Delete(path);
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.FILENOTNECESSARY));
            }

            this.uploadedFiles.add(new PhysicFile(newFile, path));

            this.necessaryFiles.Remove(newFile);
            return newFile.hash;
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

        public byte[][] getFilesToUpload()
        {
            this.checkAuthentication();
            this.checkTransactionIsEnabled();

            necessaryFiles = FBVersion.getNecessaryFilesToUpgrade(this.inSyncVersion, this.realFiles.filesAlreadyRepresented());
            this.uploadedFiles = new PhysicFilesList();

            byte[][] ret = new byte[necessaryFiles.Count][];
            for (int i = 0; i < necessaryFiles.Count; ++i)
            {
                ret[i] = necessaryFiles.ElementAt(i).serialize();
            }
            return ret;
        }
    }
}