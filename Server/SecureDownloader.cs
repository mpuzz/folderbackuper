using FolderBackup.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace FolderBackup.Server
{
    class SecureDownloader : SecureChannel
    {
        private Stream fileStream;
        public SecureDownloader(Server server, string token,
            NotifyReceiveComplete completeEvent, NotifyErrorReceiving errorEvent,
            Stream fileStream) :
            base(server, token, completeEvent, errorEvent)
        {
            this.fileStream = fileStream;
        }

        protected override void ServeRequest(Stream ssl)
        {
            string token = "";
            char[] chars = new char[20];

            StreamReader sr = new StreamReader(ssl, Encoding.Default);
            sr.Read(chars, 0, 20);
            foreach (char c in chars)
            {
                token += c;
            }

            if (token != this.token)
            {
                if(this.errorEvent != null) this.errorEvent(this.token);

                return;
            }

            UsefullMethods.CopyStream(this.fileStream, ssl);
            ssl.Close();
            fileStream.Close();
        }
    }
}
