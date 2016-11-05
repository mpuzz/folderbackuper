using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

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
            if (type == "success")
            {   
                el.Content = text;
                Color c = (Color)ColorConverter.ConvertFromString("#FFdff0d8");
                el.Background = new SolidColorBrush(c);
                c = (Color)ColorConverter.ConvertFromString("#FFd6e9c6");
                el.BorderBrush = new SolidColorBrush(c);
                c = (Color)ColorConverter.ConvertFromString("#FF3c763d");
                el.Foreground = new SolidColorBrush(c);
            }
            else if (type == "danger")
            {
                el.Content = text;
                Color c = (Color)ColorConverter.ConvertFromString("#FFF2DeDe");
                el.Background = new SolidColorBrush(c);
                c = (Color)ColorConverter.ConvertFromString("#FFebccc1");
                el.BorderBrush = new SolidColorBrush(c);
                c = (Color)ColorConverter.ConvertFromString("#FFa94442");
                el.Foreground = new SolidColorBrush(c);
            }
            else if (type == "none")
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
    }
}
