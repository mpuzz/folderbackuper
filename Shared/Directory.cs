using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderBackup.Shared
{

    [Serializable()]
    public class FBDirectory : FBAbstractElement
    {
        public Dictionary<String, FBAbstractElement> content;

        public List<FBFile> fileList
        {
            get
            {
                List<FBFile> fl = new List<FBFile>();
                foreach (KeyValuePair<string, FBAbstractElement> entry in this.content)
                {
                    if (entry.Value.GetType() != this.GetType())
                        fl.Add((FBFile)entry.Value);
                    else
                        fl.AddRange(((FBDirectory)entry.Value).fileList);
                }

                return fl;
            }
        }

        public FBDirectory(string name) : base(name)
        {
            this.content = new Dictionary<String, FBAbstractElement>();
        }

        public void addContent(FBAbstractElement newElement)
        {
            this.content.Add(newElement.Name, newElement);
        }

        override public Boolean isEqualTo(FBAbstractElement other)
        {
            if (other.GetType() != this.GetType())
            {
                return false;
            }

            FBDirectory othdir = (FBDirectory) other;
            if (othdir.content.Count != this.content.Count)
                return false;

            foreach (KeyValuePair<string, FBAbstractElement> entry in this.content)
            {
                if (!othdir.content.ContainsKey(entry.Key))
                    return false;
                else
                    if (!entry.Value.isEqualTo(othdir.content[entry.Key]))
                        return false;
            }

            return true;
        }
    }
}
