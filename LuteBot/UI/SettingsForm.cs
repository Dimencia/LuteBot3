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
        private readonly string versionAvaliable = "A new version is avaliable to download";
        private static string VERSION { get; set; }
        private static string VERSION_FILE_URL = "https://raw.githubusercontent.com/Dimencia/LuteBot3/master/Version.txt";
        private static string THREAD_URL = "https://github.com/Dimencia/LuteBot3/releases";
        private static string GUILD_URL = "https://discord.gg/4xnJVuz";
        private string latestVersion;
        private int Timeout = 200;
        private MidiPlayer player;

        public SettingsForm(MidiPlayer player)
        {
            InitializeComponent();
            this.player = player;
            UpdateLinkLabel.LinkArea = new LinkArea() { Length = 0, Start = 0 };
            SetVersion();
            InitSettings();
            CheckLatestVersion(Timeout);
        }

        private void CheckLatestVersion(int timeout)
        {
            try
            {
                Thread latestVersionFetchThread;
                latestVersionFetchThread = new Thread(() => DownloadUrlSynchronously(VERSION_FILE_URL));
                latestVersionFetchThread.Start();
                latestVersionFetchThread.Join(timeout);

                if (!latestVersionFetchThread.IsAlive)
                {
                    try
                    {
                        var onlineVersion = latestVersion.Split('.').Select(s => int.Parse(s)).ToArray();
                        var curVersion = VERSION.Split('.').Select(s => int.Parse(s)).ToArray();

                        if ((onlineVersion[0] > curVersion[0]) || (onlineVersion[0] == curVersion[0] && onlineVersion[1] > curVersion[1]) || (onlineVersion[0] == curVersion[0] && onlineVersion[1] == curVersion[1] && onlineVersion[2] > curVersion[2]))
                        {
                            UpdateLinkLabel.Text = "New version avaliable : Click here";
                            UpdateLinkLabel.Links.Clear();
                            UpdateLinkLabel.Links.Add(24, 33, THREAD_URL);
                        }
                        else
                        {
                            UpdateLinkLabel.Text = "You have the latest version avaliable";
                            UpdateLinkLabel.Links.Clear();
                        }
                    }
                    catch
                    {
                        UpdateLinkLabel.Text = "Couldn't retrieve version. Retry";
                        UpdateLinkLabel.Links.Clear();
                        UpdateLinkLabel.Links.Add(27, 31, THREAD_URL);
                    }
                }
                else
                {
                    UpdateLinkLabel.Text = "Couldn't retrieve version. Retry";
                    UpdateLinkLabel.Links.Clear();
                    UpdateLinkLabel.Links.Add(27, 31, THREAD_URL);
                    latestVersionFetchThread.Abort();
                }
            }
            catch
            {
                UpdateLinkLabel.Text = "Couldn't retrieve version. Retry";
            }
        }

        public void DownloadUrlSynchronously(string url)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // Didn't I already do this?
                using (WebClient client = new WebClient())
                {
                    //var data = client.DownloadData(url);
                    //string downloadString = UTF8Encoding.UTF8.GetString(data);
                    string downloadString = client.DownloadString(url);
                    latestVersion = downloadString.Trim();
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void UpdateLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel.Link Link = UpdateLinkLabel.Links[UpdateLinkLabel.Links.IndexOf(e.Link)];
            if (Link.Start == 27)
            {
                if (Timeout < 3000)
                {

                }
                CheckLatestVersion(Timeout + 1000);
            }
            else
            {
                Link.Visited = true;
                System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
            }
        }



        private void InitSettings()
        {
            SoundBoardCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.SoundBoard);
            PlaylistCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.PlayList);
            TrackSelectionCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.TrackSelection);
            OnlineSyncCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.OnlineSync);
            SoundEffectsCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.SoundEffects);

            InitRadioButtons();

            NoteConversionMode.SelectedIndex = ConfigManager.GetIntegerProperty(PropertyItem.NoteConversionMode);
            LowestNoteNumeric.Value = ConfigManager.GetIntegerProperty(PropertyItem.LowestNoteId);
            NoteCountNumeric.Value = ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount);
            NoteCooldownNumeric.Value = ConfigManager.GetIntegerProperty(PropertyItem.NoteCooldown);
            LiveMidiCheckBox.Checked = ConfigManager.GetBooleanProperty(PropertyItem.LiveMidi);

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

            outputDeviceBox.SelectedIndex = ConfigManager.GetIntegerProperty(PropertyItem.OutputDevice);
        }

        private void SetVersion()
        {
            VersionLabel.Text = VersionLabel.Text.Replace("[VERSION]", ConfigManager.GetVersion());
            VERSION = ConfigManager.GetVersion();
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

        private void OnlineSyncCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.OnlineSync, OnlineSyncCheckBox.Checked.ToString());
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
    }
}
