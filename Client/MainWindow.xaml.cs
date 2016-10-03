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
            string username = this.usernameTxtBox.Text;
            string password = this.paswordTxtBox.Password;

            if (username.Equals(""))
            {
                MessageBox.Show(this, "Username cannot be empty!", "Missing username", MessageBoxButton.OK);
                return;
            }

            if (password.Equals(""))
            {
                MessageBox.Show(this, "Password cannot be empty!", "Missing password", MessageBoxButton.OK);
                return;
            }
            string token;
            BackupServiceClient server = logIn(username, password, out token);
            if (server == null) return;//mostra qlc di errore
            MessageBox.Show(this, "Log in succeed!");
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
            catch
            { // catch only foultexception guarda meglio il nome
                MessageBox.Show(this, "Username doesn't exist.", "Wrong username", MessageBoxButton.OK);
                token = null;
                return null;
            }

            try
            {
                token = server.authStep2(ad.token, username, AuthenticationPrimitives.hashPassword(password, ad.salt, ad.token));
            }
            catch
            {
                MessageBox.Show(this, "Wrong Password", "", MessageBoxButton.OK);
                token = null;
                return null;
            }
            return server;
        }
    }
}
