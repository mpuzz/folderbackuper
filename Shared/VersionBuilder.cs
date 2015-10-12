using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FolderBackup.Shared
{
    public class FBVersionBuilder : FBAbstractBuilder
    {
        
        public FBVersionBuilder(string path) : base(path)
        { }

        override public FBAbstractElement generate()
        {
            FBVersion vers = new FBVersion();
            FBDirectoryBuilder db = new FBDirectoryBuilder(this.path);
            vers.root = (FBDirectory) db.generate();

            return vers;
        }
    }
}
