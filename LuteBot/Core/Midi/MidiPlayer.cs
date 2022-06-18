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
    public class MidiPlayer : Player, IDisposable
    {
        private OutputDevice outDevice;
        public Sequence sequence;
        private Sequencer sequencer;
        public MordhauOutDevice mordhauOutDevice;
        public RustOutDevice rustOutDevice;
        public TrackSelectionManager trackSelectionManager;

        private bool isPlaying;
        private Dictionary<int, MidiChannelItem> channels = new Dictionary<int, MidiChannelItem>();
        private Dictionary<int, TrackItem> tracks = new Dictionary<int, TrackItem>();

        public Dictionary<int, List<MidiNote>> tickMetaNotes { get; private set; } = new Dictionary<int, List<MidiNote>>();

        public MidiPlayer(TrackSelectionManager trackSelectionManager)
        {
            trackSelectionManager.Player = this;
            isPlaying = false;
            mordhauOutDevice = new MordhauOutDevice(trackSelectionManager);
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
            sequence.LoadCompleted += HandleLoadCompleted;

            if (!(OutputDevice.DeviceCount == 0))
            {
                outDevice = new OutputDevice(ConfigManager.GetIntegerProperty(PropertyItem.OutputDevice));
            }
        }

        public void ResetDevice()
        {
            if (outDevice != null)
            {
                outDevice.Reset();
                //outDevice.Dispose();
            }
            //try
            //{
            //    outDevice = new OutputDevice(ConfigManager.GetIntegerProperty(PropertyItem.OutputDevice));
            //}
            //catch { } // TODO: Somehow alert them that it didn't work
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
            if (outDevice != null)
                outDevice.Reset();
        }

        public Dictionary<int, MidiChannelItem> GetMidiChannels()
        {
            return channels;
        }

        public Dictionary<int, TrackItem> GetMidiTracks()
        {
            return tracks;
        }

        public override int GetLength()
        {
            return sequence.GetLength();
        }

        public override string GetFormattedPosition()
        {
            // Let's try doing this by percentage of sequencer.Position vs formattedLength

            TimeSpan time = TimeSpan.FromSeconds(0);
            if (lastDuration != TimeSpan.Zero && lastLength != 0)
            {
                double percentage = (double)sequencer.Position / lastLength; // We don't want to GetLength() every time so this works
                double totalMinutes = lastDuration.TotalMinutes * percentage;
                time = TimeSpan.FromMinutes(totalMinutes);
            }
            else // Old method, in case something screws up
                time = TimeSpan.FromSeconds((((double)sequencer.Position / sequence.Division) * this.sequence.FirstTempo / 1000000d));
            return time.ToString(@"mm\:ss");
        }

        private TimeSpan lastDuration = TimeSpan.Zero;
        private int lastLength = 0;

        public TimeSpan GetTimeLength()
        {
            Dictionary<int, int> tempoEvents = new Dictionary<int, int>(); // Ordered by: AbsoluteTicks, Tempo
            int id = 0;
            foreach (var t in sequence.Tracks)
            {
                foreach (var something in t.Iterator())
                {
                    if (something.MidiMessage.MessageType == MessageType.Meta)
                    {
                        MetaMessage meta = (MetaMessage)something.MidiMessage;
                        if (meta.MetaType == MetaType.Tempo)
                        {
                            // I think, something.AbsoluteTicks is useful
                            // As for getting the actual Tempo out of it... 
                            var bytes = meta.GetBytes();
                            // Apparently it's... backwards?
                            byte[] tempoBytes = new byte[4];
                            tempoBytes[2] = bytes[0];
                            tempoBytes[1] = bytes[1];
                            tempoBytes[0] = bytes[2];
                            int tempo = BitConverter.ToInt32(tempoBytes, 0);
                            // This seems legit
                            tempoEvents[something.AbsoluteTicks] = tempo;
                        }
                    }
                }
            }
            TimeSpan time = TimeSpan.Zero;
            int lastTempo = sequence.FirstTempo;
            int lastTime = 0;
            foreach (var kvp in tempoEvents)
            {
                time += TimeSpan.FromSeconds((((double)(kvp.Key - lastTime) / sequence.Division) * lastTempo / 1000000d));
                lastTempo = kvp.Value;
                lastTime = kvp.Key;
            }
            lastLength = sequence.GetLength();
            // And then get the rest to the end
            if (time != TimeSpan.Zero)
                time += TimeSpan.FromSeconds((((double)(lastLength - lastTime) / sequence.Division) * lastTempo / 1000000d));


            //time = TimeSpan.FromSeconds((int)((sequence.GetLength() / sequence.Division) * this.sequence.FirstTempo / 1000000f));
            if (time == TimeSpan.Zero)
                time = TimeSpan.FromSeconds((((double)lastLength / sequence.Division) * this.sequence.FirstTempo / 1000000d)); // Old method
            lastDuration = time;

            return time;
        }

        public override string GetFormattedLength()
        {
            return GetTimeLength().ToString(@"mm\:ss");
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
            if (outDevice != null)
                outDevice.Reset();
        }


        public object loadLock = new object();
        private bool loading = false;

        public override void LoadFile(string filename)
        {
            lock (loadLock)
            {
                channels = new Dictionary<int, MidiChannelItem>();
                tracks = new Dictionary<int, TrackItem>();
                // Reset the sequence because we can't cancel the load or detect if one is occurring
                //sequence.LoadCompleted -= HandleLoadCompleted;
                //sequence.Dispose();
                //sequence = new Sequence() { Format = 1 };
                //sequencer.Sequence = sequence;
                //sequence.LoadCompleted += HandleLoadCompleted;
                sequence.LoadAsync(filename);
            }
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
            if (outDevice != null)
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
            if (outDevice != null && !outDevice.IsDisposed)
                outDevice.Reset();
        }


        //------------- Handlers -------------

        private int[] drumNoteCounts = new int[128];
        public List<KeyValuePair<int, int>> DrumNoteCounts = new List<KeyValuePair<int, int>>(128);

        // Let's add something new here: Either save data we can use to do it, or do it
        // Detect the best Flute instrument

        // The best flute instrument: Has many notes, has long notes, covers a large octave area
        // So while we already track the octaves covered, we should track numNotes and avgNoteLength of each track or channel

        // That also might mean we need to start tracking data for tracks, instead of just channels, and allow a toggle to show those instead

        private void HandleLoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            tickMetaNotes.Clear();
            var startTime = DateTime.Now;
            lock (loadLock)
            {
                if (e.Error == null)
                {
                    //channelNames = new Dictionary<int, string>();
                    //trackNames = new Dictionary<int, string>();

                    channels = new Dictionary<int, MidiChannelItem>();
                    tracks = new Dictionary<int, TrackItem>();

                    // Set default channel names
                    foreach (var channelNum in sequence.Channels)
                    {
                        var midiChannel = new MidiChannelItem();
                        midiChannel.Id = channelNum;
                        midiChannel.Name = MidiChannelTypes.Names[channelNum];
                        midiChannel.Active = true;
                        channels[channelNum] = midiChannel;
                    }
                    foreach (var midiTrack in sequence.Tracks)
                    {
                        var track = new TrackItem();
                        track.Id = midiTrack.Id;
                        if (!string.IsNullOrWhiteSpace(midiTrack.Name))
                            track.Name = midiTrack.Name;
                        else
                            track.Name = "Untitled Track";
                        track.lowestNote = midiTrack.MinNoteId;
                        track.highestNote = midiTrack.MaxNoteId;
                        track.Active = true;
                        tracks[midiTrack.Id] = track;
                    }

                    //Parallel.ForEach(sequence, track =>
                    //{
                    int lastTempo = sequence.FirstTempo;

                    Dictionary<int, int> lastNote = new Dictionary<int, int>();
                    Dictionary<int, int> lastTrackNote = new Dictionary<int, int>();

                    foreach (var track in sequence)
                    {
                        foreach (var midiEvent in track.Iterator())
                        {
                            if (midiEvent.MidiMessage is ChannelMessage c)
                            {
                                if (!channels.ContainsKey(c.MidiChannel))
                                {
                                    var midiChannel = new MidiChannelItem();
                                    midiChannel.Id = c.MidiChannel;
                                    midiChannel.Name = MidiChannelTypes.Names[c.MidiChannel];
                                    midiChannel.Active = true;
                                    channels[c.MidiChannel] = midiChannel;
                                }


                                if (c.Data2 > 0 && c.Command == ChannelCommand.NoteOn)
                                {
                                    if (c.MidiChannel == 9) // Glockenspiel - always channel 9
                                    {
                                        drumNoteCounts[c.Data1]++; // They're all 0 by default
                                        channels[c.MidiChannel].Active = false; // Disabled by default
                                    }

                                    if (c.Data1 > channels[c.MidiChannel].highestNote)
                                        channels[c.MidiChannel].highestNote = c.Data1;
                                    if (c.Data1 < channels[c.MidiChannel].lowestNote)
                                        channels[c.MidiChannel].lowestNote = c.Data1;

                                    // We'll pre-filter drums from tracks, because their only purpose right now is to be used for ML
                                    if (c.MidiChannel != 9)
                                    {
                                        if (c.Data1 > tracks[track.Id].highestNote)
                                            tracks[track.Id].highestNote = c.Data1;
                                        if (c.Data1 < tracks[track.Id].lowestNote)
                                            tracks[track.Id].lowestNote = c.Data1;

                                        tracks[track.Id].noteTicks[c.Data1] = midiEvent.AbsoluteTicks;
                                        tracks[track.Id].numNotes++;

                                        if (lastTrackNote.ContainsKey(track.Id))
                                        {
                                            tracks[track.Id].avgVariation += Math.Abs(c.Data1 - lastTrackNote[track.Id]);
                                        }
                                        lastTrackNote[track.Id] = c.Data1;

                                        if (!tracks[track.Id].tickNotes.ContainsKey(midiEvent.AbsoluteTicks))
                                            tracks[track.Id].tickNotes[midiEvent.AbsoluteTicks] = new List<MidiNote>();

                                        tracks[track.Id].tickNotes[midiEvent.AbsoluteTicks].Add(new MidiNote() { note = c.Data1, tickNumber = midiEvent.AbsoluteTicks, track = track.Id, channel = c.MidiChannel });
                                    }

                                    // Track noteOn events and when we get a noteOff or velocity 0, add up note lengths
                                    channels[c.MidiChannel].noteTicks[c.Data1] = midiEvent.AbsoluteTicks;
                                    channels[c.MidiChannel].numNotes++;

                                    if (lastNote.ContainsKey(c.MidiChannel))
                                    {
                                        channels[c.MidiChannel].avgVariation += Math.Abs(c.Data1 - lastNote[c.MidiChannel]);
                                    }
                                    lastNote[c.MidiChannel] = c.Data1;

                                    if (!channels[c.MidiChannel].tickNotes.ContainsKey(midiEvent.AbsoluteTicks))
                                        channels[c.MidiChannel].tickNotes[midiEvent.AbsoluteTicks] = new List<MidiNote>();

                                    channels[c.MidiChannel].tickNotes[midiEvent.AbsoluteTicks].Add(new MidiNote() { note = c.Data1, tickNumber = midiEvent.AbsoluteTicks, track = track.Id, channel = c.MidiChannel });

                                    // Ideally I'd like to parse out chords of size 3 or more, using some setting like RemoveLowest, RemoveHighest, RemoveMiddle
                                    // Though I'm unsure how I'd really do that to anything except lutemod; lutebot is reading straight from the midi data isn't it?
                                }
                                else if (c.Command == ChannelCommand.ProgramChange)
                                {
                                    string channelName = MidiChannelTypes.Names[c.Data1];
                                    channels[c.MidiChannel].Name = channelName;
                                    channels[c.MidiChannel].midiInstrument = c.Data1;
                                    // Tracks can't really have this, can they... Well, we can just, vaguely take the most recent one
                                    tracks[track.Id].midiInstrument = c.Data1;
                                }
                                else if (c.Command == ChannelCommand.NoteOff || (c.Command == ChannelCommand.NoteOn && c.Data2 == 0))
                                {

                                    if (channels[c.MidiChannel].noteTicks.ContainsKey(c.Data1))
                                    {
                                        // Find the original note...
                                        var startTick = channels[c.MidiChannel].noteTicks[c.Data1];
                                        var length = midiEvent.AbsoluteTicks - startTick; // How to turn number of ticks into a length with tempo in mind?
                                                                                          // TimeSpan.FromSeconds((((double)(deltaTick) / sequence.Division) * lastTempo / 1000000d))

                                        var timeLength = (length / (float)sequence.Division) * lastTempo / 1000000f;

                                        var note = channels[c.MidiChannel].tickNotes[startTick].Where(n => n.note == c.Data1).FirstOrDefault();
                                        note.length = length;
                                        note.timeLength = timeLength;


                                        // If we already have a note with a length on this same tick, we don't need to add to totalNoteLength...?
                                        // I want to avoid a chord of 3 having 3x the detected total length... 
                                        // I'll just have to deal with that later, and we won't bother tracking totalLength here

                                        // Just add all the lengths, we'll divide after we read everything
                                        channels[c.MidiChannel].avgNoteLength += timeLength;
                                        channels[c.MidiChannel].noteTicks.Remove(c.Data1);
                                    }
                                    if (tracks[track.Id].noteTicks.ContainsKey(c.Data1))
                                    {
                                        // Find the original note...
                                        var startTick = tracks[track.Id].noteTicks[c.Data1];
                                        var length = midiEvent.AbsoluteTicks - startTick; // How to turn number of ticks into a length with tempo in mind?
                                                                                          // TimeSpan.FromSeconds((((double)(deltaTick) / sequence.Division) * lastTempo / 1000000d))

                                        var timeLength = (length / (float)sequence.Division) * lastTempo / 1000000f;

                                        var note = tracks[track.Id].tickNotes[startTick].Where(n => n.note == c.Data1).FirstOrDefault();
                                        note.length = length;
                                        note.timeLength = timeLength;


                                        // If we already have a note with a length on this same tick, we don't need to add to totalNoteLength...?
                                        // I want to avoid a chord of 3 having 3x the detected total length... 
                                        // I'll just have to deal with that later, and we won't bother tracking totalLength here

                                        // Just add all the lengths, we'll divide after we read everything
                                        tracks[track.Id].avgNoteLength += timeLength;
                                        tracks[track.Id].noteTicks.Remove(c.Data1);
                                    }
                                }
                            }
                            else if (midiEvent.MidiMessage is MetaMessage meta)
                            {
                                if (meta.MetaType == MetaType.TrackName)
                                {
                                    var bytes = meta.GetBytes();
                                    // Byte 3 is the length, bytes 4+ are the text in ascii
                                    // But apparently the library trims out that for me, and the entire bytes array is what we want
                                    if (bytes.Length > 0)
                                    {
                                        string trackName = ASCIIEncoding.ASCII.GetString(bytes);
                                        tracks[track.Id].Name = trackName;
                                    }
                                }
                                else if (meta.MetaType == MetaType.Tempo)
                                {
                                    var newTempo = BytesToInt(meta.GetBytes()); // We divide by division when converting for lutemod only, apparently

                                    if (!tickMetaNotes.ContainsKey(midiEvent.AbsoluteTicks))
                                        tickMetaNotes[midiEvent.AbsoluteTicks] = new List<MidiNote>();

                                    tickMetaNotes[midiEvent.AbsoluteTicks].Add(new MidiNote() { note = newTempo, tickNumber = midiEvent.AbsoluteTicks, track = track.Id });
                                    lastTempo = newTempo;

                                }
                            }
                        }
                    }//);

                    // So, now we have a totalNoteLength... except, no.  That's not quite useful
                    // We do have the list of all notes with lengths on them, though.  
                    // So we can get a total length of parts by, for each tick/entry, take the longest length for that tick
                    // And then only count new notes if their start+length > thatTick+thatLength


                    // Compile noteLengths of channels
                    foreach (var kvp in channels)
                    {
                        var channel = kvp.Value;
                        

                        int nextValidTick = 0;
                        channel.averageNote = 0;

                        foreach (var noteKvp in channel.tickNotes.OrderBy(n => n.Key))
                        {
                            var noteList = noteKvp.Value;
                            var note = noteList.OrderByDescending(n => n.length).First();

                            if (note.tickNumber > nextValidTick)
                            {
                                channel.totalNoteLength += note.timeLength;
                                nextValidTick = note.tickNumber + note.length;
                            }
                            else if (note.tickNumber + note.length > nextValidTick)
                            {
                                var diff = (note.tickNumber + note.length) - nextValidTick;
                                channel.totalNoteLength += note.timeLength * (diff/note.length);
                                nextValidTick = nextValidTick + diff;
                            }
                            foreach(var n in noteList)
                                channel.averageNote += n.note;
                        }
                        if (channel.numNotes > 0)
                        {
                            channel.avgNoteLength = channel.avgNoteLength / channel.numNotes;
                            channel.averageNote = (int)((float)channel.averageNote / channel.numNotes);
                            channel.avgVariation = channel.avgVariation / channel.numNotes;
                        }

                        if (channel.tickNotes.Count > 0)
                            channel.maxChordSize = channel.tickNotes.Max(t => t.Value.Count);
                    }
                    foreach (var kvp in tracks)
                    {
                        var channel = kvp.Value;


                        int nextValidTick = 0;
                        channel.averageNote = 0;

                        foreach (var noteKvp in channel.tickNotes.OrderBy(n => n.Key))
                        {
                            var noteList = noteKvp.Value;
                            var note = noteList.OrderByDescending(n => n.length).First();

                            if (note.tickNumber > nextValidTick)
                            {
                                channel.totalNoteLength += note.timeLength;
                                nextValidTick = note.tickNumber + note.length;
                            }
                            else if (note.tickNumber + note.length > nextValidTick)
                            {
                                var diff = (note.tickNumber + note.length) - nextValidTick;
                                channel.totalNoteLength += note.timeLength * (diff / note.length);
                                nextValidTick = nextValidTick + diff;
                            }
                            foreach (var n in noteList)
                                channel.averageNote += n.note;
                        }
                        if (channel.numNotes > 0)
                        {
                            channel.avgNoteLength = channel.avgNoteLength / channel.numNotes;
                            channel.averageNote = (int)((float)channel.averageNote / channel.numNotes);
                            channel.avgVariation = channel.avgVariation / channel.numNotes;
                        }

                        if (channel.tickNotes.Count > 0)
                            channel.maxChordSize = channel.tickNotes.Max(t => t.Value.Count);
                    }

                    // Add a note on the drum channel
                    if (channels.ContainsKey(9))
                        channels[9].Name += " (DRUMS)";

                    // Remove any channels or tracks that don't contain any notes
                    channels = channels.Values.Where(c => c.numNotes > 0).ToDictionary(c => c.Id);
                    tracks = tracks.Values.Where(c => c.numNotes > 0).ToDictionary(c => c.Id);

                    // When we're finished, it would also be nice to find the first actual audible note, move any tempo events to that tick, and make that tick 0
                    var minimumTick = channels.Values.SelectMany(c => c.tickNotes).SelectMany(n => n.Value).Min(n => n.tickNumber); // This will have gotten all notes in tracks too

                    if (tickMetaNotes.Count > 0)
                    {
                        var temposToMove = tickMetaNotes.Select(kvp => kvp.Value).SelectMany(nl => nl).Where(n => n.tickNumber < minimumTick);
                        foreach (var n in temposToMove)
                            n.tickNumber = minimumTick;
                    }
                    // Then just subtract minimumTick from every note, including meta notes
                    // And store it back in under its adjusted key... I guess we need a temp dictionary for that
                    var tempMetaNotes = new Dictionary<int, List<MidiNote>>();
                    
                    foreach (var nl in tickMetaNotes.Values)
                    {
                        foreach (var n in nl)
                        {
                            n.tickNumber -= minimumTick;
                            if (!tempMetaNotes.ContainsKey(n.tickNumber))
                                tempMetaNotes[n.tickNumber] = new List<MidiNote>();
                            tempMetaNotes[n.tickNumber].Add(n);
                        }
                    }
                    foreach(var c in channels.Values)
                    {
                        var tempNotes = new Dictionary<int, List<MidiNote>>();
                        foreach (var nl in c.tickNotes.Values)
                        {
                            foreach (var n in nl)
                            {
                                n.tickNumber -= minimumTick;
                                if (!tempNotes.ContainsKey(n.tickNumber))
                                    tempNotes[n.tickNumber] = new List<MidiNote>();
                                tempNotes[n.tickNumber].Add(n);
                            }
                        }
                        c.tickNotes = tempNotes;
                    }
                    foreach (var c in tracks.Values)
                    {
                        var tempNotes = new Dictionary<int, List<MidiNote>>();
                        foreach (var nl in c.tickNotes.Values)
                        {
                            foreach (var n in nl)
                            {
                                n.tickNumber -= minimumTick;
                                if (!tempNotes.ContainsKey(n.tickNumber))
                                    tempNotes[n.tickNumber] = new List<MidiNote>();
                                tempNotes[n.tickNumber].Add(n);
                            }
                        }
                        c.tickNotes = tempNotes;
                    }

                    tickMetaNotes = tempMetaNotes;
                    

                    Console.WriteLine("Read track data in " + (DateTime.Now - startTime).TotalMilliseconds);
                    // Now let's get a sorted list of drum note counts
                    // I can't figure out how to do this nicely.
                    //for (int i = 0; i < drumNoteCounts.Length; i++)
                    //{
                    //    DrumNoteCounts.Add(new KeyValuePair<int, int>(i, drumNoteCounts[i]));
                    //}
                    //DrumNoteCounts = DrumNoteCounts.OrderBy((kvp) => kvp.Value).ToList();

                    base.LoadCompleted(this, e);

                    mordhauOutDevice.HighMidiNoteId = sequence.MaxNoteId;
                    mordhauOutDevice.LowMidiNoteId = sequence.MinNoteId;
                    rustOutDevice.HighMidiNoteId = sequence.MaxNoteId;
                    rustOutDevice.LowMidiNoteId = sequence.MinNoteId;
                }
            }
        }


        public string GetChannelName(int id)
        {
            if (channels.ContainsKey(id))
                return channels[id].Name;
            else
                return "Unknown";
        }


        private void HandleStopped(object sender, StoppedEventArgs e)
        {
            foreach (ChannelMessage message in e.Messages)
            {
                if (ConfigManager.GetBooleanProperty(PropertyItem.SoundEffects) && !disposed && outDevice != null)
                {
                    outDevice.Send(message);
                }
            }
        }

        private void HandleChased(object sender, ChasedEventArgs e)
        {
            if (ConfigManager.GetBooleanProperty(PropertyItem.SoundEffects) && !disposed && outDevice != null)
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
                    if (outDevice != null && !outDevice.IsDisposed) // If they change song prefs while playing, this can fail, so just skip then
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
                                    if (DrumMappings.MidiToRustMap.ContainsKey(filtered.Data1))
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
                                    (trackSelectionManager.MidiChannels.ContainsKey(e.Message.MidiChannel) ? trackSelectionManager.MidiChannels[e.Message.MidiChannel].offset : 0));
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
                       (trackSelectionManager.MidiChannels.ContainsKey(e.Message.MidiChannel) ? trackSelectionManager.MidiChannels[e.Message.MidiChannel].offset : 0));
                }
            }
            else
            {
                if (outDevice != null)
                    outDevice.Send(trackSelectionManager.FilterMidiEvent(e.Message, e.TrackId));
            }
        }

        private void HandlePlayingCompleted(object sender, EventArgs e)
        {
            //FinalizeMC();
            //midiOut.Dispose();
            isPlaying = false;
            if (outDevice != null)
                outDevice.Reset();
        }

        public List<LuteMod.Sequencing.Note> ExtractMidiContent()
        {
            List<LuteMod.Sequencing.Note> result = new List<LuteMod.Sequencing.Note>();
            Track tempTrack;
            ChannelMessage tempMessage;
            MetaMessage tempMetaMessage;
            int tempTick;
            bool isChannelActive = false;


            var instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            // This is kindof annoying.  It only provides tracks to iterate through, which contain events, which have an absoluteTick
            // But I want to iterate all the ticks.  I swear something had an iterator...

            // Wait.  I can just... get a list of ticks to iterate over... 
            // Let's do this one without linq and compare how bad they look, both will probably suck
            List<int> ticksToIterate = new List<int>();
            foreach (var channel in trackSelectionManager.MidiChannels.Values)
            {
                if (channel.Active)
                {
                    foreach (var kvp in channel.tickNotes)
                    {
                        foreach (var note in kvp.Value)
                        {
                            if (trackSelectionManager.MidiTracks[note.track].Active && note.active)
                                if (!ticksToIterate.Contains(kvp.Key))
                                ticksToIterate.Add(kvp.Key);
                        }
                    }
                    foreach(var kvp in tickMetaNotes)
                    {
                        if (!ticksToIterate.Contains(kvp.Key))
                            ticksToIterate.Add(kvp.Key);
                    }
                }
            }
            // Yeah still cleaner looking and more obvious that it's doing some real serious iteration

            // Problem: I need meta notes.  I can either try storing them earlier - which means finding and changing any code that was selecting through them for notes, like highest/lowest
            // Or I could store them in a separate thing, but would have to select them out separately 
            // Or I could just access the sequence and check for them.  That seems best for now

            // But later I should really just... redo... all of this.  Store all the data I need from the start, in the way I need it

            foreach (var tickNumber in ticksToIterate)
            {
                List<MidiNote> notesThisTick = trackSelectionManager.MidiChannels.Values.Where(c => c.Active).SelectMany(c => { if (c.tickNotes.TryGetValue(tickNumber, out var noteList)) return noteList.Where(n => trackSelectionManager.MidiTracks[n.track].Active); else return new List<MidiNote>(); }).GroupBy(n => n.note).SelectMany(n => n).OrderBy(n => n.note).Where(n => n.active).ToList();
                // This groupBy is a hacky Distinct method to prevent duplicate notes

                // OK here it is, all the notes for all tracks and all channels this tick
                // Find out if it's flute or lute
                if (instrumentId == 0)
                {
                    // Lute.  Add only 2? 3? NotesPerChord?

                    // Let's see what it sounds like to add only the first and last
                    if (notesThisTick.Count > 0)
                    {
                        var note = notesThisTick.First();
                        result.Add(new LuteMod.Sequencing.Note() { Tick = tickNumber, Tone = note.note + trackSelectionManager.MidiChannels[note.channel].offset + trackSelectionManager.NoteOffset, Type = LuteMod.Sequencing.NoteType.On });
                    }
                    if (notesThisTick.Count > 1 && trackSelectionManager.NumChords > 1)
                    {
                        var note = notesThisTick.Last();
                        result.Add(new LuteMod.Sequencing.Note() { Tick = tickNumber, Tone = note.note + trackSelectionManager.MidiChannels[note.channel].offset + trackSelectionManager.NoteOffset, Type = LuteMod.Sequencing.NoteType.On });
                    }
                    if (notesThisTick.Count > 2 && trackSelectionManager.NumChords > 2)
                    {
                        // Add a middle one then
                        var note = notesThisTick[(notesThisTick.Count) / 2];
                        result.Add(new LuteMod.Sequencing.Note() { Tick = tickNumber, Tone = note.note + trackSelectionManager.MidiChannels[note.channel].offset + trackSelectionManager.NoteOffset, Type = LuteMod.Sequencing.NoteType.On });
                    }
                    // And then all the rest in case it gets to them?  Unsure... let's see how it plays
                    // I still don't really know.  I'm pretty sure it is now just, prioritizing the important notes
                    // And I think new lutemod is more able to play cleanly itself as well
                    // So I think it is best for now to include them because trimming to 3 did cause some minor issues on some specific songs

                    // Though... I should really use notesPerChord for this

                    // Note, that this implementation prefers high notes, after the first 3.  Might should add an option or something, or pick randomly?  idk

                    //if (trackSelectionManager.NumChords > 3)
                    //{
                    foreach (var t in notesThisTick)
                    {
                        if (result.Count >= trackSelectionManager.NumChords)
                            break;
                        if (!result.Any(n => n.Tone == t.note && n.Type == LuteMod.Sequencing.NoteType.On))
                        {
                            result.Add(new LuteMod.Sequencing.Note() { Tick = tickNumber, Tone = t.note + trackSelectionManager.MidiChannels[t.channel].offset + trackSelectionManager.NoteOffset, Type = LuteMod.Sequencing.NoteType.On });
                        }
                    }
                    //}
                }
                //else if(instrumentId == 1)
                //{
                //    // Flute.  Add only 1?  Which one?  Highest isn't really good... and if there's only two, the other option is lowest, that's even worse
                //    // Let's just leave flute alone for now
                //
                //}
                else
                {
                    // Add them all 
                    foreach (var note in notesThisTick)
                        result.Add(new LuteMod.Sequencing.Note() { Tick = tickNumber, Tone = note.note + trackSelectionManager.MidiChannels[note.channel].offset + trackSelectionManager.NoteOffset, Type = LuteMod.Sequencing.NoteType.On });
                }

                // Then look for meta notes
                // Damn, so, the notes in track don't seem easily accessible via knowing the tick we want... 
                // Which means it'd be crazy not to just, track them.  
                if (tickMetaNotes.ContainsKey(tickNumber))
                {
                    foreach (var metaNote in tickMetaNotes[tickNumber])
                    {
                        result.Add(new LuteMod.Sequencing.Note() { Tick = tickNumber, Tone = metaNote.note, Type = LuteMod.Sequencing.NoteType.Tempo });
                    }
                }
            }

            /*
            foreach (TrackItem track in trackSelectionManager.MidiTracks.Values)
            {
                if (track.Active)
                {
                    tempTrack = sequence.ElementAt(track.Id);
                    for (int i = 0; i < tempTrack.Count; i++)
                    {
                        tempTick = tempTrack.GetMidiEvent(i).AbsoluteTicks;
                        // So let's manually handle chords here, parse them down to 2-3 total
                        // Find all active channels, and find every note on this tick for those channels...

                        // We can try using tsm.MidiChannels.tickNotes
                        List<MidiNote> notesThisTick = trackSelectionManager.MidiChannels.Values.Where(c => c.Active).SelectMany(c => { if (c.tickNotes.TryGetValue(tempTick, out var noteList)) return noteList.Where(n => trackSelectionManager.MidiTracks[n.track].Active); else return new List<MidiNote>(); }).OrderBy(n => n.note).ToList();
                        // That's a really dumb and ugly linq statement but whatever

                        // Now all we have to do is, if we're lute, take the top and bottom one from this ordered list (maybe a third one from the middle?)

                        // If we're flute, take just the top?

                        if (tempTrack.GetMidiEvent(i).MidiMessage.MessageType == MessageType.Channel)
                        {
                            tempMessage = (ChannelMessage)tempTrack.GetMidiEvent(i).MidiMessage;
                            foreach (MidiChannelItem item in trackSelectionManager.MidiChannels.Values) // TODO: Enforce an order for consistency?
                            {
                                isChannelActive = isChannelActive || (item.Id == tempMessage.MidiChannel && item.Active);
                            }
                            if (isChannelActive && tempMessage.Command == ChannelCommand.NoteOn && tempMessage.Data2 > 0)
                            {
                                int offset = trackSelectionManager.NoteOffset + trackSelectionManager.MidiChannels[tempMessage.MidiChannel].offset;
                                result.Add(new LuteMod.Sequencing.Note() { Tick = tempTick, Tone = tempMessage.Data1 + offset, Type = LuteMod.Sequencing.NoteType.On });
                            }
                            isChannelActive = false;
                        }
                        if (tempTrack.GetMidiEvent(i).MidiMessage.MessageType == MessageType.Meta)
                        {
                            tempMetaMessage = (MetaMessage)tempTrack.GetMidiEvent(i).MidiMessage;
                            if (tempMetaMessage.MetaType == MetaType.Tempo)
                            {
                                var newTempo = BytesToInt(tempMetaMessage.GetBytes()); // We divide by division when converting for lutemod only, apparently
                                result.Add(new LuteMod.Sequencing.Note() { Tick = tempTick, Tone = newTempo, Type = LuteMod.Sequencing.NoteType.Tempo });
                            }
                        }
                    }
                }
            }
            */
            return result;
        }

        private int BytesToInt(byte[] bytes)
        {
            return (int)((bytes[0] * Math.Pow(2, 16)) + (bytes[1] * Math.Pow(2, 8)) + bytes[2]);
        }

        public sealed override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (sequence != null)
                        sequence.Dispose();
                    if (sequencer != null)
                        sequencer.Dispose();
                    if (outDevice != null && !outDevice.IsDisposed)
                        outDevice.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposed = true;
            }
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

