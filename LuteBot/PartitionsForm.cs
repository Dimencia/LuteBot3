using LuteBot.Config;
using LuteBot.Core.Midi;
using LuteBot.IO.Files;
using LuteBot.TrackSelection;
using LuteBot.UI;
using LuteBot.UI.Utils;

using LuteMod.Indexing;
using LuteMod.Sequencing;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleML;
using static System.Resources.ResXFileRef;
using System.Xml.Linq;
using System.IO.Compression;
using System.Diagnostics;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using Melanchall.DryWetMidi.MusicTheory;
using System.Security.Cryptography;

namespace LuteBot
{
    public partial class PartitionsForm : Form
    {
        public PartitionsForm(TrackSelectionManager tsm, MidiPlayer player)
        {
            this.tsm = tsm;
            this.player = player;
            InitializeComponent();

            this.FormClosing += PartitionsForm_FormClosing1;
            listBoxPartitions.MouseMove += ListBoxPartitions_MouseMove;
            this.DragEnter += PartitionsForm_DragEnter;

            editSelectedButton.Enabled = false;
            reloadSelectedButton.Enabled = false;
            exportSelectedButton.Enabled = false;
            renameSelectedButton.Enabled = false;

            try
            {
                if (!LuteBotForm.luteBotForm.IsLuteModInstalled())
                {
                    using (var popup = new PopupForm("Install LuteMod", "Would you like to update/install LuteMod?", "You need this to play music.\n\nIf you already have a working LuteMod installed, this means there's an important update\n\nThanks to Monty for LuteMod, and cswic for the autoloader\n\nFor more information, see:",
                    new Dictionary<string, string>() {
                    { "What is LuteMod", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod" } ,
                    { "LuteMod mod.io page", "https://mordhau.mod.io/lutemod" },
                    { "Autoloader mod.io page", "https://mordhau.mod.io/clientside-mod-autoloader" },
                    { "Flute and Duets", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Flute_and_Duets" },
                    { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" },
                    }, MessageBoxButtons.YesNo))
                    {
                        popup.ShowDialog(this);
                        if (popup.DialogResult == DialogResult.Yes)
                            LuteBotForm.luteBotForm.InstallLuteMod();
                        else
                            Hide();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            RefreshPartitionList();
        }

        private void PartitionsForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        public static readonly string partitionMidiPath = Path.Combine(LuteBotForm.lutebotPath, "Partition MIDIs");

        private void ListBoxPartitions_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && listBoxPartitions.SelectedItems.Count > 0)
            {
                int mouseIndex = listBoxPartitions.IndexFromPoint(e.Location);
                if (mouseIndex > -1)
                {
                    ListBox.SelectedObjectCollection x = new ListBox.SelectedObjectCollection(listBoxPartitions);
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        int i1 = Math.Min(listBoxPartitions.SelectedIndex, mouseIndex);
                        int i2 = Math.Max(listBoxPartitions.SelectedIndex, mouseIndex);
                        for (int i = i1; i <= i2; ++i)
                        {
                            x.Add(listBoxPartitions.Items[i]);
                        }
                    }
                    else
                    {
                        x = listBoxPartitions.SelectedItems;
                    }
                    var dropResult = DoDragDrop(x, DragDropEffects.Move);
                }
            }
        }



        private void PartitionsForm_FormClosing1(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private MidiPlayer player;
        public PartitionIndex index { get; private set; }
        private TrackSelectionManager tsm;

        private void RefreshPartitionList()
        {
            index = new PartitionIndex();
            index.LoadIndex();
            if (!index.Loaded)
            {
                MessageBox.Show("No partition index found.  If LuteMod is installed, you can't add songs until you start Mordhau, go into a game, and kick with a Lute until the menu opens\n\nOr choose Settings -> Install LuteMod, which now includes the partition file, and will update an existing install if necessary");
                Hide();
            }

            PopulateIndexList();
        }

        public async Task PopulateIndexListAsync()
        {
            await LuteBotForm.luteBotForm.InvokeAsync(() =>
            {
                LuteBotForm.luteBotForm.partitionsForm.PopulateIndexList();
            }).ConfigureAwait(false);
        }

        public void PopulateIndexList()
        {
            listBoxPartitions.Items.Clear();
            foreach (string item in index.PartitionNames)
            {
                listBoxPartitions.Items.Add(item);
            }
        }

        private void ContextMenuHelper()
        {
            if (listBoxPartitions.Items.Count > 0 && listBoxPartitions.SelectedIndex >= 0)
            {
                ContextMenu indexContextMenu = new ContextMenu();

                string name = (string)listBoxPartitions.SelectedItems[0];
                if (listBoxPartitions.SelectedItems.Count > 1)
                    name += " + ...";


                if (listBoxPartitions.SelectedIndices.Count == 1)
                {
                    //string midiPath = Path.Combine(partitionMidiPath, listBoxPartitions.SelectedItems[0] + ".mid");
                    //if (File.Exists(midiPath))
                    //{
                    MenuItem editItem = indexContextMenu.MenuItems.Add("Edit " + name);
                    editItem.Click += EditItem_Click;
                    //}
                }
                else
                {
                    MenuItem exportItem = indexContextMenu.MenuItems.Add("Export " + name);
                    exportItem.Click += ExportItem_Click;
                }

                MenuItem deleteItem = indexContextMenu.MenuItems.Add("Delete " + name);
                deleteItem.Click += new EventHandler(DeleteItem_Click);

                listBoxPartitions.ContextMenu = indexContextMenu; // TODO: I'd love to make it popup at the selected item, not at mouse pos, but whatever
                indexContextMenu.Show(listBoxPartitions, listBoxPartitions.PointToClient(Cursor.Position));
            }
            else
            {
                listBoxPartitions.ContextMenu = null;
            }
        }

        private void ExportItem_Click(object sender, EventArgs e)
        {
            ExportMidis(listBoxPartitions.SelectedItems.Cast<string>());
        }

        private async void EditItem_Click(object sender, EventArgs e)
        {
            string midiPath = Path.Combine(partitionMidiPath, listBoxPartitions.SelectedItems[0] + ".mid");
            if (File.Exists(midiPath))
                await LuteBotForm.luteBotForm.LoadFile(midiPath).ConfigureAwait(false);
            else
                await tsm.LoadSavFiles(SaveManager.SaveFilePath, (string)listBoxPartitions.SelectedItems[0]).ConfigureAwait(false);
            if (!LuteBotForm.luteBotForm.trackSelectionForm.Visible)
            {
                await LuteBotForm.luteBotForm.InvokeAsync(() =>
                {
                    Point coords = WindowPositionUtils.CheckPosition(ConfigManager.GetCoordsProperty(PropertyItem.TrackSelectionPos));
                    LuteBotForm.luteBotForm.trackSelectionForm.Show();
                    LuteBotForm.luteBotForm.trackSelectionForm.Top = coords.Y;
                    LuteBotForm.luteBotForm.trackSelectionForm.Left = coords.X;
                }).ConfigureAwait(false);
            }
        }

        private void PartitionIndexBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private async void PartitionIndexBox_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var filesObject = e.Data.GetData(DataFormats.FileDrop, false);
                if (filesObject != null)
                {
                    var files = (string[])filesObject;
                    await AutoSaveFiles(files).ConfigureAwait(false);
                }
                else if (listBoxPartitions.SelectedIndices.Count > 0)
                {
                    Point point = listBoxPartitions.PointToClient(new Point(e.X, e.Y));
                    int i = this.listBoxPartitions.IndexFromPoint(point);
                    // Try to handle multi-drag-drop, may reorder things
                    var selectedIndices = listBoxPartitions.SelectedIndices.Cast<int>().ToArray();
                    //var selectedItems = listBoxPartitions.SelectedItems.Cast<string>().ToArray();
                    //int[] selectedIndices = e.Data.GetData(typeof(int[])) as int[];
                    string[] selectedItems = new string[selectedIndices.Length];
                    for (int j = 0; j < selectedIndices.Length; j++)
                        selectedItems[j] = (string)listBoxPartitions.Items[selectedIndices[j]];


                    if (selectedItems != null && selectedItems.Length > 0 && i != selectedIndices[0])
                    {
                        foreach (string data in selectedItems)
                        {
                            // First just remove them all
                            this.listBoxPartitions.Items.Remove(data);
                            //this.listBoxPartitions.Items.Insert(i, data);
                            index.PartitionNames.Remove((string)data);
                            //index.PartitionNames.Insert(i, (string)data);

                        }
                        if (i < 0 || i >= listBoxPartitions.Items.Count)
                            i = this.listBoxPartitions.Items.Count - 1;
                        // Then insert them at i+j in their original order
                        for (int j = 0; j < selectedItems.Length; j++)
                        {
                            this.listBoxPartitions.Items.Insert(i + j, selectedItems[j]);
                            index.PartitionNames.Insert(i + j, (string)selectedItems[j]);
                        }
                        // Then re-select all the same things
                        listBoxPartitions.ClearSelected();
                        for (int j = 0; j < selectedItems.Length; j++)
                        {
                            listBoxPartitions.SetSelected(i + j, true);
                        }
                        index.SaveIndex();
                    }
                }
            }
            catch (Exception ex)
            {
                await LuteBotForm.luteBotForm.HandleError(ex, "Failed to drag/drop").ConfigureAwait(false);
            }


        }

        private async Task SelectedItemsChanged()
        {
            await LuteBotForm.luteBotForm.InvokeAsync(async () =>
            {
                if (listBoxPartitions.SelectedIndices.Count == 0)
                {
                    editSelectedButton.Enabled = false;
                    reloadSelectedButton.Enabled = false;
                    exportSelectedButton.Enabled = false;
                    renameSelectedButton.Enabled = false;
                }
                else if (listBoxPartitions.SelectedIndices.Count == 1)
                {
                    editSelectedButton.Enabled = true;
                    reloadSelectedButton.Enabled = true;
                    exportSelectedButton.Enabled = true;
                    renameSelectedButton.Enabled = true;
                    var filePath = Path.Combine(partitionMidiPath, (string)(listBoxPartitions.SelectedItems[0]) + ".mid");
                    await LuteBotForm.luteBotForm.LoadFile(filePath).ConfigureAwait(false);
                }
                else
                {
                    editSelectedButton.Enabled = false;
                    reloadSelectedButton.Enabled = true;
                    exportSelectedButton.Enabled = true;
                    renameSelectedButton.Enabled = false;
                }
            }).ConfigureAwait(false);
        }

        private async void PartitionIndexBox_MouseDown(object sender, MouseEventArgs e)
        {

            if (this.listBoxPartitions.SelectedItem == null) return;
            // If something that's already selected was clicked, keep all selected indices that we detected last time
            //Point point = listBoxPartitions.PointToClient(new Point(e.X, e.Y));
            //int i = this.listBoxPartitions.IndexFromPoint(e.Location);
            //if (lastSelectedIndices.Contains(i))
            //{
            //    foreach (var last in lastSelectedIndices)
            //        listBoxPartitions.SetSelected(last, true);
            //}
            // Looked weird and was kindof annoying and not expected behavior.  No longer necessary now that it works without

            // IDK why this works because I supposedly skip if they have a modifier key, but shift+drag to move multi selections is good now
            if (Control.ModifierKeys == Keys.None && e.Button == MouseButtons.Left) // Prevents a bug when multi-selecting
                this.listBoxPartitions.DoDragDrop(listBoxPartitions.SelectedIndices, DragDropEffects.Move);
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuHelper();
            }

            if (e.Button == MouseButtons.Left)
            {
                await Task.Delay(100).ConfigureAwait(false); // There's no good way to detect when selected is changed, and this fires before... so we wait for a sec
                await SelectedItemsChanged().ConfigureAwait(false);
            }
        }

        private void DeleteItem_Click(object sender, EventArgs e)
        {
            DialogResult confirmResult = MessageBox.Show("Do you want to delete this partition ?",
                                     "Confirm Deletion",
                                     MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                var selectedList = new List<int>();
                IEnumerable<int> selectedEnum;

                selectedEnum = listBoxPartitions.SelectedIndices.Cast<int>();
                // Just in case some weird shit happens again
                // TLDR, because MouseDown had DoDragDrop, it induced a rare bug in .net framework which made it fail to iterate or AddRange for this type of ListBox
                // We now only DoDragDrop if a key isn't held, so it should be relatively impossible to induce, but if it does, it will silently fail
                try
                {
                    selectedList.AddRange(selectedEnum);
                }
                catch { return; }
                selectedList.Sort((a, b) => b.CompareTo(a)); // Sort largest first so we don't have issues when we remove them
                foreach (int selected in selectedList)
                {
                    try
                    {
                        DeletePartition(selected);
                    }
                    catch { }
                }
                PopulateIndexList();
                index.SaveIndex();
            }
        }

        private void DeletePartition(int selected)
        {
            SaveManager.DeleteData(SaveManager.SaveFilePath + index.PartitionNames[selected]);
            // Also delete the MIDI file
            var midFileName = Path.Combine(partitionMidiPath, index.PartitionNames[selected] + ".mid");
            if (File.Exists(midFileName))
                File.Delete(midFileName);
            index.PartitionNames.RemoveAt(selected);
        }

        public void ImportPartitionButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openSavFileDialog = new OpenFileDialog();
            string[] fileNames;
            openSavFileDialog.DefaultExt = "sav";
            openSavFileDialog.Filter = "SAV files|*.sav";
            openSavFileDialog.Title = "Open SAV file";
            openSavFileDialog.Multiselect = true;
            if (openSavFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileNames = openSavFileDialog.FileNames;
                foreach (string fileName in fileNames)
                {
                    index.AddFileInIndex(fileName);
                }
                index.SaveIndex();
                PopulateIndexList();
            }
        }


