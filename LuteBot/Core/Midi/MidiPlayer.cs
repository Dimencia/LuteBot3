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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.Core.Midi
{
    public class MidiPlayer
    {
        public TrackSelectionManager trackSelectionManager;

        private Dictionary<int, MidiChannelItem> channels = new Dictionary<int, MidiChannelItem>();
        private Dictionary<int, TrackItem> tracks = new Dictionary<int, TrackItem>();

        public MidiFile dryWetFile { get; set; }
        private long lastTick { get; set; }

        public string fileName { get; set; }

        public Dictionary<int, List<MidiNote>> tickMetaNotes { get; private set; } = new Dictionary<int, List<MidiNote>>();

        public MidiPlayer(TrackSelectionManager trackSelectionManager)
        {
            trackSelectionManager.Player = this;
            
            this.trackSelectionManager = trackSelectionManager;
            
        }

        public Dictionary<int, MidiChannelItem> GetMidiChannels()
        {
            return channels;
        }

        public Dictionary<int, TrackItem> GetMidiTracks()
        {
            return tracks;
        }
        // Length in ticks, the last tick basically
        public long GetLength()
        {
            return lastTick;
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

        public string GetFormattedLength()
        {
            return GetTimeLength().ToString(@"mm\:ss");
        }


        //------------- Handlers -------------

        private int[] drumNoteCounts = new int[128];
        public List<KeyValuePair<int, int>> DrumNoteCounts = new List<KeyValuePair<int, int>>(128);

        // Let's add something new here: Either save data we can use to do it, or do it
        // Detect the best Flute instrument

        // The best flute instrument: Has many notes, has long notes, covers a large octave area
        // So while we already track the octaves covered, we should track numNotes and avgNoteLength of each track or channel

        // That also might mean we need to start tracking data for tracks, instead of just channels, and allow a toggle to show those instead

        private SemaphoreSlim loadSemaphore = new SemaphoreSlim(1, 1);

        public async Task LoadFileAsync(string filename)
        {
            tickMetaNotes.Clear();
            var startTime = DateTime.Now;
            this.fileName = filename;

            await loadSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {

                dryWetFile = null;
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                using (var ms = new MemoryStream())
                {
                    await fs.CopyToAsync(ms).ConfigureAwait(false);
                    await fs.FlushAsync().ConfigureAwait(false);
                    ms.Position = 0;
                    dryWetFile = MidiFile.Read(ms, new ReadingSettings
                    {
                        ExtraTrackChunkPolicy = ExtraTrackChunkPolicy.Skip,
                        UnknownChunkIdPolicy = UnknownChunkIdPolicy.Skip,
                        InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
                        InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits,
                        InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid
                    });
                }


                if (dryWetFile != null)
                {
                    //channelNames = new Dictionary<int, string>();
                    //trackNames = new Dictionary<int, string>();
                    lastTick = dryWetFile.GetTimedEvents().Max(t => t.Time);

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
                                    if (dryNote.Channel == 9)
                                        continue; // Don't even process drums
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
                                foreach (var channelNotes in chord.Notes.Where(n => n.Channel != 9).GroupBy(n => n.Channel))
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
                }
            }
            finally
            {
                loadSemaphore.Release();
            }
        }



        public string GetChannelName(int id)
        {
            if (channels.ContainsKey(id))
                return channels[id].Name;
            else
                return "Unknown";
        }


        public List<LuteMod.Sequencing.Note> ExtractMidiContent(Melanchall.DryWetMidi.Core.MidiFile dryWetFile, int? trackId = null)
        {

            var tempoMap = dryWetFile.GetTempoMap();
            int chunkNumber = -1;

            var notes = dryWetFile.GetTrackChunks().SelectMany(t =>
            {
                chunkNumber++; return t.GetNotes(new NoteDetectionSettings { NoteSearchContext = NoteSearchContext.AllEventsCollections, NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn }).Where(n => (!trackSelectionManager.MidiTracks.ContainsKey(chunkNumber) || trackSelectionManager.MidiTracks[chunkNumber].Active) && (!trackId.HasValue || trackId.Value == chunkNumber)
                    && (!trackSelectionManager.MidiChannels.ContainsKey(n.Channel) || trackSelectionManager.MidiChannels[n.Channel].Active) && n.Velocity > 0 && n.Channel != 9)
                    .Select(n => new LuteMod.Sequencing.Note() { duration = Math.Min((float)n.LengthAs<MetricTimeSpan>(tempoMap).TotalSeconds - (0.0171f*2), 1), Tick = n.Time, Tone = n.NoteNumber + trackSelectionManager.MidiChannels[n.Channel].offset + trackSelectionManager.NoteOffset, Type = LuteMod.Sequencing.NoteType.On });
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


    }

}

