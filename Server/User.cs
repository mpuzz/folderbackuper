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

        static public User authUser(string username, string password, string token)
        {
            DatabaseManager db = DatabaseManager.getInstance();
            User ret = db.getUser(username, password, token);

            if (ret == null)
            {
                throw new Exception();
            }

            return ret;
        }

        static public string getSalt(string username)
        {
            DatabaseManager db = DatabaseManager.getInstance();
            string salt = db.getSalt(username);

            return salt;
        }

        static public bool register(string username, string password, string salt)
        {
            return DatabaseManager.getInstance().register(username, password, salt);
        }

        public void Delete()
        {
            DatabaseManager.getInstance().Delete(this.username);
        }
    }
}
