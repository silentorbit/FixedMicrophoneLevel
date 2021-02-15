using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SilentOrbit.FixVolume
{
    class VolumeWatcher : IDisposable
    {
        readonly Thread t;

        public static int Target => Muted ? 0 : Volume;
        public static int Volume { get; private set; } = 100;
        public static bool Muted { get; private set; }

        static CoreAudioController audio;

        public VolumeWatcher()
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

                    //NotifyIconContext.Info(1000, "Active", "Forcing all capture levels to " + Volume + "%");
                    NotifyIconContext.Volume(Target);

                    while (true)
                    {
                        UpdateVolume(reportVolumeFix: true);
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

        static void UpdateVolume(bool reportVolumeFix)
        {
            if (audio == null)
                return;

            //Communication capture
            {
                var dev = audio.GetDefaultDevice(AudioSwitcher.AudioApi.DeviceType.Capture, AudioSwitcher.AudioApi.Role.Communications);
                UpdateVolume(dev, reportVolumeFix);
            }
            foreach (var dev in audio.GetDevices())
            {
                if (dev.State != AudioSwitcher.AudioApi.DeviceState.Active)
                    continue;

                if ((dev.DeviceType & AudioSwitcher.AudioApi.DeviceType.Capture) != 0)
                {
                    UpdateVolume(dev, reportVolumeFix);
                }
            }
        }

        static void UpdateVolume(CoreAudioDevice dev, bool reportVolumeFix)
        {
            var orig = dev.IsMuted ? 0 : dev.Volume;
            if (orig != Target)
            {
                dev.Mute(Target == 0);
                if (Target != 0)
                    dev.Volume = Target;

                if (reportVolumeFix)
                    NotifyIconContext.Warning(5000, "Forced volume " + orig + " --> " + Target + " %", dev.FullName);
            }
        }

        public static void ToggleMute() => SetMuted(!Muted);

        public static void SetMuted(bool muted)
        {
            Muted = muted;

            UpdateVolume(reportVolumeFix: false);

            NotifyIconContext.Volume(Target);
        }

        public static void SetVolume(int volume)
        {
            Muted = false;
            Volume = volume;

            UpdateVolume(reportVolumeFix: false);

            NotifyIconContext.Volume(Target);
        }
    }
}
