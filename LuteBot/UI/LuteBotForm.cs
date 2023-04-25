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
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
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
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace LuteBot
{
    public partial class LuteBotForm : Form
    {

        public static readonly string lutebotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuteBot");
        public static readonly string libraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuteBot", "GuildLibrary");

        public TrackSelectionForm trackSelectionForm = null;
        public PartitionsForm partitionsForm = null;
        public GuildLibraryForm guildLibraryForm = null;

        MidiPlayer player;
        public static LuteBotForm luteBotForm;

        public string currentTrackName { get; set; } = "";

        public static TrackSelectionManager trackSelectionManager;

        private const string musicNameLabelHeader = "Loaded: ";
        private static string lutemodPakName = "FLuteMod_2.61.pak"; // TODO: Get this dynamically or something.  Really, get the file itself from github, but this will do for now
        private static int lutemodVersion1 = 2;
        private static int lutemodVersion2 = 6;
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
        private static string removeFromGame = @"[/Game/Mordhau/Maps/ClientModMap/BP_ClientModLoaderActor.BP_ClientModLoaderActor_C]
ClientMods=/Game/Mordhau/Maps/LuteMod/Client/BP_LuteModClientLoader.BP_LuteModClientLoader_C
ModListWidgetStayTime=5.0";

        private static string MordhauPakPath;

        public static LuteBotVersion LatestVersion = null;

        public LuteBotForm()
        {
            luteBotForm = this;
            InitializeComponent();


            MordhauPakPath = GetPakPath();

            trackSelectionManager = new TrackSelectionManager();
            player = new MidiPlayer(trackSelectionManager);

            this.Shown += LuteBotForm_Shown;

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
                ConfigManager.SetProperty(PropertyItem.NumChords, "6");
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

            try
            {
                // If it's not set, it's below 3.3; the rest is also there just for posterity so I can use it in the future
                if (!string.IsNullOrWhiteSpace(ConfigManager.GetProperty(PropertyItem.LastVersion)))
                {

                    // Parse the version into something we can compare
                    var firstGoodVersions = "3.6.0".Split('.');
                    var lastversions = ConfigManager.GetProperty(PropertyItem.LastVersion).Split('.');


                    for (int i = 0; i < firstGoodVersions.Length; i++)
                    {
                        // If we've run out of numbers to compare on either side and none have been less yet, or if any of the numbers are less, the version is below our target

                        // I think.  Target: 3.3 vs 3.31, we ran out of numbers but we're above.  So it depends in which direction
                        // If we run out of numbers in the target, it is not less
                        // Target: 3.31 vs 3.3, we run out of numbers in the lastversions, so it is less.  Good.  

                        if (i >= lastversions.Length || int.Parse(lastversions[i]) < int.Parse(firstGoodVersions[i]))
                        {
                            // I don't see how this could fail.  We know their last version and it had the prefab there
                            // I guess if they deleted all but one prefab.  
                            Instrument.Prefabs[1].ForbidsChords = false;
                            // The last version ran is below the target, and changes should be applied
                            Instrument.Write();

                            // We might should messagebox to let them know, but that'd be like a third messagebox for new installs, and that's just annoying at that point
                            InstallLuteMod(); // Update their lutemod
                        }
                        else if (int.Parse(lastversions[i]) > int.Parse(firstGoodVersions[i]))
                            break; // Their last version was higher than we need

                    }
                }
            }
            catch
            {
                MessageBox.Show("Could not perform version upgrade\nYou may need to use the option to Install Lutemod\nAnd may need to delete and re-generate your configuration", "Version upgrade failed");
            }
            ConfigManager.SetProperty(PropertyItem.LastVersion, ConfigManager.GetVersion());
            ConfigManager.SaveConfig();

            this.Text = "LuteBot v" + ConfigManager.GetVersion();

        }

        private async void LuteBotForm_Shown(object sender, EventArgs e)
        {
            await CheckUpdates(false).ConfigureAwait(false);
        }

        public async Task CheckUpdates(bool ignoreSettings = false)
        {
            if (ConfigManager.GetBooleanProperty(PropertyItem.CheckForUpdates))
            {
                // Try to update the version.  This is an async void by necessity, so errors will be dropped if we don't log them - but they get logged in there
                LatestVersion = await UpdateManager.GetLatestVersion().ConfigureAwait(false);
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
                                    else if (LatestVersion.VersionArray[i] < currentVersion[i])
                                        break;// Don't keep looking if we're already out of date
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
                                using (var installForm = new UI.PopupForm("Update LuteBot?", $"A new minor LuteBot version is available: v" + LatestVersion.Version,
                                    $"{LatestVersion.Title}\n\n{LatestVersion.Description}\n\n\n   Would you like to install it?",
                                    new Dictionary<string, string>() { { "Direct Download", LatestVersion.DownloadLink }, { "LuteBot Releases", "https://github.com/Dimencia/LuteBot3/releases" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } }
                                    , MessageBoxButtons.YesNoCancel, "Don't ask again for minor updates"))
                                {
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
                                        Process.Start(Path.Combine(updaterPath, "LuteBotUpdater.exe"), assemblyLocation);
                                        // And close
                                        Close();
                                    }
                                    else if (installForm.DialogResult == DialogResult.Cancel)
                                    {
                                        ConfigManager.SetProperty(PropertyItem.MinorUpdates, "False");
                                    }
                                }
                            }
                        }
                        else if (updateType == PropertyItem.MajorUpdates)
                        {
                            this.Text += $"    (Major Update Available: v{LatestVersion.Version})";
                            if (ignoreSettings || ConfigManager.GetBooleanProperty(PropertyItem.MajorUpdates))
                            {
                                using (var installForm = new UI.PopupForm("Update LuteBot?", $"A new major LuteBot version is available: v" + LatestVersion.Version,
                                    $"{LatestVersion.Title}\n\n{LatestVersion.Description}\n\n\n    Would you like to install it?", new Dictionary<string, string>() { { "Direct Download", LatestVersion.DownloadLink }, { "LuteBot Releases", "https://github.com/Dimencia/LuteBot3/releases" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } }
                                    , MessageBoxButtons.YesNoCancel, "Don't ask again for major updates"))
                                {
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
                    using (var popup = new UI.PopupForm("Version Check/Update Failed", $"Could not determine the latest LuteBot version",
                        $"You may want to manually check for an updated version at the following link\n\n{ex.Message}\n{ex.StackTrace}", new Dictionary<string, string>() { { "LuteBot Releases", "https://github.com/Dimencia/LuteBot3/releases" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } }))
                        popup.ShowDialog(this);
                }
            }
        }

        public bool IsLuteModInstalled()
        {
            var inputIniPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mordhau", "Saved", "Config", "WindowsClient", "Input.ini");
            if (File.Exists(inputIniPath) && string.IsNullOrEmpty(ConfigManager.GetProperty(PropertyItem.MordhauInputIniLocation)))
            {
                Config.ConfigManager.SetProperty(Config.PropertyItem.MordhauInputIniLocation, inputIniPath);
                Config.ConfigManager.SaveConfig();
            }
            else
                inputIniPath = ConfigManager.GetProperty(PropertyItem.MordhauInputIniLocation);
            // If the file exists, save it into config... this doesn't really go here... oh well.
            

            if (string.IsNullOrWhiteSpace(MordhauPakPath))
            {
                return true; // They have disabled installs or otherwise didn't input the path correctly, so don't check
            }

            if (!IsMordhauPakPathValid())
            {
                MordhauPakPath = GetMordhauPathFromPrompt();
                if (string.IsNullOrWhiteSpace(MordhauPakPath))
                    return true;
                return IsLuteModInstalled();
            }
            // Just check for the lutemod pak in CustomPaks, if they messed it up beyond that they can click the install button themselves, this is just to prompt them to install if necessary
            var pakPath = Path.Combine(MordhauPakPath, lutemodPakName);
            if (File.Exists(pakPath) && !string.IsNullOrWhiteSpace(inputIniPath))
            {
                string engineIniPath = "";
                try
                {
                    // Actually.  If they have it, we should check engine.ini for the bad line, and if it's there, recommend install
                    engineIniPath = Path.Combine(Path.GetDirectoryName(inputIniPath), "Engine.ini");

                    var content = File.ReadAllText(engineIniPath);
                    return !content.Contains(removeFromEngine);
                }
                catch (Exception e)
                {
                    using (var popup = new PopupForm("Mordhau Detection Failed", $"Could not access Engine.ini at {engineIniPath}", $"LuteBot will be unable to fix the LuteMod startup crash from old versions\nThis also indicates something is generally wrong.  You may want to run LuteBot as Administrator\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "LuteMod Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    )
                        popup.ShowDialog(this);
                    return false;
                }
            }
            else
            {
                try
                {
                    // Check if they have a similar version instead
                    Directory.CreateDirectory(MordhauPakPath); // Prevent crash if it doesn't exist
                    var curVers = Regex.Replace(lutemodPakName, "[^0-9]", "").Select(c => int.Parse(c.ToString())).ToArray();
                    foreach (var f in Directory.GetFiles(MordhauPakPath))
                    {
                        Match m = Regex.Match(Path.GetFileName(f), @"LuteMod_([0-9])\.([0-9])([0-9]*)");
                        if (m.Success)
                        {
                            var existingVers = Regex.Replace(Path.GetFileName(f), "[^0-9]", "").Select(c => int.Parse(c.ToString())).ToArray();
                            for(int i = 0; i < curVers.Length; i++)
                            {
                                if (existingVers.Length > i && existingVers[i] > curVers[i])
                                    return true;
                                if (existingVers.Length <= i || existingVers[i] < curVers[i])
                                {
                                    // This doesn't really go here but oh well; if they have an existing old version, force an update then return true
                                    InstallLuteMod();
                                    return true;
                                }
                            }
                            return true;
                            
                        }
                    }
                }
                catch (Exception e)
                {
                    using (var popup = new PopupForm("Mordhau Detection Failed", $"Could not access the Mordhau path at {MordhauPakPath}", $"If you wish to enable LuteMod installs, choose Settings -> Set Mordhau Path\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "LuteMod Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    )
                        popup.ShowDialog(this);
                    installLuteModToolStripMenuItem.Enabled = false;
                    return true;
                }
            }
            return false;
        }

        public void InstallLuteMod()
        {
            var mordhauProcesses = Process.GetProcessesByName("Mordhau");
            while (mordhauProcesses.Length > 0)
            {
                using (var popup = new PopupForm("Close Mordhau to Continue", $"Close Mordhau to Continue", $"LuteMod can't be installed while Mordhau is open"))
                {
                    popup.ShowDialog(this);
                    if (popup.DialogResult == DialogResult.OK)
                    {
                        mordhauProcesses = Process.GetProcessesByName("Mordhau");
                    }
                    else
                    {
                        return;
                    }
                }
            }

            // This may require admin access.  TODO: Detect if we need it and prompt them for it
            var pakPath = MordhauPakPath;
            if (!IsMordhauPakPathValid())
            { // Shouldn't really happen.  More likely is they have mordhau installed in more than one place and I pick the wrong one.  Might need to let them choose the location
                MordhauPakPath = GetMordhauPathFromPrompt();
                InstallLuteMod(); // Then try again after setting it
                return;
            }

            try
            {

                Directory.CreateDirectory(pakPath);

                string lutemodPakTarget = Path.Combine(pakPath, lutemodPakName);
                if (!File.Exists(lutemodPakTarget))
                    File.Copy(Path.Combine(Application.StartupPath, "lib", "LuteMod", lutemodPakName), lutemodPakTarget);


                string loaderPakTarget = Path.Combine(pakPath, loaderPakName);
                if (!File.Exists(loaderPakTarget))
                    File.Copy(Path.Combine(Application.StartupPath, "lib", "LuteMod", loaderPakName), loaderPakTarget);
            }
            catch (Exception e)
            {
                using (var popup = new PopupForm("Install Failed", $"Could not copy LuteMod files to {pakPath}", $"LuteBot may need to run as Administrator\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "Manual Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    )
                    popup.ShowDialog(this);
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
                    var name = Path.GetFileName(f); // TODO: Only do this if it's older than the current version
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
                using (var popup = new PopupForm("Could not remove old versions", $"Could not remove old versions of Paks at {pakPath}", $"Install will continue, but you may have conflicts if these files are not removed\n\nLuteBot may need to run as Administrator, or Mordhau may need to be closed\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "Manual Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    )
                    popup.ShowDialog(this);
            }


            string gameIniPath = Path.Combine(Path.GetDirectoryName(ConfigManager.GetProperty(PropertyItem.MordhauInputIniLocation)), "Game.ini");

            try
            {
                var content = File.ReadAllText(gameIniPath);

                // So if they already have other mods, and we add a second set of [/Autoloader/...LoaderActor_C], Mordhau combines them together after
                // So it ends up with just one heading, then multiple things below it
                // We need to handle this a bit more robustly and look for lines 1 and 2 of loaderstring1
                // If either doesn't exist, we write line 0, then the ones that don't exist

                var loaderLines = loaderString1.Replace("\r\n", "\n").Split('\n');
                bool replace1 = false;
                string loaderString1Modified = loaderLines[0] + "\n";

                if (!content.Contains(loaderLines[1]))
                {
                    replace1 = true;
                    loaderString1Modified += loaderLines[1] + "\n";
                }
                if (!content.Contains(loaderLines[2]))
                {
                    replace1 = true;
                    loaderString1Modified += loaderLines[2] + "\n";
                }

                if (content.Contains(removeFromGame))
                    content = content.Replace(removeFromGame, "\n");


                if (replace1)
                    content = content + "\n" + loaderString1Modified;
                if (!content.Contains(loaderString2))
                    content = content + "\n" + loaderString2;

                File.WriteAllText(gameIniPath, content);
            }
            catch (Exception e)
            {
                using (var popup = new PopupForm("Install Failed", $"Could not access Game.ini at {gameIniPath}", $"LuteBot may need to run as Administrator\nYou can set a custom path in the Key Bindings menu\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "Manual Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    )
                    popup.ShowDialog(this);
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
                using (var popup = new PopupForm("Crash Fix Failed", $"Could not access Engine.ini at {engineIniPath}", $"This is not fatal, and install will continue, but if you are experiencing startup crashes from an old version, they will not be fixed\n\nLuteBot may need to run as Administrator\nYou can set a custom path in the Key Bindings menu\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "Manual Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    )
                    popup.ShowDialog(this);
            }

            string partitionIndexTarget = Path.Combine(SaveManager.SaveFilePath, partitionIndexName);
            try
            {
                Directory.CreateDirectory(SaveManager.SaveFilePath); // Some people don't have one yet apparently
                                                                     // TODO: Testing on this.  Supposedly each user needs to generate their own empty PartitionIndex, but that may have just been some other bug that was fixed since?
                if (!File.Exists(partitionIndexTarget))
                    File.Copy(Path.Combine(Application.StartupPath, "lib", "LuteMod", partitionIndexName), partitionIndexTarget);
            }
            catch (Exception e)
            {
                using (var popup = new PopupForm("Could not create PartitionIndex", $"Could not copy to {partitionIndexTarget}", $"This is not fatal, and install will continue\n\nYou must initialize LuteMod yourself by pressing Kick with a Lute until the menu appears\n\n\n{e.Message}\n{e.StackTrace}", new Dictionary<string, string>() { { "Manual Install", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Install" }, { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" } })
                    )
                    popup.ShowDialog(this);
            }

            using (var popup = new PopupForm("Install Complete", "LuteMod Successfully Installed", "Use LuteBot to create Partitions out of your songs for LuteMod\n\nUse Kick in-game with a lute to open the LuteMod menu",
                new Dictionary<string, string>() {
                    { "Controls", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Controls" },
                    { "Adding Songs", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Adding_Songs" } ,
                    { "Playing Songs", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Playing_Songs" },
                    { "Flute and Duets", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Flute_and_Duets" },
                    { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" },
                }))
                popup.ShowDialog(this);
        }

        private string GetPakPath()
        {
            try
            {
                string originalPath = ConfigManager.GetProperty(PropertyItem.MordhauPakPath);
                if (IsMordhauPakPathValid(originalPath))
                {
                    return originalPath;
                }


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
            var epicDefaultPath = @"C:\Program Files\Epic Games\Mordhau\Mordhau\Content\CustomPaks";
            if (IsMordhauPakPathValid(epicDefaultPath))
                return epicDefaultPath;

            return GetMordhauPathFromPrompt();
        }

        private string GetMordhauPathFromPrompt()
        {
            var inputForm = new MordhauPathInputForm(MordhauPakPath);
            inputForm.ShowDialog(this);

            if (inputForm.result == DialogResult.OK && inputForm.path != string.Empty && IsMordhauPakPathValid(inputForm.path))
            {
                installLuteModToolStripMenuItem.Enabled = true;
                var result = Path.Combine(Path.GetDirectoryName(inputForm.path), "Mordhau", "Content", "CustomPaks");
                Directory.CreateDirectory(result);
                ConfigManager.SetProperty(PropertyItem.MordhauPakPath, result);
                installLuteModToolStripMenuItem.Enabled = true;
                return result;
            }
            else
            {
                if (!IsMordhauPakPathValid())
                    installLuteModToolStripMenuItem.Enabled = false;
                return MordhauPakPath;
            }
        }

        public static bool IsMordhauPakPathValid(string path = null)
        {
            path = path ?? MordhauPakPath;
            // See if the mordhau exe is where it should be
            if (string.IsNullOrWhiteSpace(path))
                return false;
            var exePath = Path.Combine(path, "..", "..", "..", "Mordhau.exe");

            System.IO.FileInfo fi = null;
            try
            {
                fi = new System.IO.FileInfo(exePath);
            }
            catch (ArgumentException) { }
            catch (System.IO.PathTooLongException) { }
            catch (NotSupportedException) { }
            if (ReferenceEquals(fi, null))
            {
                return false;
            }
            else
            {
                return fi.Exists;
            }
        }

        private async Task PlayerLoadCompleted(bool skipUI, bool reorderTracks = false)
        {
            SuspendLayout();

            trackSelectionManager.UnloadTracks();
            if (player.GetType() == typeof(MidiPlayer))
            {
                MidiPlayer midiPlayer = player as MidiPlayer;
                trackSelectionManager.LoadTracks(midiPlayer.GetMidiChannels(), midiPlayer.GetMidiTracks(), reorderTracks);
                trackSelectionManager.FileName = currentTrackName;
            }

            if (trackSelectionManager.autoLoadProfile)
            {
                trackSelectionManager.LoadTrackManager();
            }
            if (!skipUI)
            {
                await InvokeAsync(() =>
                {
                    if (trackSelectionForm != null && !trackSelectionForm.IsDisposed && trackSelectionForm.IsHandleCreated)
                    {
                        var tempoMap = player.dryWetFile.GetTempoMap();
                        var tempo = tempoMap.GetTempoAtTime(new MetricTimeSpan(1));
                        var secondsPerTick = ((float)tempo.MicrosecondsPerQuarterNote / ((TicksPerQuarterNoteTimeDivision)player.dryWetFile.TimeDivision).TicksPerQuarterNote) / 1000f;
                        // TickLength is pixels per tick
                        // I'd like to fit 30s into the window of about 700px
                        trackSelectionForm.tickLength = 30f / 700f * secondsPerTick;
                        trackSelectionForm.InitLists();
                    }
                }).ConfigureAwait(false);
            }
            ResumeLayout();
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

            this.Focus();
        }

        private void OpenDialogs()
        {
            trackSelectionForm = new TrackSelectionForm(trackSelectionManager, this);
            trackSelectionForm.StartPosition = FormStartPosition.Manual;
            trackSelectionForm.Location = new Point(0, 0);
            if (ConfigManager.GetBooleanProperty(PropertyItem.TrackSelection))
            {
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.TrackSelectionPos));
                trackSelectionForm.Show();
                trackSelectionForm.Top = coords.Y;
                trackSelectionForm.Left = coords.X;
            }

            partitionsForm = new PartitionsForm(trackSelectionManager, player);
            partitionsForm.TopLevel = false;
            partitionsForm.FormBorderStyle = FormBorderStyle.None;
            partitionsForm.Dock = DockStyle.Fill;
            partitionPanel.Controls.Add(partitionsForm);
            partitionsForm.Show();
        }


        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new SettingsForm(this)).ShowDialog(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            WindowPositionUtils.UpdateBounds(PropertyItem.MainWindowPos, new Point() { X = Left, Y = Top });

            if (trackSelectionForm != null)
            {
                trackSelectionForm.Close();
                //trackSelectionForm.Dispose(); // Uncomment this after I'm done messing everything else up
            }
            if (partitionsForm != null)
            {
                partitionsForm.Close();
                partitionsForm.Dispose();
            }
            ConfigManager.SaveConfig();
            base.OnClosing(e);
        }

        private async void LoadFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openMidiFileDialog = new OpenFileDialog();
            openMidiFileDialog.DefaultExt = "mid";
            openMidiFileDialog.Filter = "MIDI files|*.mid|All files|*.*";
            openMidiFileDialog.Title = "Open MIDI file";
            if (openMidiFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = openMidiFileDialog.FileName;
                await LoadFile(fileName).ConfigureAwait(false);
            }
        }

        public async Task HandleError(Exception ex, string message)
        {
            await InvokeAsync(() =>
            {
                splitContainer1.Panel2Collapsed = false;
                richTextBox1.AppendText($"{Environment.NewLine} [{DateTime.Now.ToString("T")}] Error: " + message);
                richTextBox1.AppendText($"{Environment.NewLine} {ex?.Message}");
                richTextBox1.ScrollToCaret();
            }).ConfigureAwait(false);
        }


        public async Task InvokeAsync(Action action)
        {
            if (InvokeRequired)
            {
                await Task.CompletedTask.ConfigureAwait(false);
                Invoke((MethodInvoker)delegate
                {
                    action();
                });
            }
            else
                action();
        }


        public async Task<bool> LoadFile(string fileName, bool skipUI = false, bool reorderTracks = false)
        {
            if (!skipUI)
                await InvokeAsync(() =>
                {
                    partitionsForm.savePartitionButton.Enabled = false;
                }).ConfigureAwait(false);
            try
            {
                await player.LoadFileAsync(fileName).ConfigureAwait(false);
                currentTrackName = fileName;
                await PlayerLoadCompleted(skipUI, reorderTracks).ConfigureAwait(false);
                return player.dryWetFile != null;
            }
            catch (Exception ex)
            {
                await HandleError(ex, "Failed to load file").ConfigureAwait(false);
            }
            finally
            {
                if (!skipUI)
                    await InvokeAsync(() =>
                    {
                        partitionsForm.savePartitionButton.Enabled = true;
                    }).ConfigureAwait(false);
            }
            return false;
        }

        private void TrackFilteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (trackSelectionForm == null || trackSelectionForm.IsDisposed)
            {
                var midiPlayer = player as MidiPlayer;
                trackSelectionForm = new TrackSelectionForm(trackSelectionManager, this);
                Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.TrackSelectionPos));
                trackSelectionForm.Top = coords.Y;
                trackSelectionForm.Left = coords.X;
            }
            trackSelectionForm.Show();
            trackSelectionForm.BringToFront();
            trackSelectionForm.Focus();
        }


        private void GuildLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (guildLibraryForm == null)
                guildLibraryForm = new GuildLibraryForm(this);
            guildLibraryForm.Show();
            guildLibraryForm.BringToFront();
            guildLibraryForm.Focus();
        }


        private async void ReloadButton_Click(object sender, EventArgs e)
        {
            // First grab the Track Filtering settings for our current track
            // Re-load the same midi track from file
            // And re-apply those settings

            // I don't think getting the settings is that easy but we'll try
            // Oh hey it can be.
            try
            {
                var instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
                var data = trackSelectionManager.GetTrackSelectionData(instrumentId);
                await LoadFile(currentTrackName).ConfigureAwait(false);
                trackSelectionManager.SetTrackSelectionData(data);
                //trackSelectionManager.SaveTrackManager(); // Don't save when we reload, that's bad.  
                if (trackSelectionForm != null && !trackSelectionForm.IsDisposed && trackSelectionForm.IsHandleCreated) // Everything I can think to check
                    trackSelectionForm.Invoke((MethodInvoker)delegate { trackSelectionForm.Invalidate(); trackSelectionForm.RefreshOffsetPanel(); }); // Invoking just in case this is on a diff thread somehow
                Refresh();
            }
            catch (Exception ex)
            {
                await HandleError(ex, "Failed to reload file");
            }
        }


        public void OnInstrumentChanged(int oldInstrument)
        {
            // This is called when an instrument is changed.  TrackSelectionManager should be updated with the new config values
            // Though first we'll have to setup the data to actually have different values based on the currently selected instrument.
            trackSelectionManager.UpdateTrackSelectionForInstrument(oldInstrument);
            // And TrackSelectionForm should be refreshed
            if (trackSelectionForm != null && !trackSelectionForm.IsDisposed && trackSelectionForm.IsHandleCreated) // Everything I can think to check
                trackSelectionForm.Invoke((MethodInvoker)delegate { trackSelectionForm.InitLists(); trackSelectionForm.RefreshOffsetPanel(); }); // Invoking just in case this is on a diff thread somehow

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
            using (var popup = new PopupForm("Help", "Useful Links and Info", "The Bard's Guild Wiki contains all information about LuteMod and LuteBot - and if it doesn't, you can add to it\n\nFurther troubleshooting is available in the #mordhau channel of the Bard's Guild Discord",
                new Dictionary<string, string>() {
                    { "What is LuteMod", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod" } ,
                    { "Controls", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Controls" },
                    { "Getting Songs", "https://mordhau-bards-guild.fandom.com/wiki/Getting_Songs" },
                    { "LuteBot Usage", "https://mordhau-bards-guild.fandom.com/wiki/LuteBot#Usage" },
                    { "LuteMod Usage", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Playing_Songs" },
                    { "Flute and Duets", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Flute_and_Duets" },
                    { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" },
                }))
                popup.ShowDialog(this);
        }

        private async void checkInstallUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await CheckUpdates(true); // .ConfigureAwait(false)...? Don't want to do it rn cuz I'm in the middle of a breaking refactor, don't want this breaking too
        }

        private void setMordhauPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MordhauPakPath = GetMordhauPathFromPrompt();
            ConfigManager.SetProperty(PropertyItem.MordhauPakPath, MordhauPakPath);
        }

        private void importPartitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            partitionsForm.importPartitions_Click(sender, e);
        }

        private void exportPartitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            partitionsForm.exportPartitionsToolStripMenuItem_Click(sender, e);
        }

        private void exportMidisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            partitionsForm.exportMidisToolStripMenuItem_Click(sender, e);
        }

        private void openSavFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            partitionsForm.openSaveFolder_Click(sender, e);
        }

        private void saveMultipleSongsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            partitionsForm.saveMultipleSongsToolStripMenuItem_Click(sender, e);
        }

        private void trainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            partitionsForm.trainToolStripMenuItem_Click(sender, e);
        }

        private void buttonConsoleToggle_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
        }

        private void openMidiFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            partitionsForm.openMidiFolderToolStripMenuItem_Click(sender, e);
        }

        new protected void Dispose()
        {
            partitionsForm?.Dispose();
            trackSelectionForm?.Dispose();
            guildLibraryForm?.Dispose();
            base.Dispose();
        }

        private async void withoutReorderingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await partitionsForm.reloadAll(false).ConfigureAwait(false);
        }

        private async void forceAIReorderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await partitionsForm.reloadAll(true).ConfigureAwait(false);
        }
    }
}
