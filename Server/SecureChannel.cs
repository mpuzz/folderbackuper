using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using FolderBackup.CommunicationProtocol;
using FolderBackup.Shared;
using System.Collections;
using System.Globalization;

namespace FolderBackup.Server
{
    class SecureChannel
    {
        static private X509Certificate certificate;
        private Server server;
        private string token;
        private TcpListener listener;
        private Thread thread;
        private AutoResetEvent start;
        private NotifyReceiveComplete completeEvent;
        private NotifyErrorReceiving errorEvent;
        private Session session;

        public SecureChannel(Server server, string token,
            NotifyReceiveComplete completeEvent, NotifyErrorReceiving errorEvent)
        {
            if (certificate == null)
            {
                certificate = new X509Certificate("Server.cer");
            }
            this.token = token;
            this.server = server;
            this.session = this.server.session;
            this.listener = new TcpListener(IPAddress.Any, 0);
            this.thread = new Thread(this.ThreadCode);
            this.errorEvent = errorEvent;
            this.completeEvent = completeEvent;
        }

        public UInt16 port
        {
            get
            { 
                UInt16 ret = (UInt16)((IPEndPoint)listener.LocalEndpoint).Port;
                start.Set();
                return ret;
            }
        }

        private void ThreadCode()
        {
            start.WaitOne();

            this.listener.Start(1);
            var sockl = new ArrayList {listener};
            Socket.Select(sockl, null, null, 5000 * 1000);
            if(!sockl.Contains(listener))
            {
                this.errorEvent(token);
                return;
            }

            try
            {
                TcpClient client = listener.AcceptTcpClient();

                SslStream ssl = new SslStream(client.GetStream(), false);

                ssl.AuthenticateAsServer(certificate, false, System.Security.Authentication.SslProtocols.Tls12, false);

                SaveFile(ssl);
            }
            catch(SocketException e)
            {
                this.errorEvent(token);
                return;
            }
        }

        private void SaveFile(Stream fileStream)
        {
            string token = "";
            char[] chars = new char[20];

            if (!Directory.Exists("tmp")) Directory.CreateDirectory("tmp");
            SaveStreamToFile(fileStream, "tmp\\prova");
            fileStream.Close();

            FileStream fs = new FileStream("tmp\\prova", FileMode.Open, FileAccess.Read);

            StreamReader sr = new StreamReader(fs, Encoding.Default);
            sr.Read(chars, 0, 20);
            foreach (char c in chars)
            {
                token += c;
            }
            
            if (token != this.token)
            {
                this.errorEvent(this.token);
                return;
            }
            
        }

        private void uploadFile(Stream fileStream)
        {
            string path = this.session.user.rootDirectory.FullName + "\\" + DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss_fff", CultureInfo.InvariantCulture);
            FBFile newFile;
            FBFileBuilder fb;
            fileStream.Seek(20, SeekOrigin.Begin);
            SaveStreamToFile(fileStream, path);
            fb = new FBFileBuilder(path);
            newFile = (FBFile)fb.generate();

            if (!this.session.necessaryFiles.Contains(newFile))
            {
                File.Delete(path);
                throw new FaultException<ServiceErrorMessage>(new ServiceErrorMessage(ServiceErrorMessage.FILENOTNECESSARY));
            }

            this.session.uploadedFiles.add(new PhysicFile(newFile, path));

            this.session.necessaryFiles.Remove(newFile);
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
    }
}
