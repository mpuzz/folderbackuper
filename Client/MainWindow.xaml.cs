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
using System.Threading;
using System.ServiceModel;

namespace FolderBackup.Client
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Config conf = Config.Instance();

        public MainWindow()
        {
            InitializeComponent();
            if (conf.userName.get() != null)
            {
                this.usernameTxtBox.Text = conf.userName.get();
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = this.usernameTxtBox.Text;
            string password = this.paswordTxtBox.Password;

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
            string token;
            BackupServiceClient server = logIn(username, password, out token);
            if (server == null) return;//mostra qlc di errore
            UsefullMethods.setLabelAlert("success", this.errorBox, "Log in succeed!");
            conf.userName.set(this.usernameTxtBox.Text);
            Thread.Sleep(500);
            this.Hide();
            string targetPath = conf.targetPath.get();
            // if the path is not setted a windows for selecting the path must be shown
            if (targetPath == null)
            {
                WelcomeWindows ww = new WelcomeWindows();
                ww.parent = this;
                ww.Show();
                ww.Activate();
            }
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

        private void registerLabel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RegisterWindow rw = new RegisterWindow();
            rw.parent = this;
            rw.Show();
            rw.Activate();
            this.Hide();
        }

        private BackupServiceClient logIn(string username, string password, out string token)
        {
            BackupServiceClient server = new BackupServiceClient();
            AuthenticationData ad;
            try
            {
                ad = server.authStep1(username);
            }
            catch(FaultException)
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "Username doesn't exist!");
                token = null;
                return null;
            }catch
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "No internet connection! Check it and retry");
                token = null;
                return null;
            }

            try
            {
                token = server.authStep2(ad.token, username, AuthenticationPrimitives.hashPassword(password, ad.salt, ad.token));
            }
            catch (FaultException)
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "Wrong password!");
                token = null;
                return null;
            }
            catch
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, "No internet connection! Check it and retry");
                token = null;
                return null;
            }
            return server;
        }
    }
}
