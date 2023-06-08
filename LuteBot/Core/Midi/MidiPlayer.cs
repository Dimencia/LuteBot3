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
using LuteBot.LuteMod.Sequencing;
using LuteBot.UI.Utils;

namespace LuteBot.Core.Midi
{
    public class MidiPlayer
    {
        public MidiFile dryWetFile { get; set; }
        private long lastTick { get; set; }

        public string FileName { get; set; }

        public Dictionary<int, List<MidiNote>> tickMetaNotes { get; private set; } = new Dictionary<int, List<MidiNote>>();

        public MidiPlayer()
        {
        }

        // Length in ticks, the last tick basically
        public long GetLength()
        {
            return lastTick;
        }

        private TimeSpan currentTimeLength;

        public TimeSpan GetTimeLength()
        {
            return currentTimeLength;
        }

        public string GetFormattedLength()
        {
            return GetTimeLength().ToString(@"mm\:ss");
        }


        public async Task<TrackSelectionManager> LoadFileAsync(string filename, bool reorderTracks = false, bool autoEnableFlutes = false, bool clearOffsets = false)
        {
            Dictionary<int, MidiChannelItem> channels = new Dictionary<int, MidiChannelItem>();
            Dictionary<int, MidiChannelItem> tracks = new Dictionary<int, MidiChannelItem>();

            var tsm = new TrackSelectionManager(this);

            tickMetaNotes.Clear();
            var startTime = DateTime.Now;
            this.FileName = filename;

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
                    InvalidChannelEventParameterValuePolicy = InvalidChannelEventParameterValuePolicy.ReadValid,
                    NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                    UnknownChannelEventPolicy = UnknownChannelEventPolicy.SkipStatusByte,
                    InvalidSystemCommonEventParameterValuePolicy = InvalidSystemCommonEventParameterValuePolicy.SnapToLimits,
                    NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore
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
                tracks = new Dictionary<int, MidiChannelItem>();

                int trackNum = 0;
                foreach (var tc in trackChunks)
                {

                    MidiChannelItem track;
                    if (tracks.TryGetValue(trackNum, out var existing))
                    {
                        track = existing;
                    }
                    else
                    {
                        track = new MidiChannelItem(trackNum, true);
                        track.Name = "Untitled Track";
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
                                    channel = new MidiChannelItem(dryNote.Channel, false);
                                    channel.Name = MidiChannelTypes.Names[dryNote.Channel];
                                    channels[dryNote.Channel] = channel;
                                }

                                var prog = (int)dryNote.Channel;
                                if (programNumbers.TryGetValue(dryNote.Channel, out var pn))
                                    prog = pn.ProgramNumber;
                                channel.MidiInstrument = prog;
                                track.MidiInstrument = prog;

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

                                if (!channel.TickNotes.ContainsKey(absoluteTicks))
                                    channel.TickNotes[absoluteTicks] = new List<MidiNote>();
                                if (!track.TickNotes.ContainsKey(absoluteTicks))
                                    track.TickNotes[absoluteTicks] = new List<MidiNote>();

                                channels[dryNote.Channel].TickNotes[absoluteTicks].Add(note);
                                tracks[trackNum].TickNotes[absoluteTicks].Add(note);


                                if (dryNote.NoteNumber < channel.LowestNote)
                                    channel.LowestNote = dryNote.NoteNumber;
                                if (dryNote.NoteNumber > channel.HighestNote)
                                    channel.HighestNote = dryNote.NoteNumber;
                                channel.AverageNote += dryNote.NoteNumber;
                                channel.NumNotes++;


                                if (dryNote.NoteNumber < track.LowestNote)
                                    track.LowestNote = dryNote.NoteNumber;
                                if (dryNote.NoteNumber > track.HighestNote)
                                    track.HighestNote = dryNote.NoteNumber;
                                track.AverageNote += dryNote.NoteNumber;
                                track.NumNotes++;
                            }
                            // Check for channel chords
                            foreach (var channelNotes in chord.Notes.Where(n => n.Channel != 9).GroupBy(n => n.Channel))
                            {
                                var channel = channels[channelNotes.Key];
                                if (channelNotes.Count() > channel.MaxChordSize)
                                    channel.MaxChordSize = channelNotes.Count();
                            }
                        }
                    }
                    if (track.TickNotes.Any(tn => tn.Value.Any()))
                        track.MaxChordSize = track.TickNotes.Max(tn => tn.Value.Count);

                    var nameEvents = tc.Events.OfType<SequenceTrackNameEvent>();
                    if (nameEvents.Any())
                        track.Name = nameEvents.Last().Text;


