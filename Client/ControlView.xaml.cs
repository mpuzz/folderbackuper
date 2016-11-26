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

        const string TIMESTAMP_FORMAT = "MMM_dd_yyyy_HH_MM_ss";

        private static ControlView instance;
        private FBVersion selectedVersion;
        private class TreeViewItemFat : TreeViewItem
        {
            public FBAbstractElement item;
            public FBVersion version;
            public string relativePath;
        }

        public static ControlView Instance()
        {
            if (instance == null)
            {
                instance = new ControlView();
            }
            return instance;
        }
        List<System.Windows.Controls.Button> versionButtons = new List<System.Windows.Controls.Button> ();
        public Window parent { get; set; }
       
        private ControlView()
        {
            this.server = Const<BackupServiceClient>.Instance().get();   
            InitializeComponent();
            targetPath = conf.targetPath.get();
            se.threadCallback += ThreadMonitor;

            buildGraphic();
            versionView.MouseDoubleClick += VersionView_MouseDoubleClick;
            versionView.SelectedItemChanged += VersionView_SelectedItemChanged;
        }

        private void VersionView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItemFat selectedItem = (TreeViewItemFat)this.versionView.SelectedItem;
            if (selectedItem !=null) {
                if (selectedItem.item.GetType() == typeof(FBDirectory))
                {
                    this.preview.IsEnabled = false;
                } else
                {
                    this.preview.IsEnabled = true;
                }
            }

        }

        private void buildGraphic()
        {
            SerializedVersion[] sversions = server.getOldVersions();
            this.versions = new FBVersion[sversions.Length];
            int i = 0;
            se.watcher.EnableRaisingEvents = false;
            versionBox.Items.Clear();
            foreach (SerializedVersion v in sversions)
            {
                versions[i] = FBVersion.deserialize(v.encodedVersion);
                System.Windows.Controls.Button button = new System.Windows.Controls.Button();
                button.Name = versions[i].timestamp.ToString(TIMESTAMP_FORMAT);
                button.Content = versions[i].timestamp.ToString("MMM, dd yyyy HH:MM");
                button.Click += versionClick;
                button.MinWidth = 200;
                button.MinHeight = 22;
                if (i == sversions.Length - 1)
                {
                    Color c = (Color)ColorConverter.ConvertFromString("#FF9C1A04");
                    button.Background = new SolidColorBrush(c);
                    c = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
                    button.Foreground = new SolidColorBrush(c);
                }
                else
                {
                    Color c = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
                    button.Background = new SolidColorBrush(c);
                    c = (Color)ColorConverter.ConvertFromString("#FF000000");
                    button.Foreground = new SolidColorBrush(c);
                }
                versionBox.Items.Add(button);
                versionButtons.Add(button);
                i++;
            }

            versionView.Items.Clear();
            if (versions.Length > 0)
            {
                this.selectedVersion = versions[versions.Length - 1];
                versionView.Items.Add(CreateDirectoryNode(this.selectedVersion.root, this.selectedVersion.root.Name));
            }
        }

        private void versionClick(object sender, RoutedEventArgs e)
        {
            String name = ((System.Windows.Controls.Button)sender).Name;
            //search version
            FBVersion v=null;
            foreach (System.Windows.Controls.Button x in versionBox.Items)
            {
                if (sender == x) {
                    Color c = (Color)ColorConverter.ConvertFromString("#FF9C1A04");
                    x.Background = new SolidColorBrush(c);
                    c = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
                    x.Foreground = new SolidColorBrush(c);
                }
                else { 
                    Color c = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
                    x.Background = new SolidColorBrush(c);
                    c = (Color)ColorConverter.ConvertFromString("#FF000000");
                    x.Foreground = new SolidColorBrush(c);
                }
            }
            foreach (FBVersion x in this.versions)
            {
                if (x.timestamp.ToString(TIMESTAMP_FORMAT) == name)
                {
                    v = x;
                    break;
                }
            }
            versionView.Items.Clear();
            this.selectedVersion = v;
            versionView.Items.Add(CreateDirectoryNode(this.selectedVersion.root, this.selectedVersion.root.Name));



        }
        void ThreadMonitor(SyncEngine.TypeThread type, SyncEngine.StatusCode sc, String status)
        {
            if (type == SyncEngine.TypeThread.SYNC)
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (sc == SyncEngine.StatusCode.WORKING)
                    {
                        this.sync.Content = "Stop Sync";
                        this.sync.Name = "StopSync";
                        this.sync.IsEnabled = false;
                        this.errorBox.Content = status;
                    }
                    else if (sc == SyncEngine.StatusCode.SUCCESS)
                    {
                        this.errorBox.Content = "Synced";
                        buildGraphic();
                        this.sync.Content = "Start Sync";
                        this.sync.Name = "StartSync";
                        this.sync.IsEnabled = true;
                        this.errorBox.Content = status;
                    }
                    else
                    {
                        this.sync.Content = "Start Sync";
                        this.sync.Name = "StartSync";
                        this.sync.IsEnabled = true;
                        this.errorBox.Content = status;
                    }
                    
                });
            }
        }

        private void VersionView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItemFat selectedItem = (TreeViewItemFat)this.versionView.SelectedItem;
            if (selectedItem!=null && selectedItem.item != null) {
                Console.WriteLine(getPathTreeItem(selectedItem));


            }
        }

        private string getPathTreeItem(TreeViewItem item)
        {
            if (item.Parent.GetType()==typeof(TreeViewItem))
            {
                return getPathTreeItem((TreeViewItem)item.Parent)+"\\"+item.Header ;
            }
            else
            {
               return (string) item.Header;
            }

        }

        private TreeViewItem CreateDirectoryNode(FBDirectory root,string path)
        {            
            TreeViewItemFat treeItem = new TreeViewItemFat();
            treeItem.version = selectedVersion;
            treeItem.item = root;
            treeItem.Header = root.Name;
            treeItem.relativePath = path;

            foreach (String key in root.content.Keys)
            {
                if (root.content[key].GetType() == typeof(FBDirectory))
                {
                    FBDirectory child = (FBDirectory)root.content[key];
                    treeItem.Items.Add(CreateDirectoryNode(child,path+"\\"+child.Name));
                    treeItem.IsExpanded = true;
                }else
                {
                    TreeViewItemFat ti = new TreeViewItemFat() { Header = key };
                    ti.version = selectedVersion;
                    ti.item = root.content[key];
                    ti.relativePath = path;
                    treeItem.Items.Add(ti);

                }
            }
            if (root.content.Count == 0)
            {
                treeItem.Items.Add(new TreeViewItemFat() { Header = "" });
            }

            return treeItem;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            ControlView.instance = null;
            se.watcher.EnableRaisingEvents = true;
        }
        private void sync_Click(object sender, RoutedEventArgs e)
        {
            if(((string)((System.Windows.Controls.Button)sender).Content).Equals( "Start Sync"))
            {
                se.StartSync();
            }
            else
            {
                se.StopSync();
            }
        }

        private void preview_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItemFat seletedItem = (TreeViewItemFat)versionView.SelectedItem;
            string filePath=null;
            try
            {
                filePath = se.getFile((FBFile)seletedItem.item);
            }
            catch
            {
                UsefullMethods.setLabelAlert("danger",errorBox,"Error with communication! Check your connection!");
            }
            System.Diagnostics.Process.Start(filePath);
        }
    }
}

