using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FolderBackup.Shared
{

    [Serializable()]
    public class FBVersion : FBAbstractElement
    {
        public DateTime timestamp { get; set; }
        public FBDirectory root;

        public List<FBFile> fileList
        {
            get
            {
                return root.fileList;
            }
        }

        public FBVersion()
        {
            this.timestamp = DateTime.UtcNow;
            root = new FBDirectory(this.timestamp.ToString());
        }

        public FBVersion(string name)
        {
            this.timestamp = DateTime.UtcNow;
            root = new FBDirectory(this.timestamp.ToString());
        }

        public void addElement(FBAbstractElement element)
        {
            this.root.addContent(element);
        }

        override public Boolean isEqualTo(FBAbstractElement other)
        {
            if (other.GetType() != this.GetType())
            {
                return false;
            }

            return this.root.isEqualTo(((FBVersion)other).root);
        }

        public byte[] serialize()
        {
            return FBVersion.serialize(this);
        }

        static public byte[] serialize(FBVersion version)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(ms, version);

            return ms.ToArray();
        }

        static public FBVersion deserialize(byte[] serializedVersion)
        {
            MemoryStream ms = new MemoryStream(serializedVersion);
            BinaryFormatter deserializer = new BinaryFormatter();
            return (FBVersion)deserializer.Deserialize(ms);
        }

        static public List<FBFile> getNecessaryFilesToUpgrade(FBVersion newV, List<FBFile> old)
        {
            List<FBFile> necessaryFiles = new List<FBFile>();
            List<FBFile> newVersionFiles = newV.fileList;

            foreach (FBFile newF in newVersionFiles)
            {
                if (necessaryFiles.Contains(newF)) continue;
                if (old.Contains(newF)) continue;
                necessaryFiles.Add(newF);
            }

            return necessaryFiles;
        }

        static public FBVersion operator -(FBVersion first, FBVersion second)
        {
            FBVersion ret = new FBVersion();
            ret.Name = "Diff";
            ret.timestamp = new DateTime();
            ret.root = first.root - second.root;
            return ret;
        }

        public void setAbsoluteNameToFile()
        {
            this.root.Name = "";
            this.root.setAbsoluteNameToFile();
        }

        public override FBAbstractElement Clone()
        {
            FBVersion cloned = new FBVersion(this.Name);
            cloned.timestamp = new DateTime(timestamp.Ticks);
            cloned.root = (FBDirectory) this.root.Clone();
            return cloned;
        }

    }
}
