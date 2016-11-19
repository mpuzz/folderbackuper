using FolderBackup.CommunicationProtocol;
using FolderBackup.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FolderBackup.Client
{
    /// <summary>
    /// Interaction logic for ApplicationView.xaml
    /// </summary>
    public partial class ControlView : Window
    {
        private BackupServiceClient server;
        Config conf = Config.Instance();
        private String targetPath;
        FBVersion[] versions;
        SyncEngine se = SyncEngine.Instance();

        public Window parent { get; set; }
        public ControlView()
        {
            this.server = Const<BackupServiceClient>.Instance().get();   
            InitializeComponent();

            se.statusUpdate += new SyncEngine.StatusUpdate(UpdateStatus);
            targetPath = conf.targetPath.get();
            SerializedVersion[] sversions = server.getOldVersions();
            this.versions = new FBVersion[sversions.Length];
            int i = 0;
            foreach (SerializedVersion v in sversions ) {
                versions[i] = FBVersion.deserialize(v.encodedVersion);
                System.Windows.Controls.Button button = new System.Windows.Controls.Button();
                button.Name = versions[i].timestamp.ToString("MMM_dd_yyyy_HH_MM_ss");
                button.Content = versions[i].timestamp.ToString("MMM, dd yyyy HH:MM");
                button.Click+=versionClick;
                button.MinWidth = 200;
                button.MinHeight = 22;
                Color c = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
                button.Background = new SolidColorBrush(c);
                versionBox.Items.Add(button);
                i++;
            }
            if (versions.Length>0) {
                versionView.Items.Add(CreateDirectoryNode(versions[versions.Length - 1].root));
            }
            // if the path is not setted a windows for selecting the path must be shown
            if (targetPath == null)
            {
                WelcomeWindows ww = new WelcomeWindows();
                ww.parent = this;
                ww.Show();
                ww.Activate();
                this.Hide(); 
            }
        }

        private void versionClick(object sender, RoutedEventArgs e)
        {
            String name = ((System.Windows.Controls.Button)sender).Name;
            //search version
            FBVersion v=null;
            foreach(FBVersion x in this.versions)
            {
                if (x.timestamp.ToString("MMM_dd_yyyy__HH_MM_ss") == name)
                {
                    v = x;
                    break;
                }
            }
            versionView.Items.Clear();
            versionView.Items.Add(CreateDirectoryNode(v.root));



        }

        private void UpdateStatus(string status)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.errorBox.Content = status;
            });
        }

        private static TreeViewItem CreateDirectoryNode(FBDirectory root)
        {            
            TreeViewItem treeItem = new TreeViewItem();
            treeItem.Header = root.Name;

            foreach (String key in root.content.Keys)
            {
                if (root.content[key].GetType() == typeof(FBDirectory))
                {
                    treeItem.Items.Add(CreateDirectoryNode((FBDirectory)root.content[key]));
                }else
                {
                    treeItem.Items.Add(new TreeViewItem() { Header = key });
                }
            }

            return treeItem;
        }

    }
}
