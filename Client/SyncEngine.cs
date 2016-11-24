﻿using System;
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
                            i++;
                            FBFileClient cf = FBFileClient.generate(f);
                            UploadData cedential = server.uploadFile();
                            UsefullMethods.SendFile(cedential.ip,cedential.port,cedential.token, new FileStream(cf.FullName, FileMode.Open));
                        }else {
                            break;
                        }
                    }
                    if (!this.stopSync)
                    {
                        server.commit();
                        threadCallback.Invoke(TypeThread.SYNC, StatusCode.IDLE, "Sync completed");

                    }
                    else
                    {
                        threadCallback.Invoke(TypeThread.SYNC, StatusCode.ABORTED, "Syncing Stopped");
                        server.rollback();

                    }
                }
                catch
                {
                    server.rollback();
                }
                    
                
                this.status = "Idle";
                
            }else
            {
                threadCallback.Invoke(TypeThread.SYNC, StatusCode.IDLE, "Nothing to be done");
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


    }
}
