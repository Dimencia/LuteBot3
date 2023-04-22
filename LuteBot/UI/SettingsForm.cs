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
        private LuteBotForm mainForm;

        public SettingsForm(LuteBotForm mainForm)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            SetVersion();
            InitSettings();
        }

        private void InitSettings()
        {
            TrackSelectionCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.TrackSelection);
            PartitionListCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.PartitionList);
            LowestNoteNumeric.Value = ConfigManager.GetIntegerProperty(PropertyItem.LowestNoteId);
            NoteCountNumeric.Value = ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount);
            NoteCooldownNumeric.Value = ConfigManager.GetIntegerProperty(PropertyItem.NoteCooldown);

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
        }

        private void InitInstruments()
        {
            Instrument.Read();
            instrumentsBox.DisplayMember = "Name";
            foreach (Instrument i in Instrument.Prefabs)
                instrumentsBox.Items.Add(i);
            instrumentsBox.SelectedIndex = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
        }

        private void SetVersion()
        {
            VersionLabel.Text = VersionLabel.Text.Replace("[VERSION]", ConfigManager.GetVersion());
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

        private void TrackSelectionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.TrackSelection, TrackSelectionCheckBox.Checked.ToString());
        }

        private void PartitionListCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.PartitionList, PartitionListCheckBox.Checked.ToString());
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

        private void InstrumentsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If it's already the instrument we have as our property
            // Then don't re-set the values
            int currentInstrument = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            if (currentInstrument != instrumentsBox.SelectedIndex)
            {
                ConfigManager.SetProperty(PropertyItem.Instrument, instrumentsBox.SelectedIndex.ToString());
                Instrument target = (Instrument)instrumentsBox.SelectedItem;

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
