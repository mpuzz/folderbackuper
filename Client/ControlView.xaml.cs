using FolderBackup.CommunicationProtocol;
using FolderBackup.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        const string TIMESTAMP_FORMAT = "MMM_dd_yyyy_HH_mm_ss";

        private static ControlView instance;
        private FBVersion selectedVersion;
        private class TreeViewItemFat : TreeViewItem
        {
            public FBAbstractElement item;
            public FBVersion version;
            public string relativePath;

            public bool Select
            {
                set
                {
                    if (value)
                    {
                        Color c = (Color)ColorConverter.ConvertFromString("#FFCFE581");
                        this.Background = new SolidColorBrush(c);
                    }else
                    {
                        Color c = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
                        this.Background = new SolidColorBrush(c);
                    }
                }
            }
            public bool Equals(TreeViewItemFat other)
            {
                return (this.item.Name.Equals(other.item.Name) && this.relativePath.Equals(other.relativePath));
            }
            public TreeViewItemFat Duplicate()
            {
                TreeViewItemFat newNode = new TreeViewItemFat() { Header = this.Header };
                newNode.item = this.item;
                newNode.version = this.version;
                newNode.relativePath = this.relativePath;
                return newNode;
            }
        }

        public static ControlView Instance()
        {
            if (instance == null)
            {
                instance = new ControlView();
            }
            return instance;
        }
        List<System.Windows.Controls.Button> versionButtons = new List<System.Windows.Controls.Button>();
        public Window parent { get; set; }

        private ControlView()
        {
            this.server = Const<BackupServiceClient>.Instance().get();
            InitializeComponent();
            targetPath = conf.targetPath.get();
            se.threadCallback += ThreadMonitor;

            buildGraphic();
            versionView.MouseDoubleClick += VersionView_MouseDoubleClick;
            revertList.MouseDoubleClick += VersionView_MouseDoubleClick;
            versionView.SelectedItemChanged += VersionView_SelectedItemChanged;
        }

        private void VersionView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItemFat selectedItem = (TreeViewItemFat)this.versionView.SelectedItem;
            if (selectedItem != null)
            {
                if (selectedItem.item.GetType() == typeof(FBDirectory))
                {
                    this.preview.IsEnabled = false;
                }
                else
                {
                    this.preview.IsEnabled = true;
                }
            }

        }

        private void buildGraphic()
        {
            SerializedVersion[] sversions = null;
            try
            {
                sversions = server.getOldVersions();
            }
            catch
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "No internet connection!Check it and retry");
                return;
            }
            this.versions = new FBVersion[sversions.Length];
            int i = 0;
            se.watcher.EnableRaisingEvents = false;
            versionBox.Items.Clear();
            foreach (SerializedVersion v in sversions)
            {
                versions[i] = FBVersion.deserialize(v.encodedVersion);
                System.Windows.Controls.Button button = new System.Windows.Controls.Button();
                button.Name = versions[i].timestamp.ToString(TIMESTAMP_FORMAT);
                button.Content = versions[i].timestamp.ToString("MMM, dd yyyy HH:mm");
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
                this.revertVersion.IsEnabled = false;
            }
        }

        private void versionClick(object sender, RoutedEventArgs e)
        {
            String name = ((System.Windows.Controls.Button)sender).Name;
            //search version
            FBVersion v = null;
            foreach (System.Windows.Controls.Button x in versionBox.Items)
            {
                if (sender == x)
                {
                    Color c = (Color)ColorConverter.ConvertFromString("#FF9C1A04");
                    x.Background = new SolidColorBrush(c);
                    c = (Color)ColorConverter.ConvertFromString("#FFFFFFFF");
                    x.Foreground = new SolidColorBrush(c);
                }
                else
                {
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

            if (selectedVersion == versions[versions.Length-1])
            {
                this.revertVersion.IsEnabled = false;
            }else
            {
                this.revertVersion.IsEnabled = true;
            }

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

     

        private string getPathTreeItem(TreeViewItem item)
        {
            if (item.Parent.GetType() == typeof(TreeViewItem))
            {
                return getPathTreeItem((TreeViewItem)item.Parent) + "\\" + item.Header;
            }
            else
            {
                return (string)item.Header;
            }

        }

        private TreeViewItem CreateDirectoryNode(FBDirectory root, string path)
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
                    treeItem.Items.Add(CreateDirectoryNode(child, path + "\\" + child.Name));
                    treeItem.IsExpanded = true;
                }
                else
                {
                    TreeViewItemFat ti = new TreeViewItemFat() { Header = key };
                    ti.version = selectedVersion;
                    ti.item = root.content[key];
                    ti.relativePath = path;
                    if (findItem(revertList.Items, ti)!=null)
                    {
                        ti.Select = true;
                    }
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
            ControlView.instance = null;
            se.watcher.EnableRaisingEvents = true;
            this.Hide();
        }
        private void sync_Click(object sender, RoutedEventArgs e)
        {
            if (((string)((System.Windows.Controls.Button)sender).Content).Equals("Start Sync"))
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
            string filePath = null;
            try
            {
                filePath = se.getFile((FBFile)seletedItem.item);
                System.Diagnostics.Process.Start(filePath);
            }
            catch (System.ServiceModel.CommunicationException)
            {
                UsefullMethods.setLabelAlert("danger", errorBox, "Error with communication! Check your connection!");
            }
            catch
            {
                UsefullMethods.setLabelAlert("danger", errorBox, "No file selected!");

            }
        }

        private async void flashItem(TreeViewItemFat item)
        {
            Brush pc = item.Background;
            Color c = (Color)ColorConverter.ConvertFromString("#FFB13D14");
            item.Background = new SolidColorBrush(c);
            await Task.Delay(700);
            item.Background = pc;
        }
        private TreeViewItemFat findItem(ItemCollection view,TreeViewItemFat item)
        {
            TreeViewItemFat ret = null;
            
            foreach (TreeViewItemFat el in view)
            {
                if (el.item != null)
                {
                    if (el.item.GetType() == typeof(FBDirectory))
                    {
                        ret = findItem(el.Items, item);
                        if (ret != null)
                        {
                            return ret;
                        }
                    }
                    if (el.Equals(item))
                    {
                        return el;
                    }
                }
                
            }
            return null;
        }
        private TreeViewItemFat addItemInRevertList(TreeViewItemFat item)
        {
            TreeViewItemFat ret = null;
            if (item.item.GetType() == typeof(FBDirectory))
            {
                foreach (TreeViewItemFat i in item.Items)
                {
                    addItemInRevertList(i);
                }
            }
            else
            {
                TreeViewItemFat el = findItem(revertList.Items, item);
                if (el != null)
                {
                    UsefullMethods.setLabelAlert("danger", errorBox, "File is already in the revert list");
                    flashItem(el);
                }else
                {
                    ret = item.Duplicate();
                    revertList.Items.Add(ret);
                    item.Select = true;
                }
            }
            return ret;
        }
        private  void VersionView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItemFat selectedItem = (TreeViewItemFat)((TreeView)sender).SelectedItem;
            if (selectedItem != null && selectedItem.item != null)
            {
                if (sender == versionView)
                {
                    addItemInRevertList(selectedItem);
                }
                else
                {
                    revertList.Items.Remove(selectedItem);
                    TreeViewItemFat el = findItem(versionView.Items,selectedItem);
                    el.Select = false;
                }

            }
        }
        private void revert_Click(object sender, RoutedEventArgs e)
        {
            if (this.versions!=null) {
                FBVersion lastVers = (FBVersion)this.versions[versions.Length - 1].Clone();
                FBDirectory dir = lastVers.root;
                Dictionary<string, List<TreeViewItemFat>> revertItems = new Dictionary<string, List<TreeViewItemFat>>();
                if (revertList.Items.Count==0) {
                    MessageBox.Show("There are no file to revert in the list please add them!", "List empty");
                }
                foreach (TreeViewItemFat el in revertList.Items)
                {
                    if (!revertItems.ContainsKey(el.relativePath))
                    {
                        revertItems[el.relativePath] = new List<TreeViewItemFat>();
                    }
                    revertItems[el.relativePath].Add(el);
                }
                modifyDir(revertItems, dir, dir.Name);
                se.resetToVersion(lastVers);
                revertList.Items.Clear();
            }
            else
            {
                MessageBox.Show("There is a problem with connection, please retry to login!", "Error in connection");
            }   
        }
        private void RevertToVersion_Click(object sender, RoutedEventArgs e)
        {
            if (this.versions != null)
            {
                int vIndex = 0;
                for (; vIndex < versions.Length; vIndex++)
                {
                    if (versions[versions.Length - vIndex - 1] == selectedVersion) break;
                }
                se.resetPrevoiusVersion(vIndex, versions[versions.Length - vIndex - 1]);
            }
            else
            {
                MessageBox.Show("There is a problem with connection, please retry to login!", "Error in connection");
            }
        }
        private void modifyDir(Dictionary<string, List<TreeViewItemFat>> revertItems, FBDirectory dir, string relPath)
        {
            // recursion inside the existing directories
            foreach (FBAbstractElement el in dir.content.Values)
            {
                if (el.GetType() == typeof(FBDirectory))
                {
                    modifyDir(revertItems, (FBDirectory)el, relPath+"\\"+el.Name);
                }
            }
            //add new directory if necessary
            foreach (string key in revertItems.Keys)
            {
                if (key.Contains(relPath+"\\"))
                {
                    string newDirName = key.Substring((relPath + "\\").Length);
                    newDirName = newDirName.Split('\\')[0];
                    if (!dir.content.ContainsKey(newDirName)) {
                        dir.addContent(new FBDirectory(newDirName));
                        modifyDir(revertItems, (FBDirectory)dir.content[newDirName], relPath + "\\" + newDirName);
                    }
                }
            }
            //adding or replacing files in the directory
            if (revertItems.ContainsKey(relPath))
            {
                foreach (TreeViewItemFat tv in revertItems[relPath])
                {
                    if (dir.content.ContainsKey(tv.item.Name))
                    {
                        dir.content.Remove(tv.item.Name);
                    }
                    dir.addContent(tv.item);

                }
            }
            //revertItems.Remove(relPath);
        }
        
    }
}

