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
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            this.usernameTxtBox.Text="";
            this.paswordTxtBox.Password="";
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
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
            BackupServiceClient server = null;
            try
            {
                server = logIn(username, password, out token);
            }
            catch (LoginExcpetion ex)
            {
                UsefullMethods.setLabelAlert("danger", this.errorBox, ex.Message);

            }
            if (server != null)
            {

                Const<BackupServiceClient>.Instance().set(server);
                UsefullMethods.setLabelAlert("success", this.errorBox, "Log in succeed!");
                conf.userName.set(this.usernameTxtBox.Text);
                await Task.Delay(500);
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
                else
                {
                    TrayiconMode.Instance();
                }
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

        public static BackupServiceClient logIn(string username, string password, out string token)
        {
            BackupServiceClient server = new BackupServiceClient();
            AuthenticationData ad;
            token = null;

            try
            {
                ad = server.authStep1(username);
            }
            catch(FaultException)
            {
                throw new LoginExcpetion("Username doesn't exist!");
                
            }catch
            {
                throw new LoginExcpetion("No internet connection!Check it and retry");
            }

            try
            {
                token = server.authStep2(ad.token, username, AuthenticationPrimitives.hashPassword(password, ad.salt, ad.token));
            }
            catch (FaultException)
            {
                throw new LoginExcpetion("Wrong password!");
            }
            catch
            {
                throw new LoginExcpetion("No internet connection!Check it and retry");
            }
            return server;
        }
    }
}

public class LoginExcpetion : Exception
{
        public LoginExcpetion(string message)
        : base(message)
    {
    }
}