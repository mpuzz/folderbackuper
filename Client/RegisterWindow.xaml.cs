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
using FolderBackup.Shared;
using System.Threading;

namespace FolderBackup.Client
{
    /// <summary>
    /// Logica di interazione per RegisterWindow.xaml
    /// </summary>
    
    public partial class RegisterWindow : Window
    {
        public Window parent { get; set; }
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = this.usernameTxtBox.Text;
            string password = this.passwordTxtBox.Password;

            if (username.Equals(""))
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "Missing username! Username field cannot be empty.");
                return;
            }

            if (password.Equals(""))
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "Missing password! Password field cannot be empty.");
                return;
            }

            BackupServiceClient server = new BackupServiceClient();
            string salt;
            try
            {
                salt = server.registerStep1(username);
            }
            catch
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "No internet connection!Check it and retry");
                return;
            }
            if (salt == null)
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "Username already choosen! Try another!");
                return;
                
            }
            if (server.registerStep2(username, AuthenticationPrimitives.hashPassword(password, salt), salt))
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "Registration succeed. You can log in now.");
                Thread.Sleep(500);
                this.Hide();
                this.parent.Activate();
                this.parent.Show();
            }
            else
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "Registration procedure failed!");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.parent.Show();
            this.parent.Activate();
        }

        
    }
}
