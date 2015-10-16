using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FolderBackup.CommunicationProtocol;
using FolderBackup.Shared;
using System.IO;

namespace FolderBackup.Client
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BackupServiceClient server = new BackupServiceClient();
            DirectoryInfo rinfo;
            string token = server.auth("test1", "test1");

            string[] lines = { "First line", "Second line", "Third line" };
            string[] lines1 = { "First line", "Second line", "Third lines" };
            System.IO.Directory.CreateDirectory("asd");
            System.IO.Directory.CreateDirectory(@"asd\ciao");
            rinfo = new DirectoryInfo("asd");
            System.IO.File.WriteAllLines(@"asd\uno.txt", lines);
            System.IO.File.WriteAllLines(@"asd\due.txt", lines1);
            System.IO.File.WriteAllLines(@"asd\ciao\due.txt", lines);

            FBVersionBuilder vb = new FBVersionBuilder(rinfo.FullName);
            FolderBackup.Shared.FBVersion v = (FolderBackup.Shared.FBVersion)vb.generate();

            SerializedVersion serV = new SerializedVersion();
            serV.encodedVersion = v.serialize();


            server.newTransaction(token, serV);

            byte[][] bfiles = server.getFilesToUpload(token);
            foreach (byte[] bf in bfiles)
            {
                FBFile f = FBFile.deserialize(bf);
            }

            FBFile file = (FBFile)new FBFileBuilder(@"asd\uno.txt").generate();
            lines[0] = token + lines[0];
            System.IO.File.WriteAllLines(@"asd\uno.txt", lines);
            FileStream fstream = new FileStream(@"asd\uno.txt", FileMode.Open, FileAccess.Read);
            server.uploadFile(fstream);
            fstream.Close();

            file = (FBFile)new FBFileBuilder(@"asd\due.txt").generate();
            lines1[0] = token + "First line";
            System.IO.File.WriteAllLines(@"asd\due.txt", lines1);
            fstream = new FileStream(@"asd\due.txt", FileMode.Open, FileAccess.Read);
            server.uploadFile(fstream);
            fstream.Close();

            server.commit(token);

            System.IO.Directory.Delete("asd", true);

        }

        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            Color c = (Color) ColorConverter.ConvertFromString("#FF024FFF");
            this.registerLabel.Foreground = new SolidColorBrush(c);
        }

        private void registerLabel_MouseLeave(object sender, MouseEventArgs e)
        {
            Color c = (Color)ColorConverter.ConvertFromString("#FF000000");
            this.registerLabel.Foreground = new SolidColorBrush(c);
        }
    }
}
