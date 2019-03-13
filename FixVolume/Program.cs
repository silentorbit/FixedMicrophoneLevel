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
        static NotifyIconContext context;

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            context = new NotifyIconContext();

            Thread t = new Thread(Run);
            t.IsBackground = true;
            t.Start();

            Application.Run(context);
        }

        private static void Run()
        {
            try
            {
                context.Error(1000, "Loading", "Starting controller");
                Console.WriteLine("Loading controller");

                var ctrl = new CoreAudioController();

                context.Info(1000, "Active", "Forcing all capture levels to 100%");

                while (true)
                {
                    //Communication capture
                    {
                        var dev = ctrl.GetDefaultDevice(AudioSwitcher.AudioApi.DeviceType.Capture, AudioSwitcher.AudioApi.Role.Communications);
                        FixVolume(dev);
                    }
                    foreach (var dev in ctrl.GetDevices())
                    {
                        if (dev.State != AudioSwitcher.AudioApi.DeviceState.Active)
                            continue;

                        if ((dev.DeviceType & AudioSwitcher.AudioApi.DeviceType.Capture) != 0)
                        {
                            FixVolume(dev);
                        }
                    }

                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                context.Error(60000, ex.GetType().Name, ex.Message);
                Thread.Sleep(60000);
                context.Exit(null, null);
            }
        }

        private static void FixVolume(CoreAudioDevice dev)
        {
            if (dev.Volume != 100)
            {
                dev.Volume = 100;
                Console.WriteLine(DateTime.Now + " Forced Volume: 100");
                context.Warning(5000, "Forced volume 100%", dev.FullName);
            }
        }
    }
}
