using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace FolderBackup.Client
{

    public class TrayiconMode : System.Windows.Forms.Form
    {

        static private TrayiconMode instance;
        private System.Windows.Forms.NotifyIcon trayicon;
        private System.Windows.Forms.ContextMenu Content;
        private System.Windows.Forms.MenuItem exitItem;
        private System.Windows.Forms.MenuItem syncItem;
        private System.Windows.Forms.MenuItem cpItem;
        private System.Windows.Forms.MenuItem messageItem;

        private System.ComponentModel.IContainer components;

        private TrayiconMode()
        {
            this.components = new System.ComponentModel.Container();
            this.Content = new System.Windows.Forms.ContextMenu();
            this.exitItem = new System.Windows.Forms.MenuItem();
            this.cpItem = new System.Windows.Forms.MenuItem();
            this.syncItem = new System.Windows.Forms.MenuItem();
            this.messageItem = new System.Windows.Forms.MenuItem();

            // Initialize contextMenu1
            this.Content.MenuItems.AddRange( new System.Windows.Forms.MenuItem[] { this.exitItem });
            this.Content.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { this.cpItem });
            this.Content.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { this.syncItem });
            this.Content.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { this.messageItem });

            // Initialize menuItem1
            this.exitItem.Index = 3;
            this.exitItem.Text = "E&xit";
            this.exitItem.Click += new System.EventHandler(this.exit_Click);


            this.cpItem.Index = 2;
            this.cpItem.Text = "Control Panel";
            this.cpItem.Click += new System.EventHandler(this.controlPanel_Click);


            this.syncItem.Index = 1;
            this.syncItem.Text = "Start Sync";
            this.syncItem.Click += new System.EventHandler(this.startSync_Click);

            this.messageItem.Index = 0;
            this.messageItem.Text = "";

            // Set up how the form should be displayed.
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Text = "Notify Icon Example";

            // Create the NotifyIcon.
            this.trayicon = new System.Windows.Forms.NotifyIcon(this.components);

            // The Icon property sets the icon that will appear
            // in the systray for this application.
            trayicon.Icon = new Icon("Icons\\applicationIcon.ico");

            // The ContextMenu property sets the menu that will
            // appear when the systray icon is right clicked.
            trayicon.ContextMenu = this.Content;

            // The Text property sets the text that will be displayed,
            // in a tooltip, when the mouse hovers over the systray icon.
            trayicon.Text = "Folder Backup";
            trayicon.Visible = true;

            // Handle the DoubleClick event to activate the form.
            trayicon.DoubleClick += new System.EventHandler(this.trayicon_DoubleClick);

        }

        public static TrayiconMode Instance()
        {
            if (instance == null)
            {
                instance = new TrayiconMode();
            }
            return instance;
        }
        protected override void Dispose(bool disposing)
        {
            // Clean up any components being used.
            if (disposing)
                if (components != null)
                    components.Dispose();

            base.Dispose(disposing);
        }

        private void trayicon_DoubleClick(object Sender, EventArgs e)
        {
            // Show the form when the user double clicks on the notify icon.

            // Set the WindowState to normal if the form is minimized.
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            // Activate the form.
            this.Activate();
        }

        private void exit_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            this.Close();
            Environment.Exit(0);
        }

        private void controlPanel_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            ControlView av = new ControlView();
            av.Show();
            av.Activate();
        }
        private void startSync_Click(object Sender, EventArgs e)
        {
            if (((Button)Sender).Name == "")
            {
                SyncEngine se = new SyncEngine();
                se.StartSync();
            }
        }
    }
}