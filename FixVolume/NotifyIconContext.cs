using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SilentOrbit.FixVolume
{
    internal class NotifyIconContext : ApplicationContext
    {
        static NotifyIcon trayIcon;

        static readonly Icon StartupInactive = Icon.FromHandle(Resource.mic_gray.GetHicon());
        static readonly Icon Muted = Icon.FromHandle(Resource.mic_off.GetHicon());
        static readonly Icon FullVolume = Icon.FromHandle(Resource.mic_on.GetHicon());

        readonly MenuItem autoStart;
        readonly MenuItem mapCapsLock;
        readonly MenuItem mapCapsLockSet;
        readonly MenuItem mapCapsLockReset;
        readonly MenuItem mute;

        public NotifyIconContext()
        {
            autoStart = new MenuItem("&Auto Start", ToggleAutoStart);
            mapCapsLock = new MenuItem("Map &CapsLock");
            mapCapsLockSet = new MenuItem("To F13", MapCapsLock);
            mapCapsLockReset = new MenuItem("Reset CapsLock", ResetCapsLock);
            mapCapsLock.MenuItems.Add(mapCapsLockSet);
            mapCapsLock.MenuItems.Add(mapCapsLockReset);
            mute = new MenuItem("&Mute", (s, e) => VolumeWatcher.ToggleMute());
            var about = new MenuItem("About", (s, e) => Process.Start("https://github.com/hultqvist/FixVolume"));
            var exit = new MenuItem("E&xit", (s, e) => Application.Exit());

            trayIcon = new NotifyIcon()
            {
                Icon = StartupInactive,
                ContextMenu = new ContextMenu(new MenuItem[] { autoStart, mapCapsLock, mute, about, exit }),
                Visible = true
            };

            trayIcon.ContextMenu.Popup += (object sender, EventArgs e) =>
            {
                autoStart.Checked = RegAutoStart.Get();
                mute.Checked = VolumeWatcher.Volume == 0;
            };

            trayIcon.Click += TrayIcon_Click;
        }

        void TrayIcon_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs m)
            {
                if (m.Button == MouseButtons.Left)
                    VolumeWatcher.ToggleMute();
            }
        }

        protected override void Dispose(bool disposing)
        {
            //Remove tray icon at exit
            trayIcon.Visible = false;

            base.Dispose(disposing);
        }

        void ToggleAutoStart(object sender, EventArgs e)
        {
            var val = RegAutoStart.Get();

            if (val)
                RegAutoStart.Remove();
            else
                RegAutoStart.Set();
        }

        void MapCapsLock(object sender, EventArgs e)
        {
            RegMapCapsLock.Set();
            trayIcon.ShowBalloonTip(10000, "Restart Windows", "Log out and back in to activate the map.", ToolTipIcon.Info);
        }

        void ResetCapsLock(object sender, EventArgs e)
        {
            RegMapCapsLock.Reset();
            trayIcon.ShowBalloonTip(10000, "Restart Windows", "Log out and back in to complete the reset.", ToolTipIcon.Info);
        }

        #region Messaging

        internal static void ToolTip(string message)
        {
            trayIcon.Text = message;
        }

        public static void Error(int timeout, string title, string message)
        {
            trayIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.Error);
            trayIcon.Text = title + ": " + message;
        }

        public static void Info(int timeout, string title, string message)
        {
            trayIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.Info);
            trayIcon.Text = title + ": " + message;
        }

        public static void Warning(int timeout, string title, string message)
        {
            trayIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.Warning);
            trayIcon.Text = title + ": " + message;
        }

        public static void Volume(int volume)
        {
            //Hack, hide previous messages
            trayIcon.Visible = false;
            trayIcon.Visible = true;

            var title = volume == 0 ? "Muted" : "Full Volume";
            var message = volume + " %";

            trayIcon.ShowBalloonTip(100, title, message, ToolTipIcon.Info);
            trayIcon.Text = title + ": " + message;
            trayIcon.Icon = volume == 0 ? Muted : FullVolume;
        }

        #endregion
    }
}