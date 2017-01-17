using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderBackup.Shared;
using FolderBackup.CommunicationProtocol;
using System.IO;
using System.Net.Sockets;
using System.Net.Security;
using FolderBackup.Client;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FolderBackup.Client
{
    public class SyncEngine
    {
        public enum TypeThread
        {
            SYNC
        };
        public enum StatusCode
        {
            WORKING,
            IDLE,
            SUCCESS,
            ABORTED
        }
        Config conf = Config.Instance();
        FBVersionBuilder vb = null;
        FBVersion cv;
        BackupServiceClient server;
        public FileSystemWatcher watcher;
        Dictionary<String,Thread> workingThread= new Dictionary<String,Thread>();
        bool stopSync = false;
        private static SyncEngine instance;

        //Del statusUpdateQueue = new List<Delegate>();
        public delegate void StatusUpdate(String newstatus);
        public StatusUpdate statusUpdate ;
        private String _status;
        public String status {
            get
            {
                return _status;
            }
            set
            {
                if (statusUpdate != null) {
                    statusUpdate.Invoke(value);
                }
                _status = value;
            }

        }
        public static SyncEngine Instance()
        {
            if (instance == null)
            {
                instance = new SyncEngine();
            }
            return instance;
        }

        private SyncEngine()
        {
            status = "Idle";
            this.server = Const<BackupServiceClient>.Instance().get();
            String dirPath = conf.targetPath.get();
            threadCallback = new ThreadStatus(this.ThreadMonitor);
            watcher = new FileSystemWatcher();
            watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnChanged);
            watcher.Path = dirPath;
            // Begin watching.
            watcher.EnableRaisingEvents = true;
            //Directory.SetCurrentDirectory(dirPath);
            vb = new FBVersionBuilder(dirPath);

        }

        public void WaitSync()
        {
            workingThread["sync"].Join();
        }

        public void StartSync()
        {
            ThreadStart ts = new ThreadStart(this.sync);
            Thread t = new Thread(ts);
            this.stopSync = false;
            t.Start();
            workingThread["sync"] = t;
        }

        public void StopSync()
        {
            this.stopSync = true;
            workingThread["sync"].Join();
            workingThread["sync"]=null;
            this.stopSync = false;
        }

        public delegate void ThreadStatus(TypeThread type, StatusCode ts, String status);
        public ThreadStatus threadCallback;
        private void sync()
        {
                cv = (FBVersion)vb.generate();
            
            String dirPath = conf.targetPath.get();
            if (dirPath == null || !Directory.Exists(dirPath))
            {
                throw new DirectoryNotFoundException("Directory in configuration is not valid");
            }
            SerializedVersion serV = new SerializedVersion();
            serV.encodedVersion = cv.serialize();
            threadCallback.Invoke(TypeThread.SYNC, StatusCode.WORKING, "Start syncing");
            try { 
                if (server.newTransaction(serV))
                {
                    this.status = "Syncing";
                    byte[][] bfiles = server.getFilesToUpload();
                    List<FBFile> fileToSync = new List<FBFile>();
                    foreach (byte[] bf in bfiles)
                    {
                        fileToSync.Add(FBFile.deserialize(bf));
                    }

                    int i = 0;
                    FileStream fs=null;
                    try
                    {
                        foreach (FBFile f in fileToSync)
                        {
                            if (!this.stopSync)
                            {
                                threadCallback.Invoke(TypeThread.SYNC, StatusCode.WORKING, "Syncing file "+i+" of " + fileToSync.Count);
                                i++;
                                FBFileClient cf = FBFileClient.generate(f);
                                UploadData cedential = server.uploadFile();
                                fs = new FileStream(cf.FullName, FileMode.Open);
                                UsefullMethods.SendFile(cedential.ip,cedential.port,cedential.token,fs);
                                fs = null;
                            }else {
                                break;
                            }
                        }
                        if (!this.stopSync)
                        {
                            server.commit();
                            threadCallback.Invoke(TypeThread.SYNC, StatusCode.SUCCESS, "Sync completed");

                        }
                        else
                        {
                            threadCallback.Invoke(TypeThread.SYNC, StatusCode.ABORTED, "Syncing Stopped");
                            server.rollback();

                        }
                    }
                    catch
                    {
                        if (fs!=null)
                        {
                            fs.Close();
                        }
                        server.rollback();
                    }
                    
                
                    this.status = "Idle";
                
                }else
                {
                    threadCallback.Invoke(TypeThread.SYNC, StatusCode.IDLE, "Nothing to be done");
                }
            }
            catch (System.ServiceModel.CommunicationException e)
            {
                MessageBox.Show("There is a problem with connection, please retry to login!", "Error in connection" );
                threadCallback.Invoke(TypeThread.SYNC, StatusCode.ABORTED, "Connection error");
            }
        }

        private void SendFile(UploadData credential, FileStream fstream)
        {
            TcpClient client = new TcpClient(credential.ip, credential.port);
            SslStream ssl = new SslStream(
                client.GetStream(), false,
                new RemoteCertificateValidationCallback(AuthenticationPrimitives.ValidateServerCertificate),
                null, EncryptionPolicy.RequireEncryption);
            
            ssl.AuthenticateAsClient(credential.ip, null, System.Security.Authentication.SslProtocols.Tls12, false);
            ssl.Write(UsefullMethods.GetBytesFromString(credential.token));
            fstream.CopyTo(ssl);
            ssl.Close();
            fstream.Close();
            
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!SyncEngine.instance.stopSync)
            {
                SyncEngine.instance.sync();
            }

        }



        void ThreadMonitor(TypeThread type,StatusCode sc, String status)
        {
            if (type == TypeThread.SYNC)
            {
                this.status = status;
            }
        }

        public string getFile(FBFile f)
        {
            SerializedVersion serV = new SerializedVersion();
            FBVersion tmpVer = new FBVersion();
            tmpVer.addElement(f);
            serV.encodedVersion = tmpVer.serialize();
            var uploadData = server.getFile(serV);
            string tmpPath = Path.GetTempPath()+f.Name;
            if (File.Exists(tmpPath))
            {
                File.Delete(tmpPath);
            }
            UsefullMethods.ReceiveFile(uploadData.ip, uploadData.port, uploadData.token, tmpPath);
            return tmpPath;

        }
        public void resetPrevoiusVersion(int vIndex, FBVersion v)
        {
            this.sync();
            UploadData ud = server.resetToPreviousVersion(vIndex);
            resetVersion(ud,v);
        }
        public void resetToVersion(FBVersion v)
        {
            this.sync();
            SerializedVersion serV = new SerializedVersion();
            serV.encodedVersion = v.serialize();
            UploadData ud = server.resetToCraftedVersion(serV);
            resetVersion(ud, v);
        }
        private void resetVersion(UploadData ud,FBVersion v)
        {
            string filename = Path.GetTempFileName();
            UsefullMethods.ReceiveFile(ud.ip, ud.port, ud.token, filename);

            String pathFiles;
            List<Instruction> instructionList = UsefullMethods.ExtractInstructions(filename, out pathFiles);

            foreach (Instruction i in instructionList)
            {
                ExecuteInstruction(i, pathFiles);
            }
            CleanUpDir(v.root,"");
            StartSync();

        }

        private void ExecuteInstruction(Instruction i,String path)
        {
            string dirPath = conf.targetPath.get();
            string dstPath;
            switch (i.cmd){
                case InstructionType.COPY:
                    dstPath = dirPath + "\\" + Path.GetDirectoryName(i.dst);
                    if (!Directory.Exists(dstPath))
                    {
                        Directory.CreateDirectory(dstPath);
                    }
                    File.Copy(dirPath + "\\" + i.src, dirPath + "\\" + i.dst);
                    break;
                case InstructionType.DELETE:
                    File.Delete(dirPath + "\\" +i.src);
                    break;
                case InstructionType.NEW:
                    dstPath = dirPath + "\\" + Path.GetDirectoryName(i.dst);
                    if (!Directory.Exists(dstPath))
                    {
                        Directory.CreateDirectory(dstPath);
                    }
                    File.Copy(path + "\\" + i.src, dirPath + "\\" + i.dst);

                    break;
                default:
                    throw new Exception("Wrong instruction type");
            }
        }

        private void CleanUpDir(FBDirectory dir,string relPath)
        {
            string targetPath = conf.targetPath.get();
            string[] dirs = Directory.GetDirectories(targetPath +"\\"+ relPath);
            List<string> realdirs = new List<string>();
            List<string> versdirs = new List<string>();

            foreach (string d in dirs)
            {
                realdirs.Add(Path.GetFileName(d));
            }
            foreach (FBAbstractElement el in dir.content.Values)
            {
                if (el.GetType() == typeof(FBDirectory))
                {
                    if (!realdirs.Contains( el.Name ))
                    {
                        Directory.CreateDirectory(targetPath+"\\"+relPath+"\\"+el.Name);
                    }

                    CleanUpDir((FBDirectory)el, relPath + "\\" + el.Name);
                    versdirs.Add(el.Name);
                }
            }
            foreach(string d in realdirs)
            {
                if (!versdirs.Contains(d))
                {
                    Directory.Delete(targetPath + "\\" + relPath + "\\" + d,true);
                }
            }
        }
        
    }
}
