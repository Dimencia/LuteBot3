using LuteBot.Config;
using LuteBot.LiveInput.Midi;
using LuteBot.UI.Utils;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.UI
{
    public partial class LiveInputForm : Form
    {
        private LiveMidiManager liveInputManager;
        private bool on = false;
        private bool keySetting = false;
        private int keyIndex = -1;
        private string keybindLabelText = "Press a key to bind the note [noteId]";

        private string octavesCovered = "Octaves covered by mordhau [first] to [last]";

        public LiveInputForm(LiveMidiManager liveMidiManager)
        {
            InitializeComponent();
            liveInputManager = liveMidiManager;
            liveInputManager.ChannelEventReceived += ChannelEventHandler;
            liveInputManager.RecordingStateChanged += RefreshListeningButton;
            DeviceCount();
            int lastDeviceUsed = ConfigManager.GetIntegerProperty(PropertyItem.LastMidiDeviceUsed);
            if (lastDeviceUsed > -1 && lastDeviceUsed < liveInputManager.DeviceCount)
            {
                DeviceComboBox.SelectedIndex = lastDeviceUsed;
                liveInputManager.SetMidiDevice(lastDeviceUsed);
            }
            int lastMidiLowBoundUSed = ConfigManager.GetIntegerProperty(PropertyItem.LastMidiLowBoundUsed);
            if (lastMidiLowBoundUSed >= 0 && lastMidiLowBoundUSed + ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount) <= 127)
            {
                liveInputManager.ForceLowBound(lastMidiLowBoundUSed);
            }
            RefreshOctaveLabel();
            KeybindLabel.Text = "";
            InitPiano();
        }

        private void RefreshListeningButton(object sender, EventArgs e)
        {
            if (!liveInputManager.Recording)
            {
                OnOffButton.Text = "Off";
                OnOffButton.BackColor = Color.FromArgb(255, 128, 128);
            }
            else
            {
                OnOffButton.Text = "On";
                OnOffButton.BackColor = Color.FromArgb(128, 255, 128);
            }
        }

        private void InitPiano()
        {
            int i = 0;
            foreach (Keys key in liveInputManager.Binds)
            {
                PianoControl.ChangePianoKeyBind(i, $@"{CodeToNote(i + liveInputManager.OutDevice.LowNoteId)} :
{key}");
                i++;
            }
            PianoControl.Refresh();
        }

        private string CodeToNote(int noteCode)
        {
            if (noteCode >= 0 && noteCode < 127)
            {
                int noteID = noteCode % 12;
                int octaveID = (noteCode / 12) - 2;
                string noteName = "ERR";
                switch (noteID)
                {
                    case 0:
                        noteName = "C";
                        break;
                    case 1:
                        noteName = "C#";
                        break;
                    case 2:
                        noteName = "D";
                        break;
                    case 3:
                        noteName = "D#";
                        break;
                    case 4:
                        noteName = "E";
                        break;
                    case 5:
                        noteName = "F";
                        break;
                    case 6:
                        noteName = "F#";
                        break;
                    case 7:
                        noteName = "G";
                        break;
                    case 8:
                        noteName = "G#";
                        break;
                    case 9:
                        noteName = "A";
                        break;
                    case 10:
                        noteName = "A#";
                        break;
                    case 11:
                        noteName = "B";
                        break;
                }
                return noteName + octaveID;
            }
            return "ERR";
        }

        private void RefreshOctaveLabel()
        {
            MordhauOctavesRangeLabel.Text = octavesCovered.Replace("[first]", CodeToNote(liveInputManager.OutDevice.LowNoteId)).Replace("[last]", CodeToNote(liveInputManager.OutDevice.HighNoteId));
            InitPiano();
        }

        private void DeviceCount()
        {
            DeviceComboBox.Items.Add(new MidiDeviceItem() { Id = -1, Name = "Keyboard Mode" });
            int count = liveInputManager.DeviceCount;
            for (int i = 0; i < count; i++)
            {
                string name = InputDevice.GetDeviceCapabilities(i).name;
                DeviceComboBox.Items.Add(new MidiDeviceItem() { Id = i, Name = name });
            }

        }

        private void ChannelEventHandler(object sender, ChannelMessageEventArgs e)
        {
            MidiListeningLabel.Text = $"Received channel event : c :{e.Message.MidiChannel} | n:{e.Message.Data1} | v:{e.Message.Data2} ";
        }

        private void PianoControl_PianoKeyDown(object sender, Sanford.Multimedia.Midi.UI.PianoKeyEventArgs e)
        {
            if (!keySetting)
            {
                keySetting = !keySetting;
                keyIndex = e.NoteID;
                PianoControl.Enabled = false;
                KeybindLabel.Text = keybindLabelText.Replace("[noteId]", e.NoteID.ToString());
            }
        }

        private void OnOffButton_Click(object sender, EventArgs e)
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

        private void DeviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (DeviceComboBox.SelectedIndex != 0)
                {
                    liveInputManager.SetMidiDevice((DeviceComboBox.SelectedItem as MidiDeviceItem).Id);
                    MidiDeviceStatusLabel.Text = $@"Connected to device
{(DeviceComboBox.SelectedItem as MidiDeviceItem).Id}";
                }
                else
                {
                    liveInputManager.KeyboardMode = true;
                    MidiDeviceStatusLabel.Text = $@"Keyboard mode enabled";
                }

            }
            catch (Exception ex)
            {
                MidiDeviceStatusLabel.Text = $"Error occured loading Midi device : {ex.Message}";
            }
        }

        private void LiveInputForm_FormClosed(object sender, FormClosedEventArgs e)
        {

            ConfigManager.SetProperty(PropertyItem.LastMidiDeviceUsed, liveInputManager.CurrentDevice.ToString());
            WindowPositionUtils.UpdateBounds(PropertyItem.LiveMidiPos, new Point() { X = Left, Y = Top });
            ConfigManager.SetProperty(PropertyItem.LastMidiLowBoundUsed, liveInputManager.OutDevice.LowNoteId.ToString());
            liveInputManager.Dispose();
            ConfigManager.SaveConfig();
        }

        private void MinusOctaveButton_Click(object sender, EventArgs e)
        {
            if ((liveInputManager.OutDevice.LowNoteId - 12) >= 0)
            {
                liveInputManager.ForceLowBound(liveInputManager.OutDevice.LowNoteId - 12);
                RefreshOctaveLabel();
                InitPiano();
            }
        }

        private void PlusOctaveButton_Click(object sender, EventArgs e)
        {
            if ((liveInputManager.OutDevice.HighNoteId + 12) < 127)
            {
                liveInputManager.ForceLowBound(liveInputManager.OutDevice.LowNoteId + 12);
                RefreshOctaveLabel();
                InitPiano();
            }
        }

        private void LiveInputForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (keySetting)
            {
                keySetting = !keySetting;
                PianoControl.ChangePianoKeyBind(keyIndex, $@"{CodeToNote(keyIndex + liveInputManager.OutDevice.LowNoteId)} :
{e.KeyData.ToString()}");
                PianoControl.Enabled = true;
                KeybindLabel.Text = "";
                liveInputManager.Binds.Insert(keyIndex, e.KeyData);
            }
        }

        private void MuteCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            liveInputManager.OutDevice.MuteOutOfRange = MuteCheckBox.Checked;
        }
    }
}