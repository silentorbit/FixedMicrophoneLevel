using System;
using System.Collections.Generic;
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

        readonly MenuItem volume;
        readonly MenuItem autoStart;
        readonly MenuItem mapCapsLock;
        readonly MenuItem mapCapsLockSet;
        readonly MenuItem mapCapsLockReset;
        readonly MenuItem mute;
        readonly MenuItem autoMute;

        public NotifyIconContext()
        {
            volume = new MenuItem("&Volume");
            BuildVolumeMenu(volume);
            autoStart = new MenuItem("&Auto Start", ToggleAutoStart);
            autoMute = new MenuItem("Auto mute on key presses", ToggleAutoMute);
            mapCapsLock = new MenuItem("Map &CapsLock");
            mapCapsLockSet = new MenuItem("To F13", MapCapsLock);
            mapCapsLockReset = new MenuItem("Reset CapsLock", ResetCapsLock);
            mapCapsLock.MenuItems.Add(mapCapsLockSet);
            mapCapsLock.MenuItems.Add(mapCapsLockReset);
            mute = new MenuItem("&Mute", (s, e) => VolumeWatcher.ToggleMute());

            var about = new MenuItem("About", (s, e) => Process.Start("https://github.com/SilentOrbit/FixVolume"));
            var exit = new MenuItem("E&xit", (s, e) => Application.Exit());

            trayIcon = new NotifyIcon()
            {
                Icon = StartupInactive,
                ContextMenu = new ContextMenu(new MenuItem[] { volume, mute, autoMute, autoStart, mapCapsLock, about, exit }),
                Visible = true
            };

            trayIcon.ContextMenu.Popup += (object sender, EventArgs e) =>
            {
                autoStart.Checked = RegAutoStart.Get();
                autoMute.Checked = KeyMonitoring.Enabled;
                mute.Checked = VolumeWatcher.Volume == 0;
            };

            trayIcon.Click += TrayIcon_Click;
        }

        void BuildVolumeMenu(MenuItem volume)
        {
            var dic = new Dictionary<int, MenuItem>();

            for (int n = 100; n > 0; n -= 5)
            {
                var vol = new MenuItem(n + "%", GenerateVolumeEvent(n));
                volume.MenuItems.Add(vol);
                dic.Add(n, vol);
            }

            volume.Popup += (object sender, EventArgs e) =>
            {
                foreach (var m in dic.Values)
                    m.Checked = false;

                if (dic.TryGetValue(VolumeWatcher.Target, out var menu))
                    menu.Checked = true;
            };
        }

        EventHandler GenerateVolumeEvent(int n)
        {
            return (object sender, EventArgs e) =>
            {
                VolumeWatcher.SetVolume(n);
            };
        }

        void ToggleAutoMute(object sender, EventArgs e)
        {
            KeyMonitoring.Enabled = !KeyMonitoring.Enabled;
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
            Text = message;
        }

        public static void Error(int timeout, string title, string message)
        {
            trayIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.Error);
            Text = title + ": " + message;
        }

        public static void Info(int timeout, string title, string message)
        {
            trayIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.Info);
            Text = title + ": " + message;
        }

        public static void Warning(int timeout, string title, string message)
        {
            trayIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.Warning);
            Text = title + ": " + message;
        }

        public static string Text
        {
            set
            {
                var t = value;
                if (t.Length > 63)
                    t = t.Substring(0, 63);
                trayIcon.Text = t;
            }
        }

        public static void Volume(int volume)
        {
            //Hack, hide previous messages
            trayIcon.Visible = false;
            trayIcon.Visible = true;

            var title = volume == 0 ? "Microphone Muted" : "Microphone ON";
            var message = volume + " %";

            Text = title + ": " + message;
            trayIcon.Icon = volume == 0 ? Muted : FullVolume;

            //Show tip after chaning the tray icon
            trayIcon.ShowBalloonTip(100, title, message, ToolTipIcon.Info);
        }

        #endregion
    }
}