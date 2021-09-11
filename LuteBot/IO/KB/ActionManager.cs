using LuteBot.Config;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.IO.KB
{
    public static class ActionManager
    {
        public enum AutoConsoleMode
        {
            Old,
            New,
            Off
        }

        public const UInt32 WM_KEYDOWN = 0x0100;
        public const UInt32 WM_KEYUP = 0x0101;

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        private static string consoleCommand = "EQUIPMENTCOMMAND";

        private static bool consoleOn = false;

        public static event EventHandler<int> NotePlayed;

        public static string AutoConsoleModeToString(AutoConsoleMode mode)
        {
            switch (mode)
            {
                case AutoConsoleMode.Old:
                    return "Old";
                case AutoConsoleMode.New:
                    return "New";
                case AutoConsoleMode.Off:
                    return "Off";
                default:
                    return "Off";
            }
        }

        public static AutoConsoleMode AutoConsoleModeFromString(string str)
        {
            switch (str)
            {
                case "Old":
                    return AutoConsoleMode.Old;
                case "New":
                    return AutoConsoleMode.New;
                case "Off":
                    return AutoConsoleMode.Off;
                default:
                    return AutoConsoleMode.Off;
            }
        }

        public static void ToggleConsole(bool consoleOpen)
        {
            if (consoleOpen != consoleOn)
            {
                Process[] processes = Process.GetProcessesByName("Mordhau-Win64-Shipping");
                foreach (Process proc in processes)
                {
                    PostMessage(proc.MainWindowHandle, WM_KEYDOWN, (int)ConfigManager.GetKeybindProperty(PropertyItem.OpenConsole), 0);
                    if (!consoleOn)
                    {
                        PostMessage(proc.MainWindowHandle, WM_KEYDOWN, (int)ConfigManager.GetKeybindProperty(PropertyItem.OpenConsole), 0);
                        consoleOn = true;
                    }
                    else
                    {
                        consoleOn = false;
                    }
                }
            }
        }


        private static Keys[] importantModifierKeys = new Keys[] { Keys.Alt, Keys.Control, Keys.ControlKey, Keys.LControlKey, Keys.RControlKey, Keys.Shift, Keys.ShiftKey, Keys.LShiftKey, Keys.RShiftKey };

        private static void InputCommand(int noteId, int channel) // Channel is just for identification to invoke a NotePlayed event
        {
            Process[] processes = Process.GetProcessesByName("Mordhau-Win64-Shipping");
            var modKeys = Control.ModifierKeys;

            foreach (var modKey in importantModifierKeys)
                if ((modKeys & modKey) == modKey)
                {
                    Thread.Sleep(ConfigManager.GetIntegerProperty(PropertyItem.NoteCooldown));
                    return; // Drop notes if they're holding any of the important keys, so Mordhau doesn't go nuts
                    // But also sleep the expected amount of time to help avoid situations where they let go of the key and mordhau hasn't noticed yet
                }
            if (AutoConsoleModeFromString(ConfigManager.GetProperty(PropertyItem.ConsoleOpenMode)) == AutoConsoleMode.New)
            {
                foreach (Process proc in processes)
                {
                    PostMessage(proc.MainWindowHandle, WM_KEYDOWN, (int)ConfigManager.GetKeybindProperty(PropertyItem.OpenConsole), 0);
                }
            }
            foreach (char key in consoleCommand)
            {
                Enum.TryParse<Keys>("" + key, out Keys tempKey);
                foreach (Process proc in processes)
                {
                    PostMessage(proc.MainWindowHandle, WM_KEYDOWN, (int)tempKey, 0);
                }
            }
            foreach (Process proc in processes)
            {
                PostMessage(proc.MainWindowHandle, WM_KEYDOWN, (int)Keys.Space, 0);
            }
            foreach (char key in noteId.ToString())
            {
                Enum.TryParse<Keys>("NumPad" + key, out Keys tempKey);
                foreach (Process proc in processes)
                {
                    PostMessage(proc.MainWindowHandle, WM_KEYDOWN, (int)tempKey, 0);
                }
            }
            foreach (Process proc in processes)
            {
                Thread.Sleep(ConfigManager.GetIntegerProperty(PropertyItem.NoteCooldown));
                PostMessage(proc.MainWindowHandle, WM_KEYUP, (int)Keys.Enter, 0);
                NotePlayed?.Invoke(null, channel);
            }
        }

        public static void PlayNote(int noteId, int channel) // Channel is just to identify it for a NotePlayed event
        {
            InputCommand(noteId, channel);
        }

        private static void InputAngle()
        {

        }
    }
}
