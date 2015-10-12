using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderBackup.Shared
{
    public abstract class FBAbstractBuilder
    {
        public string path {get; set;}

        public FBAbstractBuilder(string path)
        {
            this.path = path;
        }

        public FBAbstractBuilder() { }

        abstract public FBAbstractElement generate();
    }
}
