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

            if (tsm.MidiTracks.Values.Where(t => t.Active).Count() > 0)
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
                                        converter.SetPartitionTempo(player.sequence.FirstTempo);
                                        converter.AddTrack();
                                        converter.SetEnabledTracksInTrack(i, tsm.MidiTracks.Values.ToList());
                                        converter.SetEnabledMidiChannelsInTrack(i, tsm.MidiChannels.Values.ToList());

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
                    trackConverter.SetEnabledTracksInTrack(trackConverter.GetTrackCount() - 1, tsm.MidiTracks.Values.ToList());
                    trackConverter.SetEnabledMidiChannelsInTrack(trackConverter.GetTrackCount() - 1, tsm.MidiChannels.Values.ToList());

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

        private async void saveMultipleSongsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try // async void won't propagate errors, always try/catch
            {
                openMidiFileDialog.Title = "Auto-Convert MIDI files to Partitions";
                if (openMidiFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filenames = openMidiFileDialog.FileNames;
                    foreach (var f in filenames)
                    {
                        await LuteBotForm.luteBotForm.LoadHelperAsync(f);
                        savePartitionsButton_Click(null, null);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async void trainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check every partition for a midi file
            // If that file has more than one channel and one is selected for flute
            // Setup training for it

            // So far: 
            // I think the training makes sense and is converging as well as it can... sorta... 
            // We're getting 50/113 incorrect still, at best, with avgNoteLength,totalNoteLength,highest,lowest,numnotes, all relative
            // So let's try all the others relative too

            // I may just need more params to be able to do this broad set

            // Not bad.  Down to 49 incorrect, and it's only avgNoteLength and maxChordSize that make anything disagree
            // ... 47 incorrect before it started iterating... I swear it must be doing something backwards... 




            // OK, I've implemented a simple neural network we can try also... 
            // No idea about layer sizes
            // Input would be 7 layers

            // Output is expected to be basically just 0 or 1, 1 if it's an acceptable flute channel, 0 if it's not...
            // This would allow multiple flute channels to be selected for a given song, which is interesting... 
            // And also probably ends up giving values between 0 and 1, letting us rank them if we want?

            // We'll just try 5 in the hidden layer and see how that does

            try
            {
                // Gave us nans at 7,5,1; does hidden have to be bigger than input?  Let's try 7,10,1.  It really shouldn't have to, though... 

                // Yeah something's broken, 7,10,1 gives an index out of bounds error during backpropagate... 

                // The fuck.  Lots of things were broken, but I think they're all fixed and it just, puts the weights to infinity and -infinity immediately
                // Let's try normal relu... Nope.  I think learningRate just needs to be way, way, way smaller than default of 0.01
                // FIIIINE I need to give it normalized values... 
                //string[] activation = new string[] { "tanh", "tanh", "softmax" }; // TODO make this an enum, what kind of madman made them strings... 
                int numParamsPerChannel = Extensions.numParamsPerChannel;

                // Let's try a compare implementation - 
                    // For each song, take two arbitrary channels and compare for best flute between them
                    // Take the result of that and continue, ending when the last channel is compared and the result is given for the one best flute
                    // Consider it a bubble sort... hmm.... but that doesn't really let us do more than one

                // Well then, maybe instead, do individual channels for flute suitability and rank those results
                    // We won't normalize anything and we might make the network a little complex

                // We can softmax all the outputs once we have them for each channel
                string[] activation = new string[] { "tanh", "tanh", "tanh" };
                tsm.neural = new NeuralNetwork(new int[] { numParamsPerChannel, 64, 32, 1 }, activation);




                // These below work great and are the settings for 'v2Neural'
                //string[] activation = new string[] { "tanh", "softmax" };
                //tsm.neural = new NeuralNetwork(new int[] { 16 * numParamsPerChannel, 64, 16 }, activation);


                tsm.simpleML = new SimpleML<MidiChannelItem>(
                    (c => c.avgNoteLength), 
                    (c => c.maxChordSize), 
                    (c => c.totalNoteLength), 
                    (c => c.highestNote), 
                    (c => c.lowestNote), 
                    (c => c.numNotes), 
                    (c => c.Id) 
                    );

                var candidates = new List<TrainingCandidate<MidiChannelItem>>();

                var neuralCandidates = new List<MidiChannelItem[]>(); // It can accept multiple correct answers, hopefully... 

                foreach(var part in index.PartitionNames)
                {
                    string midiPath = Path.Combine(partitionMidiPath, part + ".mid");
                    if (File.Exists(midiPath))
                    {
                        await LuteBotForm.luteBotForm.LoadHelperAsync(midiPath);
                        if (tsm.MidiChannels.Count() > 1 && tsm.DataDictionary.ContainsKey(1)) // To keep the percentages from getting weird in training, at least 4 channels?
                        {
                            if (tsm.MidiTracks.All(t => t.Value.Active))
                            {
                                // Actually, I think the flute track may not have the appropriate data...
                                var tempNeuralCandidates = tsm.MidiChannels.Values.Where(c => c.Id != 9).ToArray();
                                foreach (var ca in tempNeuralCandidates)
                                {
                                    ca.Active = tsm.DataDictionary[1].MidiChannels.Where(c => c.Id == ca.Id).Single().Active;
                                }
                                neuralCandidates.Add(tempNeuralCandidates);
                                var fluteChannels = tsm.DataDictionary[1].MidiChannels.Where(c => c.Active);
                                if (fluteChannels.Count() == 1)
                                {
                                    var fluteChannel = fluteChannels.First();
                                    // We have what we need, one flute track and more than one total track
                                    // We need to make sure our passed target exactly matches the one in our passed list
                                    var candidate = new TrainingCandidate<MidiChannelItem>(tsm.MidiChannels.Values, fluteChannel);
                                    candidates.Add(candidate);
                                }
                            }
                            else if(tsm.MidiChannels.All(c => c.Value.Active))
                            {
                                // If some tracks are disabled and all channels are enabled, we'll take the tracks as data
                                // Track notes should already have discarded anything with channel 9
                                MidiChannelItem[] tempNeuralCandidates = tsm.MidiTracks.Values.ToArray();
                                foreach (var ca in tempNeuralCandidates)
                                { // This may not be necessary; I could probably just take the MidiTracks from DataDictionary[1] but, it seemed to give odd results like it didn't have everything
                                    ca.Active = tsm.DataDictionary[1].MidiTracks.Where(c => c.Id == ca.Id).Single().Active;
                                }
                                neuralCandidates.Add(tempNeuralCandidates);
                            }
                        }
                    }
                }
                Console.WriteLine($"Training with {candidates.Count} candidates...");
                var trainingTarget = new TrainingTarget<MidiChannelItem>(candidates, ((a,b) => a.Id == b.Id));

                tsm.simpleML.Train(trainingTarget);
                Console.WriteLine("Trained - Final Values:");
                int count = 0;
                foreach (var p in tsm.simpleML.Parameters)
                    Console.WriteLine($"Parameter {count++}: Target={p.Target}; Weight={p.Weight}");

                // Now train the neural one

                // So after 100k trainings, all the channels give a negative answer which is pretty weird
                // And they seem completely wrong or arbitrary, nothing useful.  

                // Probably need to make the network deeper... 
                int trainCount = 2000;
                float costTotal = 999f;

                Console.WriteLine($"Training neural network {trainCount} times");

                // Partition candidates into test sets, probably 70/30%?
                // So, first shuffle them

                var random = new Random();

                var orderedCandidates = neuralCandidates.OrderBy(c => random.Next());
                var numTrainingCandidates = (int)(orderedCandidates.Count() * 0.3f);
                var neuralTrainingCandidates = orderedCandidates.Skip(numTrainingCandidates);
                var neuralTestCandidates = orderedCandidates.Take(numTrainingCandidates);

                int numTestsCorrect = 0;
                int numActualTestsCorrect = 0;

                int i = 0;
                while(numActualTestsCorrect < neuralTestCandidates.Count())
                //while(costTotal > 0.1f && i < trainCount)
                //for (int i = 0; i < trainCount; i++)
                {
                    costTotal = 0;
                    foreach (var song in neuralTrainingCandidates)
                    {
                        float maxAvgNoteLength = song.Max(c => c.Id == 9 ? 0 : c.avgNoteLength);
                        float maxNoteLength = song.Max(c => c.Id == 9 ? 0 : c.totalNoteLength);
                        float maxNumNotes = song.Max(c => c.Id == 9 ? 0 : c.numNotes);

                        foreach (var channel in song)
                        {
                            var inputs = channel.GetNeuralInputs(maxAvgNoteLength, maxNumNotes, maxNoteLength);
                            var expected = new float[1];

                            if (channel.Active)
                                expected[0] = 0.5f;
                            else
                                expected[0] = -0.5f;

                            /*
                            var inputs = song.GetNeuralInput();

                            float numActive = song.Count(c => c.Active);
                            float[] expected = new float[16];

                            for(int j = 0; j < 16; j++)
                            {
                                var channel = song.Where(c => c.Id == j).SingleOrDefault();
                                //var channel = song.Values[j];

                                expected[j] = 0;

                                if (channel != null) {
                                    if (channel.Active)
                                        expected[j] = 1 / numActive;
                                }
                                else
                                {
                                    for (int k = 0; k < numParamsPerChannel; k++)
                                        inputs[j * numParamsPerChannel + k] = 0; // Tanh should evaluate this as -1 to keep it from taking weight
                                }
                            }
                            */

                            tsm.neural.BackPropagate(inputs, expected);
                            costTotal += tsm.neural.cost;
                        }
                    }
                    numTestsCorrect = 0;
                    numActualTestsCorrect = 0;
                    foreach (var song in neuralTrainingCandidates)
                    {
                        float maxAvgNoteLength = song.Max(c => c.Id == 9 ? 0 : c.avgNoteLength);
                        float maxNoteLength = song.Max(c => c.Id == 9 ? 0 : c.totalNoteLength);
                        float maxNumNotes = song.Max(c => c.Id == 9 ? 0 : c.numNotes);

                        Dictionary<MidiChannelItem, float> channelResults = new Dictionary<MidiChannelItem, float>();
                        foreach(var channel in song)
                        {
                            var inputs = channel.GetNeuralInputs(maxAvgNoteLength, maxNumNotes, maxNoteLength);
                            var neuralResults = tsm.neural.FeedForward(inputs);
                            channelResults[channel] = neuralResults[0];
                        }

                        var orderedResults = channelResults.OrderByDescending(kvp => kvp.Value);


                        /*
                        var inputs = song.GetNeuralInput();

                        var neuralResults = tsm.neural.FeedForward(inputs);
                        // The output here is a channel ID to confidence map...
                        // I need to find the ID of the best one... and rank the rest...
                        // So let's just build a quick dictionary I guess
                        var results = new Dictionary<int, float>();

                        for (int j = 0; j < neuralResults.Length; j++)
                        {
                            results[j] = neuralResults[j];
                        }

                        var orderedResults = results.OrderByDescending(kvp => kvp.Value);
                        */
                        // Check the number of active flute channels...
                        var numFlute = song.Where(s => s.Active).Count();
                        bool correct = true;

                        for (int j = 0; j < numFlute; j++)
                        {
                            bool? existsAndCorrect = song.Where(s => s.Id == orderedResults.ElementAt(j).Key.Id).SingleOrDefault()?.Active;
                            if (!existsAndCorrect.HasValue || !existsAndCorrect.Value)
                                correct = false;
                        }

                        if (correct)
                            numTestsCorrect++;
                        
                    }
                    foreach (var song in neuralTestCandidates)
                    {
                        float maxAvgNoteLength = song.Max(c => c.Id == 9 ? 0 : c.avgNoteLength);
                        float maxNoteLength = song.Max(c => c.Id == 9 ? 0 : c.totalNoteLength);
                        float maxNumNotes = song.Max(c => c.Id == 9 ? 0 : c.numNotes);

                        Dictionary<MidiChannelItem, float> channelResults = new Dictionary<MidiChannelItem, float>();
                        foreach (var channel in song)
                        {
                            var inputs = channel.GetNeuralInputs(maxAvgNoteLength, maxNumNotes, maxNoteLength);
                            var neuralResults = tsm.neural.FeedForward(inputs);
                            channelResults[channel] = neuralResults[0];
                        }

                        var orderedResults = channelResults.OrderByDescending(kvp => kvp.Value);


                        /*
                        var inputs = song.GetNeuralInput();

                        var neuralResults = tsm.neural.FeedForward(inputs);
                        // The output here is a channel ID to confidence map...
                        // I need to find the ID of the best one... and rank the rest...
                        // So let's just build a quick dictionary I guess
                        var results = new Dictionary<int, float>();

                        for (int j = 0; j < neuralResults.Length; j++)
                        {
                            results[j] = neuralResults[j];
                        }

                        var orderedResults = results.OrderByDescending(kvp => kvp.Value);
                        */
                        // Check the number of active flute channels...
                        var numFlute = song.Where(s => s.Active).Count();
                        bool correct = true;

                        for (int j = 0; j < numFlute; j++)
                        {
                            bool? existsAndCorrect = song.Where(s => s.Id == orderedResults.ElementAt(j).Key.Id).SingleOrDefault()?.Active;
                            if (!existsAndCorrect.HasValue || !existsAndCorrect.Value)
                                correct = false;
                        }

                        if (correct)
                        {
                            numTestsCorrect++;
                            numActualTestsCorrect++;
                        }

                        //int fluteCount = 0;
                        //foreach (var channel in channelResults.Keys)
                        ////foreach (var channel in activeChannels)
                        //{
                        //    //var channel = activeChannels.Where(c => c.Id == orderedResults.ElementAt(i).Key).SingleOrDefault();
                        //    
                        //    if (channel != null)
                        //    {
                        //        Console.WriteLine($"{channel.Name} ({channel.Id}) - Neural Score: {channelResults[channel]}");
                        //        //Console.WriteLine($"{channel.Name} ({channel.Id}) - Neural Score: {neuralResults[channel.Id]}");
                        //        //channel.Name += $"(Flute Rank {++fluteCount} - {Math.Round(channelResults[channel], 2)}%)";
                        //    }
                        //}
                    }
                    Console.WriteLine($"{numTestsCorrect} tests correct out of {neuralCandidates.Count()}; {numActualTestsCorrect}/{neuralTestCandidates.Count()} - Training #{i++} - TotalCost: {costTotal}");

                    //orderedCandidates = neuralCandidates.OrderBy(c => random.Next());
                    //neuralTrainingCandidates = orderedCandidates.Take(numTrainingCandidates);
                    //neuralTestCandidates = orderedCandidates.Skip(numTrainingCandidates);
                }

                numTestsCorrect = 0;
                foreach (var song in neuralCandidates)
                {
                    float maxAvgNoteLength = song.Max(c => c.Id == 9 ? 0 : c.avgNoteLength);
                    float maxNoteLength = song.Max(c => c.Id == 9 ? 0 : c.totalNoteLength);
                    float maxNumNotes = song.Max(c => c.Id == 9 ? 0 : c.numNotes);

                    Dictionary<MidiChannelItem, float> channelResults = new Dictionary<MidiChannelItem, float>();
                    foreach (var channel in song)
                    {
                        var inputs = channel.GetNeuralInputs(maxAvgNoteLength, maxNumNotes, maxNoteLength);
                        var neuralResults = tsm.neural.FeedForward(inputs);
                        channelResults[channel] = neuralResults[0];
                    }

                    var orderedResults = channelResults.OrderByDescending(kvp => kvp.Value);


                    /*
                    var inputs = song.GetNeuralInput();

                    var neuralResults = tsm.neural.FeedForward(inputs);
                    // The output here is a channel ID to confidence map...
                    // I need to find the ID of the best one... and rank the rest...
                    // So let's just build a quick dictionary I guess
                    var results = new Dictionary<int, float>();

                    for (int j = 0; j < neuralResults.Length; j++)
                    {
                        results[j] = neuralResults[j];
                    }

                    var orderedResults = results.OrderByDescending(kvp => kvp.Value);
                    */
                    // Check the number of active flute channels...
                    var numFlute = song.Where(s => s.Active).Count();
                    bool correct = true;

                    for (int j = 0; j < numFlute; j++)
                    {
                        bool? existsAndCorrect = song.Where(s => s.Id == orderedResults.ElementAt(j).Key.Id).SingleOrDefault()?.Active;
                        if (!existsAndCorrect.HasValue || !existsAndCorrect.Value)
                            correct = false;
                    }

                    if (correct)
                        numTestsCorrect++;
                }
                Console.WriteLine($"Complete - {numTestsCorrect} tests correct out of {neuralCandidates.Count()}");
                // TODO: Save the values somewhere... I've got them pasted as a screenshot in officers chat, if I need to
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
