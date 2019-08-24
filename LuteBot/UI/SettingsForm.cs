using LuteBot.Config;
using LuteBot.IO.KB;
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
        private static string VERSION;
        private static string THREAD_URL = "https://mordhau.com/forum/topic/13519/mordhau-lute-bot/";
        private string latestVersion;
        private int Timeout = 200;

        public SettingsForm()
        {
            InitializeComponent();
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
                latestVersionFetchThread = new Thread(() => DownloadUrlSynchronously(THREAD_URL));
                latestVersionFetchThread.Start();
                latestVersionFetchThread.Join(timeout);

                if (!latestVersionFetchThread.IsAlive)
                {
                    if (latestVersion.CompareTo(VERSION) <= 0)
                    {
                        UpdateLinkLabel.Text = "You have the latest version avaliable";
                        UpdateLinkLabel.Links.Clear();
                    }
                    else
                    {
                        UpdateLinkLabel.Text = "New version avaliable : Click here";
                        UpdateLinkLabel.Links.Clear();
                        UpdateLinkLabel.Links.Add(24, 33, THREAD_URL);
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
            catch (WebException ex)
            {
                UpdateLinkLabel.Text = "Couldn't retrieve version. Retry";
            }
            catch (ThreadInterruptedException ex)
            {
                UpdateLinkLabel.Text = "Couldn't retrieve version. Retry";
            }
        }

        public void DownloadUrlSynchronously(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string downloadString = client.DownloadString(THREAD_URL);
                    string pattern = @"Mordhau Lute Bot V\d\.(\d\d|\d)";
                    Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                    Match m = r.Match(downloadString);
                    if (m.Success)
                    {
                        latestVersion = m.ToString().Split('V')[1];
                    }
                }
            }
            catch (WebException ex)
            {
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
    }
}
