using AudioSwitcher.AudioApi.CoreAudio;
using SilentOrbit.FixedMicrophoneLevel.Keyboard;
using SilentOrbit.FixedMicrophoneLevel.Microphone;
using SilentOrbit.FixedMicrophoneLevel.Reg;
using SilentOrbit.FixedMicrophoneLevel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SilentOrbit.FixedMicrophoneLevel
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
            using (var level = new LevelWatcher())
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
