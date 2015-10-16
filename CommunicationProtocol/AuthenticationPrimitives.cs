using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace FolderBackup.CommunicationProtocol
{
    public static class AuthenticationPrimitives
    {
        static public string hashPassword(string password, AuthenticationData ad)
        {
            return Hash(Hash(Hash(password) + ad.salt) + ad.token);
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
    }
}
