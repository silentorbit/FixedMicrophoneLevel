using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SilentOrbit.FixVolume
{
    /// <summary>
    /// Monitor for key presses and mute the mic for a short while.
    /// </summary>
    class KeyMonitoring : IDisposable
    {
        #region DLLImport

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x0100;
        static LowLevelKeyboardProc _proc = HookCallback;
        static IntPtr _hookID = IntPtr.Zero;

        public static bool Enabled { get; set; } = true;

        public KeyMonitoring()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                var vkCode = (Keys)Marshal.ReadInt32(lParam);
                Console.WriteLine(vkCode);

                switch (vkCode)
                {
                    //Ignore keys that are used in class GlobalHotKey
                    case Keys.CapsLock:
                    case Keys.F13:
                    case Keys.LWin:
                    case Keys.RWin:
                    case Keys.Insert:
                        break;

                    default:
                        if (Enabled)
                            MuteTimer.Reset();
                        break;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
