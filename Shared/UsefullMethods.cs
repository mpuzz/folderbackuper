using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Net.Security;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace FolderBackup.Shared
{
    public static class UsefullMethods
    {
        public static byte[] GetBytesFromString(string str)
        {
            return System.Text.Encoding.UTF8.GetBytes(str);
        }

        public static string GetStringFromBytes(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static int GetAvailablePort(int startingPort)
        {
            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            //getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            //getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            //getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (int i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }

        public static void SaveStreamToFile(System.IO.Stream stream, string filePath)
        {
            FileStream outstream = File.Open(filePath, FileMode.Create, FileAccess.Write);
            CopyStream(stream, outstream);
            outstream.Close();
            stream.Close();
        }

        public static void CopyStream(System.IO.Stream instream, System.IO.Stream outstream)
        {
            const int bufferLen = 4096;
            byte[] buffer = new byte[bufferLen];
            int count = 0;
            int bytecount = 0;
            while ((count = instream.Read(buffer, 0, bufferLen)) > 0)
            {
                outstream.Write(buffer, 0, count);
                bytecount += count;
            }
        }

        public static void setLabelAlert(string type, Label el, string text)
        {
            if (type.Equals("success"))
            {   
                el.Content = text;
                Color c = (Color)ColorConverter.ConvertFromString("#FFdff0d8");
                el.Background = new SolidColorBrush(c);
                c = (Color)ColorConverter.ConvertFromString("#FFd6e9c6");
                el.BorderBrush = new SolidColorBrush(c);
                c = (Color)ColorConverter.ConvertFromString("#FF3c763d");
                el.Foreground = new SolidColorBrush(c);
            }
            else if (type.Equals("danger"))
            {
                el.Content = text;
                Color c = (Color)ColorConverter.ConvertFromString("#FFF2DeDe");
                el.Background = new SolidColorBrush(c);
                c = (Color)ColorConverter.ConvertFromString("#FFebccc1");
                el.BorderBrush = new SolidColorBrush(c);
                c = (Color)ColorConverter.ConvertFromString("#FFa94442");
                el.Foreground = new SolidColorBrush(c);
            }
            else if (type.Equals("none"))
            {
                el.Content = text;
                Color c = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
                el.Background = new SolidColorBrush(c);
                c = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
                el.BorderBrush = new SolidColorBrush(c);
                c = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
                el.Foreground = new SolidColorBrush(c);
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }


        public static void SendFile(String ip, UInt16 port, String token, Stream fstream)
        {
            System.Threading.Thread.Sleep(100);
            TcpClient client = new TcpClient(ip, port);
            SslStream ssl = new SslStream(
                client.GetStream(), false,
                new RemoteCertificateValidationCallback(AuthenticationPrimitives.ValidateServerCertificate),
                null, EncryptionPolicy.RequireEncryption);
            try
            {
                ssl.AuthenticateAsClient(ip, null, System.Security.Authentication.SslProtocols.Tls12, false);
                ssl.Write(UsefullMethods.GetBytesFromString(token));
                fstream.CopyTo(ssl);
                ssl.Close();
                fstream.Close();
            }
            catch { }
        }

        public static void ReceiveFile(String ip, UInt16 port, String token, String path)
        {
            System.Threading.Thread.Sleep(100);
            TcpClient client = new TcpClient(ip, port);
            SslStream ssl = new SslStream(
                client.GetStream(), false,
                new RemoteCertificateValidationCallback(AuthenticationPrimitives.ValidateServerCertificate),
                null, EncryptionPolicy.RequireEncryption);
            FileStream fstream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            try
            {
                ssl.AuthenticateAsClient(ip, null, System.Security.Authentication.SslProtocols.Tls12, false);
                ssl.Write(UsefullMethods.GetBytesFromString(token));
                ssl.Flush();
                ssl.CopyTo(fstream);
                ssl.Close();
                fstream.Close();
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public static List<Instruction> ExtractInstructions(String path, out String extractedDirectory)
        {
            ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Update);
            String tmpDir = Path.GetTempFileName();

            File.Delete(tmpDir);
            Directory.CreateDirectory(tmpDir);
            zip.ExtractToDirectory(tmpDir);
            zip.Dispose();
            File.Delete(path);

            FileStream fstream = new FileStream(tmpDir + @"\instructions.bin", FileMode.Open, FileAccess.Read);
            BinaryFormatter deserializer = new BinaryFormatter();
            List<Instruction> instrucionList = (List<Instruction>)deserializer.Deserialize(fstream);
            fstream.Close();

            extractedDirectory = tmpDir;
            return instrucionList;
        }
    }
}
