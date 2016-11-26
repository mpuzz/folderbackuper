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
            if (newElement == null) return;
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

        static public FBDirectory operator -(FBDirectory first, FBDirectory second)
        {
            FBDirectory ret = new FBDirectory(first.Name);
            ret.Name = first.Name;

            foreach (KeyValuePair<string, FBAbstractElement> entry in first.content)
            {
                if (!second.content.ContainsKey(entry.Key))
                {
                    ret.addContent(entry.Value.Clone());
                }
                else
                {
                    if (!entry.Value.isEqualTo(second.content[entry.Key]))
                    {
                        if (entry.Value.GetType().Equals(first.GetType()) && second.content[entry.Key].GetType().Equals(first.GetType()))
                        {
                            ret.addContent((FBDirectory)entry.Value - (FBDirectory)second.content[entry.Key]);
                            continue;
                        }
                        ret.addContent(entry.Value.Clone());
                        continue;
                    }
                    if (entry.Value.GetType() != first.GetType())
                    {
                        if (entry.Value.Name != second.content[entry.Key].Name)
                        {
                            ret.addContent(entry.Value.Clone());
                        }
                    }
                }
            }

            if (ret.content.Count == 0) return null;
            return ret;
        }

        public void setAbsoluteNameToFile()
        {
            foreach (FBAbstractElement abs in this.content.Values)
            {
                abs.Name = this.Name + abs.Name;
                if (abs.GetType() == this.GetType())
                {
                    abs.Name = abs.Name + @"\";
                    ((FBDirectory)abs).setAbsoluteNameToFile();
                }
            }
        }

        public override FBAbstractElement Clone()
        {
            FBDirectory newDir = new FBDirectory(this.Name);
            foreach (FBAbstractElement abs in this.content.Values)
            {
                newDir.addContent(abs.Clone());
            }
            return newDir;
        }
    }
}
