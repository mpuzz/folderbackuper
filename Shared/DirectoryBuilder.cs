using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderBackup.Shared
{
    public class FBDirectoryBuilder : FBAbstractBuilder
    {
        public FBDirectoryBuilder(string path) : base(path)
        {}

        override public FBAbstractElement generate()
        {
            DirectoryInfo dinfo = new DirectoryInfo(this.path);
            FBDirectory newDir = new FBDirectory(dinfo.Name);

            foreach (DirectoryInfo dir in dinfo.GetDirectories())
            {
                if ((dir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
                    FBDirectoryBuilder db = new FBDirectoryBuilder(dir.FullName);
                    newDir.addContent(db.generate());
                }
            }

            foreach (FileInfo fil in dinfo.GetFiles())
            {
                FBFileBuilder fb = new FBFileBuilder(fil.FullName);
                newDir.addContent(fb.generate());
            }

            return newDir;
        }
    }
}
