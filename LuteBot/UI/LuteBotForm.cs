using Lutebot.UI;
using LuteBot.Config;
using LuteBot.Core;
using LuteBot.Core.Midi;
using LuteBot.IO.KB;
using LuteBot.LiveInput.Midi;
using LuteBot.OnlineSync;
using LuteBot.playlist;
using LuteBot.Soundboard;
using LuteBot.TrackSelection;
using LuteBot.UI;
using LuteBot.UI.Utils;
using LuteBot.Utils;
using Sanford.Multimedia.Midi;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot
{
    public partial class LuteBotForm : Form
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static readonly string libraryPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\LuteBot\GuildLibrary\";

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        static HotkeyManager hotkeyManager;

        TrackSelectionForm trackSelectionForm;
        OnlineSyncForm onlineSyncForm;
        SoundBoardForm soundBoardForm;
        public PlayListForm playListForm;
        LiveInputForm liveInputForm;
        TimeSyncForm timeSyncForm = null;

        Player player;


        string playButtonStartString = "Play";
        string playButtonStopString = "Pause";
        string musicNameLabelHeader = "Playing : ";
        bool playButtonIsPlaying = false;
        string currentTrackName = "";
        bool autoplay = false;
        bool isDonePlaying = false;

        public static PlayListManager playList;
        static SoundBoardManager soundBoardManager;
        public static TrackSelectionManager trackSelectionManager;
        static OnlineSyncManager onlineManager;
        static LiveMidiManager liveMidiManager;

        bool closing = false;

        public LuteBotForm()
        {
            InitializeComponent();

            onlineManager = new OnlineSyncManager();
            playList = new PlayListManager();
            trackSelectionManager = new TrackSelectionManager();
            playList.PlayListUpdatedEvent += new EventHandler<PlayListEventArgs>(HandlePlayListChanged);
            soundBoardManager = new SoundBoardManager();
            soundBoardManager.SoundBoardTrackRequest += new EventHandler<SoundBoardEventArgs>(HandleSoundBoardTrackRequest);
            player = new MidiPlayer(trackSelectionManager);
            player.SongLoaded += new EventHandler<AsyncCompletedEventArgs>(PlayerLoadCompleted);
            hotkeyManager = new HotkeyManager();
            hotkeyManager.NextKeyPressed += new EventHandler(NextButton_Click);
            hotkeyManager.PlayKeyPressed += new EventHandler(PlayButton_Click);
            hotkeyManager.SynchronizePressed += HotkeyManager_SynchronizePressed;
            hotkeyManager.PreviousKeyPressed += new EventHandler(PreviousButton_Click);
            trackSelectionManager.OutDeviceResetRequest += new EventHandler(ResetDevice);
            trackSelectionManager.ToggleTrackRequest += new EventHandler<TrackItem>(ToggleTrack);
            liveMidiManager = new LiveMidiManager(trackSelectionManager);
            hotkeyManager.LiveInputManager = liveMidiManager;

            PlayButton.Enabled = false;
            StopButton.Enabled = false;
            PreviousButton.Enabled = false;
            NextButton.Enabled = false;
            MusicProgressBar.Enabled = false;

            _hookID = SetHook(_proc);
            OpenDialogs();
            this.StartPosition = FormStartPosition.Manual;
            Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.MainWindowPos));
            Top = coords.Y;
            Left = coords.X;

            // We may package this with a guild library for now.  Check for it and extract it, if so
            var files = Directory.GetFiles(Environment.CurrentDirectory, "BGML*.zip", SearchOption.TopDirectoryOnly);
            if(files.Length > 0)
            {
                Task.Run(() =>
                {
                    // extract to libraryPath + "\songs\"
                    try
                    {
                        ZipFile.ExtractToDirectory(files[0], libraryPath + @"\songs\");
                        //File.Delete(files[0]);
                    }
                    catch (Exception e) { } // Gross I know, but no reason to do anything
                });
            }
        }

        private void HotkeyManager_SynchronizePressed(object sender, EventArgs e)
        {
            if(timeSyncForm != null)
            {
                timeSyncForm.StartAtNextInterval(10);
            }
        }

        private void PlayerLoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                StopButton_Click(null, null);
                PlayButton.Enabled = true;
                MusicProgressBar.Enabled = true;
                StopButton.Enabled = true;

                trackSelectionManager.UnloadTracks();
                if (player.GetType() == typeof(MidiPlayer))
                {
                    MidiPlayer midiPlayer = player as MidiPlayer;
                    trackSelectionManager.LoadTracks(midiPlayer.GetMidiChannels(), midiPlayer.GetMidiTracks());
                    trackSelectionManager.FileName = currentTrackName;
                }

                if (trackSelectionManager.autoLoadProfile)
                {
                    trackSelectionManager.LoadTrackManager();
                }

                MusicProgressBar.Value = 0;
                MusicProgressBar.Maximum = player.GetLenght();
                StartLabel.Text = TimeSpan.FromSeconds(0).ToString(@"mm\:ss");
                EndTimeLabel.Text = player.GetFormattedLength();
                CurrentMusicLabel.Text = musicNameLabelHeader + Path.GetFileNameWithoutExtension(currentTrackName);
                if (autoplay)
                {
                    Play();
                    autoplay = false;
                }
            }
            else
            {
                MessageBox.Show(e.Error.Message + " in " + e.Error.Source + e.Error.TargetSite + "\n" + e.Error.InnerException + "\n" + e.Error.StackTrace);
            }
        }

        private void ToggleTrack(object sender, TrackItem e)
        {
            timer1.Stop();
            (player as MidiPlayer).UpdateMutedTracks(e);
            timer1.Start();
        }

        private void ResetDevice(object sender, EventArgs e)
        {
            (player as MidiPlayer).ResetDevice();
        }

        private void LuteBotForm_Focus(object sender, EventArgs e)
        {
            if (trackSelectionForm != null && !trackSelectionForm.IsDisposed)
            {
                if (trackSelectionForm.WindowState == FormWindowState.Minimized)
                {
                    trackSelectionForm.WindowState = FormWindowState.Normal;
                }
                trackSelectionForm.Focus();
            }
            if (onlineSyncForm != null && !onlineSyncForm.IsDisposed)
            {
                if (onlineSyncForm.WindowState == FormWindowState.Minimized)
                {
                    onlineSyncForm.WindowState = FormWindowState.Normal;
                }
                onlineSyncForm.Focus();
            }
            if (soundBoardForm != null && !soundBoardForm.IsDisposed)
            {
                if (soundBoardForm.WindowState == FormWindowState.Minimized)
                {
                    soundBoardForm.WindowState = FormWindowState.Normal;
                }
                soundBoardForm.Focus();
            }
            if (playListForm != null && !playListForm.IsDisposed)
            {
                if (playListForm.WindowState == FormWindowState.Minimized)
                {
                    playListForm.WindowState = FormWindowState.Normal;
                }
                playListForm.Focus();
            }
            if (liveInputForm != null && !liveInputForm.IsDisposed)
            {
                if (liveInputForm.WindowState == FormWindowState.Minimized)
                {
                    liveInputForm.WindowState = FormWindowState.Normal;
                }
                liveInputForm.Focus();
            }
            this.Focus();
        }

        private void HandleSoundBoardTrackRequest(object sender, SoundBoardEventArgs e)
        {
            isDonePlaying = false;
            Pause();
            LoadHelper(e.SelectedTrack);
            autoplay = true;
        }

        private void HandlePlayListChanged(object sender, PlayListEventArgs e)
        {
            if (e.EventType == PlayListEventArgs.UpdatedComponent.UpdateNavButtons)
            {
                ToggleNavButtons(playList.HasNext());
            }
            if (e.EventType == PlayListEventArgs.UpdatedComponent.PlayRequest)
            {
                isDonePlaying = false;
                Pause();
                LoadHelper(playList.Get(e.Id));
                autoplay = true;
            }
        }

        private void ToggleNavButtons(bool enable)
        {
            PreviousButton.Enabled = enable;
            NextButton.Enabled = enable;
        }

        private void MusicProgressBar_Scroll(object sender, EventArgs e)
        {
            player.SetPosition(MusicProgressBar.Value);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (player.GetPosition() < MusicProgressBar.Maximum)
            {
                MusicProgressBar.Value = player.GetPosition();
                StartLabel.Text = player.GetFormattedPosition();
            }
            else
            {
                if (ActionManager.AutoConsoleModeFromString(ConfigManager.GetProperty(PropertyItem.ConsoleOpenMode)) == ActionManager.AutoConsoleMode.Old)
                {
                    ActionManager.ToggleConsole(false);
                }
                StartLabel.Text = EndTimeLabel.Text;
                PlayButton.Text = playButtonStartString;
                isDonePlaying = true;
                timer1.Stop();
                if (NextButton.Enabled)
                {
                    NextButton.PerformClick();
                }
            }
        }

        private void OpenDialogs()
        {
            if (ConfigManager.GetBooleanProperty(PropertyItem.SoundBoard))
            {
                soundBoardForm = new SoundBoardForm(soundBoardManager);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.SoundBoardPos));
                soundBoardForm.Show();
                soundBoardForm.Top = coords.Y;
                soundBoardForm.Left = coords.X;
            }
            if (ConfigManager.GetBooleanProperty(PropertyItem.PlayList))
            {
                playListForm = new PlayListForm(playList);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.PlayListPos));
                playListForm.Show();
                playListForm.Top = coords.Y;
                playListForm.Left = coords.X;

            }
            if (ConfigManager.GetBooleanProperty(PropertyItem.TrackSelection))
            {
                var midiPlayer = player as MidiPlayer;
                trackSelectionForm = new TrackSelectionForm(trackSelectionManager, midiPlayer.mordhauOutDevice, midiPlayer.rustOutDevice);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.TrackSelectionPos));
                trackSelectionForm.Show();
                trackSelectionForm.Top = coords.Y;
                trackSelectionForm.Left = coords.X;
            }
            if (ConfigManager.GetBooleanProperty(PropertyItem.LiveMidi))
            {
                liveInputForm = new LiveInputForm(liveMidiManager);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.LiveMidiPos));
                liveInputForm.Show();
                liveInputForm.Top = coords.Y;
                liveInputForm.Left = coords.X;
            }
        }

        protected override void WndProc(ref Message m)
        {
            hotkeyManager.HotkeyPressed(m.Msg);
            base.WndProc(ref m);
        }

        private void KeyBindingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new KeyBindingForm()).ShowDialog();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new SettingsForm(player as MidiPlayer)).ShowDialog();
            player.Pause();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            player.Dispose();
            WindowPositionUtils.UpdateBounds(PropertyItem.MainWindowPos, new Point() { X = Left, Y = Top });
            if (soundBoardForm != null)
            {
                soundBoardForm.Close();
            }
            if (playListForm != null)
            {
                playListForm.Close();
            }
            if (trackSelectionForm != null)
            {
                trackSelectionForm.Close();
            }
            if (liveInputForm != null)
            {
                liveInputForm.Close();
            }
            ConfigManager.SaveConfig();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnClosed(e);
        }

        private void LoadFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openMidiFileDialog = new OpenFileDialog();
            openMidiFileDialog.DefaultExt = "mid";
            openMidiFileDialog.Filter = "MIDI files|*.mid|All files|*.*";
            openMidiFileDialog.Title = "Open MIDI file";
            if (openMidiFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openMidiFileDialog.FileName;
                player.LoadFile(fileName);
                //if (fileName.Contains("\\"))
                //{
                //    string[] fileNameSplit = fileName.Split('\\');
                //    string filteredFileName = fileNameSplit[fileNameSplit.Length - 1].Replace(".mid", "");
                //    currentTrackName = filteredFileName;
                //}
                //else
                //{
                    currentTrackName = fileName;
                //}
            }
        }

        private void LoadHelper(PlayListItem item)
        {
            player.LoadFile(item.Path);
            currentTrackName = item.Name;
        }

        private void LoadHelper(SoundBoardItem item)
        {
            player.LoadFile(item.Path);
            currentTrackName = item.Name;
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            if (PlayButton.Enabled)
            {

                if (isDonePlaying)
                {
                    player.Stop();
                    player.Play();
                    playButtonIsPlaying = false;
                    isDonePlaying = false;
                }
                if (!playButtonIsPlaying)
                {
                    Play();
                }
                else
                {
                    Pause();
                }
            }
        }

        public void Play()
        {
            if (ActionManager.AutoConsoleModeFromString(ConfigManager.GetProperty(PropertyItem.ConsoleOpenMode)) == ActionManager.AutoConsoleMode.Old)
            {
                ActionManager.ToggleConsole(true);
            }
            PlayButton.Text = playButtonStopString;
            player.Play();
            
            timer1.Start();
            playButtonIsPlaying = true;
        }

        private void Pause()
        {
            if (ActionManager.AutoConsoleModeFromString(ConfigManager.GetProperty(PropertyItem.ConsoleOpenMode)) == ActionManager.AutoConsoleMode.Old)
            {
                ActionManager.ToggleConsole(false);
            }
            PlayButton.Text = playButtonStartString;
            player.Pause();
            timer1.Stop();
            playButtonIsPlaying = false;
        }

        private void PlayListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (playListForm == null || playListForm.IsDisposed)
            {
                playListForm = new PlayListForm(playList);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.PlayListPos));
                playListForm.Show();
                playListForm.Top = coords.Y;
                playListForm.Left = coords.X;
            }


        }

        private void SoundBoardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (soundBoardForm == null || soundBoardForm.IsDisposed)
            {
                soundBoardForm = new SoundBoardForm(soundBoardManager);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.SoundBoardPos));
                soundBoardForm.Show();
                soundBoardForm.Top = coords.Y;
                soundBoardForm.Left = coords.X;
            }

        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            player.Stop();
            timer1.Stop();
            MusicProgressBar.Value = 0;
            PlayButton.Enabled = false;
            MusicProgressBar.Enabled = false;
            StopButton.Enabled = false;
            StartLabel.Text = "00:00";
            EndTimeLabel.Text = "00:00";
            CurrentMusicLabel.Text = "";
            playButtonIsPlaying = false;
            PlayButton.Text = playButtonStartString;
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (NextButton.Enabled)
            {
                Pause();
                playList.Next();
                autoplay = true;
                LoadHelper(playList.Get(playList.CurrentTrackIndex));
                playButtonIsPlaying = true;
                isDonePlaying = false;
            }
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            if (PreviousButton.Enabled)
            {
                Pause();
                playList.Previous();
                autoplay = true;
                LoadHelper(playList.Get(playList.CurrentTrackIndex));
                playButtonIsPlaying = true;
                isDonePlaying = false;
            }
        }

        private void OnlineSyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            onlineSyncForm = new OnlineSyncForm(onlineManager);
            onlineSyncForm.Show();
        }

        private void TrackFilteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (trackSelectionForm == null || trackSelectionForm.IsDisposed)
            {
                var midiPlayer = player as MidiPlayer;
                trackSelectionForm = new TrackSelectionForm(trackSelectionManager, midiPlayer.mordhauOutDevice, midiPlayer.rustOutDevice);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.TrackSelectionPos));
                trackSelectionForm.Show();
                trackSelectionForm.Top = coords.Y;
                trackSelectionForm.Left = coords.X;
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                hotkeyManager.HotkeyPressed(vkCode);
                if (Enum.TryParse(vkCode.ToString(), out Keys tempkey))
                {
                    soundBoardManager.KeyPressed(tempkey);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void liveInputToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (liveInputForm == null || liveInputForm.IsDisposed)
            {
                liveInputForm = new LiveInputForm(liveMidiManager);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.LiveMidiPos));
                liveInputForm.Show();
                liveInputForm.Top = coords.Y;
                liveInputForm.Left = coords.X;
            }
        }

        private void GuildLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GuildLibraryForm guildLibraryForm;
            if (GuildLibrarySongList == null)
                guildLibraryForm = new GuildLibraryForm(this);
            else
                guildLibraryForm = new GuildLibraryForm(this,GuildLibrarySongList);
            guildLibraryForm.Show();
        }

        private static SortableBindingList<Song> GuildLibrarySongList = null;

        public static void SetGuildLibraryData(SortableBindingList<Song> songList)
        {
            GuildLibrarySongList = songList;
        }

        private void TimeSyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (timeSyncForm != null)
                timeSyncForm.Dispose();
            timeSyncForm = new TimeSyncForm(this);
                timeSyncForm.Show();
            
        }
    }
}
