using Lutebot.UI;

using LuteBot.Config;
using LuteBot.Core;
using LuteBot.Core.Midi;
using LuteBot.IO.Files;
using LuteBot.IO.KB;
using LuteBot.LiveInput.Midi;
using LuteBot.OnlineSync;
using LuteBot.playlist;
using LuteBot.Soundboard;
using LuteBot.TrackSelection;
using LuteBot.UI;
using LuteBot.UI.Utils;
using LuteBot.Utils;

using Microsoft.Win32;

using Sanford.Multimedia.Midi;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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

        public static readonly string lutebotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuteBot");
        public static readonly string libraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuteBot", "GuildLibrary");

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

        TrackSelectionForm trackSelectionForm = null;
        OnlineSyncForm onlineSyncForm;
        SoundBoardForm soundBoardForm;
        public PlayListForm playListForm;
        LiveInputForm liveInputForm;
        TimeSyncForm timeSyncForm = null;
        PartitionsForm partitionsForm = null;

        MidiPlayer player;
        public static LuteBotForm luteBotForm;


        string playButtonStartString = "Play";
        string playButtonStopString = "Pause";
        string musicNameLabelHeader = "Playing : ";
        bool playButtonIsPlaying = false;
        public string currentTrackName { get; set; } = "";
        bool autoplay = false;
        bool isDonePlaying = false;

        public static PlayListManager playList;
        static SoundBoardManager soundBoardManager;
        public static TrackSelectionManager trackSelectionManager;
        static OnlineSyncManager onlineManager;
        static LiveMidiManager liveMidiManager;
        static KeyBindingForm keyBindingForm = null;

        private static string lutemodPakName = "FLuteMod_1.3.pak"; // TODO: Get this dynamically or something.  Really, get the file itself from github, but this will do for now
        private static string loaderPakName = "AutoLoaderWindowsClient.pak";
        private static string partitionIndexName = "PartitionIndex[0].sav";
        private static string loaderString1 = @"[/AutoLoader/BP_AutoLoaderActor.BP_AutoLoaderActor_C]
ClientMods=/Game/Mordhau/Maps/LuteMod/Client/BP_LuteModClientLoader.BP_LuteModClientLoader_C
ModListWidgetStayTime=5.0";
        private static string loaderString2 = @"[Mods]
ModStartupMap=/AutoLoader/ClientModNew_MainMenu.ClientModNew_MainMenu";
        private static string removeFromEngine = @"[/Script/EngineSettings.GameMapsSettings]
