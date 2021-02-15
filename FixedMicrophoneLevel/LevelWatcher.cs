using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SilentOrbit.FixedMicrophoneLevel
{
    class LevelWatcher : IDisposable
    {
        readonly Thread t;

        public static int Target => Muted ? 0 : Level;
        public static int Level { get; private set; } = 100;
        public static bool Muted { get; private set; }

        static CoreAudioController audio;

        public LevelWatcher()
        {
            t = new Thread(Run);
            t.IsBackground = true;
            t.Start();
        }

        public void Dispose()
        {
            t.Abort();
        }

        void Run()
        {
            try
            {
                NotifyIconContext.ToolTip("Starting controller");

                using (var ctrl = new CoreAudioController())
                {
                    audio = ctrl;

                    //NotifyIconContext.Info(1000, "Active", "Forcing all capture levels to " + Level + "%");
                    NotifyIconContext.Level(Target);

                    while (true)
                    {
                        UpdateLevel(reportFix: true);
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                //Silent exit
            }
            catch (Exception ex)
            {
                NotifyIconContext.Error(60000, ex.GetType().Name, ex.Message);
                Thread.Sleep(60000);
                Application.Exit();
            }
        }

        static void UpdateLevel(bool reportFix)
        {
            if (audio == null)
                return;

            //Communication capture
            {
                var dev = audio.GetDefaultDevice(AudioSwitcher.AudioApi.DeviceType.Capture, AudioSwitcher.AudioApi.Role.Communications);
                UpdateLevel(dev, reportFix);
            }
            foreach (var dev in audio.GetDevices())
            {
                if (dev.State != AudioSwitcher.AudioApi.DeviceState.Active)
                    continue;

                if ((dev.DeviceType & AudioSwitcher.AudioApi.DeviceType.Capture) != 0)
                {
                    UpdateLevel(dev, reportFix);
                }
            }
        }

        static void UpdateLevel(CoreAudioDevice dev, bool reportFix)
        {
            var orig = dev.IsMuted ? 0 : dev.Volume;
            if (orig != Target)
            {
                dev.Mute(Target == 0);
                if (Target != 0)
                    dev.Volume = Target;

                if (reportFix)
                    NotifyIconContext.Warning(5000, "Forced level " + orig + " --> " + Target + " %", dev.FullName);
            }
        }

        public static void ToggleMute() => SetMuted(!Muted);

        public static void SetMuted(bool muted)
        {
            Muted = muted;

            UpdateLevel(reportFix: false);

            NotifyIconContext.Level(Target);
        }

        public static void SetLevel(int level)
        {
            Muted = false;
            Level = level;

            UpdateLevel(reportFix: false);

            NotifyIconContext.Level(Target);
        }
    }
}
