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
        static int Main(string[] args)
        {
            if (args.Length == 1)
            {
                if (args[0] == RegMapCapsLock.CommandMap)
                    RegMapCapsLock.SetCommandLine();
                if (args[0] == RegMapCapsLock.CommandReset)
                    RegMapCapsLock.ResetCommandLine();
                return 0;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var context = new NotifyIconContext())
            using (var volume = new VolumeWatcher())
            using (var hotkey = new GlobalHotKey())
            using (var timer = new MuteTimer())
            using (var keymon = new KeyMonitoring())
            {
                Application.Run(context);
                return 0;
            }
        }
    }
}
