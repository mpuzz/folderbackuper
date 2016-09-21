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
using System.IO.Compression;

namespace FolderBackup.Server
{
    public class Session
    {
        static public Dictionary<string, Session> sessions = new Dictionary<string, Session>();
        private List<FBFile> necessaryFiles;
        private PhysicFilesList realFiles;
        private PhysicFilesList uploadedFiles;
        public string token;

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

        public Session()
        {
        }

        private void checkAuthentication()
        {
            if (this.user == null)
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.PERMISSIONDENIED));
        }

        public void initializeUser() {
            if (!File.Exists(this.user.rootDirectory + @"\files.bin")) //primo accesso
            {
                this.realFiles = new PhysicFilesList();
                Stream FilesStream = File.OpenWrite(this.user.rootDirectory + @"\files.bin");
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(FilesStream, realFiles);
                FilesStream.Close();

                this.user.rootDirectory.CreateSubdirectory("1970_01_01__00_00_00");
                FBVersionBuilder vb = new FBVersionBuilder(user.rootDirectory.FullName + @"\1970_01_01__00_00_00");
                FBVersion v = (FBVersion)vb.generate();
                FilesStream = File.OpenWrite(this.user.rootDirectory.FullName + @"\1970_01_01__00_00_00\version.bin");
                serializer.Serialize(FilesStream, v);
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
                Console.WriteLine(e.Message);
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


            FBVersion vers = FBVersion.deserialize(newVersion.encodedVersion);
            if (vers.Equals(this.getCurrentVersion()))
            {
                return false;
            }

            this.transactionEnabled = true;
            this.inSyncVersion = vers;

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

                FileStream = File.Open(this.user.rootDirectory + @"\files.bin", FileMode.Create, FileAccess.Write, FileShare.None);
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
            fileStream.Seek(20, SeekOrigin.Begin);
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

        public LinkedList<FBVersion> getOldVersions()
        {
            DirectoryInfo[] versionDirs = user.rootDirectory.GetDirectories();
            LinkedList<FBVersion> versions = new LinkedList<FBVersion>();
            
            if (versionDirs == null) throw new Exception();

            if (versionDirs.Length == 0)
            {
                Directory.CreateDirectory(user.rootDirectory.FullName + @"\1970_01_01__00_00_00");
                versionDirs = user.rootDirectory.GetDirectories();
            }


            foreach (DirectoryInfo di in versionDirs)
            {
                Stream TestFileStream = File.OpenRead(di.FullName + @"\version.bin");
                BinaryFormatter deserializer = new BinaryFormatter();
                FBVersion version = (FBVersion)deserializer.Deserialize(TestFileStream);
                versions.AddLast(version);
                TestFileStream.Close();
            }

            

            return versions;
        }

        public Stream revertVersion(int versionAgo)
        {
            var directories = Directory.EnumerateDirectories(user.rootDirectory.FullName).OrderByDescending(filename => filename);
            
            Stream TestFileStream = File.OpenRead((new DirectoryInfo(directories.ElementAt(versionAgo))).FullName + @"\version.bin");
            BinaryFormatter deserializer = new BinaryFormatter();
            FBVersion old = (FBVersion)deserializer.Deserialize(TestFileStream);
            TestFileStream.Close();

            FBVersion actual = this.currentVersion();

            FBVersion diff = old - actual;

            old.setAbsoluteNameToFile();
            diff.setAbsoluteNameToFile();
            actual.setAbsoluteNameToFile();

            List<Instruction> instrucionList = new List<Instruction>();

            try { File.Delete(user.rootDirectory.FullName + @"\tmp.zip"); }
            catch { }

            ZipArchive zip = ZipFile.Open(user.rootDirectory.FullName + @"\tmp.zip", ZipArchiveMode.Update);

            foreach (FBAbstractElement to in diff.fileList)
            {
                bool found = false;
                foreach (FBAbstractElement from in actual.fileList)
                {
                    if (from.Equals(to))
                    {
                        found = true;
                        instrucionList.Add(new Instruction(InstructionType.COPY, from.Name, to.Name));
                        break;
                    }
                }

                if (!found)
                {
                    foreach (PhysicFile ph in realFiles.list)
                    {
                        if (ph.getFBFile().Equals(to))
                        {
                            if (zip.GetEntry(ph.getRealFileInfo().Name) != null)
                            {
                                zip.CreateEntryFromFile(ph.getRealFileInfo().FullName, ph.getRealFileInfo().Name, CompressionLevel.Optimal);
                            }
                            instrucionList.Add(new Instruction(InstructionType.NEW, ph.getRealFileInfo().Name, to.Name));
                        }
                    }
                }
            }

            diff = actual - old;

            foreach (FBAbstractElement toDelete in diff.fileList)
            {
                instrucionList.Add(new Instruction(InstructionType.DELETE, toDelete.Name, ""));
            }

            Stream FilesStream = File.OpenWrite(this.user.rootDirectory + @"\instructions.bin");
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(FilesStream, instrucionList);
            FilesStream.Close();

            zip.CreateEntryFromFile(this.user.rootDirectory + @"\instructions.bin", "instructions.bin", CompressionLevel.Optimal);
            File.Delete(this.user.rootDirectory + @"\instructions.bin");
            return new FileStream(user.rootDirectory.FullName + @"\tmp.zip", FileMode.Open, FileAccess.Read);
        }
    }
}
