using LuteBot.Config;
using LuteBot.LiveInput.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.IO.KB
{
    public class HotkeyManager
    {

        private LiveMidiManager liveInputManager;

        public LiveMidiManager LiveInputManager { get => liveInputManager; set => liveInputManager = value; }

        public event EventHandler PlayKeyPressed;
        public event EventHandler NextKeyPressed;
        public event EventHandler PreviousKeyPressed;
        public event EventHandler ConsoleKeyPressed;
        public event EventHandler ReadyPressed;
        public event EventHandler SynchronizePressed;

        public void HotkeyPressed(int keyCode)
        {
            Keys tempKey = (Keys)keyCode;
            PropertyItem performedAction = ConfigManager.GetKeybindPropertyFromAction(tempKey);
            if (performedAction != PropertyItem.None)
            {
                if (performedAction == PropertyItem.Play)
                {
                    EventHandler handler = PlayKeyPressed;
                    handler?.Invoke(this, null);
                }
                else
                if (performedAction == PropertyItem.Next)
                {
                    EventHandler handler = NextKeyPressed;
                    handler?.Invoke(this, null);
                }
                else
                if (performedAction == PropertyItem.Previous)
                {
                    EventHandler handler = PreviousKeyPressed;
                    handler?.Invoke(this, null);
                }
                else
                if (performedAction == PropertyItem.OpenConsole)
                {
                    EventHandler handler = ConsoleKeyPressed;
                    handler?.Invoke(this, null);
                }
                else
                if (performedAction == PropertyItem.Ready)
                {
                    EventHandler handler = ReadyPressed;
                    handler?.Invoke(this, null);
                }
                else
                if (performedAction == PropertyItem.LiveMidiListen)
                {
                    if (liveInputManager.Recording)
                    {
                        liveInputManager.Off();
                    }
                    else
                    {
                        liveInputManager.On();
                    }
                }
                else
                if (performedAction == PropertyItem.SynchronizedPlay)
                {
                    EventHandler handler = SynchronizePressed;
                    handler?.Invoke(this, null);
                }
            }
            else
            {
                if (LiveInputManager != null)
                {
                    LiveInputManager.HandleKeybindPressed(tempKey);
                }
            }
        }
    }
}
