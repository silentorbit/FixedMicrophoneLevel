using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SilentOrbit.FixedMicrophoneLevel
{
    class GlobalHotKey : NativeWindow, IDisposable
    {
        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static int WM_HOTKEY = 0x0312;
        int currentId = 0;

        public GlobalHotKey()
        {
            this.CreateHandle(new CreateParams());

            RegisterHotKey(ModifierKeys.Win, Keys.Insert);

            //Use windows
            RegisterHotKey(ModifierKeys.None, Keys.F13);

            RegisterHotKey(ModifierKeys.None, Keys.CapsLock);
        }

        void RegisterHotKey(ModifierKeys mod, Keys key)
        {
            currentId++;
            if (!RegisterHotKey(Handle, currentId, (uint)mod, (uint)key))
                NotifyIconContext.Error(10000, "HotKey Error", "Failed to register hotkey " + mod + " + " + key);
        }

        public void Dispose()
        {
            while (currentId > 0)
            {
                UnregisterHotKey(Handle, currentId);
                currentId--;
            }

            this.DestroyHandle();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                if (key == Keys.CapsLock)
                    LevelWatcher.SetMuted(Control.IsKeyLocked(Keys.CapsLock));
                else
                    LevelWatcher.ToggleMute();
            }
        }

        [Flags]
        enum ModifierKeys : uint
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }
    }
}
