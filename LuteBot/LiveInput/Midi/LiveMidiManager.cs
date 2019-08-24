using LuteBot.Config;
using LuteBot.Core;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.LiveInput.Midi
{
    public class LiveMidiManager
    {
        private InputDevice inputDevice;
        private MordhauOutDevice outDevice;
        private List<Keys> binds;
        private bool keyboardMode;
        private bool recording;

        public EventHandler<ChannelMessageEventArgs> ChannelEventReceived;
        public int DeviceCount = InputDevice.DeviceCount;
        public int CurrentDevice { get => (inputDevice != null) ? inputDevice.DeviceID : -1; }

        public MordhauOutDevice OutDevice { get => outDevice; }
        public List<Keys> Binds { get => binds; set => binds = value; }
        public bool KeyboardMode { get => keyboardMode; set => keyboardMode = value; }
        public bool Recording { get => recording; }
        public EventHandler RecordingStateChanged;

        public LiveMidiManager()
        {
            outDevice = new MordhauOutDevice();
            outDevice.HighMidiNoteId = 127;
            outDevice.LowMidiNoteId = 0;
            outDevice.CooldownNeeded = false;
            recording = false;
            keyboardMode = false;
            binds = new List<Keys>();
            InitBinds();
        }

        public void HandleKeybindPressed(Keys key)
        {
            if (keyboardMode && Recording)
            {
                int index = binds.IndexOf(key);
                if (index != -1)
                {
                    outDevice.SendNote(new ChannelMessage(ChannelCommand.NoteOn, 1, index + outDevice.LowNoteId, 1));
                }
            }
        }

        public void SaveKeyBinds()
        {
            StringBuilder strbld = new StringBuilder();
            foreach (Keys bind in binds)
            {
                strbld.Append(bind.ToString()).Append(';');
            }
            strbld.Remove(strbld.Length - 1, 1);
            ConfigManager.SetProperty(PropertyItem.VirtualKeyboardBinds, strbld.ToString());
        }

        public void Dispose()
        {
            if (inputDevice != null)
            {
                inputDevice.Dispose();
            }
            SaveKeyBinds();
        }

        public void SetMidiDevice(int id)
        {
            if (inputDevice != null)
            {
                inputDevice.Dispose();
            }
            inputDevice = new InputDevice(id);
            inputDevice.ChannelMessageReceived += ChannelMessageReceived;
            keyboardMode = false;
        }

        public void ForceLowBound(int value)
        {
            outDevice.LowNoteId = value;
        }

        private void ChannelMessageReceived(object sender, ChannelMessageEventArgs e)
        {
            if (!keyboardMode)
            {
                ChannelEventReceived.Invoke(sender, e);
                outDevice.SendNote(e.Message);
            }
        }

        private void InitBinds()
        {
            string[] configBinds = ConfigManager.GetProperty(PropertyItem.VirtualKeyboardBinds).Split(';');
            for (int i = 0; i < ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount); i++)
            {
                if (i < configBinds.Length)
                {
                    if (Enum.TryParse<Keys>(configBinds[i], out Keys result))
                    {
                        binds.Add(result);
                        continue; //oof
                    }
                }
                binds.Add(Keys.None);
            }
        }

        public void On()
        {
            if (inputDevice != null)
            {
                inputDevice.StartRecording();
            }
            recording = true;
            RecordingStateChanged.Invoke(this, new EventArgs());
        }

        public void Off()
        {
            if (inputDevice != null)
            {
                inputDevice.StopRecording();
            }
            recording = false;
            RecordingStateChanged.Invoke(this, new EventArgs());
        }
    }
}
