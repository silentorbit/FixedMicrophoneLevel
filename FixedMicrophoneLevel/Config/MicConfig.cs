using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SilentOrbit.FixedMicrophoneLevel.Config
{
    /// <summary>
    /// Stored settings
    /// </summary>
    class MicConfig
    {
        /// <summary>
        /// Microphone level when not muted
        /// </summary>
        public int Level { get; set; } = 75;

        public bool Muted { get; set; }

        /// <summary>
        /// Mute microphone for a short while when a key is pressed.
        /// Prevent loud keyboards from being heard.
        /// </summary>
        public bool MuteOnKeyPress { get; set; }
    }
}
