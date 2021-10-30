using LuteBot.Config;
using LuteBot.IO.Files;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.UI
{
    public partial class PartitionAdjustmentForm : Form
    {
        public PartitionAdjustmentForm()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void submit_Click(object sender, EventArgs e)
        {
            if(int.TryParse(textBoxOctaveAdjust.Text, out int octaveAdjust))
            {
                button1.Enabled = false;
                button2.Enabled = false;
                // iterate every file in the mordhau Save folder matching format [A-Za-z0-9]*\[[0-9]*\]
                // skip any named PartitionIndex


                // First, move them to a backup folder

                // Then parse each set...
                // Parse out each command and convert it to MidiNote
                // Create a collection of MidiNotes for each set/song
                // For all notes of type 1, increase the second number (ex 1320-14-1;) by 12*octaveAdjust
                // Run the collection back through the partition save routine

                // This is all divison is used for: 
                // partition.Tempo = note.Tone / division;
                // return new Note() { Tick = note.Tick, Tone = note.Tone / division, Type = note.Type };
                // So when I read the Tempo, NoteType 2, I can pass it straight back without sending through BuildNote
                // The rest I do need to send through BuildNote
                // But also be sure to set partition.Tempo



                // The save file requires: ([a-zA-Z0-9][a-zA-Z0-9 -]*[a-zA-Z0-9])
                Regex filenameReg = new Regex(@"^([a-zA-Z0-9][a-zA-Z0-9 -]*[a-zA-Z0-9])\[([0-9]+)\].sav$");
                // The first capture group is the song name, the second is the index

                Dictionary<string, string> fileContents = new Dictionary<string, string>();
                // Dictionary of filename:contents
                // We are also using a dictionary of index to string, because we can't guarantee what order we read these in (or, we could I guess, IDK)

                string backupDir = Path.Combine(SaveManager.SaveFilePath, "PartitionBackup " + DateTime.Now.ToString("MM-dd-yyyy"));
                Directory.CreateDirectory(backupDir);

                try
                {
                    foreach (var f in Directory.GetFiles(SaveManager.SaveFilePath))
                    {
                        var filename = Path.GetFileName(f);
                        var match = filenameReg.Match(filename);
                        if (match.Success)
                        {
                            var songName = match.Groups[1].Value;
                            int index = int.Parse(match.Groups[2].Value);

                            if (songName != "PartitionIndex")
                            {

                                if (!fileContents.ContainsKey(songName))
                                    fileContents[songName] = "";

                                // BOF: "SavedPartition    StrProperty Ž       Š  "
                                // EOF: "    None    "

                                var contents = File.ReadAllText(f);

                                int splitLocation = contents.IndexOf("SavedPartition\0\f\0\0\0StrProperty\0�\u0013\0\0\0\0\0\0\0�\u0013\0\0", StringComparison.InvariantCulture) + "SavedPartition    StrProperty Ž       Š  ".Length;

                                string songContent = contents.Substring(splitLocation);
                                songContent = songContent.Replace("\0\u0005\0\0\0None\0\0\0\0", "");

                                fileContents[songName] += songContent;

                                // Move it to a new backup folder
                                File.Move(f, Path.Combine(backupDir, filename));
                            }
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to backup files.  If a backup folder already exists in the Save Folder that should now open, rename, move, or delete it");
                    button1.Enabled = true;
                    button2.Enabled = false;
                    Process.Start(SaveManager.SaveFilePath);
                    return;
                }

                // I think old lutemod saved the track name into the file or something, it's there on a few specific ones
                // This might be why dashes were disallowed, doing [^-;] would be great, but instead I'll put the naming thing and make it optional
                Regex tempoRegex = new Regex(@"^\|[a-zA-Z0-9]?[a-zA-Z0-9 -]*[a-zA-Z0-9]?;([0-9]+)\|");
                Regex noteRegex = new Regex(@"([0-9]+)-([0-9]+)-([0-9]+);");

                int lowNote = ConfigManager.GetIntegerProperty(PropertyItem.LowestPlayedNote);

                List<string> errorSongs = new List<string>();

                foreach (var song in fileContents.Keys)
                {
                    try
                    {
                        // Parse out each command and convert it to MidiNote
                        // Create a collection of MidiNotes for each set/song
                        // For all notes of type 1, increase the second number (ex 1320-14-1;) by 12*octaveAdjust
                        // Run the collection back through the partition save routine
                        List<LuteMod.Sequencing.Note> notes = new List<LuteMod.Sequencing.Note>();

                        string content = fileContents[song];

                        var tempoMatch = tempoRegex.Match(content);


                        var converter = new LuteMod.Converter.MordhauConverter();
                        converter.Range = ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount);
                        converter.LowNote = lowNote;
                        converter.IsConversionEnabled = true;
                        //converter.SetDivision(player.sequence.Division);
                        converter.AddTrack();

                        converter.SetPartitionTempo(int.Parse(tempoMatch.Groups[1].Value));
                        converter.SetDivision(1);

                        var noteMatches = noteRegex.Matches(content);
                        foreach (Match noteMatch in noteMatches)
                        {
                            int tone = int.Parse(noteMatch.Groups[2].Value);
                            int noteType = int.Parse(noteMatch.Groups[3].Value);
                            if (noteType == 1)
                                tone = tone + octaveAdjust * 12 + lowNote;
                            notes.Add(new LuteMod.Sequencing.Note() { Tick = int.Parse(noteMatch.Groups[1].Value), Tone = tone, Type = (LuteMod.Sequencing.NoteType)noteType });
                        }

                        // result.Add(new LuteMod.Sequencing.Note() { Tick = tempTick, Tone = tempMessage.Data1 + offset, Type = LuteMod.Sequencing.NoteType.On });


                        converter.FillTrack(0, notes); // We let it re-process them to fit new ranges, which is why we re-added lowNote above - it needs it, and will remove it again
                                                       // Interestingly, this also automatically moves things for our lute update, which is great
                        SaveManager.WriteSaveFile(Path.Combine(SaveManager.SaveFilePath, song), converter.GetPartitionToString());

                    }
                    catch
                    {
                        errorSongs.Add(song);
                    }

                }

                if (errorSongs.Count > 0)
                {
                    MessageBox.Show("Some songs caused exceptions while converting: " + string.Join("\n", errorSongs) + "\n\nYou can restore these from the backup folder that was opened");
                    Process.Start(backupDir);
                }
                else
                {
                    //index.SaveIndex();
                    //PopulateIndexList();
                    Close();
                    MessageBox.Show("Successfully converted all songs");
                }
            }
            else
            {
                MessageBox.Show("Please enter a value in the Octave Adjust field");
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
