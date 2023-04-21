using LuteBot.Config;
using LuteBot.TrackSelection;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
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

        public string fileName { get; set; }

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
                try
                {
                    outDevice = new OutputDevice(ConfigManager.GetIntegerProperty(PropertyItem.OutputDevice));
                }
                catch // If the config's OutputDevice is invalid, default them to 0 since we know there's at least one
                {
                    outDevice = new OutputDevice(0);
                    ConfigManager.SetProperty(PropertyItem.OutputDevice, "0");
                }
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

        public void UpdateMutedTracks(MidiChannelItem item)
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

        private TimeSpan currentTimeLength;

        public TimeSpan GetTimeLength()
        {
            return currentTimeLength;
            /*
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
            */
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
                fileName = filename;
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

        private void HandleLoadCompleted(object sender, AsyncCompletedEventArgs ev)
        {
            tickMetaNotes.Clear();
            var startTime = DateTime.Now;
            lock (loadLock)
            {
                if (ev.Error == null)
                {
                    //channelNames = new Dictionary<int, string>();
                    //trackNames = new Dictionary<int, string>();
                    var dryWetFile = Melanchall.DryWetMidi.Core.MidiFile.Read(fileName,
                                new ReadingSettings
                                {
                                    ExtraTrackChunkPolicy = ExtraTrackChunkPolicy.Skip,
                                    UnknownChunkIdPolicy = UnknownChunkIdPolicy.Skip,
                                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,


                                });

                    // I think I'm gonna do this my own way... we're looking to fill Channels and Tracks with notes

                    // So... let's find all notes.  But we have to go by track to keep track of that (lul)
                    // Hold up, gonna copy from where I've done this before
                    var tempoMap = dryWetFile.GetTempoMap();
                    var trackChunks = dryWetFile.GetTrackChunks().ToList();

                    currentTimeLength = dryWetFile.GetDuration<MetricTimeSpan>();

                    var programNumbers = dryWetFile.GetTimedEvents()
                        .OfType<ProgramChangeEvent>()
                        .ToDictionary(e => e.Channel, e => e);

                    channels = new Dictionary<int, MidiChannelItem>();
                    tracks = new Dictionary<int, TrackItem>();

                    int trackNum = 0;
                    foreach (var tc in trackChunks)
                    {

                        TrackItem track;
                        if (tracks.TryGetValue(trackNum, out var existing))
                        {
                            track = existing;
                        }
                        else
                        {
                            track = new TrackItem();
                            track.Id = trackNum;
                            track.Name = "Untitled Track";
                            //track.lowestNote = midiTrack.MinNoteId;
                            //track.highestNote = midiTrack.MaxNoteId;
                            track.Active = true;
                            tracks[trackNum] = track;
                        }


                        var chords = tc.GetChords();

                        foreach (var chord in chords) // new ChordDetectionSettings { NotesTolerance } - It can pretty easily wrap them to the nearest 16ms
                        {
                            if (chord != null)
                            {
                                // A chord correlates pretty much to a Tick

                                //StartTime = chord.TimeAs<MetricTimeSpan>(tempoMap),
                                //TickNumber = chord.Time

                                // I guess these are the same kind of Note I'm thinking of; real playable ones with times on them already
                                foreach (var dryNote in chord.Notes)
                                {
                                    MidiChannelItem channel;
                                    if (channels.TryGetValue(dryNote.Channel, out var existingChannel))
                                    {
                                        channel = existingChannel;
                                    }
                                    else
                                    {
                                        channel = new MidiChannelItem();
                                        channel.Id = dryNote.Channel;
                                        channel.Name = MidiChannelTypes.Names[dryNote.Channel];
                                        channel.Active = dryNote.Channel != 9;
                                        channels[dryNote.Channel] = channel;
                                    }

                                    var prog = (int)dryNote.Channel;
                                    if (programNumbers.TryGetValue(dryNote.Channel, out var pn))
                                        prog = pn.ProgramNumber;

                                    var absoluteTicks = (int)dryNote.TimeAs<MidiTimeSpan>(tempoMap).TimeSpan;

                                    var note = new MidiNote()
                                    {
                                        note = dryNote.NoteNumber,
                                        tickNumber = absoluteTicks,
                                        track = track.Id,
                                        channel = dryNote.Channel,
                                        timeLength = (float)dryNote.LengthAs<MetricTimeSpan>(tempoMap).TotalSeconds,
                                        startTime = dryNote.TimeAs<MetricTimeSpan>(tempoMap),
                                        active = true,
                                        length = (int)dryNote.LengthAs<MidiTimeSpan>(tempoMap).TimeSpan
                                    };

                                    if (!channel.tickNotes.ContainsKey(absoluteTicks))
                                        channel.tickNotes[absoluteTicks] = new List<MidiNote>();
                                    if (!track.tickNotes.ContainsKey(absoluteTicks))
                                        track.tickNotes[absoluteTicks] = new List<MidiNote>();

                                    channels[dryNote.Channel].tickNotes[absoluteTicks].Add(note);
                                    tracks[trackNum].tickNotes[absoluteTicks].Add(note);

                                    channel.totalNoteLength += note.timeLength;
                                    if (dryNote.NoteNumber < channel.lowestNote)
                                        channel.lowestNote = dryNote.NoteNumber;
                                    if (dryNote.NoteNumber > channel.highestNote)
                                        channel.highestNote = dryNote.NoteNumber;
                                    channel.averageNote += dryNote.NoteNumber;
                                    channel.numNotes++;

                                    track.totalNoteLength += note.timeLength;
                                    if (dryNote.NoteNumber < track.lowestNote)
                                        track.lowestNote = dryNote.NoteNumber;
                                    if (dryNote.NoteNumber > track.highestNote)
                                        track.highestNote = dryNote.NoteNumber;
                                    track.averageNote += dryNote.NoteNumber;
                                    track.numNotes++;
                                }
                                // Check for channel chords
                                foreach (var channelNotes in chord.Notes.GroupBy(n => n.Channel))
                                {
                                    var channel = channels[channelNotes.Key];
                                    if (channelNotes.Count() > channel.maxChordSize)
                                        channel.maxChordSize = channelNotes.Count();
                                }
                            }
                        }
                        if (track.tickNotes.Any(tn => tn.Value.Any()))
                            track.maxChordSize = track.tickNotes.Max(tn => tn.Value.Count);

                        var nameEvents = tc.Events.OfType<SequenceTrackNameEvent>();
                        if (nameEvents.Any())
                            track.Name = nameEvents.Last().Text;


                        trackNum++;
                    }

                    foreach (var channel in channels.Values)
                    {
                        if (channel.numNotes > 0)
                        {
                            channel.avgNoteLength = channel.totalNoteLength / channel.numNotes;
                            channel.averageNote = (int)((float)channel.averageNote / channel.numNotes);
                            // avg Variation: iterate notes, find difference to last note, add up and divide
                            int lastNote = -1;
                            int totalVariation = 0;
                            foreach (var noteList in channel.tickNotes.Values)
                                foreach (var note in noteList)
                                {
                                    if (lastNote > -1)
                                        totalVariation += Math.Abs(note.note - lastNote);
                                    lastNote = note.note;
                                }
                            channel.avgVariation = totalVariation / channel.numNotes;
                        }
                    }

                    foreach (var track in tracks.Values)
                    {
                        if (track.numNotes > 0)
                        {
                            track.avgNoteLength = track.totalNoteLength / track.numNotes;
                            track.averageNote = (int)((float)track.averageNote / track.numNotes);
                            // avg Variation: iterate notes, find difference to last note, add up and divide
                            int lastNote = -1;
                            int totalVariation = 0;
                            foreach (var noteList in track.tickNotes.Values)
                                foreach (var note in noteList)
                                {
                                    if (lastNote > -1)
                                        totalVariation += Math.Abs(note.note - lastNote);
                                    lastNote = note.note;
                                }
                            track.avgVariation = totalVariation / track.numNotes;
                        }
                    }

                    var firstTempo = tempoMap.GetTempoAtTime(new MetricTimeSpan(0));
                    tickMetaNotes[0] = new List<MidiNote> { new MidiNote() { note = (int)(firstTempo.MicrosecondsPerQuarterNote / (tempoMap.TimeDivision as TicksPerQuarterNoteTimeDivision).TicksPerQuarterNote), tickNumber = 0, track = 0 } };
                    foreach (var tempo in tempoMap.GetTempoChanges())
                    {
                        var absoluteTicks = (int)tempo.TimeAs<MidiTimeSpan>(tempoMap).TimeSpan;
                        if (!tickMetaNotes.ContainsKey(absoluteTicks))
                            tickMetaNotes[absoluteTicks] = new List<MidiNote>();
                        tickMetaNotes[absoluteTicks] = new List<MidiNote> { new MidiNote() { note = (int)tempo.Value.MicrosecondsPerQuarterNote, tickNumber = absoluteTicks, track = 0 } };
                    }

                    // Add a note on the drum channel
                    if (channels.ContainsKey(9))
                        channels[9].Name += " (DRUMS)";

                    // Remove any channels or tracks that don't contain any notes
                    //channels = channels.Values.Where(c => c.numNotes > 0).ToDictionary(c => c.Id);
                    //tracks = tracks.Values.Where(c => c.numNotes > 0).ToDictionary(c => c.Id);

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
                    foreach (var c in channels.Values)
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

                    base.LoadCompleted(this, ev);

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

        public List<LuteMod.Sequencing.Note> ExtractMidiContent(Melanchall.DryWetMidi.Core.MidiFile dryWetFile, int? trackId = null)
        {

            var tempoMap = dryWetFile.GetTempoMap();
            int chunkNumber = -1;

            var notes = dryWetFile.GetTrackChunks().SelectMany(t =>
            {
                chunkNumber++; return t.GetNotes(new NoteDetectionSettings { NoteSearchContext = NoteSearchContext.AllEventsCollections, NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }).Where(n => (!trackSelectionManager.MidiTracks.ContainsKey(chunkNumber) || trackSelectionManager.MidiTracks[chunkNumber].Active) && (!trackId.HasValue || trackId.Value == chunkNumber)
                    && (!trackSelectionManager.MidiChannels.ContainsKey(n.Channel) || trackSelectionManager.MidiChannels[n.Channel].Active) && n.Velocity > 0)
                    .Select(n => new LuteMod.Sequencing.Note() { duration = Math.Min((float)n.LengthAs<MetricTimeSpan>(tempoMap).TotalSeconds-0.0171f,1), Tick = n.Time, Tone = n.NoteNumber + trackSelectionManager.MidiChannels[n.Channel].offset + trackSelectionManager.NoteOffset, Type = LuteMod.Sequencing.NoteType.On });
            }).ToList();
            // Always add the tempo at 0 time
            var startTempo = tempoMap.GetTempoAtTime(new MetricTimeSpan(0));
            var division = (TicksPerQuarterNoteTimeDivision)(dryWetFile.TimeDivision);
            notes.Add(new LuteMod.Sequencing.Note() { Tick = 0, Tone = (int)startTempo.MicrosecondsPerQuarterNote, Type = LuteMod.Sequencing.NoteType.Tempo });
            foreach (var tempoChange in tempoMap.GetTempoChanges())
                notes.Add(new LuteMod.Sequencing.Note() { Tick = tempoChange.Time, Tone = (int)tempoChange.Value.MicrosecondsPerQuarterNote, Type = LuteMod.Sequencing.NoteType.Tempo });

            return notes.OrderBy(n => n.Tick).ToList();
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

