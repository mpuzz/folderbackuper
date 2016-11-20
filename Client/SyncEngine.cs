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
            ABORTED
        }
        Config conf = Config.Instance();
        FBVersionBuilder vb = null;
        FBVersion cv;
        BackupServiceClient server;

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
            //Directory.SetCurrentDirectory(dirPath);
            vb = new FBVersionBuilder(dirPath);
            cv = (FBVersion)vb.generate();

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
        }

        public delegate void ThreadStatus(TypeThread type, StatusCode ts, String status);
        public ThreadStatus threadCallback;
        private void sync()
        {
            String dirPath = conf.targetPath.get();
            if (dirPath == null || !Directory.Exists(dirPath))
            {
                throw new DirectoryNotFoundException("Directory in configuration is not valid");
            }
            SerializedVersion serV = new SerializedVersion();
            serV.encodedVersion = cv.serialize();
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
                try
                {
                    foreach (FBFile f in fileToSync)
                    {
                        if (!this.stopSync)
                        {
                            threadCallback.Invoke(TypeThread.SYNC, StatusCode.WORKING, "Syncing file "+i+" of " + fileToSync.Count);
                            FBFileClient cf = FBFileClient.generate(f);
                            SerializedFile sf = new SerializedFile();
                            sf.encodedFile = f.serialize();
                            UploadData cedential = server.uploadFile(sf);
                            SendFile(cedential, new FileStream(cf.FullName, FileMode.Open));
                        }else
                        {
                            threadCallback.Invoke(TypeThread.SYNC, StatusCode.ABORTED, "Syncing Stopped");
                            server.rollback();
                            break;
                        }
                    }
                }
                catch
                {
                    server.rollback();
                }
                finally
                {
                    server.commit();
                }
                this.status = "Idle";
                threadCallback.Invoke(TypeThread.SYNC, StatusCode.IDLE, "Sync completed");

            }

        }

        private void SendFile(UploadData credential, FileStream fstream)
        {
            TcpClient client = new TcpClient("127.0.0.1", credential.port);
            SslStream ssl = new SslStream(
                client.GetStream(), false,
                new RemoteCertificateValidationCallback(AuthenticationPrimitives.ValidateServerCertificate),
                null, EncryptionPolicy.RequireEncryption);
            
            ssl.AuthenticateAsClient("127.0.0.1", null, System.Security.Authentication.SslProtocols.Tls12, false);
            ssl.Write(UsefullMethods.GetBytesFromString(credential.token));
            fstream.CopyTo(ssl);
            ssl.Close();
            fstream.Close();
            
        }

       void ThreadMonitor(TypeThread type,StatusCode sc, String status)
        {
            if (type == TypeThread.SYNC)
            {
                this.status = status;
            }
        }


    }
}
