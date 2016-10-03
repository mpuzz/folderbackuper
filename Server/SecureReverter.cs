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
    class SecureReverter : SecureChannel
    {
        private Stream fileStream;
        public SecureReverter(Server server, string token,
            NotifyReceiveComplete completeEvent, NotifyErrorReceiving errorEvent,
            Stream fileStream) :
            base(server, token, completeEvent, errorEvent)
        {
            this.fileStream = fileStream;
        }

        protected override void ServeRequest(Stream ssl)
        {
            UsefullMethods.CopyStream(this.fileStream, ssl);
        }
    }
}
