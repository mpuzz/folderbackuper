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

namespace FolderBackup.Client
{
    public class SyncEngine
    {
        Config conf = Config.Instance();
        FBVersionBuilder vb = null;
        FBVersion cv;
        BackupServiceClient server;

        public SyncEngine(BackupServiceClient server)
        {
            this.server = server;
            String dirPath = conf.targetPath.get();
            if (dirPath == null || !Directory.Exists(dirPath))
            {
                throw new DirectoryNotFoundException("Directory in configuration is not valid");
            }
            //Directory.SetCurrentDirectory(dirPath);
            vb = new FBVersionBuilder(dirPath);
            cv = (FBVersion)vb.generate();

        }

        public void sync()
        {
            SerializedVersion serV = new SerializedVersion();
            serV.encodedVersion = cv.serialize();
            if (server.newTransaction(serV))
            {
                byte[][] bfiles = server.getFilesToUpload();
                List<FBFile> fileToSync = new List<FBFile>();
                foreach (byte[] bf in bfiles)
                {
                    fileToSync.Add(FBFile.deserialize(bf));
                }
                try
                {
                    foreach (FBFile f in fileToSync)
                    {
                        FBFileClient cf = FBFileClient.generate(f);
                        SerializedFile sf = new SerializedFile();
                        sf.encodedFile = f.serialize();
                        UploadData cedential = server.uploadFile(sf);
                        SendFile(cedential, new FileStream(cf.FullName, FileMode.Open));

                    }
                }
                catch {
                    server.rollback();
                }
                server.commit();
            }
        }

        private void SendFile(UploadData credential, FileStream fstream)
        {
            System.Threading.Thread.Sleep(100);
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


    }
}
