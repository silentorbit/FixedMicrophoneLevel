using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SilentOrbit.FixedMicrophoneLevel.Microphone;

namespace SilentOrbit.FixedMicrophoneLevel.Keyboard
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
        public static void Reset() => instance.ResetInstance();

        /// <summary>
        /// Mute by typing
        /// </summary>
        bool typingMute = false;

        void ResetInstance()
        {
            if (LevelWatcher.Muted && !typingMute)
                return; //Keep muted

            typingMute = true;

            if (LevelWatcher.Muted == false)
            {
                Console.WriteLine($"Muting for {msMuteTime} ms");
                LevelWatcher.SetMuted(true);
            }
            timer.Change(msMuteTime, Timeout.Infinite);
        }

        void Tick(object state)
        {
            typingMute = false;

            if (LevelWatcher.Muted)
            {
                Console.WriteLine($"Unmuted after {msMuteTime} ms");
                LevelWatcher.SetMuted(false);
            }
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
