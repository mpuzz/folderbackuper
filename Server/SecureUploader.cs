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
    class SecureUploader : SecureChannel
    {
        
        public SecureUploader(Server server, string token,
            NotifyReceiveComplete completeEvent, NotifyErrorReceiving errorEvent) :
            base(server, token, completeEvent, errorEvent)
        {}

        protected override void ServeRequest(Stream fileStream)
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
            string path = this.server.user.rootDirectory.FullName + "\\"
                + DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss_fff",
                CultureInfo.InvariantCulture);
            FBFile newFile;
            FBFileBuilder fb;
            
            UsefullMethods.SaveStreamToFile(fileStream, path);
            fb = new FBFileBuilder(path);
            newFile = (FBFile)fb.generate();

            if (!this.server.necessaryFiles.Contains(newFile))
            {
                File.Delete(path);
                this.errorEvent(token);
                return;
            }

            this.completeEvent(newFile, new PhysicFile(newFile, path), token);
        }
    }
}
