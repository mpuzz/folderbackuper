using FolderBackup.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FolderBackup.Server
{
    abstract class SecureChannel
    {
        static private X509Certificate2 certificate;
        protected Server server;
        protected string token;
        protected TcpListener listener;
        private Thread thread;
        private AutoResetEvent start;
        protected AutoResetEvent clientConnected;
        protected NotifyReceiveComplete completeEvent;
        protected NotifyErrorReceiving errorEvent;
        private SslStream ssl;

        public UInt16 port
        {
            get
            {
                UInt16 ret = (UInt16)((IPEndPoint)listener.LocalEndpoint).Port;
                start.Set();
                return ret;
            }
        }

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
            this.listener = new TcpListener(IPAddress.Any,
                UsefullMethods.GetAvailablePort(30000));
            this.thread = new Thread(this.ThreadCode);
            this.errorEvent = errorEvent;
            this.completeEvent = completeEvent;
            this.start = new AutoResetEvent(false);
            this.clientConnected = new AutoResetEvent(false);
            this.thread.Start();
        }

        private void ThreadCode()
        {
            start.WaitOne();

            this.listener.Start(1);
            var sockl = new ArrayList { listener.Server };
            Socket.Select(sockl, null, null, 5000 * 1000);
            if (!sockl.Contains(listener.Server))
            {
                this.errorEvent(token);
                return;
            }

            try
            {
                TcpClient client = listener.AcceptTcpClient();

                ssl = new SslStream(client.GetStream(), false);

                ssl.AuthenticateAsServer(certificate, false, System.Security.Authentication.SslProtocols.Tls12, false);

                ServeRequest(ssl);
            }
            catch (Exception)
            {
                this.errorEvent(token);
                return;
            }
        }

        protected abstract void ServeRequest(Stream fileStream);
    }
}
