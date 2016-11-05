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
using System.Windows.Shapes;

namespace FolderBackup.Client
{
    /// <summary>
    /// Interaction logic for ApplicationView.xaml
    /// </summary>
    public partial class ApplicationView : Window
    {
        private BackupServiceClient server;
        Config conf = Config.Instance();
        private String targetPath;

        public Window parent { get; set; }
        public ApplicationView(BackupServiceClient server)
        {
            this.server = server;   
            InitializeComponent();
            targetPath = conf.targetPath.get();
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

    }
}
