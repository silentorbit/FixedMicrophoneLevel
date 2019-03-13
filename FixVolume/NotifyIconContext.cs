using System;
using System.Drawing;
using System.Windows.Forms;

namespace SilentOrbit.FixVolume
{
    internal class NotifyIconContext : ApplicationContext
    {
        readonly NotifyIcon trayIcon;

        readonly Icon Red = GenerateIcon(Color.Red);
        readonly Icon Blue = GenerateIcon(Color.Blue);
        readonly Icon Yellow = GenerateIcon(Color.Yellow);

        public NotifyIconContext()
        {
            trayIcon = new NotifyIcon()
            {
                Icon = GenerateIcon(Color.Red),
                ContextMenu = new ContextMenu(new MenuItem[] { new MenuItem("Exit", Exit) }),
                Visible = true
            };
        }

        internal void Exit(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        public void Error(int timeout, string title, string message)
        {
            trayIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.Error);
            trayIcon.Text = title + ": " + message;
            trayIcon.Icon = Red;
        }

        public void Info(int timeout, string title, string message)
        {
            trayIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.Info);
            trayIcon.Text = title + ": " + message;
            trayIcon.Icon = Blue;
        }

        public void Warning(int timeout, string title, string message)
        {
            trayIcon.ShowBalloonTip(timeout, title, message, ToolTipIcon.Warning);
            trayIcon.Text = title + ": " + message;
            trayIcon.Icon = Yellow;
        }

        static Icon GenerateIcon(Color color)
        {
            using (Bitmap b = new Bitmap(32, 32))
            {
                using (var g = Graphics.FromImage(b))
                {
                    g.Clear(color);
                }

                return Icon.FromHandle(b.GetHicon());
            }
        }
    }
}