        private LuteMod.Converter.MordhauConverter trackConverter = null;

        private async void savePartitionsButton_Click(object sender, EventArgs e)
        {
            try
            {
                await ShowPartitionSaveForm().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await LuteBotForm.luteBotForm.HandleError(ex, "Failed to save partition").ConfigureAwait(false);
            }
        }

        public async Task<bool> ShowPartitionSaveForm()
        {
            if (tsm.MidiTracks.Values.Where(t => t.Active).Count() > 0)
            {
                var namingForm = new TrackNamingForm(Path.GetFileNameWithoutExtension(tsm.FileName));

                await LuteBotForm.luteBotForm.InvokeAsync(() =>
                {
                    namingForm.ShowDialog(this);
                }).ConfigureAwait(false);
                //await Task.Run(() =>
                //{
                if (namingForm.DialogResult == DialogResult.OK)
                    return await SavePartition(namingForm.textBoxPartName.Text, namingForm.checkBoxOverwrite.Checked).ConfigureAwait(false);

                return false;
            }
            else
            {
                throw new Exception("The song is empty");
            }
        }

        private async Task<bool> SavePartition(string name, bool overwrite = false)
        {
            try
            {
                await LuteBotForm.luteBotForm.InvokeAsync(() =>
                {

                    var dryWetFile = player.dryWetFile;

                    //var quant = new Quantizer();
                    //var tempoMap = dryWetFile.GetTempoMap();
                    //quant.Quantize(dryWetFile.GetTimedEvents(), new SteppedGrid(new MetricTimeSpan(TimeSpan.FromSeconds(0.017))), tempoMap, new QuantizingSettings { QuantizingLevel = 1, RandomizingSettings = new RandomizingSettings { Filter = t => false } });

                    if (name == "Song Name" || name.Trim() == "")
                    {
                        throw new Exception("Please name your song");
                    }
                    else
                    {
                        while (index.PartitionNames.Contains(name) && !overwrite)
                        {
                            var namingForm = new TrackNamingForm(name);
                            namingForm.Text = "Name already exists - Enter new Song Name";
                            namingForm.ShowDialog(this);
                            if (namingForm.DialogResult == DialogResult.OK)
                            {
                                name = namingForm.textBoxPartName.Text;
                                overwrite = namingForm.checkBoxOverwrite.Checked;
                            }
                            else
                            {
                                name = null;
                                break;
                            }

                        }
                        if (name != null)
                        {
                            while (!Regex.IsMatch(name, "^([a-zA-Z0-9][a-zA-Z0-9 -]*[a-zA-Z0-9])$"))
                            {
                                var namingForm = new TrackNamingForm(name);
                                namingForm.Text = "Name contains invalid characters - Enter new Song Name";
                                namingForm.ShowDialog(this);
                                if (namingForm.DialogResult == DialogResult.OK)
                                {
                                    name = namingForm.textBoxPartName.Text;
                                    overwrite = namingForm.checkBoxOverwrite.Checked;
                                }
                                else
                                {
                                    name = null;
                                    break;
                                }
                            }
                            if (name != null)
                            {
                                if (overwrite && index.PartitionNames.Contains(name))
                                    index.PartitionNames.Remove(name);

                                index.PartitionNames.Insert(0, name);

                                //if (trackConverter == null)
                                //{
                                var converter = new LuteMod.Converter.MordhauConverter();

                                int firstInstrument = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
                                // Step 1, load solo lute into track 0 - this profile should always exist
                                // Actually, all of the first 4 instruments get loaded in, under the same ID we use in lutebot.  Convenient.
                                for (int i = 0; i < 2; i++)
                                {
                                    int oldInstrument = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);

                                    if (tsm.DataDictionary.ContainsKey(i))
                                    {

                                        if (oldInstrument != i)
                                        {
                                            ConfigManager.SetProperty(PropertyItem.Instrument, i.ToString());
                                            Instrument target = Instrument.Prefabs[i];

                                            bool soundEffects = !target.Name.StartsWith("Mordhau", true, System.Globalization.CultureInfo.InvariantCulture);
                                            ConfigManager.SetProperty(PropertyItem.SoundEffects, soundEffects.ToString());
                                            ConfigManager.SetProperty(PropertyItem.LowestNoteId, target.LowestSentNote.ToString());
                                            ConfigManager.SetProperty(PropertyItem.AvaliableNoteCount, target.NoteCount.ToString());
                                            ConfigManager.SetProperty(PropertyItem.NoteCooldown, target.NoteCooldown.ToString());
                                            ConfigManager.SetProperty(PropertyItem.LowestPlayedNote, target.LowestPlayedNote.ToString());
                                            ConfigManager.SetProperty(PropertyItem.ForbidsChords, target.ForbidsChords.ToString());
                                            tsm.UpdateTrackSelectionForInstrument(oldInstrument);
                                        }

                                        converter.Range = ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount);
                                        converter.LowNote = ConfigManager.GetIntegerProperty(PropertyItem.LowestNoteId);
                                        converter.LowestPlayed = ConfigManager.GetIntegerProperty(PropertyItem.LowestPlayedNote);
                                        converter.IsConversionEnabled = true;
                                        converter.SetDivision(((TicksPerQuarterNoteTimeDivision)dryWetFile.TimeDivision).TicksPerQuarterNote);
                                        converter.SetPartitionTempo((int)dryWetFile.GetTempoMap().GetTempoAtTime(new MetricTimeSpan(0)).MicrosecondsPerQuarterNote);

                                        //converter.AddTrack(i);
                                        //converter.FillTrack(i, player.ExtractMidiContent());
                                        var activeTracks = tsm.MidiTracks.Values.Where(t => t.Active && t.tickNotes.Any(tn => tn.Value.Any(n => n.active && tsm.MidiChannels.ContainsKey(n.channel) && tsm.MidiChannels[n.channel].Active))).OrderBy(t => t.Rank).ToList();
                                        for (int j = 0; j < activeTracks.Count; j++)
                                        {
                                            var trackID = converter.AddTrack(i);
                                            //converter.SetEnabledTracksInTrack(trackID, new List<MidiChannelItem> { activeTracks[j] });
                                            //converter.SetEnabledMidiChannelsInTrack(trackID, tsm.MidiChannels.Values.ToList());

                                            converter.FillTrack(trackID, player.ExtractMidiContent(dryWetFile, activeTracks[j].Id));
                                        }
                                    }
                                }

                                SaveManager.WriteSaveFile(Path.Combine(SaveManager.SaveFilePath, name), converter.GetPartitionToString());
                                index.SaveIndex();

                                // And put the instrument back
                                if (ConfigManager.GetIntegerProperty(PropertyItem.Instrument) != firstInstrument)
                                {
                                    int oldInstrument = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
                                    ConfigManager.SetProperty(PropertyItem.Instrument, firstInstrument.ToString());
                                    Instrument target = Instrument.Prefabs[firstInstrument];

                                    bool soundEffects = !target.Name.StartsWith("Mordhau", true, System.Globalization.CultureInfo.InvariantCulture);
                                    ConfigManager.SetProperty(PropertyItem.SoundEffects, soundEffects.ToString());
                                    ConfigManager.SetProperty(PropertyItem.LowestNoteId, target.LowestSentNote.ToString());
                                    ConfigManager.SetProperty(PropertyItem.AvaliableNoteCount, target.NoteCount.ToString());
                                    ConfigManager.SetProperty(PropertyItem.NoteCooldown, target.NoteCooldown.ToString());
                                    ConfigManager.SetProperty(PropertyItem.LowestPlayedNote, target.LowestPlayedNote.ToString());
                                    ConfigManager.SetProperty(PropertyItem.ForbidsChords, target.ForbidsChords.ToString());
                                    tsm.UpdateTrackSelectionForInstrument(oldInstrument);
                                }
                                //}
                                //else
                                //{
                                //    SaveManager.WriteSaveFile(SaveManager.SaveFilePath + namingForm.textBoxPartName.Text, trackConverter.GetPartitionToString());
                                //    index.SaveIndex();
                                //    PopulateIndexList();
                                //    trackConverter = null;
                                //}

                                // Lastly, save the settings in a midi file with the same name, in the same folder, for ease of sharing...
                                // TODO: Consider storing these in appdata, and providing a button to access them.  Both might get complicated if I make partition playlists
                                // Actually... I think I will store them in appdata.
                                var midFileName = Path.Combine(partitionMidiPath, name + ".mid");
                                Directory.CreateDirectory(partitionMidiPath);
                                Task.Run(() => tsm.SaveTrackManager(midFileName)); // Lutebot doesn't need this anytime soon - and shouldn't offer the option to load it until it exists anyway

                                //Invoke((MethodInvoker)delegate {
                                PopulateIndexList();
                                //});
                            }
                        }
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await LuteBotForm.luteBotForm.HandleError(ex, $"Failed to save {name} to LuteMod").ConfigureAwait(false);
                return false;
            }
            return name != null;
        }

        private void PartitionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            WindowPositionUtils.UpdateBounds(PropertyItem.PartitionListPos, new Point() { X = Left, Y = Top });
            ConfigManager.SaveConfig();
        }

