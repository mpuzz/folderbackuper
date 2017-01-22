using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows;

namespace FolderBackup.Shared
{
    public class FBFileBuilder : FBAbstractBuilder
    {
        public FBFileBuilder(string path) : base(path)
        {}

        override public FBAbstractElement generate()
        {
            FileInfo finf = new FileInfo(this.path);
            FBFile file = new FBFile(finf.Name);
            file.dimension = finf.Length;
            try
            {
                FileStream fileStream = finf.Open(FileMode.Open);

                fileStream.Position = 0;
                SHA512 hasher = new SHA512Managed();
                file.hash = System.Text.Encoding.Default.GetString(hasher.ComputeHash(fileStream));
                fileStream.Close();
            }
            catch (Exception e)
            {
                if (Environment.UserInteractive)
                {
                    MessageBox.Show(e.Message + "\nThe application will be closed", "Error opening file");
                    Environment.Exit(0);
                }
                else
                {
                    Console.Write(e.Message);
                }

            }
            return file;
        }
    }
}
