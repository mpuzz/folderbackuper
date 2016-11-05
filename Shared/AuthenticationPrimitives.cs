using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace FolderBackup.Shared
{
    public static class AuthenticationPrimitives
    {
        static public string hashPassword(string password, string salt, string token)
        {
            return Hash(Hash(Hash(password) + salt) + token);
        }

        static public string hashPassword(string password, string salt)
        {
            return Hash(Hash(password) + salt);
        }

        static private string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            X509Certificate cert = new X509Certificate("Certificates\\certificate.pfx","malnati");
            if (cert.Equals(cert))
                return true;
            return false;
        }

    }
}
