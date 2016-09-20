using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FolderBackup.Shared
{
    [Serializable()]
    public class FBFile : FBAbstractElement, IEquatable<FBFile>
    {
        public long dimension { get; set; }
        public string hash { get; set; }

        public FBFile(string name) : base(name)
        {}
        override public Boolean isEqualTo(FBAbstractElement other)
        {
            return Equals(other);
        }

        public bool Equals(FBFile other)
        {
            FBFile otherF = (FBFile)other;
            if (this.dimension == otherF.dimension && this.hash == otherF.hash)
            {
                return true;
            }

            return false;
            
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((FBFile) obj);
        }

        public byte[] serialize()
        {
            return FBFile.serialize(this);
        }

        static public byte[] serialize(FBFile file)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(ms, file);

            return ms.ToArray();
        }

        static public FBFile deserialize(byte[] serializedVersion)
        {
            MemoryStream ms = new MemoryStream(serializedVersion);
            BinaryFormatter deserializer = new BinaryFormatter();
            return (FBFile)deserializer.Deserialize(ms);
        }

        public override FBAbstractElement Clone()
        {
            FBFile cloned = new FBFile(this.Name);
            cloned.hash = String.Copy(hash);
            cloned.dimension = dimension;

            return cloned;
        }
    }
}
