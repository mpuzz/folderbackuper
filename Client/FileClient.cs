using FolderBackup.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Security;

namespace FolderBackup.Client
{
    class FBFileClient : FBFile
    {
        public string FullName { get; }

        public static FBFileClient generate(FBFile file)
        {
            Config conf = Config.Instance();
            FileInfo f = whoami(file, new DirectoryInfo(conf.targetPath.get()));

            return new FBFileClient(f);
        }
        

        private FBFileClient(FileInfo finf) :base("")
        {
            FullName = finf.FullName;
            FBFile file = new FBFile(finf.Name);
            this.Name = finf.Name;
            this.dimension = finf.Length;

            FileStream fileStream = finf.Open(FileMode.Open);
            fileStream.Position = 0;

            SHA512 hasher = new SHA512Managed();
            file.hash = System.Text.Encoding.Default.GetString(hasher.ComputeHash(fileStream));
            fileStream.Close();
        }

        public static FileInfo whoami(FBFile file, DirectoryInfo dinfo)
        {
            foreach (FileInfo f in dinfo.GetFiles())
            {
                if (f.Name==file.Name)
                {
                    FBFileBuilder fb = new FBFileBuilder(f.FullName);
                    if (fb.generate().Equals(file))
                    {
                        return f;
                    }
                }
            }


            foreach (DirectoryInfo dir in dinfo.GetDirectories())
            {
                return whoami(file, dir);
            }

           
            return null;

        }
    }
}