                    trackNum++;
                }
                var totalNumNotes = channels.Values.Sum(c => c.NumNotes);
                foreach (var channel in channels.Values)
                {
                    if (channel.NumNotes > 0)
                    {
                        // Total note length... Can't just sum durations
                        TimeSpan lastTime = TimeSpan.Zero;
                        TimeSpan lastDuration = TimeSpan.Zero;
                        int lastNote = -1;
                        int totalVariation = 0;
                        foreach (var tickNote in channel.TickNotes)
                        {
                            if (tickNote.Value.Any())
                            {
                                var tickDuration = TimeSpan.FromSeconds(tickNote.Value.Max(n => n.timeLength));
                                var noteStartTime = tickNote.Value.First().startTime;
                                if (noteStartTime > lastTime + lastDuration)
                                {
                                    channel.TotalNoteLength += (float)tickDuration.TotalSeconds;
                                }
                                else if (noteStartTime + tickDuration > lastTime + lastDuration)
                                {
                                    channel.TotalNoteLength += (float)((noteStartTime + tickDuration) - (lastTime + lastDuration)).TotalSeconds;
                                }
                                lastTime = noteStartTime;
                                lastDuration = tickDuration;
                                // avg Variation: iterate notes, find difference to last note, add up and divide
                                foreach (var note in tickNote.Value)
                                {
                                    if (lastNote > -1)
                                        totalVariation += Math.Abs(note.note - lastNote);
                                    lastNote = note.note;
                                }
                            }
                        }

                        channel.AvgNoteLength = channel.TotalNoteLength / channel.NumNotes;
                        channel.AverageNote = (int)((float)channel.AverageNote / channel.NumNotes);
                        channel.AvgVariation = totalVariation / channel.NumNotes;
                        if (currentTimeLength.TotalSeconds > 0)
                            channel.PercentTimePlaying = (float)(channel.TotalNoteLength / currentTimeLength.TotalSeconds);
                        else
                            channel.PercentTimePlaying = 0;
                        channel.PercentSongNotes = channel.NumNotes / (float)totalNumNotes;
                    }
                }

                foreach (var channel in tracks.Values)
                {
                    if (channel.NumNotes > 0)
                    {
                        // Total note length... Can't just sum durations
                        TimeSpan lastTime = TimeSpan.Zero;
                        TimeSpan lastDuration = TimeSpan.Zero;
                        int lastNote = -1;
                        int totalVariation = 0;
                        foreach (var tickNote in channel.TickNotes)
                        {
                            if (tickNote.Value.Any())
                            {
                                var tickDuration = TimeSpan.FromSeconds(tickNote.Value.Max(n => n.timeLength));
                                var noteStartTime = tickNote.Value.First().startTime;
                                if (noteStartTime > lastTime + lastDuration)
                                {
                                    channel.TotalNoteLength += (float)tickDuration.TotalSeconds;
                                }
                                else if (noteStartTime + tickDuration > lastTime + lastDuration)
                                {
                                    channel.TotalNoteLength += (float)((noteStartTime + tickDuration) - (lastTime + lastDuration)).TotalSeconds;
                                }
                                lastTime = noteStartTime;
                                lastDuration = tickDuration;
                                // avg Variation: iterate notes, find difference to last note, add up and divide
                                foreach (var note in tickNote.Value)
                                {
                                    if (lastNote > -1)
                                        totalVariation += Math.Abs(note.note - lastNote);
                                    lastNote = note.note;
                                }
                            }
                        }

                        channel.AvgNoteLength = channel.TotalNoteLength / channel.NumNotes;
                        channel.AverageNote = (int)((float)channel.AverageNote / channel.NumNotes);
                        channel.AvgVariation = totalVariation / channel.NumNotes;
                        if (currentTimeLength.TotalSeconds > 0)
                            channel.PercentTimePlaying = (float)(channel.TotalNoteLength / currentTimeLength.TotalSeconds);
                        else
                            channel.PercentTimePlaying = 0;
                        channel.PercentSongNotes = channel.NumNotes / (float)totalNumNotes;
                    }
                }

                tsm.LoadTracks(channels, tracks);
                tsm.LoadTrackManager(reorderTracks, autoEnableFlutes, clearOffsets);

                Console.WriteLine("Read track data in " + (DateTime.Now - startTime).TotalMilliseconds);
            }
            return tsm;
        }

        public List<LuteMod.Sequencing.Note> ExtractMidiContent(TrackSelectionManager tsm, MidiChannelItem source, int instrumentId, int lowestTick)
        {
            var tempoMap = dryWetFile.GetTempoMap();

            var finalNotes = source.TickNotes.SelectMany(tn => tn.Value.Where(n => tsm.MidiChannels.ContainsKey(n.channel) && tsm.MidiChannels[n.channel].Settings.ContainsKey(instrumentId)
                && tsm.MidiChannels[n.channel].Settings[instrumentId].Active
                && tsm.MidiTracks.ContainsKey(n.track) && tsm.MidiTracks[n.track].Settings.ContainsKey(instrumentId) && tsm.MidiTracks[n.track].Settings[instrumentId].Active)
            .Select(n => new LuteMod.Sequencing.Note() { duration = Math.Min(n.timeLength - (0.017f * 2), 2), Tick = Math.Max(n.tickNumber - lowestTick, 0), Tone = n.note + tsm.MidiChannels[n.channel].Settings[instrumentId].Offset + tsm.MidiTracks[n.track].Settings[instrumentId].Offset, Type = LuteMod.Sequencing.NoteType.On }))
            .ToList(); // Two ticks of time off the timeLength because 1. Lutemod plays a tick late, and then  2. Rounding issues I guess idk, it still was wrong without the second one

            // Always add the tempo at 0 time
            var startTempo = tempoMap.GetTempoAtTime(new MetricTimeSpan(0));
            var division = (TicksPerQuarterNoteTimeDivision)(dryWetFile.TimeDivision);
            finalNotes.Add(new LuteMod.Sequencing.Note() { Tick = 0, Tone = (int)startTempo.MicrosecondsPerQuarterNote, Type = LuteMod.Sequencing.NoteType.Tempo });
            foreach (var tempoChange in tempoMap.GetTempoChanges())
                finalNotes.Add(new LuteMod.Sequencing.Note() { Tick = Math.Max(tempoChange.Time - lowestTick, 0), Tone = (int)tempoChange.Value.MicrosecondsPerQuarterNote, Type = LuteMod.Sequencing.NoteType.Tempo });

            return finalNotes.OrderBy(n => n.Tick).ToList();
        }

    }

}

