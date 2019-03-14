using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SilentOrbit.FixVolume
{
    static class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var context = new NotifyIconContext())
            using (var volume = new VolumeWatcher())
            using (var hotkey = new GlobalHotKey())
            {
                Application.Run(context);
            }
        }
    }
}
