using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.IO;

namespace FolderBackup.Server
{
    public class DatabaseManager
    {
        private MySqlConnection connection;
        static private DatabaseManager instance = null;

        static public DatabaseManager getInstance()
        {
            if (instance == null)
            {
                instance = new DatabaseManager();
            }

            return instance;
        }

        private DatabaseManager()
        {
            string connString = "server=127.0.0.1;uid=folderbackuper;pwd=folder;database=folderbackup;";
            try
            {
                connection = new MySqlConnection();
                connection.ConnectionString = connString;
                connection.Open();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Database connection failed: " + e.Message);
                throw e;
            }
        }

        public User getUser(string username, string password)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = this.connection;

          /*  SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] bytes = new byte[password.Length * sizeof(char)];
            System.Buffer.BlockCopy(password.ToCharArray(), 0, bytes, 0, bytes.Length);
            sha1.ComputeHash(bytes);
            password = System.Text.Encoding.Default.GetString(sha1.Hash);
           */cmd.CommandText = "SELECT * FROM users WHERE username like @username AND password like @password";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);

            MySqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                reader.Close();
                return null;
            }
            
            User ret = new User();
            ret.rootDirectory = new DirectoryInfo((string)reader["rootDirectory"]);
            ret.username = username;
            reader.Close();

            return ret;
        }
    }
}
