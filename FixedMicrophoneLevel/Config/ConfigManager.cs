using SilentOrbit.FixedMicrophoneLevel.Microphone;
using SilentOrbit.FixedMicrophoneLevel.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SilentOrbit.FixedMicrophoneLevel.Config
{
    static class ConfigManager
    {
        static readonly MicConfig config;

        public static int Target => config.Muted ? 0 : config.Level;

        #region Config Loading

        static ConfigManager()
        {
            config = Load();
        }

        static string micConfigPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SilentOrbit.FixedMicrophoneLevel",
            "config.json");

        static MicConfig Load()
        {
            try
            {
                var json = File.ReadAllText(micConfigPath);
                var config = JsonSerializer.Deserialize<MicConfig>(json);
                return config;
            }
            catch
            {
                return new MicConfig();
            }
        }

        static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(micConfigPath);
                Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize<MicConfig>(config);
                File.WriteAllText(micConfigPath, json);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        #endregion

        public static bool MuteOnKeyPress
        {
            get => config.MuteOnKeyPress;
            set
            {
                config.MuteOnKeyPress = value;
                Save();
            }
        }

        public static void ToggleMute() => Muted = !Muted;

        public static bool Muted
        {
            get => config.Muted;
            set
            {
                config.Muted = value;
                Save();

                LevelWatcher.UpdateLevel(reportFix: false);

                NotifyIconContext.Level(Target);
            }
        }

        public static bool ShowNotifications
        {
            get => config.ShowNotifications;
            set
            {
                config.ShowNotifications = value;
                Save();
            }
        }

        public static void SetLevel(int level)
        {
            config.Muted = false;
            config.Level = level;
            Save();

            LevelWatcher.UpdateLevel(reportFix: false);

            NotifyIconContext.Level(Target);
        }
    }
}
