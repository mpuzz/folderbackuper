﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
using System.Security.AccessControl;
using System.IO.Compression;

namespace FolderBackup.Server
{
    public delegate void NotifyErrorReceiving(string token);
    public delegate void NotifyReceiveComplete(FBFile file, PhysicFile pf, string token);
    

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.PerSession)]
    public class Server : IBackupService
    {
        public static string directoryFormat = "yyyy_MM_dd__HH_mm_ss_fff";
        public static string firstDirectory  = "1970_01_01__00_00_00_000";
        private ConcurrentDictionary<String, SecureChannel> channels = new ConcurrentDictionary<string, SecureChannel>();

        public ThreadSafeList<FBFile> necessaryFiles;
        private PhysicFilesList realFiles;
        public PhysicFilesList uploadedFiles;

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
        }

        public string registerStep1(string username)
        {
            if (User.getSalt(username) != null) return null;
            return Server.GetUniqueKey(10);
        }

        public bool registerStep2(string username, string password, string salt)
        {
            if (User.register(username, password, salt))
            {
                if (Directory.Exists(@"c:\folderBackup\" + username + "\\"))
                {
                    Directory.Delete(@"c:\folderBackup\" + username + "\\", true);
                }
                Directory.CreateDirectory(@"c:\folderBackup\" + username + "\\");
                return true;
            }

            return false;
        }

        private void checkAuthentication()
        {
            if (this.user == null)
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.PERMISSIONDENIED));
        }

        public AuthenticationData authStep1(string username)
        {
            string salt = User.getSalt(username);

            if(salt == null)
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.AUTHENTICATIONFAILED));

            String token = Server.GetUniqueKey(20);

            return new AuthenticationData(salt, token);
        }

        private void initializeUser()
        {
            if (!File.Exists(this.user.rootDirectory + @"\files.bin")) //primo accesso
            {
                this.realFiles = new PhysicFilesList();
                Stream FilesStream = File.OpenWrite(this.user.rootDirectory + @"\files.bin");
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(FilesStream, realFiles);
                FilesStream.Close();

                this.user.rootDirectory.CreateSubdirectory(firstDirectory);
                FBVersionBuilder vb = new FBVersionBuilder(user.rootDirectory.FullName + @"\" + firstDirectory);
                FBVersion v = (FBVersion)vb.generate();
                FilesStream = File.OpenWrite(this.user.rootDirectory.FullName + @"\" + firstDirectory + @"\version.bin");
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

        public string authStep2(string token, string username, string password)
        {
            User u;

            try
            {
                u = User.authUser(username, password, token);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " " + e.GetType());
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.AUTHENTICATIONFAILED));
            }

            user = u;
            token = GetUniqueKey(20);
            initializeUser();
            
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

        private FBVersion currentVersion()
        {

            DirectoryInfo[] versionDirs = user.rootDirectory.GetDirectories();
            if (versionDirs == null) throw new Exception();

            if (versionDirs.Length == 0)
            {
                Directory.CreateDirectory(user.rootDirectory.FullName + @"\" + firstDirectory);
                versionDirs = user.rootDirectory.GetDirectories();
            }

            DirectoryInfo last = versionDirs[0];

            foreach (DirectoryInfo di in versionDirs)
            {
                if (last.Equals(di)) continue;

                DateTime dtl = DateTime.ParseExact(last.Name, directoryFormat, CultureInfo.InvariantCulture);
                DateTime dt2 = DateTime.ParseExact(di.Name, directoryFormat, CultureInfo.InvariantCulture);
                if (dt2 > dtl) last = di;
            }
            Stream TestFileStream = null;
            try { 
                TestFileStream = File.OpenRead(last.FullName + @"\version.bin");
            }catch(FileNotFoundException )
            {
                Directory.Delete(last.FullName);
                return currentVersion();
            }
              BinaryFormatter deserializer = new BinaryFormatter();
            FBVersion version = (FBVersion)deserializer.Deserialize(TestFileStream);
            TestFileStream.Close();

            return version;
        }

        public SerializedVersion getCurrentVersion()
        {
            this.checkAuthentication();

            this.checkTransactionIsNotEnabled();

            try
            {
                FBVersion version = currentVersion();
                SerializedVersion serVersion = new SerializedVersion(version.serialize());

                return serVersion;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.ROOTDIRECTORYNOTFOUND));
            }
        }

        public bool newTransaction(SerializedVersion newVersion)
        {
            this.checkAuthentication();

            if (this.transactionEnabled)
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.TRANSACTIONALREADYENABLED));

            FBVersion vers = FBVersion.deserialize(newVersion.encodedVersion);
            FBVersion current = FBVersion.deserialize(this.getCurrentVersion().encodedVersion);
            if (vers.Equals(current))
            {
                return false;
            }

            this.transactionEnabled = true;
            this.inSyncVersion = vers;

            String newDirPath = this.user.rootDirectory.FullName;
            newDirPath += "\\" + this.inSyncVersion.timestamp.ToString(directoryFormat, CultureInfo.InvariantCulture);
            this.transactDir = Directory.CreateDirectory(newDirPath);
            if (this.transactDir == null)
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.CREATEVERSIONDIRECTORYFAILED));

            necessaryFiles = new ThreadSafeList<FBFile>(FBVersion.getNecessaryFilesToUpgrade(this.inSyncVersion, this.realFiles.filesAlreadyRepresented()));

            return true;
        }

        public bool commit()
        {
            this.checkAuthentication();
            this.checkTransactionIsEnabled();

            FBVersionBuilder fvb = new FBVersionBuilder(this.ptransactDir.FullName);
            FBVersion actualVersion = (FBVersion)fvb.generate();
            actualVersion.timestamp = inSyncVersion.timestamp;

            foreach(KeyValuePair<String, SecureChannel> pair in channels)
            {
                pair.Value.join();
            }

            if (this.necessaryFiles.Count == 0)
            {
                Stream FileStream = File.Create(this.transactDir.FullName + @"\version.bin");
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(FileStream, this.inSyncVersion);
                FileStream.Close();

                if(this.uploadedFiles != null)
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

        public void ManageCompleteUpload(FBFile f, PhysicFile pf, string token)
        {
            this.uploadedFiles.add(pf);

            this.necessaryFiles.Remove(f);
            SecureChannel chan;
            this.channels.TryRemove(token, out chan);
        }

        public void ManageFailedUpload(string token)
        {
            SecureChannel chan;
            this.channels.TryRemove(token, out chan);
        }

        public UploadData uploadFile()
        {
            this.checkAuthentication();
            this.checkTransactionIsEnabled();
            if (uploadedFiles == null)
            {
                return null;
            }

            string token = Server.GetUniqueKey(20);
            SecureUploader channel = new SecureUploader(this, token, this.ManageCompleteUpload, this.ManageFailedUpload);
            UInt16 port = channel.port;
            this.channels.TryAdd(token, channel);

            return new UploadData(UsefullMethods.GetLocalIPAddress(), port, token);
        }

        public byte[][] getFilesToUpload()
        {
            this.checkAuthentication();
            this.checkTransactionIsEnabled();

            necessaryFiles = new ThreadSafeList<FBFile>(FBVersion.getNecessaryFilesToUpgrade(this.inSyncVersion, this.realFiles.filesAlreadyRepresented()));
            this.uploadedFiles = new PhysicFilesList();

            byte[][] ret = new byte[necessaryFiles.Count][];
            for (int i = 0; i < necessaryFiles.Count; ++i)
            {
                ret[i] = necessaryFiles.ElementAt(i).serialize();
            }
            return ret;
        }

        private LinkedList<FBVersion> OldVersions()
        {
            DirectoryInfo[] versionDirs = user.rootDirectory.GetDirectories();
            LinkedList<FBVersion> versions = new LinkedList<FBVersion>();

            if (versionDirs == null) throw new Exception();

            if (versionDirs.Length == 0)
            {
                Directory.CreateDirectory(user.rootDirectory.FullName + @"\" + firstDirectory);
                versionDirs = user.rootDirectory.GetDirectories();
            }


            foreach (DirectoryInfo di in versionDirs)
            {
                Stream TestFileStream = null;
                try
                {
                    TestFileStream = File.OpenRead(di.FullName + @"\version.bin");
                }
                catch(FileNotFoundException )
                {
                    Directory.Delete(di.FullName);
                    continue;
                }
                BinaryFormatter deserializer = new BinaryFormatter();
                FBVersion version = (FBVersion)deserializer.Deserialize(TestFileStream);
                versions.AddLast(version);
                TestFileStream.Close();
            }

            return versions;
        }

        public SerializedVersion[] getOldVersions()
        {
            LinkedList<FBVersion> versions = this.OldVersions();
            SerializedVersion[] svers = new SerializedVersion[versions.Count - 1];
            int i = 0;
            foreach (FBVersion ver in versions)
            {
                if (ver.root.Name.Contains(firstDirectory)
                    && ver.fileList.Count == 0)
                    continue;

                svers[i++] = new SerializedVersion(ver.serialize());
            }

            return svers;
        }

        public UploadData resetToPreviousVersion(int versionAgo)
        {
            var directories = Directory.EnumerateDirectories(user.rootDirectory.FullName).OrderByDescending(filename => filename);
            
            Stream TestFileStream = File.OpenRead((new DirectoryInfo(directories.ElementAt(versionAgo))).FullName + @"\version.bin");
            BinaryFormatter deserializer = new BinaryFormatter();
            FBVersion old = (FBVersion)deserializer.Deserialize(TestFileStream);
            TestFileStream.Close();

            FBVersion actual = this.currentVersion();

            return resetInternals(old, actual);
        }

        private UploadData resetInternals(FBVersion newVersion, FBVersion current)
        {
            FBVersion diff = newVersion - current;

            newVersion.setAbsoluteNameToFile();
            if (diff.root != null) diff.setAbsoluteNameToFile();
            current.setAbsoluteNameToFile();

            List<Instruction> instrucionList = new List<Instruction>();

            try { File.Delete(user.rootDirectory.FullName + @"\tmp.zip"); }
            catch { }

            ZipArchive zip = ZipFile.Open(user.rootDirectory.FullName + @"\tmp.zip", ZipArchiveMode.Update);

            if (diff.fileList != null)
                foreach (FBAbstractElement to in diff.fileList)
                {
                    bool found = false;
                    foreach (FBAbstractElement from in current.fileList)
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
                                if (zip.GetEntry(ph.getRealFileInfo().Name) == null)
                                {
                                    zip.CreateEntryFromFile(ph.getRealFileInfo().FullName, ph.getRealFileInfo().Name, CompressionLevel.Optimal);
                                }
                                instrucionList.Add(new Instruction(InstructionType.NEW, ph.getRealFileInfo().Name, to.Name));
                            }
                        }
                    }
                }

            diff = current - newVersion;

            if (diff.fileList != null)
            {
                //diff.setAbsoluteNameToFile();
                foreach (FBAbstractElement toDelete in diff.fileList)
                {
                    instrucionList.Add(new Instruction(InstructionType.DELETE, toDelete.Name, ""));
                }
            }
            Stream FilesStream = File.OpenWrite(this.user.rootDirectory + @"\instructions.bin");
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(FilesStream, instrucionList);
            FilesStream.Close();

            zip.CreateEntryFromFile(this.user.rootDirectory + @"\instructions.bin", "instructions.bin", CompressionLevel.Optimal);
            File.Delete(this.user.rootDirectory + @"\instructions.bin");
            zip.Dispose();
            FilesStream = new FileStream(user.rootDirectory.FullName + @"\tmp.zip", FileMode.Open, FileAccess.Read);

            string token = Server.GetUniqueKey(20);
            SecureDownloader sr = new SecureDownloader(this, token,
                this.ManageCompleteUpload, this.ManageFailedUpload, FilesStream);
            UInt16 port = sr.port;

            return new UploadData(UsefullMethods.GetLocalIPAddress(), port, token);
        }

        private PhysicFile findPhysicFile(FBFile f)
        {
            foreach(PhysicFile ph in realFiles.list)
            {
                if (ph.getFBFile().Equals(f))
                    return ph;
            }
            return null;
        }

        public UploadData getFile(SerializedVersion serV)
        {
            FBVersion ver = FBVersion.deserialize(serV.encodedVersion);
            PhysicFile found = null;

            if (ver.fileList.Count != 1)
                return null;

            foreach(FBFile file in ver.fileList)
            {
                found = findPhysicFile(file);
                if (found == null)
                    return null;
            }

            FileStream fStream = new FileStream(found.getRealFileInfo().FullName,
                FileMode.Open, FileAccess.Read);
            String token = Server.GetUniqueKey(20);
            var secDown = new SecureDownloader(this, token, null, null, fStream);

            return new UploadData(UsefullMethods.GetLocalIPAddress(), secDown.port,
                token);
        }

        public UploadData resetToCraftedVersion(SerializedVersion serV)
        {
            FBVersion newVersion = FBVersion.deserialize(serV.encodedVersion);
            FBVersion current = this.currentVersion();
            return this.resetInternals(newVersion, current);
        }

        ~Server()
        {
            if (transactionEnabled)
                this.rollback();
        }
    }
}