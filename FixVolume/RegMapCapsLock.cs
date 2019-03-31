using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SilentOrbit.FixVolume
{
    static class RegMapCapsLock
    {
        const string RUN_LOCATION = @"SYSTEM\CurrentControlSet\Control\Keyboard Layout";
        const string keyName = "Scancode Map";

        static RegistryKey SubKey
        {
            get
            {
                try
                {
                    return Registry.LocalMachine.CreateSubKey(RUN_LOCATION);
                }
                catch (UnauthorizedAccessException)
                {
                    return null;
                }
            }
        }

        static byte[] mapToF13 = new byte[] { 00, 00, 00, 00, 00, 00, 00, 00, 0x02, 00, 00, 00, 0x64, 00, 0x3a, 00, 00, 00, 00, 00 };


        public static void Set() => RunAdmin(CommandMap);

        public static void Reset() => RunAdmin(CommandReset);

        public static void RunAdmin(string command)
        {
            var psi = new ProcessStartInfo();
            psi.Verb = "runas";
            psi.FileName = Assembly.GetExecutingAssembly().Location;
            psi.Arguments = command;
            using (var p = Process.Start(psi))
            {
                p.WaitForExit();
                Debug.Assert(p.ExitCode == 0);
            }
        }

        public const string CommandMap = "MapCapsLock";
        public const string CommandReset = "ResetCapsLock";

        /// <summary>
        /// Called from command line.
        /// Don't try to call elevatedif this fails
        /// </summary>
        public static void SetCommandLine()
        {
            SubKey.SetValue(keyName, mapToF13, RegistryValueKind.Binary);
        }

        public static void ResetCommandLine()
        {
            SubKey.DeleteValue(keyName);
        }
    }
}