GameDefaultMap=/Game/Mordhau/Maps/ClientModMap/ClientMod_MainMenu.ClientMod_MainMenu";
        private static string removeFromPaks = "zz_clientmodloadingmap_425.pak";

        private static string MordhauPakPath = GetPakPath();

        public static LuteBotVersion LatestVersion = null;

        public LuteBotForm()
        {
            luteBotForm = this;
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
            hotkeyManager.StopKeyPressed += new EventHandler(StopButton_Click);
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

            this.Shown += LuteBotForm_Shown;


            _hookID = SetHook(_proc);

            SetConsoleKey(); // Sets up an appropriate path for the mordhau ini, and sets the key if necessary.  Only alerts if it changes something

            OpenDialogs();
            this.StartPosition = FormStartPosition.Manual;
            Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.MainWindowPos));
            Top = coords.Y;
            Left = coords.X; // If we have a proper saved pos, use it unaltered.  If not, we should adjust from screen center, which is what would be returned
            if (coords != ConfigManager.GetCoordsProperty(PropertyItem.MainWindowPos))
            {
                Top = coords.Y - Height;
                Left = coords.X - Width / 2;
                // If they weren't equal, it's at default pos, so the others should also be set to good default positions
                if (trackSelectionForm != null)
                {
                    // We should always CheckPosition, just in case something goes weird, so nothing every initializes out of bounds
                    var tsPos = WindowPositionUtils.CheckPosition(new Point(Left + Width, Top));
                    WindowPositionUtils.UpdateBounds(PropertyItem.TrackSelectionPos, tsPos);
                    trackSelectionForm.Location = tsPos;
                }
                if (partitionsForm != null)
                {
                    var pfPos = WindowPositionUtils.CheckPosition(new Point(Left - partitionsForm.Width, Top));
                    WindowPositionUtils.UpdateBounds(PropertyItem.PartitionListPos, pfPos);
                    partitionsForm.Location = pfPos;
                }
            }

            // We may package this with a guild library for now.  Check for it and extract it, if so
            var files = Directory.GetFiles(Environment.CurrentDirectory, "BGML*.zip", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                Task.Run(() =>
                {
                    // extract to libraryPath + "\songs\"
                    try
                    {
                        ZipFile.ExtractToDirectory(files[0], Path.Combine(libraryPath, "songs"));
                        //File.Delete(files[0]);
                    }
                    catch (Exception e) { } // Gross I know, but no reason to do anything
                });
            }


            // Try to catch issues with mismatches in configs
            try
            {
                int chords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
            }
            catch
            {
                ConfigManager.SetProperty(PropertyItem.NumChords, "3");
                ConfigManager.SaveConfig();
            }

            // Check for FirstRun, and offer the wiki link and auto-setup
            //if (ConfigManager.GetBooleanProperty(PropertyItem.FirstRun))
            //{
            //    ConfigManager.SetProperty(PropertyItem.FirstRun, "False");
            //    // TODO: A help window with... just the wiki link?
            //    // I guess also link to the Guild's discord and put some credits there, and link to various specific wiki topics
            //    helpToolStripMenuItem_Click(null, null);
            //}
            // I think I'm going to skip this; if it's a first run, they already get two popups that it set the console key, and then lutemod install
            // The lutemod install gives them links, so, they don't need this, but the var is there if I want it later.


            // Again, also removing the following; it's pointless, 3.3.0 was the first version that uses that config path, so it will never need to modify anything for versions below

            // If it's not set, it's below 3.3; the rest is also there just for posterity so I can use it in the future
            //if (!string.IsNullOrWhiteSpace(ConfigManager.GetProperty(PropertyItem.LastVersion)))
            //{
            //
            //    // Parse the version into something we can compare
            //    var firstGoodVersions = "3.3".Split('.');
            //    var lastversions = ConfigManager.GetProperty(PropertyItem.LastVersion).Split('.');
            //
            //
            //    for (int i = 0; i < firstGoodVersions.Length; i++)
            //    {
            //        // If we've run out of numbers to compare on either side and none have been less yet, or if any of the numbers are less, the version is below our target
            //
            //        // I think.  Target: 3.3 vs 3.31, we ran out of numbers but we're above.  So it depends in which direction
            //        // If we run out of numbers in the target, it is not less
            //        // Target: 3.31 vs 3.3, we run out of numbers in the lastversions, so it is less.  Good.  
            //
            //        if (i >= lastversions.Length || int.Parse(lastversions[i]) < int.Parse(firstGoodVersions[i]))
            //        {
            //            // The last version ran is below the target, and changes should be applied
            //            Instrument.WriteDefaults();
            //            // The new config data will pull from defaultConfig and should all be OK
            //
            //            // We might should messagebox to let them know, but that'd be like a third messagebox for new installs, and that's just annoying at that point
            //        }
            //    }
            //}
            //else
            //{
            //    Instrument.WriteDefaults();
            //}
            ConfigManager.SetProperty(PropertyItem.LastVersion, ConfigManager.GetVersion());

            this.Text = "LuteBot v" + ConfigManager.GetVersion();
        }

        private async void LuteBotForm_Shown(object sender, EventArgs e)
        {
            await CheckUpdates(false);
        }

        public async Task CheckUpdates(bool ignoreSettings = false)
        {
            if (ConfigManager.GetBooleanProperty(PropertyItem.CheckForUpdates))
            {
                // Try to update the version.  This is an async void by necessity, so errors will be dropped if we don't log them - but they get logged in there
                LatestVersion = await UpdateManager.GetLatestVersion();
                try
                {
                    if (LatestVersion != null && LatestVersion.VersionArray != null)
                    {
                        var currentVersion = UpdateManager.ConvertVersion(ConfigManager.GetVersion());
                        PropertyItem updateType = PropertyItem.None;
                        // Let's dynamically handle any lengths.  'Major' updates, for our purposes, are 0 or 1 (0 will almost never change)
                        for (int i = 0; i < Math.Max(LatestVersion.VersionArray.Length, currentVersion.Length); i++)
                        {
                            if (i < currentVersion.Length)
                            {
                                if (i < LatestVersion.VersionArray.Length)
                                {
                                    if (LatestVersion.VersionArray[i] > currentVersion[i])
                                    {
                                        if (i < 2)
                                            updateType = PropertyItem.MajorUpdates;
                                        else
                                            updateType = PropertyItem.MinorUpdates;
                                        break;
                                    }
                                }
                                else
                                {
                                    break; // Out of numbers in latest, theirs is ... newer...
                                }
                            }
                            else // We're out of numbers in currentVersion, LatestVersion is minorly newer
                            {
                                updateType = PropertyItem.MinorUpdates;
                                break;
                            }
                        }
                        // Now do what we want to do with this info
                        if (updateType == PropertyItem.MinorUpdates)
                        {
                            this.Text += $"    (Update Available: v{LatestVersion.Version})";
                            if (ignoreSettings || ConfigManager.GetBooleanProperty(PropertyItem.MinorUpdates))
                            {
                                var installForm = new UI.PopupForm("Update LuteBot?", $"A new minor LuteBot version is available: v" + LatestVersion.Version,
                                    $"{LatestVersion.Title}\n\n{LatestVersion.Description}\n\n\n   Would you like to install it?",
                                    new Dictionary<string, string>() { { "Direct Download", LatestVersion.DownloadLink }, { "LuteBot Releases", "https://github.com/Dimencia/LuteBot3/releases" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } }
                                    , MessageBoxButtons.YesNoCancel, "Don't ask again for minor updates");
                                installForm.ShowDialog(this);
                                if (installForm.DialogResult == DialogResult.Yes)
                                {
                                    string assemblyLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                                    string updaterPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuteBot", "Updater");
                                    if (Directory.Exists(updaterPath))
                                        Directory.Delete(updaterPath, true);
                                    Directory.CreateDirectory(updaterPath);
                                    File.Copy(Path.Combine(assemblyLocation, "LuteBotUpdater.exe"), Path.Combine(updaterPath, "LuteBotUpdater.exe"));
                                    //File.Copy(Path.Combine(assemblyLocation, "LuteBotUpdater.dll"), Path.Combine(updaterPath, "LuteBotUpdater.dll"));
                                    // Start a separate process to run the updater
                                    Process.Start(Path.Combine(updaterPath, "LuteBotUpdater.exe"), $"{assemblyLocation} {LatestVersion.DownloadLink}");
                                    // And close
                                    Close();
                                }
                                else if (installForm.DialogResult == DialogResult.Cancel)
                                {
                                    ConfigManager.SetProperty(PropertyItem.MinorUpdates, "False");
                                }
                            }
                        }
                        else if (updateType == PropertyItem.MajorUpdates)
                        {
                            this.Text += $"    (Major Update Available: v{LatestVersion.Version})";
                            if (ignoreSettings || ConfigManager.GetBooleanProperty(PropertyItem.MajorUpdates))
                            {
                                var installForm = new UI.PopupForm("Update LuteBot?", $"A new major LuteBot version is available: v" + LatestVersion.Version,
                                    $"{LatestVersion.Title}\n\n{LatestVersion.Description}\n\n\n    Would you like to install it?", new Dictionary<string, string>() { { "Direct Download", LatestVersion.DownloadLink }, { "LuteBot Releases", "https://github.com/Dimencia/LuteBot3/releases" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } }
                                    , MessageBoxButtons.YesNoCancel, "Don't ask again for major updates");
                                installForm.ShowDialog(this);
                                if (installForm.DialogResult == DialogResult.Yes)
                                {
                                    string assemblyLocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                                    string updaterPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuteBot", "Updater");
                                    if (Directory.Exists(updaterPath))
                                        Directory.Delete(updaterPath, true);
                                    Directory.CreateDirectory(updaterPath);
                                    File.Copy(Path.Combine(assemblyLocation, "LuteBotUpdater.exe"), Path.Combine(updaterPath, "LuteBotUpdater.exe"));
                                    //File.Copy(Path.Combine(assemblyLocation, "LuteBotUpdater.dll"), Path.Combine(updaterPath, "LuteBotUpdater.dll"));
                                    // Start a separate process to run the updater
                                    Process.Start(Path.Combine(updaterPath, "LuteBotUpdater.exe"), $"{assemblyLocation} {LatestVersion.DownloadLink}");
                                    // And close
                                    Close();
                                }
                                else if (installForm.DialogResult == DialogResult.Cancel)
                                {
                                    ConfigManager.SetProperty(PropertyItem.MajorUpdates, "False");
                                }
                            }
                        }
                        else if (ignoreSettings && updateType == PropertyItem.None)
                        {
                            //this.Text += $"    (Up To Date)"; // Nah.  Kinda dumb.  
                            // Show them a popup, though, if they explicitly ran it, telling them they're good
                            // Just a normal one is fine
                            MessageBox.Show("Your LuteBot is already the most recent version", "LuteBot Up To Date");
                        }
                    }
                }
                catch (Exception ex)
                {
                    new UI.PopupForm("Version Check/Update Failed", $"Could not determine the latest LuteBot version",
                        $"Please report this bug in our Discord\nThis is likely not a network issue, and something I did wrong in the code\n\nYou may want to manually check for an updated version at the following link\n\n{ex.Message}\n{ex.StackTrace}", new Dictionary<string, string>() { { "LuteBot Releases", "https://github.com/Dimencia/LuteBot3/releases" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                        .ShowDialog();
                }
            }
        }

        public static bool IsLuteModInstalled()
        {
            // Just check for the lutemod pak in CustomPaks, if they messed it up beyond that they can click the install button themselves, this is just to prompt them to install if necessary
            var pakPath = Path.Combine(MordhauPakPath, lutemodPakName);
            if (File.Exists(pakPath))
            {
                // Actually.  If they have it, we should check engine.ini for the bad line, and if it's there, recommend install
                string engineIniPath = Path.Combine(Path.GetDirectoryName(ConfigManager.GetProperty(PropertyItem.MordhauInputIniLocation)), "Engine.ini");

                try
                {
                    var content = File.ReadAllText(engineIniPath);
                    return !content.Contains(removeFromEngine);
                }
                catch (Exception e)
                {
                    new PopupForm("Mordhau Detection Failed", $"Could not access Engine.ini at {engineIniPath}", $"LuteBot will be unable to fix the LuteMod startup crash from old versions\nThis also indicates something is generally wrong.  You may want to run LuteBot as Administrator\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "LuteMod Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    .ShowDialog();
                    return false;
                }
            }
            else
                return false;
        }

        public static void InstallLuteMod()
        {
            // This may require admin access.  TODO: Detect if we need it and prompt them for it
            var pakPath = MordhauPakPath;
            if (string.IsNullOrWhiteSpace(pakPath))
            { // Shouldn't really happen.  More likely is they have mordhau installed in more than one place and I pick the wrong one.  Might need to let them choose the location

                new PopupForm("Install Failed", $"Could not find Steam path", "LuteMod auto install not available\nPlease install LuteMod manually using the following instructions:", new Dictionary<string, string>() { { "Manual Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    .ShowDialog();
                return;
            }

            try
            {

                Directory.CreateDirectory(pakPath);

                string lutemodPakTarget = Path.Combine(pakPath, lutemodPakName);
                if (!File.Exists(lutemodPakTarget))
                    File.Copy(Path.Combine(Application.StartupPath, "LuteMod", lutemodPakName), lutemodPakTarget);


                string loaderPakTarget = Path.Combine(pakPath, loaderPakName);
                if (!File.Exists(loaderPakTarget))
                    File.Copy(Path.Combine(Application.StartupPath, "LuteMod", loaderPakName), loaderPakTarget);
            }
            catch (Exception e)
            {
                new PopupForm("Install Failed", $"Could not copy LuteMod files to {pakPath}", $"LuteBot may need to run as Administrator\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "Manual Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    .ShowDialog();
                return;
            }

            try
            {
                // Remove the old autoloader pak if it exists, to avoid potential problems like double-loading
                string removePakTarget = Path.Combine(pakPath, ".." + Path.DirectorySeparatorChar, "Paks", removeFromPaks);
                if (File.Exists(removePakTarget))
                    File.Delete(removePakTarget);

                removePakTarget = Path.Combine(pakPath, removeFromPaks);
                if (File.Exists(removePakTarget))
                    File.Delete(removePakTarget);

                // Find all instances of old lutemod paks and remove them
                var files = Directory.GetFiles(pakPath);
                foreach (var f in files)
                {
                    var name = Path.GetFileName(f);
                    if (Regex.IsMatch(name.ToLower(), "^f?-?lutemod") && name != lutemodPakName)
                    {
                        File.Delete(f);
                    }
                }
                files = Directory.GetFiles(Path.Combine(pakPath, ".." + Path.DirectorySeparatorChar, "Paks"));
                foreach (var f in files)
                {
                    var name = Path.GetFileName(f);
                    if (Regex.IsMatch(name.ToLower(), "^f?-?lutemod") && name != lutemodPakName)
                    {
                        File.Delete(f);
                    }
                }
            }
            catch (Exception e)
            {
                new PopupForm("Could not remove old versions", $"Could not remove old versions of Paks at {pakPath}", $"Install will continue, but you may have conflicts if these files are not removed\n\nLuteBot may need to run as Administrator, or Mordhau may need to be closed\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "Manual Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    .ShowDialog();
            }


            string gameIniPath = Path.Combine(Path.GetDirectoryName(ConfigManager.GetProperty(PropertyItem.MordhauInputIniLocation)), "Game.ini");

            try
            {
                var content = File.ReadAllText(gameIniPath);


                if (!content.Contains(loaderString1))
                    content = content + "\n" + loaderString1;
                if (!content.Contains(loaderString2))
                    content = content + "\n" + loaderString2;

                File.WriteAllText(gameIniPath, content);
            }
            catch (Exception e)
            {
                new PopupForm("Install Failed", $"Could not access Game.ini at {gameIniPath}", $"LuteBot may need to run as Administrator\nYou can set a custom path in the Key Bindings menu\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "Manual Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    .ShowDialog();
                return;
            }

            // removeFromEngine
            string engineIniPath = Path.Combine(Path.GetDirectoryName(ConfigManager.GetProperty(PropertyItem.MordhauInputIniLocation)), "Engine.ini");

            try
            {
                var content = File.ReadAllText(engineIniPath);


                if (content.Contains(removeFromEngine)) // Prevent rewriting the file if we don't change anything
                {
                    content = content.Replace(removeFromEngine, "");
                    File.WriteAllText(engineIniPath, content);
                }
            }
            catch (Exception e)
            {
                new PopupForm("Crash Fix Failed", $"Could not access Engine.ini at {engineIniPath}", $"This is not fatal, and install will continue, but if you are experiencing startup crashes from an old version, they will not be fixed\n\nLuteBot may need to run as Administrator\nYou can set a custom path in the Key Bindings menu\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "Manual Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    .ShowDialog();
            }

            string partitionIndexTarget = Path.Combine(SaveManager.SaveFilePath, partitionIndexName);
            try
            {
                Directory.CreateDirectory(SaveManager.SaveFilePath); // Some people don't have one yet apparently
                                                                     // TODO: Testing on this.  Supposedly each user needs to generate their own empty PartitionIndex, but that may have just been some other bug that was fixed since?
                if (!File.Exists(partitionIndexTarget))
                    File.Copy(Path.Combine(Application.StartupPath, "LuteMod", partitionIndexName), partitionIndexTarget);
            }
            catch (Exception e)
            {
                new PopupForm("Could not create PartitionIndex", $"Could not copy to {partitionIndexTarget}", $"This is not fatal, and install will continue\n\nYou must initialize LuteMod yourself by pressing Kick with a Lute until the menu appears\n\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "Manual Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    .ShowDialog();
            }

            new PopupForm("Install Complete", "LuteMod Successfully Installed", "Use LuteBot to create Partitions out of your songs for LuteMod\n\nUse Kick in-game with a lute to open the LuteMod menu\n\nIf Mordhau is open, restart it",
                new Dictionary<string, string>() {
                    { "Adding Songs", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Adding_Songs" } ,
                    { "Playing Songs", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Playing_Songs" },
                    { "Flute and Duets", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Flute_and_Duets" },
                    { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" },
                })
                    .ShowDialog();
        }

        private static string GetPakPath()
        {
            try
            {
                string mordhauId = "629760";
                string steam32 = "SOFTWARE\\VALVE\\";
                string steam64 = "SOFTWARE\\Wow6432Node\\Valve\\";
                string steam32path;
                string steam64path;
                string config32path;
                string config64path;
                RegistryKey key32 = Registry.LocalMachine.OpenSubKey(steam32);
                RegistryKey key64 = Registry.LocalMachine.OpenSubKey(steam64);

                Regex pathReg = new Regex("\"path\"\\s*\"([^\"]*)\"[^}]*\"" + mordhauId + "\""); // Puts the path in group 1

                if (key64 != null)
                {
                    foreach (string k64subKey in key64.GetSubKeyNames())
                    {
                        // Annoying.  So.  Something in here makes it hit that exception - in the one instance I've seen, for a k64subKey "Spacewar"
                        // Which means, k64subKey isn't null.  We know key64 isn't null.  So how the hell?
                        // Oh it's probably the subKey.GetValue... but no, that should just, return null.  null.ToString is... wait...
                        // I guess that's probably it.  
                        try
                        {
                            using (RegistryKey subKey = key64.OpenSubKey(k64subKey))
                            {
                                if (subKey != null)
                                {
                                    var keyValue = subKey.GetValue("InstallPath");
                                    if (keyValue != null)
                                    {
                                        steam64path = keyValue.ToString();
                                        config64path = steam64path + "/steamapps/libraryfolders.vdf";
                                        if (File.Exists(config64path))
                                        {
                                            string config = File.ReadAllText(config64path);
                                            if (pathReg.IsMatch(config)) // Not sure if this is necessary... 
                                            {
                                                Match m = pathReg.Match(config);
                                                // Stop at the Content folder so other logic can detect and move/remove paks in the wrong folder?
                                                // Nah.  They're not hurting anything there. 
                                                return Path.Combine(m.Groups[1].Value, "steamapps", "common", "Mordhau", "Mordhau", "Content", "CustomPaks");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show($"Failed to get subKey for {k64subKey} for x64\n{e.Message}\n{e.StackTrace}");
                        } // Hopefully this never triggers, but if it does, it won't break anything (being inside the loop), and someone can tell me and I can fix it.  
                    }
                }

                if (key32 != null)
                {
                    foreach (string k32subKey in key32.GetSubKeyNames())
                    {
                        try
                        {
                            using (RegistryKey subKey = key32.OpenSubKey(k32subKey))
                            {
                                if (subKey != null)
                                {
                                    var keyValue = subKey.GetValue("InstallPath");
                                    if (keyValue != null)
                                    {
                                        steam32path = keyValue.ToString();
                                        config32path = steam32path + "/steamapps/libraryfolders.vdf";
                                        if (File.Exists(config32path))
                                        {
                                            string config = File.ReadAllText(config32path);
                                            if (pathReg.IsMatch(config)) // Not sure if this is necessary... 
                                            {
                                                Match m = pathReg.Match(config);
                                                return Path.Combine(m.Groups[1].Value, "steamapps", "common", "Mordhau", "Mordhau", "Content", "CustomPaks");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show($"Failed to get subKey {k32subKey} for x32\n{e.Message}\n{e.StackTrace}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"General failure... \n{e.Message}\n{e.StackTrace}");
            }

            return string.Empty;
        }

        // Are arrow keys valid?  These names are all good WinForms names, are they good Mordhau names?  
        private static string[] validConsoleKeys = new string[] { "PageDown", "PageUp", "Home", "End", "Insert", "Delete", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12" };

        // Quiet is to do it automatically and not alert if there were changes.  Clicking the button explicitly makes quiet false
        // forceMordhau is to force Mordhau to update to LuteBot and not the other way around; used when explicitly applying a new Console Key in LuteBot
        public static void SetConsoleKey(bool quiet = true, bool forceMordhau = false)
        {
            // Checks the contents of the config file for valid console keys
            // If any match the LuteBot setting, good
            // If any valid ones are found and none match, set the LuteBot setting to one of the valid ones
            // If no valid ones are found and no matches, add one
            string configLocation = ConfigManager.GetProperty(PropertyItem.MordhauInputIniLocation);
            string configContent = SaveManager.LoadMordhauConfig(configLocation);
            configLocation = ConfigManager.GetProperty(PropertyItem.MordhauInputIniLocation); // Loading it may have changed it, make sure we're updated
            string userKey = ConfigManager.GetProperty(PropertyItem.OpenConsole).Replace("Next", "PageDown"); // I think this is the only bad one
            string newBind = $"ConsoleKeys={userKey}";
            if (configContent != null)
            {
                if (!configContent.Contains(newBind))
                {
                    // The bind we have isn't set.  Check the ones that are set, and see if any are valid
                    // While also setting up to insert, if we need to
                    int index = -1;
                    int length = -1;
                    foreach (Match match in Regex.Matches(configContent, @"ConsoleKeys=(.*)"))
                    {
                        index = match.Index;
                        length = match.Length;
                        var detectedKey = match.Groups[1].Value.Trim();
                        if (!forceMordhau && validConsoleKeys.Contains(detectedKey))
                        {
                            // They have a valid key bound, it just doesn't match ours
                            // So, set ours to match.  Store the old one into UserSavedConsoleKey for reversion
                            ConfigManager.SetProperty(PropertyItem.UserSavedConsoleKey, ConfigManager.GetProperty(PropertyItem.OpenConsole));
                            ConfigManager.SetProperty(PropertyItem.OpenConsole, detectedKey);
                            ConfigManager.SaveConfig();
                            if (keyBindingForm != null)
                                keyBindingForm.InitPropertiesList();
                            MessageBox.Show($"Valid console key already bound in Mordhau: {detectedKey}\nLuteBot will use this console key", "Setup Complete");
                            return;
                        }
                    }

                    // If we get here, there were no valid keys bound.  So we insert the LuteBot key
                    // TODO: make sure VoteNo is unbound or we might just spam no when playing; we are potentially double-binding
                    if (index >= 0 && length > 0)
                    {
                        configContent = configContent.Insert((index + length), $"\n{newBind}");
                        SaveManager.SaveMordhauConfig(configLocation, configContent);
                        MessageBox.Show($"Successfully configured Mordhau Console Key to {userKey}\n\nIf Mordhau is open, you should restart it\nYou can revert this change in the Key Binding window", "Setup Complete");
                    }
                    else
                    {
                        MessageBox.Show("Could not find existing ConsoleKey binding in config to insert at\nYou will need to set the key yourself inside Mordhau Settings", "Setup Failed");
                    }
                }
                else
                {
                    // If it already has the bind, do we alert them?  I think we want to run this automatically on startup and on binding change, so, no if it's already set
                    // Unless we make this only explicit-run because maybe it could cause issues with localization?  But let's make it auto and if there are problems, fix them
                    if (!quiet)
                    {
                        MessageBox.Show($"Your console key is already correctly bound to {userKey} in Mordhau", "Setup Complete");
                    }
                }
            }
            else
            {
                MessageBox.Show("Could not retrieve mordhau config to update your Console Key\nYou will need to set the key yourself inside Mordhau Settings", "Setup Failed");
            }

        }

        public static void AutoConfigMordhau(object sender, EventArgs e)
        {
            SetConsoleKey(true);
        }

        private void HotkeyManager_SynchronizePressed(object sender, EventArgs e)
        {
            if (timeSyncForm != null)
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
                    trackSelectionManager.LoadTracks(midiPlayer.GetMidiChannels(), midiPlayer.GetMidiTracks(), trackSelectionManager);
                    trackSelectionManager.FileName = currentTrackName;
                }

                if (trackSelectionManager.autoLoadProfile)
                {
                    trackSelectionManager.LoadTrackManager();
                }

                MusicProgressBar.Value = 0;
                MusicProgressBar.Maximum = player.GetLength();
                StartLabel.Text = TimeSpan.FromSeconds(0).ToString(@"mm\:ss");
                EndTimeLabel.Text = player.GetFormattedLength();
                CurrentMusicLabel.Text = musicNameLabelHeader + Path.GetFileNameWithoutExtension(currentTrackName);
                if (autoplay)
                {
                    Play();
                    autoplay = false;
                }
                if (trackSelectionForm != null && !trackSelectionForm.IsDisposed && trackSelectionForm.IsHandleCreated)
                    trackSelectionForm.Invoke((MethodInvoker)delegate { trackSelectionForm.Invalidate(); trackSelectionForm.RefreshOffsetPanel(); }); // Invoking just in case this is on a diff thread somehow
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
                soundBoardForm.StartPosition = FormStartPosition.Manual;
                soundBoardForm.Location = new Point(0, 0);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.SoundBoardPos));
                soundBoardForm.Show();
                soundBoardForm.Top = coords.Y;
                soundBoardForm.Left = coords.X;
            }
            if (ConfigManager.GetBooleanProperty(PropertyItem.PlayList))
            {
                playListForm = new PlayListForm(playList);
                playListForm.StartPosition = FormStartPosition.Manual;
                playListForm.Location = new Point(0, 0);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.PlayListPos));
                playListForm.Show();
                playListForm.Top = coords.Y;
                playListForm.Left = coords.X;

            }
            if (ConfigManager.GetBooleanProperty(PropertyItem.TrackSelection))
            {
                var midiPlayer = player as MidiPlayer;
                trackSelectionForm = new TrackSelectionForm(trackSelectionManager, midiPlayer.mordhauOutDevice, midiPlayer.rustOutDevice, this);
                trackSelectionForm.StartPosition = FormStartPosition.Manual;
                trackSelectionForm.Location = new Point(0, 0);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.TrackSelectionPos));
                trackSelectionForm.Show();
                trackSelectionForm.Top = coords.Y;
                trackSelectionForm.Left = coords.X;
            }
            if (ConfigManager.GetBooleanProperty(PropertyItem.LiveMidi))
            {
                liveInputForm = new LiveInputForm(liveMidiManager);
                liveInputForm.StartPosition = FormStartPosition.Manual;
                liveInputForm.Location = new Point(0, 0);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.LiveMidiPos));
                liveInputForm.Show();
                liveInputForm.Top = coords.Y;
                liveInputForm.Left = coords.X;
            }
            if (ConfigManager.GetBooleanProperty(PropertyItem.PartitionList))
            {
                partitionsForm = new PartitionsForm(trackSelectionManager, player);
                partitionsForm.StartPosition = FormStartPosition.Manual;
                partitionsForm.Location = new Point(0, 0);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.PartitionListPos));
                partitionsForm.Show();
                partitionsForm.Top = coords.Y;
                partitionsForm.Left = coords.X;
            }
        }

        protected override void WndProc(ref Message m)
        {
            hotkeyManager.HotkeyPressed(m.Msg);
            base.WndProc(ref m);
        }

        private void KeyBindingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (keyBindingForm == null)
                keyBindingForm = new KeyBindingForm();
            keyBindingForm.InitPropertiesList();
            keyBindingForm.ShowDialog();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new SettingsForm(player as MidiPlayer, this)).ShowDialog();
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
            if (partitionsForm != null)
            {
                partitionsForm.Close();
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
            currentTrackName = item.Path;
        }

        private void LoadHelper(SoundBoardItem item)
        {
            player.LoadFile(item.Path);
            currentTrackName = item.Path;
        }

        public void LoadHelper(string path)
        {
            player.LoadFile(path);
            currentTrackName = path;
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
            //PlayButton.Enabled = false;
            //MusicProgressBar.Enabled = false;
            //StopButton.Enabled = false;
            StartLabel.Text = "00:00";
            //EndTimeLabel.Text = "00:00";
            //CurrentMusicLabel.Text = "";
            playButtonIsPlaying = false;
            PlayButton.Text = playButtonStartString;
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (NextButton.Enabled)
            {
                PlayButton.Enabled = false;
                StopButton.Enabled = false;
                MusicProgressBar.Enabled = false;
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
                PlayButton.Enabled = false;
                StopButton.Enabled = false;
                MusicProgressBar.Enabled = false;
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
                trackSelectionForm = new TrackSelectionForm(trackSelectionManager, midiPlayer.mordhauOutDevice, midiPlayer.rustOutDevice, this);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.TrackSelectionPos));
                trackSelectionForm.Top = coords.Y;
                trackSelectionForm.Left = coords.X;
            }
            trackSelectionForm.Show();
            trackSelectionForm.BringToFront();
            trackSelectionForm.Focus();
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
            GuildLibraryForm guildLibraryForm = new GuildLibraryForm(this);
            guildLibraryForm.Show();
            guildLibraryForm.BringToFront();
            guildLibraryForm.Focus();
        }


        private void TimeSyncToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (timeSyncForm != null)
                timeSyncForm.Dispose();
            timeSyncForm = new TimeSyncForm(this);
            timeSyncForm.Show();

        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            // First grab the Track Filtering settings for our current track
            // Re-load the same midi track from file
            // And re-apply those settings

            // I don't think getting the settings is that easy but we'll try
            // Oh hey it can be.
            var data = trackSelectionManager.GetTrackSelectionData();
            player.LoadFile(currentTrackName);
            trackSelectionManager.SetTrackSelectionData(data);
            //trackSelectionManager.SaveTrackManager(); // Don't save when we reload, that's bad.  
            if (trackSelectionForm != null && !trackSelectionForm.IsDisposed && trackSelectionForm.IsHandleCreated) // Everything I can think to check
                trackSelectionForm.Invoke((MethodInvoker)delegate { trackSelectionForm.Invalidate(); trackSelectionForm.RefreshOffsetPanel(); }); // Invoking just in case this is on a diff thread somehow
            Refresh();
        }


        public void OnInstrumentChanged(int oldInstrument)
        {
            // This is called when an instrument is changed.  TrackSelectionManager should be updated with the new config values
            // Though first we'll have to setup the data to actually have different values based on the currently selected instrument.
            trackSelectionManager.UpdateTrackSelectionForInstrument(oldInstrument);
            // MordhauOutDevice should be refreshed
            player.mordhauOutDevice.UpdateNoteIdBounds();
            // And TrackSelectionForm should be refreshed
            if (trackSelectionForm != null && !trackSelectionForm.IsDisposed && trackSelectionForm.IsHandleCreated) // Everything I can think to check
                trackSelectionForm.Invoke((MethodInvoker)delegate { trackSelectionForm.InitLists(); trackSelectionForm.RefreshOffsetPanel(); }); // Invoking just in case this is on a diff thread somehow

        }

        public enum Totebots
        {
            Default,
            Bass,
            Synth,
            Percussion
        }

        public enum TotebotTypes
        {
            Dance = 0,
            Retro = 1
        }
        // Totebot object IDs: 
        // Default: 1c04327f-1de4-4b06-92a8-2c9b40e491aa
        // Bass: 161786c1-1290-4817-8f8b-7f80de755a06
        // Synth: a052e116-f273-4d73-872c-924a97b86720
        // Perc: 4c6e27a2-4c35-4df3-9794-5e206fef9012
        private Dictionary<Totebots, string> TotebotIds = new Dictionary<Totebots, string>()
        {
            { Totebots.Default,  "1c04327f-1de4-4b06-92a8-2c9b40e491aa"},
            { Totebots.Bass,  "161786c1-1290-4817-8f8b-7f80de755a06" },
            { Totebots.Synth,  "a052e116-f273-4d73-872c-924a97b86720"},
            { Totebots.Percussion, "4c6e27a2-4c35-4df3-9794-5e206fef9012" }
        };

        private void exportToScrapMechanicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Should save a .json file with scrap mechanic info to play the song
            // This is going to be long and hard.


            // Pitch on the totebot heads is a value from 0.0 to 1.0, and covers two octaves (0-23)
            // 1 tick is 25ms

            // Make all the axes and positions match, except the switch, for a tiny version

            // So the way we handle this, if we construct a List<SMNote> of each note in order
            // We just pass it in to our function that we just wrote and get this whole thing out

            // We need to convert Midi ticks to game ticks, where a game tick is 25ms
            // Formula: ms = 60000 / (BPM * PPQ)
            // BPM and PPQ should be in the player somewhere - PPQ = Division, BPM = Tempo

            // Let's popup a settings window...

            ScrapMechanicConfigForm configForm = new ScrapMechanicConfigForm(player);
            var dialogResult = configForm.ShowDialog(this);
            if (dialogResult != DialogResult.OK)
                return;


            List<SMNote> onNotes = new List<SMNote>();
            List<SMNote> notes = new List<SMNote>();
            toteHeads = new List<ToteHead>();
            durationTimers = new List<SMTimer>();
            startTimers = new List<SMTimer>();
            extensionTimers = new List<SMTimer>();

            // Enforce loading of filtering values
            player.mordhauOutDevice.UpdateNoteIdBounds();
            int tempo = player.sequence.FirstTempo;

            if (!string.IsNullOrEmpty(currentTrackName))
                foreach (var track in player.sequence)
                {
                    foreach (var note in track.Iterator())
                    {
                        if (note.MidiMessage.MessageType == MessageType.Channel)
                        {
                            ChannelMessage cm = note.MidiMessage as ChannelMessage;
                            double msPerTick = tempo / player.sequence.Division; // tempo may change as we move along, watch for issues with getting this now

                            if (cm.Command == ChannelCommand.NoteOn && cm.Data2 > 0) // Velocity > 0
                            {
                                int gameTicksStart = (int)Math.Ceiling(note.AbsoluteTicks * msPerTick / 1000 / 25); // Hopefully we don't have to turn this into seconds also

                                // Temporarily...
                                //if (gameTicksStart > 800)
                                //    break;


                                var filtered = player.mordhauOutDevice.FilterNote(cm, 0);

                                var newNote = new SMNote()
                                {
                                    channel = cm.MidiChannel,
                                    midiEvent = note,
                                    startTicks = gameTicksStart,
                                    noteNum = filtered.Data1 - player.mordhauOutDevice.LowNoteId,
                                    velocity = filtered.Data2,
                                    flavor = (TotebotTypes)configForm.TrackTypeDict[filtered.MidiChannel],
                                    instrument = (Totebots)configForm.TrackCategoryDict[filtered.MidiChannel],
                                    internalId = onNotes.Count + notes.Count, // I don't think I use this, oh well
                                    filtered = filtered,
                                    durationTicks = -1
                                };

                                // And the duration... we need a NoteOff before we can know
                                onNotes.Add(newNote);
                                // But we need to add it now to preserve the order... 
                                // We'll just sort them afterward
                            }
                            else if (cm.Command == ChannelCommand.NoteOff || (cm.Command == ChannelCommand.NoteOn && cm.Data2 == 0))
                            {
                                var onNote = onNotes.Where(n => ((ChannelMessage)n.midiEvent.MidiMessage).MidiChannel == cm.MidiChannel && ((ChannelMessage)n.midiEvent.MidiMessage).Data1 == cm.Data1).FirstOrDefault();

                                // Same channel and note... must be us
                                if (onNote != null)
                                {
                                    int gameTicksDuration = (int)Math.Ceiling((note.AbsoluteTicks - onNote.midiEvent.AbsoluteTicks) * msPerTick / 1000 / 25);
                                    // Everything was a bit too fast, a ceil should help
                                    onNotes.Remove(onNote);
                                    if (onNote.filtered.Data2 > 0) // Still not muted
                                    {
                                        onNote.durationTicks = gameTicksDuration;
                                        notes.Add(onNote);
                                    }

                                }
                            }
                        }
                        else if (note.MidiMessage.MessageType == MessageType.Meta)
                        {
                            MetaMessage mm = note.MidiMessage as MetaMessage;
                            if (mm.MetaType == MetaType.Tempo)
                            {
                                // As for getting the actual Tempo out of it... 
                                var bytes = mm.GetBytes();
                                // Apparently it's... backwards?  Different endianness or whatever...
                                byte[] tempoBytes = new byte[4];
                                tempoBytes[2] = bytes[0];
                                tempoBytes[1] = bytes[1];
                                tempoBytes[0] = bytes[2];
                                tempo = BitConverter.ToInt32(tempoBytes, 0);
                            }
                        }
                    }
                }

            // Now sort notes list by the startTicks ... hopefully ascending
            notes.Sort(new Comparison<SMNote>((n, m) => n.startTicks - m.startTicks));
            // And make the files
            Guid guid = Guid.NewGuid();
            string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "SM Blueprints" + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(currentTrackName) + Path.DirectorySeparatorChar + guid + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(baseDir);


            using (StreamWriter writer = new StreamWriter(baseDir + Path.DirectorySeparatorChar + "blueprint.json"))
            {
                writer.Write(getSMBlueprint(notes));
            }
            using (StreamWriter writer = new StreamWriter(baseDir + "description.json"))
            {
                writer.WriteLine("{");
                writer.WriteLine("\"description\" : \"" + Path.GetFileNameWithoutExtension(currentTrackName) + " MIDI converted using LuteBot3\",");
                writer.WriteLine("\"localId\" : \"" + guid + "\",");
                writer.WriteLine("\"name\" : \"" + Path.GetFileNameWithoutExtension(currentTrackName) + "\",");
                writer.WriteLine("\"type\" : \"Blueprint\",");
                writer.WriteLine("\"version\" : 0");
                writer.WriteLine("}");
            }
            Process.Start(baseDir);
            if (!configForm.IsDisposed)
                configForm.Dispose();
        }

        public class SMNote
        {
            public int startTicks { get; set; }
            public int durationTicks { get; set; }
            public int noteNum { get; set; }
            public int channel { get; set; }
            public int velocity { get; set; } // IDK, not yet implemented but could be
            // Some velocity values seem to not work for some notes, it's weird.  Maybe if they're not a multiple of 5?
            public Totebots instrument { get; set; }
            public TotebotTypes flavor { get; set; }
            public ToteHead totehead { get; set; }
            public MidiEvent midiEvent { get; set; }
            public int internalId { get; set; }
            public ChannelMessage filtered { get; set; }
            public SMTimer startTimer { get; set; }
            public SMTimer durationTimer { get; set; }
        }

        public class SMTimer
        {
            private List<SMNote> attachedNotes = new List<SMNote>();
            public List<SMNote> AttachedNotes { get { return attachedNotes; } }
            private int durationTicks;
            public int DurationTicks
            {
                get { return durationTicks; }
                set {
                    if (value > 40) // 40 game ticks/second
                    {
                        DurationSeconds = value / 40;
                        durationTicks = value % 40;
                    }
                    else
                        durationTicks = value;
                }
            }
            public int DurationSeconds { get; set; } // Auto-sets from ticks
            public ToteHead Totehead { get; set; }
            public int InternalId { get; set; }
            public int Id { get; set; }
            private List<SMTimer> linkedTimers = new List<SMTimer>();
            public List<SMTimer> LinkedTimers { get { return linkedTimers; } }
            public int NorGateId { get; set; }
            public int AndGateId { get; set; }

        }

        public class ToteHead
        {
            public Totebots Instrument { get; set; }
            public TotebotTypes Flavor { get; set; }
            public int Id { get; set; }
            public int Note { get; set; }
            public int InternalId { get; set; }
            private List<SMNote> playingNotes = new List<SMNote>();
            public List<SMNote> PlayingNotes { get { return playingNotes; } }
            public int SetGateId { get; set; }
            public int ResetGateId { get; set; }
            public int OrGateId { get; set; }
        }

        public void SetToteHeadForNote(SMNote note)
        {
            var availableTotes = toteHeads.Where(t => t.Instrument == note.instrument && t.Note == note.noteNum && t.Flavor == note.flavor && (t.PlayingNotes.Count == 0 || t.PlayingNotes.All(pn => pn.startTicks + pn.durationTicks < note.startTicks - 1 || note.startTicks + note.durationTicks < pn.startTicks - 1)));
            //var availableTotes = new List<ToteHead>();
            ToteHead tote;
            if (availableTotes.Count() == 0)
            {
                // Make a new Tote and return it
                tote = new ToteHead() { Flavor = note.flavor, Instrument = note.instrument, Note = note.noteNum, Id = -1, InternalId = toteHeads.Count }; // We mark ID as unset yet
                toteHeads.Add(tote);
            }
            else
                tote = availableTotes.First();
            tote.PlayingNotes.Add(note);
            note.totehead = tote;

        }

        // Meant to be called after a tote is assigned of course
        public void SetDurationTimerForNote(SMNote note)
        {
            //var availableTimers = durationTimers.Where(t => t.Totehead.InternalId == note.totehead.InternalId && t.DurationTicks + t.DurationSeconds*40 == note.durationTicks && t.AttachedNotes.All(an => an.startTicks + an.durationTicks < note.startTicks - 3 || note.startTicks + note.durationTicks < an.startTicks - 3));
            var availableTimers = new List<SMTimer>(); // Always make a new timer, we can't re-use circuits
            SMTimer timer;
            if (availableTimers.Count() == 0)
            {
                timer = new SMTimer() { DurationTicks = note.durationTicks, Totehead = note.totehead, InternalId = durationTimers.Count };
                durationTimers.Add(timer);
            }
            else
                timer = availableTimers.First();

            note.durationTimer = timer;
            timer.AttachedNotes.Add(note);
            SetExtensionTimers(timer);
        }

        // Meant to be called after a tote is assigned of course
        public void SetStartTimerForNote(SMNote note)
        { // These will have to be the same note and everything...which is handled by being the same tote
            // This will basically never have anything in it except in very rare cases
            var availableTimers = startTimers.Where(t => t.Totehead.InternalId == note.totehead.InternalId && t.DurationTicks + t.DurationSeconds * 40 == note.startTicks && t.AttachedNotes.All(an => an.durationTicks == note.durationTicks));
            SMTimer timer;
            if (availableTimers.Count() == 0)
            {
                timer = new SMTimer() { DurationTicks = note.startTicks, Totehead = note.totehead, InternalId = startTimers.Count };
                startTimers.Add(timer);
            }
            else
                timer = availableTimers.First();
            note.startTimer = timer;
            timer.AttachedNotes.Add(note);
            SetExtensionTimers(timer);
        }

        public void SetExtensionTimers(SMTimer timer)
        {
            if (timer.DurationSeconds > 60 || (timer.DurationSeconds == 60 && timer.DurationTicks > 0))
            {
                int extensionNum = (timer.DurationSeconds / 60) - 1;
                SMTimer extension;
                if (extensionNum >= extensionTimers.Count)
                {
                    // Make a new extension timer, linked to by the previous one
                    // We assume we're only 1 above, if not, a lot of things are probably wrong everywhere - these have to go in order
                    extension = new SMTimer() { InternalId = extensionTimers.Count, DurationSeconds = 60 };
                    if (extensionNum > 0)
                        extensionTimers[extensionNum - 1].LinkedTimers.Add(extension);
                    extensionTimers.Add(extension);
                }
                else
                {
                    extension = extensionTimers[extensionNum];
                }
                // Link the existing extension timer to this one
                extension.LinkedTimers.Add(timer);
                timer.DurationSeconds = timer.DurationSeconds % 60;

            }
        }

        private List<ToteHead> toteHeads = new List<ToteHead>();
        private List<SMTimer> durationTimers = new List<SMTimer>();
        private List<SMTimer> startTimers = new List<SMTimer>();
        private List<SMTimer> extensionTimers = new List<SMTimer>();

        // Requires that you pass it a sorted list that happens in order
        private string getSMBlueprint(List<SMNote> notes)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"bodies\":[{\"childs\":[");

            int xAxis = -2;
            int zAxis = -1;
            int posX = 0;
            int posY = 0;
            int posZ = 0;

            List<List<int>> timerComponents = new List<List<int>>(); // has 1 list for each timer, containing all the Ids that it should connect to
            timerComponents.Add(new List<int>()); // List 0 is for our switch

            int id = 0; // We'll use and increment this everytime we place a part, easy enough

            foreach (var note in notes)
            {
                SetToteHeadForNote(note);
                SetStartTimerForNote(note);
                SetDurationTimerForNote(note);
            }

            // Okay... Our steps, probably in this order
            // Update/set all our notes' timers and such
            // Create each totebot-setup with the RS-latches for each totebot in our list
            // Create each timer in our durationTimer list, linking them to the appropriate totebot NOR gate
            // Create each timer in our startTimer list, linking them to the appropriate durationTimer
            // Create each timer in our extensionTimers list, linking them to the appropriate startTimers
            // Add a button cuz it's cooler than the switch and should work the same, we can't turn them off without adding more chips, linking to start timers that have no extension...
            // We'll do this by iterating notes in order until we reach ones that are too far, they're linked to the startTimers

            // In this order, we can get to the Ids after making them through our traversals
            bool first = true;
            foreach (var tote in toteHeads)
            {
                // We need: NOR gate1 linked to OR linked to NOR gate2 linked to OR linked to NOR gate1
                // Gate2 linked to totehead
                tote.Id = id++;
                tote.OrGateId = id++;

                if (first)
                {
                    first = false;
                }
                else
                    sb.Append(","); // So we don't have to worry about things missing
                // Logic OR - Links to totehead, other things link to this
                sb.Append("{\"color\":\"df7f01\",\"controller\":{\"active\":true,\"controllers\":[{\"id\":" + tote.Id + "}],\"id\":" + tote.OrGateId + ",\"joints\":null,\"mode\":1},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"9f0f56e8-2c31-4d83-996c-d00a9b296c3f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "}");

                // Totehead, no link
                var r = String.Format("{0:X2}", (int)(255 * (tote.Note / 24f))).ToLower();
                var color = r + r + r; // = ex "a197b9"
                sb.Append(",{\"color\":\"" + color + "\",\"controller\":{\"audioIndex\":" + (int)tote.Flavor + ",\"controllers\":null,\"id\":" + tote.Id + ",\"joints\":null,\"pitch\":" + (tote.Note / 24f) + ",\"volume\":50},\"pos\":{\"x\":" + (posX) + ",\"y\":" + (posY) + ",\"z\":" + (posZ) + "},\"shapeId\":\"" + TotebotIds[tote.Instrument] + "\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "}");
            }
            // Now create durationTimers, and link them to a Nor gate
            foreach (var timer in durationTimers)
            {
                timer.Id = id++;
                timer.NorGateId = id++;
                timer.AndGateId = id++;
                // Duration Timer, links to Nor gate
                sb.Append(",{\"color\":\"df7f01\",\"controller\":{\"active\":false,\"controllers\":[{\"id\":" + timer.NorGateId + "}],\"id\":" + timer.Id + ",\"joints\":null,\"seconds\":" + timer.DurationSeconds + ",\"ticks\":" + timer.DurationTicks + "},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"8f7fd0e7-c46e-4944-a414-7ce2437bb30f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "}");
                // Logic NOR - Links to and gate
                sb.Append(",{\"color\":\"df7f01\",\"controller\":{\"active\":true,\"controllers\":[{\"id\":" + timer.AndGateId + "}],\"id\":" + timer.NorGateId + ",\"joints\":null,\"mode\":4},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"9f0f56e8-2c31-4d83-996c-d00a9b296c3f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "}");
                // Logic AND - Links to OR of totebot
                sb.Append(",{\"color\":\"df7f01\",\"controller\":{\"active\":true,\"controllers\":[{\"id\":" + timer.Totehead.OrGateId + "}],\"id\":" + timer.AndGateId + ",\"joints\":null,\"mode\":0},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"9f0f56e8-2c31-4d83-996c-d00a9b296c3f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "}");
            }
            // Now create startTimers, and link them to the note's durationTimer.Id and the AND (stored in the note's durationTimer)
            foreach (var timer in startTimers)
            {
                timer.Id = id++;
                // Start Timer, links to durationTimer of each of its notes
                sb.Append(",{\"color\":\"df7f01\",\"controller\":{\"active\":false,\"controllers\":["); // An empty array here seems fine
                first = true;
                foreach (var note in timer.AttachedNotes)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(",");
                    sb.Append("{\"id\":" + note.durationTimer.Id + "},{\"id\":" + note.durationTimer.AndGateId + "}");
                }
                // Then finish the timer
                sb.Append("],\"id\":" + timer.Id + ",\"joints\":null,\"seconds\":" + timer.DurationSeconds + ",\"ticks\":" + timer.DurationTicks + "},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"8f7fd0e7-c46e-4944-a414-7ce2437bb30f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "}");
            }
            // Now create extensionTimers, and link them to their linkedTimers
            foreach (var timer in extensionTimers)
            {
                timer.Id = id++;
                sb.Append(",{\"color\":\"df7f01\",\"controller\":{\"active\":false,\"controllers\":["); // An empty array here seems fine
                first = true;
                foreach (var linkedTimer in timer.LinkedTimers)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(",");
                    sb.Append("{\"id\":" + linkedTimer.Id + "}");
                } // Then finish the timer
                sb.Append("],\"id\":" + timer.Id + ",\"joints\":null,\"seconds\":" + timer.DurationSeconds + ",\"ticks\":" + timer.DurationTicks + "},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"8f7fd0e7-c46e-4944-a414-7ce2437bb30f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "}");
            }
            // And add a switch, linking it to the first extenion and every note's startTimer that has duration 60 seconds or less

            sb.Append(",{\"color\":\"df7f01\",\"controller\":{\"active\":true,\"controllers\":[");
            List<int> addedIds = new List<int>();
            first = true;
            foreach (SMNote note in notes)
            {
                if (!addedIds.Contains(note.startTimer.Id) && (note.startTimer.DurationSeconds < 60 || (note.startTimer.DurationSeconds == 60 && note.startTimer.DurationTicks == 0)))
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(",");
                    addedIds.Add(note.startTimer.Id);
                    sb.Append("{\"id\":" + note.startTimer.Id + "}");
                }
                else // Should be in order, so break once we're past them
                    break;
            }
            // And to the first extensionTimer, if there are any
            if (extensionTimers.Count > 0)
                sb.Append(",{\"id\":" + extensionTimers[0].Id + "}");
            sb.Append("],\"id\":" + (id++) + ",\"joints\":null},\"pos\":{\"x\":0,\"y\":0,\"z\":1},\"shapeId\":\"7cf717d7-d167-4f2d-a6e7-6b2c70aa3986\",\"xaxis\":3,\"zaxis\":-1}");


            // And finish the json
            sb.Append("]}],\"version\":3}");
            return sb.ToString();
            /*







            for (int i = 0; i < notes.Count; i++)
            {
                SMNote note = notes[i];

                int startSeconds = 0;
                if (note.startTicks > 40) // 40 game ticks/second
                {
                    startSeconds = note.startTicks / 40;
                    note.startTicks = note.startTicks % 40;
                }

                startSeconds -= 60 * (timerComponents.Count - 1); // Remove 60 seconds for each timer we've already made

                if (startSeconds >= 60)
                {
                    timerComponents.Add(new List<int>());
                    startSeconds -= 60;
                    // We've hit the limit for how much our timers can delay
                    // Increase to the next timer and decrement a minute
                }

                // timerComponents should never be empty... 
                timerComponents[timerComponents.Count - 1].Add(i * 5);

                double pitch = note.noteNum / 24f; // 25 notes, we have 3 C's oddly enough... 
                                                   // Build our objects... 

                // Here's our base Totebot head, id i*5
                //sb.Append("{\"color\":\"49642d\",\"controller\":{\"audioIndex\":" + (int)note.flavor + ",\"controllers\":null,\"id\":" + (i * 5) + ",\"joints\":null,\"pitch\":" + pitch + ",\"volume\":100},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"" + TotebotIds[note.instrument] + "\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "},");




                





                // Logic (And) - Links to Totebot head, id i*5+1
                sb.Append("{\"color\":\"df7f01\",\"controller\":{\"active\":false,\"controllers\":[{\"id\":" + totehead.Id + "}],\"id\":" + (i * 5 + 1) + ",\"joints\":null,\"mode\":0},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"9f0f56e8-2c31-4d83-996c-d00a9b296c3f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "},");
                // Logic (Nor) - Links to Logic And, id i*5+2
                sb.Append("{\"color\":\"df7f01\",\"controller\":{\"active\":true,\"controllers\":[{\"id\":" + (i * 5 + 1) + "}],\"id\":" + (i * 5 + 2) + ",\"joints\":null,\"mode\":4},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"9f0f56e8-2c31-4d83-996c-d00a9b296c3f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "},");
                // Timer - How long to play current note - links to our Nor, id i*5+3
                sb.Append("{\"color\":\"df7f01\",\"controller\":{\"active\":false,\"controllers\":[{\"id\":" + (i * 5 + 2) + "}],\"id\":" + (i * 5 + 3) + ",\"joints\":null,\"seconds\":0,\"ticks\":" + note.durationTicks + "},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"8f7fd0e7-c46e-4944-a414-7ce2437bb30f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "},");
                // Timer - How long to delay before playing our note - links to our DurationTimer and our And, id i*5+4
                sb.Append("{\"color\":\"df7f01\",\"controller\":{\"active\":false,\"controllers\":[{\"id\":" + (i * 5 + 1) + "},{\"id\":" + (i * 5 + 3) + "}],\"id\":" + (i * 5 + 4) + ",\"joints\":null,\"seconds\":" + startSeconds + ",\"ticks\":" + note.startTicks + "},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"8f7fd0e7-c46e-4944-a414-7ce2437bb30f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "},");
            }
            // Slap a switch on at the end
            // And all the intermediate timers we need

            // Switch - links to every start Timer at i*5+4
            sb.Append("{\"color\":\"df7f01\",\"controller\":{\"active\":false,\"controllers\":[");
            for (int i = 0; i < timerComponents[0].Count; i++) // Guaranteed to run at least once
            {
                if (i != 0)
                    sb.Append(",");
                sb.Append("{\"id\":" + (timerComponents[0][i] + 4) + "}"); // timerComponents is our base, +4 is our start Timer that it links to
            }
            // TODO: Then also links to every And no matter what so it actually works
            for(int i = 0; i < notes.Count; i++)
            {
                // These are id i*5+1
                sb.Append(",{\"id\":" + (i * 5 + 1) + "}");
            }

            // Link it to only the first timer we're about to make, if any, id notes.Count*5+1
            if (timerComponents.Count > 1)
                sb.Append(",{\"id\":" + (notes.Count * 5 + 1) + "}");
            // Finish the switch
            sb.Append("],\"id\":" + (notes.Count * 5) + ",\"joints\":null},\"pos\":{\"x\":0,\"y\":0,\"z\":1},\"shapeId\":\"7cf717d7-d167-4f2d-a6e7-6b2c70aa3986\",\"xaxis\":3,\"zaxis\":-1}");

            // Make any timers
            for (int i = 1; i < timerComponents.Count; i++)
            {
                sb.Append(",{\"color\":\"df7f01\",\"controller\":{\"active\":false,\"controllers\":[");
                // Iterate the inner list
                bool first = true;
                foreach (int j in timerComponents[i])
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                        sb.Append(",");
                    sb.Append("{\"id\":" + (j + 4) + "}"); // Attach to the timer of the target
                }
                // If there's another timer, link to it
                if (i < timerComponents.Count - 1)
                    sb.Append(",{\"id\":" + (notes.Count * 5 + i + 1) + "}");
                // Finish the timer for 60 seconds
                sb.Append("],\"id\":" + (notes.Count * 5 + i) + ",\"joints\":null,\"seconds\":60,\"ticks\":0},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"8f7fd0e7-c46e-4944-a414-7ce2437bb30f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "}");
            }

            // Make the totebot heads and OR gates
            // Currently everything points to the totebot.Id, which we'll actually make the OR gates
            // And then heads themselves get Ids starting at notes.Count*5 + timerComponents.Count

            var random = new Random();
            for (int i = 0; i < toteHeads.Count; i++)
            {
                int headId = notes.Count * 5 + timerComponents.Count + i;
                var totehead = toteHeads[i];
                // Make an OR gate
                // Logic (Or) - Links to Totebot head
                sb.Append(",{\"color\":\"df7f01\",\"controller\":{\"active\":false,\"controllers\":[{\"id\":" + headId + "}],\"id\":" + totehead.Id + ",\"joints\":null,\"mode\":1},\"pos\":{\"x\":" + posX + ",\"y\":" + posY + ",\"z\":" + posZ + "},\"shapeId\":\"9f0f56e8-2c31-4d83-996c-d00a9b296c3f\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "}");

                // Make the head.  You can actually see the color poke through when it plays
                // So let's make the color black to white dependent on the pitch
                
                var color = String.Format("{0:X6}", 0x1000000 * totehead.Note / 24f).ToLower(); // = ex "a197b9"
                sb.Append(",{\"color\":\"" + color + "\",\"controller\":{\"audioIndex\":" + (int)totehead.Flavor + ",\"controllers\":null,\"id\":" + headId + ",\"joints\":null,\"pitch\":" + (totehead.Note / 24f) + ",\"volume\":50},\"pos\":{\"x\":" + posX + ",\"y\":" + (posY) + ",\"z\":" + (posZ) + "},\"shapeId\":\"" + TotebotIds[totehead.Instrument] + "\",\"xaxis\":" + xAxis + ",\"zaxis\":" + zAxis + "}");
            }


            sb.Append("]}],\"version\":3}");
            return sb.ToString();
            */
        }

        private void authToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var authForm = new DiscordAuthForm();
            authForm.Show(this);
        }

        private void lutemodPartitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (partitionsForm == null || partitionsForm.IsDisposed)
                partitionsForm = new PartitionsForm(trackSelectionManager, player);
            partitionsForm.Show();
            partitionsForm.BringToFront();
            partitionsForm.Focus();
        }

        private void adjustLutemodPartitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var adjustForm = new PartitionAdjustmentForm();
            adjustForm.Show();
        }

        private void installLuteModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InstallLuteMod();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var popup = new PopupForm("Help", "Useful Links and Info", "The Bard's Guild Wiki contains all information about LuteMod and LuteBot - and if it doesn't, you can add to it\n\nFurther troubleshooting is available in the #mordhau channel of the Bard's Guild Discord",
                new Dictionary<string, string>() {
                    { "What is LuteMod", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod" } ,
                    { "Getting Songs", "https://mordhau-bards-guild.fandom.com/wiki/Getting_Songs" },
                    { "LuteBot Usage", "https://mordhau-bards-guild.fandom.com/wiki/LuteBot#Usage" },
                    { "LuteMod Usage", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Playing_Songs" },
                    { "Flute and Duets", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Flute_and_Duets" },
                    { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" },
                });
            popup.ShowDialog();
        }

        private async void checkInstallUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await CheckUpdates(true);
        }
    }
}
