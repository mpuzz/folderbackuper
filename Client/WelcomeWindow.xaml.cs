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
using System.Windows.Forms;
using System.IO;
using FolderBackup.Shared;

namespace FolderBackup.Client
{
    /// <summary>
    /// Interaction logic for WelcomeWindows.xaml
    /// </summary>
    public partial class WelcomeWindows : Window
    {
        public Window parent { get; set; }
        private string PathBoxPlaceholder = "Insert path . . .";
        Config conf = Config.Instance();

        public WelcomeWindows()
        {
            InitializeComponent();
            this.pathTxtBox.Text = PathBoxPlaceholder;
        }
        
        public void gotFocusHandler(object sender, EventArgs e)
        {
            if (this.PathBoxPlaceholder == this.pathTxtBox.Text)
            {
                this.pathTxtBox.Text = "";
            }
        }

        public void lostFocusHandler(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(this.pathTxtBox.Text))
                this.pathTxtBox.Text = PathBoxPlaceholder;
        }
        private void pathTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Directory.Exists(this.pathTxtBox.Text)) {
                UsefullMethods.setLabelAlert("success",this.errorBox,"Ok!");
            }
            else if (this.PathBoxPlaceholder != this.pathTxtBox.Text && this.pathTxtBox.Text!="")
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "Selected path does not exists");

            }else
            {
                UsefullMethods.setLabelAlert("none", this.errorBox, "");
            }
        }

        private void folderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result.ToString() == "OK") {
                this.pathTxtBox.Text = fbd.SelectedPath;
            }

        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(this.pathTxtBox.Text))
            {
                conf.targetPath.set(this.pathTxtBox.Text);
                this.Hide();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to exit without setting the sync folder?", "Please enter the folder", MessageBoxButtons.YesNo);
            if (dialogResult == System.Windows.Forms.DialogResult.Yes)
            {
                this.Hide();
            }

        }

    }
}

