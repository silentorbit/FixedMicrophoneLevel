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

        public static int Volume { get; private set; } = 100;

        public static bool Muted
        {
            get => Volume == 0;
            set => SetMute(value);
        }

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
                    NotifyIconContext.Volume(Volume);

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
            if (orig != Volume)
            {
                dev.Mute(Volume == 0);
                if (Volume != 0)
                    dev.Volume = Volume;

                if (reportVolumeFix)
                    NotifyIconContext.Warning(5000, "Forced volume " + orig + " --> " + Volume + " %", dev.FullName);
            }
        }

        internal static void ToggleMute()
        {
            Muted = !Muted;
        }

        static void SetMute(bool muted)
        {
            if (muted)
                Volume = 0;
            else
                Volume = 100;

            UpdateVolume(reportVolumeFix: false);

            NotifyIconContext.Volume(Volume);
        }
    }
}
