using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FolderBackup.Server
{
    public class User
    {
        public String username { get; set; }
        public DirectoryInfo rootDirectory { get; set; }

        static public User authUser(string username, string password)
        {
            DatabaseManager db = DatabaseManager.getInstance();
            User ret = db.getUser(username, password);

            if (ret == null)
            {
                throw new Exception();
            }

            return ret;
        }
    }
}