        public void openSaveFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(SaveManager.SaveFilePath);
        }

        public void importPartitions_Click(object sender, EventArgs e)
        {
            OpenFileDialog openSavFileDialog = new OpenFileDialog();
            string[] fileNames;
            openSavFileDialog.DefaultExt = "sav";
            openSavFileDialog.Filter = "SAV files|*.sav";
            openSavFileDialog.Title = "Open SAV file";
            openSavFileDialog.Multiselect = true;
            if (openSavFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileNames = openSavFileDialog.FileNames;
                foreach (string fileName in fileNames)
                {
                    index.AddFileInIndex(fileName);
                }
                index.SaveIndex();
                PopulateIndexList();
            }
        }

        public async void saveMultipleSongsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try // async void won't propagate errors, always try/catch
            {
                openMidiFileDialog.Title = "Auto-Convert MIDI files to Partitions";
                if (openMidiFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filenames = openMidiFileDialog.FileNames;
                    foreach (var f in filenames.Reverse()) // So the first ones are first again
                    {
                        await LuteBotForm.luteBotForm.LoadFile(f, true).ConfigureAwait(false);
                        await SavePartition(Regex.Replace(Path.GetFileName(f).Replace(".mid", ""), "[^a-zA-Z0-9]", "")).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async void trainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var trainingForm = new NeuralNetworkForm(tsm, this);
            trainingForm.ShowDialog(this);

        }

        public void openMidiFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(partitionMidiPath);
        }

        public void exportPartitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    string tempDir = Path.Combine(partitionMidiPath, "TempFiles");
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                    Directory.CreateDirectory(tempDir);
                    foreach (var name in index.PartitionNames)
                    {
                        string midiPath = Path.Combine(partitionMidiPath, name + ".mid");
                        if (File.Exists(midiPath))
                            File.Copy(midiPath, Path.Combine(tempDir, name + ".mid"), true);

                        int i = 0;
                        string currentFile = Path.Combine(SaveManager.SaveFilePath, name + "[" + i + "].sav");
                        while (File.Exists(currentFile))
                        {
                            File.Copy(currentFile, Path.Combine(tempDir, name + "[" + i + "].sav"), true);
                            currentFile = Path.Combine(SaveManager.SaveFilePath, name + "[" + ++i + "].sav");
                        }
                    }

                    int c = 0;
                    string partitionFile = Path.Combine(SaveManager.SaveFilePath, "PartitionIndex[" + c + "].sav");
                    while (File.Exists(partitionFile))
                    {
                        File.Copy(partitionFile, Path.Combine(tempDir, "PartitionIndex[" + c + "].sav"));
                        partitionFile = Path.Combine(SaveManager.SaveFilePath, "PartitionIndex[" + ++c + "].sav");
                    }
                    var zipDir = Path.Combine(partitionMidiPath, "Export");
                    Directory.CreateDirectory(zipDir);
                    // Zip the dir
                    ZipFile.CreateFromDirectory(tempDir, Path.Combine(zipDir, DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "-Export.zip"));
                    // Delete temp
                    Directory.Delete(tempDir, true);
                    // Show them the zip
                    Process.Start(zipDir);
                    Invoke((MethodInvoker)delegate { MessageBox.Show($"The export folder `{zipDir}` has been opened", "Export Success"); });
                }
                catch (Exception ex)
                {
                    Invoke((MethodInvoker)delegate { MessageBox.Show(ex.StackTrace, ex.Message); });
                }
            });
        }

        public void exportMidisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportMidis(index.PartitionNames);
        }

        private void ExportMidis(IEnumerable<string> partitionNames)
        {
            Task.Run(() =>
            {
                try
                {
                    string tempDir = Path.Combine(partitionMidiPath, "TempFiles");
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, true);
                    Directory.CreateDirectory(tempDir);
                    Parallel.ForEach(partitionNames, name =>
                    {
                        string midiPath = Path.Combine(partitionMidiPath, name + ".mid");
                        if (File.Exists(midiPath))
                            File.Copy(midiPath, Path.Combine(tempDir, name + ".mid"), true);
                    });

                    var zipDir = Path.Combine(partitionMidiPath, "Export");
                    Directory.CreateDirectory(zipDir);
                    // Zip the dir
                    ZipFile.CreateFromDirectory(tempDir, Path.Combine(zipDir, DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "-Export.zip"));
                    // Delete temp
                    Directory.Delete(tempDir, true);
                    // Show them the zip
                    Process.Start(zipDir);
                    Invoke((MethodInvoker)delegate { MessageBox.Show(this, $"The export folder `{zipDir}` has been opened", "Export Success"); });
                }
                catch (Exception ex)
                {
                    Invoke((MethodInvoker)delegate { MessageBox.Show(this, ex.StackTrace, ex.Message); });
                }
            });
        }

        private async void reloadSelectedButton_Click(object sender, EventArgs e)
        {
            if (listBoxPartitions.SelectedItems.Count > 0)
            {
                var filenames = listBoxPartitions.SelectedItems.Cast<string>().Reverse();

                await AutoSaveFiles(filenames).ConfigureAwait(false);
            }
        }

        public async Task AutoSaveFiles(IEnumerable<string> filenames)
        {
            string warnings = "";
            await LuteBotForm.luteBotForm.InvokeAsync(() =>
            {
                savePartitionButton.Enabled = false;
                savePartitionButton.Text = "Processing Midis...";
            }).ConfigureAwait(false);
            try
            {
                foreach (var f in filenames) // So the first ones are first again
                {
                    var filePath = f;
                    if (!Path.IsPathRooted(filePath))
                        filePath = Path.Combine(partitionMidiPath, filePath + ".mid");
                    if (!File.Exists(filePath))
                    {
                        warnings += $"Could not reload {f} because it was saved with an old LuteBot version\n";
                        continue;
                    }
                    try
                    {
                        await LuteBotForm.luteBotForm.LoadFile(filePath, true).ConfigureAwait(false);
                        if (player.dryWetFile != null)
                            await SavePartition(Regex.Replace(Path.GetFileName(filePath).Replace(".mid", ""), "[^a-zA-Z0-9]", ""), false).ConfigureAwait(false);

                    }
                    catch (Exception ex)
                    {
                        warnings += $"Failed to reload file {f}: {ex.Message}";
                    }
                }
            }
            finally
            {
                if (warnings != "")
                {
                    await LuteBotForm.luteBotForm.HandleError(new Exception(warnings), "Some Midis failed to reload: \n" + warnings).ConfigureAwait(false);
                }
                await LuteBotForm.luteBotForm.InvokeAsync(() =>
                {
                    savePartitionButton.Enabled = true;
                    savePartitionButton.Text = "Add Midis";
                }).ConfigureAwait(false);
            }
        }

        private async void renameSelectedButton_Click(object sender, EventArgs e)
        {
            if (listBoxPartitions.SelectedItems.Count == 1)
            {
                string indexName = (string)listBoxPartitions.SelectedItems[0];

                var filePath = Path.Combine(partitionMidiPath, indexName + ".mid");
                if (!File.Exists(filePath))
                {
                    var message = "Failed to rename; could not load Midi because it was saved with an old LuteBot version";
                    await LuteBotForm.luteBotForm.HandleError(new Exception(message), message).ConfigureAwait(false);
                    return;
                }
                await LuteBotForm.luteBotForm.LoadFile(filePath).ConfigureAwait(false);

                try
                {
                    var result = await ShowPartitionSaveForm().ConfigureAwait(false);
                    if (result)
                    {
                        var partitionIndex = index.PartitionNames.IndexOf(indexName);
                        if (partitionIndex > -1)
                            DeletePartition(partitionIndex);
                        index.SaveIndex();
                        await PopulateIndexListAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await LuteBotForm.luteBotForm.HandleError(ex, "Failed to save partition").ConfigureAwait(false);
                }
            }
        }
    }
}
