using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderBackup.Shared
{

    [Serializable()]
    public abstract class FBAbstractElement
    {
        
        public string  Name { get; set; }

        public FBAbstractElement() { }
        public FBAbstractElement(string name)
        {
            this.Name = String.Copy(name);
        }
        abstract public Boolean isEqualTo(FBAbstractElement other);

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType()) return false;
            return this.isEqualTo((FBAbstractElement) obj);
        }

        abstract public FBAbstractElement Clone();
    }
}
