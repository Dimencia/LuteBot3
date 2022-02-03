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

            if (!LuteBotForm.IsLuteModInstalled())
            {
                var popup = new PopupForm("Install LuteMod", "Would you like to update/install LuteMod?", "LuteMod is a Mordhau Mod that lets you manage your songs in game and move freely, and play duets with lute and flute\n\nLuteMod was not detected as installed, or an old version was detected\n\nThanks to Monty for LuteMod, and cswic for the autoloader\n\nFor more information, see:",
                new Dictionary<string, string>() {
                    { "What is LuteMod", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod" } ,
                    { "LuteMod mod.io page", "https://mordhau.mod.io/lutemod" },
                    { "Autoloader mod.io page", "https://mordhau.mod.io/clientside-mod-autoloader" },
                    { "Flute and Duets", "https://mordhau-bards-guild.fandom.com/wiki/LuteMod#Flute_and_Duets" },
                    { "The Bard's Guild Discord", "https://discord.gg/4xnJVuz" },
                }, MessageBoxButtons.YesNo);
                popup.ShowDialog(this);
                if (popup.DialogResult == DialogResult.Yes)
                    LuteBotForm.InstallLuteMod();
                else
                    Hide();
            }
            RefreshPartitionList();
        }

        private static readonly string partitionMidiPath = Path.Combine(LuteBotForm.lutebotPath, "Partition MIDIs");

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
        private PartitionIndex index;
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

        private void PopulateIndexList()
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

                MenuItem deleteItem = indexContextMenu.MenuItems.Add("Delete " + name);
                deleteItem.Click += new EventHandler(DeleteItem_Click);

                if (listBoxPartitions.SelectedIndices.Count == 1)
                {
                    string midiPath = Path.Combine(partitionMidiPath, listBoxPartitions.SelectedItems[0] + ".mid");
                    if (File.Exists(midiPath))
                    {
                        MenuItem editItem = indexContextMenu.MenuItems.Add("Load " + name);
                        editItem.Click += EditItem_Click;
                    }
                }
                
                listBoxPartitions.ContextMenu = indexContextMenu; // TODO: I'd love to make it popup at the selected item, not at mouse pos, but whatever
                indexContextMenu.Show(listBoxPartitions, listBoxPartitions.PointToClient(Cursor.Position));
            }
            else
            {
                listBoxPartitions.ContextMenu = null;
            }
        }

        private void EditItem_Click(object sender, EventArgs e)
        {
            string midiPath = Path.Combine(partitionMidiPath, listBoxPartitions.SelectedItems[0] + ".mid");
            LuteBotForm.luteBotForm.LoadHelper(midiPath);
        }

        private void PartitionIndexBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void PartitionIndexBox_DragDrop(object sender, DragEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }


        }

        private void PartitionIndexBox_MouseDown(object sender, MouseEventArgs e)
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
                        SaveManager.DeleteData(SaveManager.SaveFilePath + index.PartitionNames[selected]);
                        index.PartitionNames.RemoveAt(selected);
                    }
                    catch { }
                }
                PopulateIndexList();
                index.SaveIndex();
            }
        }

        private void ImportPartitionButton_Click(object sender, EventArgs e)
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

        private void savePartitionsButton_Click(object sender, EventArgs e)
        {

            if (tsm.MidiTracks.Where(t => t.Active).Count() > 0)
            {
                var namingForm = new TrackNamingForm(Path.GetFileNameWithoutExtension(tsm.FileName));
                namingForm.ShowDialog(this);
                if (namingForm.DialogResult == DialogResult.OK)
                {
                    if (namingForm.textBoxPartName.Text == "Partition Name" || namingForm.textBoxPartName.Text.Trim() == "")
                    {
                        MessageBox.Show("Please name your partition");
                    }
                    else
                    {
                        if (index.PartitionNames.Contains(namingForm.textBoxPartName.Text))
                        {
                            MessageBox.Show("That name already exists");
                        }
                        else
                        {
                            if (!Regex.IsMatch(namingForm.textBoxPartName.Text, "^([a-zA-Z0-9][a-zA-Z0-9 -]*[a-zA-Z0-9])$"))
                            {
                                MessageBox.Show("That name contains invalid characters");
                            }
                            else
                            {
                                index.PartitionNames.Add(namingForm.textBoxPartName.Text);

                                //if (trackConverter == null)
                                //{
                                var converter = new LuteMod.Converter.MordhauConverter();
                                int firstInstrument = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
                                // Step 1, load solo lute into track 0 - this profile should always exist
                                // Actually, all of the first 4 instruments get loaded in, under the same ID we use in lutebot.  Convenient.
                                for (int i = 0; i < 4; i++)
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
                                            player.mordhauOutDevice.UpdateNoteIdBounds();
                                        }

                                        converter.Range = ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount);
                                        converter.LowNote = ConfigManager.GetIntegerProperty(PropertyItem.LowestPlayedNote);
                                        converter.IsConversionEnabled = true;
                                        converter.SetDivision(player.sequence.Division);
                                        converter.AddTrack();
                                        converter.SetEnabledTracksInTrack(i, tsm.MidiTracks);
                                        converter.SetEnabledMidiChannelsInTrack(i, tsm.MidiChannels);

                                        converter.FillTrack(i, player.ExtractMidiContent());
                                    }
                                }

                                SaveManager.WriteSaveFile(Path.Combine(SaveManager.SaveFilePath, namingForm.textBoxPartName.Text), converter.GetPartitionToString());
                                index.SaveIndex();
                                PopulateIndexList();
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
                                    player.mordhauOutDevice.UpdateNoteIdBounds();
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
                                var midFileName = Path.Combine(partitionMidiPath, namingForm.textBoxPartName.Text + ".mid");
                                if (File.Exists(midFileName))
                                    File.Delete(midFileName);
                                Directory.CreateDirectory(partitionMidiPath);
                                Task.Run(() => tsm.SaveTrackManager(midFileName)); // Lutebot doesn't need this anytime soon - and shouldn't offer the option to load it until it exists anyway
                            }
                        }
                    }
                }
                else if (namingForm.DialogResult == DialogResult.Yes)
                {
                    // They wanted to just add it as a track
                    if (trackConverter == null)
                        trackConverter = new LuteMod.Converter.MordhauConverter();
                    // These ranges and settings only matter for FillTrack.  So re-setting them each time isn't a problem
                    trackConverter.Range = ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount);
                    trackConverter.LowNote = ConfigManager.GetIntegerProperty(PropertyItem.LowestPlayedNote);
                    trackConverter.IsConversionEnabled = true;
                    trackConverter.SetDivision(player.sequence.Division); // This one could be weird
                    trackConverter.AddTrack();
                    trackConverter.SetEnabledTracksInTrack(trackConverter.GetTrackCount() - 1, tsm.MidiTracks);
                    trackConverter.SetEnabledMidiChannelsInTrack(trackConverter.GetTrackCount() - 1, tsm.MidiChannels);

                    trackConverter.FillTrack(trackConverter.GetTrackCount() - 1, player.ExtractMidiContent());
                }
            }
            else
            {
                MessageBox.Show("The partition is empty");
            }

        }

        private void PartitionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            WindowPositionUtils.UpdateBounds(PropertyItem.PartitionListPos, new Point() { X = Left, Y = Top });
            ConfigManager.SaveConfig();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(SaveManager.SaveFilePath);
        }

        private void button1_Click(object sender, EventArgs e)
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

    }
}
