using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderBackup.Shared;
using System.IO;

namespace FolderBackup.Server
{
    [Serializable()]
    public class PhysicFilesList
    {
        public List<PhysicFile> list;

        public PhysicFilesList() {
            list = new List<PhysicFile>();
        }

        public void add(PhysicFile pf)
        {
            list.Add(pf);
        }

        public void add(PhysicFilesList pfl)
        {
            list.AddRange(pfl.list);
        }

        public void delete(PhysicFile pf)
        {
            list.Remove(pf);
        }

        public List<FBFile> filesAlreadyRepresented()
        {
            List<FBFile> l = new List<FBFile>();

            foreach (PhysicFile pf in this.list)
            {
                l.Add(pf.getFBFile());
            }
            return l;
        }
    }

    [Serializable()]
    public class PhysicFile
    {
        private FBFile abstractFile;
        private FileInfo hardFile;

        public PhysicFile(FBFile abs, string path) 
        {
            this.abstractFile = abs;
            this.hardFile = new FileInfo(path);
        }

        public FBFile getFBFile()
        {
            return this.abstractFile;
        }

        public FileInfo getRealFileInfo()
        {
            return this.hardFile;
        }

        public bool Equals(PhysicFile pf)
        {
            return this.abstractFile.Equals(pf.abstractFile);
        }
    }
}
