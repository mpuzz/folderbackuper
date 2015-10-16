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

        public User getUser(string username, string password, string token)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = this.connection;

            cmd.CommandText = "SELECT sha1(concat(password, @token)) as hashed, rootDirectory FROM users WHERE username like @username";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@token", token);

            MySqlDataReader reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                reader.Close();
                return null;
            }

            User ret = null;
            if(password.Equals((string) reader["hashed"])) {
                ret = new User();
                ret.rootDirectory = new DirectoryInfo((string)reader["rootDirectory"]);
                ret.username = username;
            }

            reader.Close();
            return ret;
        }

        public string getSalt(string username)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = this.connection;

            cmd.CommandText = "SELECT salt FROM users WHERE username like @username";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@username", username);

            MySqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                reader.Close();
                return null;
            }
            string ret = (string)reader["salt"];
            reader.Close();
            return ret;
        }

        public bool register(string username, string password, string salt)
        {
            MySqlTransaction mtr =  this.connection.BeginTransaction();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM users FOR UPDATE", connection, mtr);
            cmd.Prepare();
            cmd.ExecuteReader().Close();

            cmd = new MySqlCommand("SELECT username FROM users WHERE username like @usern", connection, mtr);
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@usern", username);
            MySqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                reader.Close();
                cmd = new MySqlCommand("INSERT INTO users(username, password, rootDirectory, salt) VALUES(@us, @pass, @dir, @salt)", this.connection, mtr);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@us", username);
                cmd.Parameters.AddWithValue("@pass", password);
                cmd.Parameters.AddWithValue("@dir", @"c:\folderBackup\" + username);
                cmd.Parameters.AddWithValue("@salt", salt);
                if (cmd.ExecuteNonQuery() == 0)
                {
                    mtr.Rollback();
                    return false;
                }
                mtr.Commit();
                return true;
            }
            mtr.Rollback();
            return false;
        }
    }
}
