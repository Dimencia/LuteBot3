using LuteBot.Config;
using LuteBot.Core.Midi;
using LuteBot.IO.KB;
using LuteBot.UI.Utils;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot
{
    public partial class SettingsForm : Form
    {
        private static string GUILD_URL = "https://discord.gg/4xnJVuz";
        private MidiPlayer player;
        private LuteBotForm mainForm;

        public SettingsForm(MidiPlayer player, LuteBotForm mainForm)
        {
            InitializeComponent();
            this.player = player;
            this.mainForm = mainForm;
            SetVersion();
            InitSettings();
        }

        private void InitSettings()
        {
            SoundBoardCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.SoundBoard);
            PlaylistCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.PlayList);
            TrackSelectionCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.TrackSelection);
            PartitionListCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.PartitionList);
            SoundEffectsCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.SoundEffects);

            InitRadioButtons();

            NoteConversionMode.SelectedIndex = ConfigManager.GetIntegerProperty(PropertyItem.NoteConversionMode);
            LowestNoteNumeric.Value = ConfigManager.GetIntegerProperty(PropertyItem.LowestNoteId);
            NoteCountNumeric.Value = ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount);
            NoteCooldownNumeric.Value = ConfigManager.GetIntegerProperty(PropertyItem.NoteCooldown);
            LiveMidiCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.LiveMidi);

            checkBoxCheckUpdates.Checked = ConfigManager.GetBooleanProperty(PropertyItem.CheckForUpdates);
            checkBoxMajorUpdates.Checked = ConfigManager.GetBooleanProperty(PropertyItem.MajorUpdates);
            checkBoxMinorUpdates.Checked = ConfigManager.GetBooleanProperty(PropertyItem.MinorUpdates);

            try
            {
                NotesPerChordNumeric.Value = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
            }
            catch
            {
                ConfigManager.SetProperty(PropertyItem.NumChords, "3");
                ConfigManager.SaveConfig();
                NotesPerChordNumeric.Value = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
            }

            InitInstruments();
            InitOutputDevice();
        }

        private void InitInstruments()
        {
            Instrument.Read();
            instrumentsBox.DisplayMember = "Name";
            foreach (Instrument i in Instrument.Prefabs)
                instrumentsBox.Items.Add(i);
            instrumentsBox.SelectedIndex = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
        }

        private void InitOutputDevice()
        {
            // Add each output device to the combobox in order...
            int numDevices = OutputDevice.DeviceCount;
            for (int i = 0; i < numDevices; i++)
                outputDeviceBox.Items.Add(OutputDevice.GetDeviceCapabilities(i).name);

            try
            {
                outputDeviceBox.SelectedIndex = ConfigManager.GetIntegerProperty(PropertyItem.OutputDevice);
            }
            catch { } // Some people have no output devices and that's awkward
        }

        private void SetVersion()
        {
            VersionLabel.Text = VersionLabel.Text.Replace("[VERSION]", ConfigManager.GetVersion());
        }

        private void PlaylistCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.PlayList, PlaylistCheckBox.Checked.ToString());
        }

        private void SoundBoardCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.SoundBoard, SoundBoardCheckBox.Checked.ToString());
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            ConfigManager.Refresh();
            this.Close();
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            ConfigManager.SaveConfig();
            this.Close();
        }

        private void SoundEffectsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.SoundEffects, SoundEffectsCheckBox.Checked.ToString());
        }

        private void TrackSelectionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.TrackSelection, TrackSelectionCheckBox.Checked.ToString());
        }

        private void PartitionListCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.PartitionList, PartitionListCheckBox.Checked.ToString());
        }

        private void NoteConversionMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.NoteConversionMode, NoteConversionMode.SelectedIndex.ToString());
        }

        private void LowestNoteNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.LowestNoteId, LowestNoteNumeric.Value.ToString());
        }

        private void NoteCountNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.AvaliableNoteCount, (NoteCountNumeric.Value).ToString());
        }

        private void NoteCooldownNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.NoteCooldown, NoteCooldownNumeric.Value.ToString());
        }

        private void LiveMidiCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.LiveMidi, LiveMidiCheckBox.Checked.ToString());
        }

        private void InitRadioButtons()
        {
            ActionManager.AutoConsoleMode consoleMode = ActionManager.AutoConsoleModeFromString(ConfigManager.GetProperty(PropertyItem.ConsoleOpenMode));
            switch (consoleMode)
            {
                case ActionManager.AutoConsoleMode.New:
                    NewAutoConsoleRadio.Checked = true;
                    OldAutoConsoleRadio.Checked = false;
                    OffAutoConsoleRadio.Checked = false;
                    return;
                case ActionManager.AutoConsoleMode.Old:
                    NewAutoConsoleRadio.Checked = false;
                    OldAutoConsoleRadio.Checked = true;
                    OffAutoConsoleRadio.Checked = false;
                    return;
                case ActionManager.AutoConsoleMode.Off:
                    NewAutoConsoleRadio.Checked = false;
                    OldAutoConsoleRadio.Checked = false;
                    OffAutoConsoleRadio.Checked = true;
                    return;
            }
        }

        private void OldAutoConsoleRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (OldAutoConsoleRadio.Checked)
            {
                ConfigManager.SetProperty(PropertyItem.ConsoleOpenMode, ActionManager.AutoConsoleModeToString(ActionManager.AutoConsoleMode.Old));
                NewAutoConsoleRadio.Checked = false;
                OffAutoConsoleRadio.Checked = false;
            }
        }

        private void NewAutoConsoleRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (NewAutoConsoleRadio.Checked)
            {
                ConfigManager.SetProperty(PropertyItem.ConsoleOpenMode, ActionManager.AutoConsoleModeToString(ActionManager.AutoConsoleMode.New));
                OldAutoConsoleRadio.Checked = false;
                OffAutoConsoleRadio.Checked = false;
            }
        }

        private void OffAutoConsoleRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (OffAutoConsoleRadio.Checked)
            {
                ConfigManager.SetProperty(PropertyItem.ConsoleOpenMode, ActionManager.AutoConsoleModeToString(ActionManager.AutoConsoleMode.Off));
                NewAutoConsoleRadio.Checked = false;
                OldAutoConsoleRadio.Checked = false;
            }
        }

        private void OutputDeviceBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.OutputDevice, outputDeviceBox.SelectedIndex.ToString());
            player.ResetDevice(); // I hate that we had to pass this just to do this, but whatever
        }

        private void InstrumentsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If it's already the instrument we have as our property
            // Then don't re-set the values
            int currentInstrument = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            if (currentInstrument != instrumentsBox.SelectedIndex)
            {
                ConfigManager.SetProperty(PropertyItem.Instrument, instrumentsBox.SelectedIndex.ToString());
                Instrument target = (Instrument)instrumentsBox.SelectedItem;

                SoundEffectsCheckBox.Checked = !target.Name.StartsWith("Mordhau", true, System.Globalization.CultureInfo.InvariantCulture);
                ConfigManager.SetProperty(PropertyItem.SoundEffects, SoundEffectsCheckBox.Checked.ToString());

                LowestNoteNumeric.Value = target.LowestSentNote;
                ConfigManager.SetProperty(PropertyItem.LowestNoteId, target.LowestSentNote.ToString());

                NoteCountNumeric.Value = target.NoteCount;
                ConfigManager.SetProperty(PropertyItem.AvaliableNoteCount, target.NoteCount.ToString());

                NoteCooldownNumeric.Value = target.NoteCooldown;
                ConfigManager.SetProperty(PropertyItem.NoteCooldown, target.NoteCooldown.ToString());

                ConfigManager.SetProperty(PropertyItem.LowestPlayedNote, target.LowestPlayedNote.ToString());

                ConfigManager.SetProperty(PropertyItem.ForbidsChords, target.ForbidsChords.ToString());

                mainForm.OnInstrumentChanged(currentInstrument);
            }
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(GUILD_URL); // Bard's guild discord link... 
        }

        private void LinkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.tobias-erichsen.de/software/loopmidi.html");
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.tobias-erichsen.de/software/loopmidi.html");
        }

        private void NotesPerChordNumeric_ValueChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.NumChords, NotesPerChordNumeric.Value.ToString());
        }

        private void checkBoxCheckUpdates_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.CheckForUpdates, checkBoxCheckUpdates.Checked.ToString());
        }

        private void checkBoxMajorUpdates_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.MajorUpdates, checkBoxMajorUpdates.Checked.ToString());
        }

        private void checkBoxMinorUpdates_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.MinorUpdates, checkBoxMinorUpdates.Checked.ToString());
        }
    }
}
