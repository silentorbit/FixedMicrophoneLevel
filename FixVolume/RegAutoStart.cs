using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SilentOrbit.FixVolume
{
    static class RegAutoStart
    {
        const string RUN_LOCATION = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string keyName = "SilenotOrbit.FixVolume";

        static RegistryKey SubKey => Registry.CurrentUser.CreateSubKey(RUN_LOCATION);
        static string Path => Assembly.GetExecutingAssembly().Location;

        public static void Set()
        {
            SubKey.SetValue(keyName, Path);
        }

        public static bool Get()
        {
            RegistryKey key = SubKey;
            if (key == null)
                return false;
            string val = (string)key.GetValue(keyName);
            return val == Path;
        }

        public static void Remove()
        {
            SubKey.DeleteValue(keyName);
        }
    }
}
