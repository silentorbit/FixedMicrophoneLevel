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
                        SetVolume();
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

        static void SetVolume()
        {
            if (audio == null)
                return;

            //Communication capture
            {
                var dev = audio.GetDefaultDevice(AudioSwitcher.AudioApi.DeviceType.Capture, AudioSwitcher.AudioApi.Role.Communications);
                FixVolume(dev);
            }
            foreach (var dev in audio.GetDevices())
            {
                if (dev.State != AudioSwitcher.AudioApi.DeviceState.Active)
                    continue;

                if ((dev.DeviceType & AudioSwitcher.AudioApi.DeviceType.Capture) != 0)
                {
                    FixVolume(dev);
                }
            }
        }

        static void FixVolume(CoreAudioDevice dev)
        {
            var orig = dev.Volume;
            if (orig != Volume)
            {
                dev.Volume = Volume;

                if (reportVolumeFix)
                    NotifyIconContext.Warning(5000, "Forced volume " + orig + " --> " + Volume + " %", dev.FullName);
            }
        }

        static bool reportVolumeFix = true;

        internal static void ToggleMute()
        {
            SetMute(Volume != 0);
        }

        internal static void SetMute(bool muted)
        {
            reportVolumeFix = false;

            if (muted)
                Volume = 0;
            else
                Volume = 100;

            SetVolume();

            NotifyIconContext.Volume(Volume);

            reportVolumeFix = true;
        }
    }
}
