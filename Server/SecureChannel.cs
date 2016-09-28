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
        static private X509Certificate2 certificate;
        private Server server;
        private string token;
        private TcpListener listener;
        private Thread thread;
        private AutoResetEvent start;
        private AutoResetEvent clientConnected;
        private NotifyReceiveComplete completeEvent;
        private NotifyErrorReceiving errorEvent;
        private Session session;

        public SecureChannel(Server server, string token,
            NotifyReceiveComplete completeEvent, NotifyErrorReceiving errorEvent)
        {
            if (certificate == null)
            {
                System.Console.WriteLine(Directory.GetCurrentDirectory());
                certificate = new X509Certificate2("Certificates\\certificate.pfx", "");
            }
            this.token = token;
            this.server = server;
            this.session = this.server.session;
            this.listener = new TcpListener(IPAddress.Any,
                UsefullMethods.GetAvailablePort(30000));
            this.thread = new Thread(this.ThreadCode);
            this.errorEvent = errorEvent;
            this.completeEvent = completeEvent;
            this.start = new AutoResetEvent(false);
            this.clientConnected = new AutoResetEvent(false);
            this.thread.Start();
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
            var sockl = new ArrayList {listener.Server};
            Socket.Select(sockl, null, null, 5000 * 1000);
            if(!sockl.Contains(listener.Server))
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
            catch(Exception e)
            {
                this.errorEvent(token);
                return;
            }
        }

        private void SaveFile(Stream fileStream)
        {
            string token = "";
            char[] chars = new char[20];

            StreamReader sr = new StreamReader(fileStream, Encoding.Default);
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
            this.uploadFile(fileStream);
        }

        private void uploadFile(Stream fileStream)
        {
            string path = this.session.user.rootDirectory.FullName + "\\" + DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss_fff", CultureInfo.InvariantCulture);
            FBFile newFile;
            FBFileBuilder fb;
            
            SaveStreamToFile(fileStream, path);
            fb = new FBFileBuilder(path);
            newFile = (FBFile)fb.generate();

            if (!this.session.necessaryFiles.Contains(newFile))
            {
                File.Delete(path);
                this.errorEvent(token);
                return;
            }

            this.completeEvent(newFile, new PhysicFile(newFile, path));

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
