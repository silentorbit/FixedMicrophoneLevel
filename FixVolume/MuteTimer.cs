using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SilentOrbit.FixVolume
{
    /// <summary>
    /// Triggered by KeyMonitoring and will mute the mic for a short while before it's unmuted again.
    /// </summary>
    class MuteTimer : IDisposable
    {
        readonly Timer timer;
        static MuteTimer instance;

        const int msMuteTime = 300;

        public MuteTimer()
        {
            timer = new Timer(Tick);
            instance = this;
        }

        /// <summary>
        /// Reset clock, start a new interval of muted mic.
        /// </summary>
        public static void Reset()
        {
            if (VolumeWatcher.Muted)
                return; //Keep muted

            Console.WriteLine($"Muting for {msMuteTime} ms");
            VolumeWatcher.Muted = true;
            instance.timer.Change(500, Timeout.Infinite);
        }

        void Tick(object state)
        {
            if (VolumeWatcher.Muted)
            {
                Console.WriteLine($"Unmuted after {msMuteTime} ms");
                VolumeWatcher.Muted = false;
            }
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
