using LuteBot.Config;
using LuteBot.TrackSelection;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.Core.Midi
{
    public class MidiPlayer : Player
    {
        private OutputDevice outDevice;
        private Sequence sequence;
        private Sequencer sequencer;
        public MordhauOutDevice mordhauOutDevice;
        public RustOutDevice rustOutDevice;
        private TrackSelectionManager trackSelectionManager;

        private bool isPlaying;

        public MidiPlayer(TrackSelectionManager trackSelectionManager)
        {
            isPlaying = false;
            mordhauOutDevice = new MordhauOutDevice();
            rustOutDevice = new RustOutDevice();
            this.trackSelectionManager = trackSelectionManager;
            sequence = new Sequence
            {
                Format = 1
            };

            sequencer = new Sequencer
            {
                Position = 0,
                Sequence = this.sequence
            };
            sequencer.PlayingCompleted += new System.EventHandler(this.HandlePlayingCompleted);
            sequencer.ChannelMessagePlayed += new System.EventHandler<Sanford.Multimedia.Midi.ChannelMessageEventArgs>(this.HandleChannelMessagePlayed);
            sequencer.SysExMessagePlayed += new System.EventHandler<Sanford.Multimedia.Midi.SysExMessageEventArgs>(this.HandleSysExMessagePlayed);
            sequencer.Chased += new System.EventHandler<Sanford.Multimedia.Midi.ChasedEventArgs>(this.HandleChased);
            sequencer.Stopped += new System.EventHandler<Sanford.Multimedia.Midi.StoppedEventArgs>(this.HandleStopped);

            if (!(OutputDevice.DeviceCount == 0))
            {
                outDevice = new OutputDevice(ConfigManager.GetIntegerProperty(PropertyItem.OutputDevice));
                sequence.LoadCompleted += HandleLoadCompleted;
            }
        }

        public void ResetDevice()
        {
            outDevice.Reset();
            outDevice.Dispose();
            outDevice = new OutputDevice(ConfigManager.GetIntegerProperty(PropertyItem.OutputDevice));
        }

        public void UpdateMutedTracks(TrackItem item)
        {
            if (isPlaying)
            {
                sequencer.Stop();
            }
            sequence.UpdateMutedTracks(item.Id, item.Active);
            if (isPlaying)
            {
                sequencer.Continue();
            }
            outDevice.Reset();
        }

        public List<int> GetMidiChannels()
        {
            return sequence.Channels;
        }

        public List<string> GetMidiTracks()
        {
            return sequence.TrackNames();
        }

        public override int GetLenght()
        {
            return sequence.GetLength();
        }

        public override string GetFormattedPosition()
        {
            TimeSpan time = TimeSpan.FromSeconds(0);
            time = TimeSpan.FromSeconds((int)((sequencer.Position / sequence.Division) * this.sequence.FirstTempo / 1000000f));
            return time.ToString(@"mm\:ss");
        }

        public override string GetFormattedLength()
        {
            TimeSpan time = TimeSpan.FromSeconds(0);
            time = TimeSpan.FromSeconds((int)((sequence.GetLength() / sequence.Division) * this.sequence.FirstTempo / 1000000f));
            return time.ToString(@"mm\:ss");
        }

        public override void SetPosition(int position)
        {
            sequencer.Position = position;
        }

        public override int GetPosition()
        {
            return sequencer.Position;
        }

        public override void ResetSoundEffects()
        {
            outDevice.Reset();
        }

        public override void LoadFile(string filename)
        {
            sequence.LoadAsync(filename);
            /*
            if (filename.Contains(@"\"))
            {
                // Trim it to just the midi name
                filename = filename.Substring(filename.LastIndexOf(@"\"));
                filename = filename.Replace(".mid", ".lua");
                mcPath = $@"C:\Users\DMentia\AppData\Roaming\LuteBot\{filename}";
            }
            */
        }
        //private string mcPath = @"C:\Users\DMentia\AppData\Roaming\LuteBot\minecraftSong.txt";

        /*
        private void FinalizeMC()
        {
            if (mordhauOutDevice != null && mordhauOutDevice.McFile != null)
            {
                string entireFile = mordhauOutDevice.McFile.ToString();
                entireFile = entireFile.Insert(0,
                    "local speed = 1\r\n\r\n\r\nlocal speaker = peripheral.find(\"speaker\")\r\n");
                // Now check through each MidiChannel type, and if it's been used we instantiate it up top
                for (int i = 0; i < MidiChannelTypes.Names.Length; i++)
                {
                    string realName = MidiChannelTypes.Names[i].Replace(" ", "").Replace("-", "");
                    if (entireFile.Contains(realName))
                    {
                        entireFile = entireFile.Insert(0, $"_G.{realName}{i} = \"Guitar\"\r\n");
                    }
                }

                using (StreamWriter writer = new StreamWriter(mcPath, false))
                    writer.Write(entireFile);

                entireFile = null;
                mordhauOutDevice.McFile = null;
            }
        }
        */

        public override void Stop()
        {
            //FinalizeMC();
            isPlaying = false;
            sequencer.Stop();
            sequencer.Position = 0;
            outDevice.Reset();
        }

        public override void Play()
        {
            //if (File.Exists(mcPath))
            //    File.Delete(mcPath);
            //mordhauOutDevice.McFile = new StringBuilder();
            //mordhauOutDevice.stopWatch.Restart();

            //midiOut = Melanchall.DryWetMidi.Devices.OutputDevice.GetByName("Rust");

            isPlaying = true;
            if (sequencer.Position > 0)
            {
                sequencer.Continue();
            }
            else
            {
                sequencer.Start();
            }
        }

        public override void Pause()
        {
            //midiOut.Dispose();
            isPlaying = false;
            sequencer.Stop();
            outDevice.Reset();
        }

        public override void Dispose()
        {
            this.disposed = true;
            sequence.Dispose();

            if (outDevice != null)
            {
                outDevice.Dispose();
            }
        }

        //------------- Handlers -------------

        private int[] drumNoteCounts = new int[128];
        public List<KeyValuePair<int, int>> DrumNoteCounts = new List<KeyValuePair<int, int>>(128);

        private void HandleLoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                // We need to manually parse out data for Max and Min note for each Channel, in trackSelectionManager
                trackSelectionManager.MaxNoteByChannel = new Dictionary<int, int>();
                trackSelectionManager.MinNoteByChannel = new Dictionary<int, int>();
                foreach(var track in sequence)
                {
                    foreach(var midiEvent in track.Iterator())
                    {

                        if (midiEvent.MidiMessage is ChannelMessage c && c.Data2 > 0 && c.Command == ChannelCommand.NoteOn)
                        {
                            if(c.MidiChannel == 6) // Glockenspiel...
                            {
                                drumNoteCounts[c.Data1]++; // They're all 0 by default
                            }

                            if (!trackSelectionManager.MaxNoteByChannel.ContainsKey(c.MidiChannel))
                                trackSelectionManager.MaxNoteByChannel.Add(c.MidiChannel, c.Data1);
                            else if (trackSelectionManager.MaxNoteByChannel[c.MidiChannel] < c.Data1)
                                trackSelectionManager.MaxNoteByChannel[c.MidiChannel] = c.Data1;

                            if (!trackSelectionManager.MinNoteByChannel.ContainsKey(c.MidiChannel))
                                trackSelectionManager.MinNoteByChannel.Add(c.MidiChannel, c.Data1);
                            else if (trackSelectionManager.MinNoteByChannel[c.MidiChannel] > c.Data1)
                                trackSelectionManager.MinNoteByChannel[c.MidiChannel] = c.Data1;
                        }
                    }
                }
                // Now let's get a sorted list of drum note counts
                // I can't figure out how to do this nicely.
                for (int i = 0; i < drumNoteCounts.Length; i++) {
                    DrumNoteCounts.Add(new KeyValuePair<int, int>(i, drumNoteCounts[i]));
                }
                DrumNoteCounts = DrumNoteCounts.OrderBy((kvp) => kvp.Value).ToList();

                base.LoadCompleted(this, e);
                mordhauOutDevice.HighMidiNoteId = sequence.MaxNoteId;
                mordhauOutDevice.LowMidiNoteId = sequence.MinNoteId;
                rustOutDevice.HighMidiNoteId = sequence.MaxNoteId;
                rustOutDevice.LowMidiNoteId = sequence.MinNoteId;
            }
        }

        private void HandleStopped(object sender, StoppedEventArgs e)
        {
            foreach (ChannelMessage message in e.Messages)
            {
                if (ConfigManager.GetBooleanProperty(PropertyItem.SoundEffects) && !disposed)
                {
                    outDevice.Send(message);
                }
            }
        }

        private void HandleChased(object sender, ChasedEventArgs e)
        {
            if (ConfigManager.GetBooleanProperty(PropertyItem.SoundEffects) && !disposed)
            {
                foreach (ChannelMessage message in e.Messages)
                {
                    outDevice.Send(message);
                }
            }
        }

        private void HandleSysExMessagePlayed(object sender, SysExMessageEventArgs e)
        {
        }

        private void HandleChannelMessagePlayed(object sender, ChannelMessageEventArgs e)
        {
            if (ConfigManager.GetProperty(PropertyItem.NoteConversionMode) != "1")
            {
                if (ConfigManager.GetBooleanProperty(PropertyItem.SoundEffects) && !disposed)
                {
                    var filtered = trackSelectionManager.FilterMidiEvent(e.Message, e.TrackId);
                    //if (e.Message.Data2 > 0) // Avoid spamming server with 0 velocity notes
                    //{

                    //Melanchall.DryWetMidi.Core.NoteEvent nEvent;
                    // I don't know how to do this without Melanchall package but it's gonna be a bitch to convert the event types here...


                    //midiOut.SendEvent(new Melanchall.DryWetMidi.Core.NoteOnEvent((Melanchall.DryWetMidi.Common.SevenBitNumber)filtered.Data1, (Melanchall.DryWetMidi.Common.SevenBitNumber)filtered.Data2));
                    // Below: Sound to match, then unfiltered sound.  Or leave commented for no sound.
                    if (!outDevice.IsDisposed) // If they change song prefs while playing, this can fail, so just skip then
                        try
                        {
                            if (ConfigManager.GetIntegerProperty(PropertyItem.Instrument) == 9)
                            {
                                // Drums... 
                                // Ignore any notes that aren't on glockenspiel
                                if (filtered.MidiChannel == 9) // glocken
                                {
                                    // Figure out where it ranks on DrumNoteCounts
                                    //int drumNote = 0;
                                    //for(int i = 0; i < DrumNoteCounts.Count(); i++)
                                    //{ 
                                    //    if(DrumNoteCounts[i].Key == filtered.Data1)
                                    //    {
                                    //        drumNote = i;
                                    //        break;
                                    //    }
                                    //}
                                    // Assume it's no longer 0 for now...
                                    // Now just map it to a dictionary of most popular notes on drums
                                    if(DrumMappings.MidiToRustMap.ContainsKey(filtered.Data1))
                                    {
                                        var newNote = new ChannelMessage(filtered.Command, filtered.MidiChannel, DrumMappings.MidiToRustMap[filtered.Data1], filtered.Data2);
                                        outDevice.Send(newNote);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                var note = rustOutDevice.FilterNote(filtered, trackSelectionManager.NoteOffset +
                                    (trackSelectionManager.MidiChannelOffsets.ContainsKey(e.Message.MidiChannel) ? trackSelectionManager.MidiChannelOffsets[e.Message.MidiChannel] : 0));
                                if (note != null)
                                    outDevice.Send(note);
                            }
                        }
                        catch (Exception) { } // Ignore exceptions, again, they might edit things while it's trying to play
                    //}
                    //outDevice.Send(filtered);
                }
                else
                {
                    mordhauOutDevice.SendNote(trackSelectionManager.FilterMidiEvent(e.Message, e.TrackId), trackSelectionManager.NoteOffset + 
                       (trackSelectionManager.MidiChannelOffsets.ContainsKey(e.Message.MidiChannel) ? trackSelectionManager.MidiChannelOffsets[e.Message.MidiChannel] : 0));
                }
            }
            else
            {
                outDevice.Send(trackSelectionManager.FilterMidiEvent(e.Message, e.TrackId));
            }
        }

        private void HandlePlayingCompleted(object sender, EventArgs e)
        {
            //FinalizeMC();
            //midiOut.Dispose();
            isPlaying = false;
            outDevice.Reset();            
        }
    }

    public static class DrumMappings
    {
        // These strings represent what Rust wants for each of them
        public static readonly int Kick = 36;
        public static readonly int Kick2 = 36;
        public static readonly int Snare = 38;
        public static readonly int SnareLight = 38;
        public static readonly int Snare3 = 38;
        public static readonly int HiHatOpen = 46;
        public static readonly int HiHatClosed = 42;
        public static readonly int TomHigh = 47;
        public static readonly int Cowbell = 35;
        public static readonly int TomMid = 48;
        public static readonly int TomLow = 43;
        //public static readonly int HiHatStep = 57;
        public static readonly int Ride = 49;
        //public static readonly int RideBell = 59;
        public static readonly int Crash = 55;


        /// <summary>
        /// Note that the key is the midi input, the value is the suggested Rust output
        /// </summary>
        public static Dictionary<int, int> MidiToRustMap = new Dictionary<int, int>
        {
            {35, Kick },
            {36, Kick2 },
            {37, SnareLight },
            {38, Snare },
            {39, Snare3 },
            {40, SnareLight },
            {41, TomLow },
            {42, HiHatClosed },
            {43, TomLow },
            {44, HiHatClosed },
            {45, TomLow },
            {46, HiHatOpen },
            {47, TomMid },
            {48, TomMid },
            {49, Crash },
            {50, TomHigh },
            {51, Ride },
            {52, Crash },
            {53, Cowbell },
            {54, HiHatClosed },
            {55, Crash },
            {56, Cowbell },
            {57, Crash },
            {58, Snare }, // No idea, "Vibraslap"
            {59, Ride },
            {60, TomHigh },
            {61, TomLow },
            {62, TomHigh },
            {63, TomHigh },
            {64, TomLow },
            {65, TomHigh },
            {66, TomLow },
            {67, TomHigh },
            {68, TomLow },
            {69, HiHatClosed }, // No idea, "Cabasa
            {70, HiHatClosed },
            {71, Cowbell },
            {72, Cowbell },
            {73, TomMid },
            {74, TomMid },
            {75, TomMid },
            {76, TomHigh },
            {77, TomLow },
            {78, HiHatClosed },
            {79, HiHatClosed },
            {80, HiHatClosed },
            {81, HiHatClosed }
        };
    }
}

