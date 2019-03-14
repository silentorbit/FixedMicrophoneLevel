using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SilentOrbit.FixVolume
{
    class GlobalHotKey : NativeWindow, IDisposable
    {
        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static int WM_HOTKEY = 0x0312;
        const int currentId = 1;

        //Mute hotkey
        const ModifierKeys muteModifier = ModifierKeys.Win;
        const Keys muteKey = Keys.Insert;


        public GlobalHotKey()
        {
            // create the handle for the window.
            this.CreateHandle(new CreateParams());

            if (!RegisterHotKey(Handle, currentId, (uint)muteModifier, (uint)muteKey))
                NotifyIconContext.Error(10000, "HotKey Error", "Failed to register hotkey " + muteModifier + " + " + muteKey);
        }

        public void Dispose()
        {
            UnregisterHotKey(Handle, currentId);

            this.DestroyHandle();
        }

        /// <summary>
        /// Overridden to get the notifications.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // check if we got a hot key pressed.
            if (m.Msg == WM_HOTKEY)
            {
                // get the keys.
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                // invoke the event to notify the parent.
                if (modifier == muteModifier && key == muteKey)
                    VolumeWatcher.ToggleMute();
            }
        }

        [Flags]
        enum ModifierKeys : uint
        {
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }
    }
}
