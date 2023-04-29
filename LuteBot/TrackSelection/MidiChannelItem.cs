using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.TrackSelection
{
    //[DataContract]
    public class MidiNote
    {
        public int tickNumber { get; set; }
        public int note { get; set; }
        public int length { get; set; }
        [IgnoreDataMember]
        public float timeLength { get; set; }
        public int track { get; set; }
        public int channel { get; set; }
        public bool active { get; set; } = true;
        [IgnoreDataMember]
        public System.Drawing.Rectangle pianoRect { get; set; }
        [IgnoreDataMember]
        public TimeSpan startTime { get; set; }

        public MidiNote() { }

        public MidiNote(MidiNote old)
        {
            this.tickNumber = old.tickNumber;
            this.note = old.note;
            this.length = old.length;
            this.timeLength = old.timeLength;
            this.track = old.track;
            this.channel = old.channel;
            this.active = old.active;
            this.startTime = old.startTime;
            // We don't copy the pianoRect I guess, force it to regenerate it...
        }

        public MidiNote(MidiNote original, SimpleMidiNote modified)
        {
            if (modified.note.HasValue)
                this.note = modified.note.Value;
            else
                this.note = original.note;
            if (modified.track.HasValue)
                this.track = modified.track.Value;
            else
                this.track = original.track;
            if (modified.channel.HasValue)
                this.channel = modified.channel.Value;
            else
                this.channel = original.channel;
            if (modified.active.HasValue)
                this.active = modified.active.Value;
            else
                this.active = original.active;
        }

        public override bool Equals(object obj)
        {
            if (obj is MidiNote)
            {
                var other = obj as MidiNote;
                return other.active == this.active && other.channel == this.channel && other.length == this.length && other.note == this.note && other.tickNumber == this.tickNumber && other.track == this.track;
            }
            else
                return base.Equals(obj);

        }

        public override int GetHashCode()
        {
            return this.active.GetHashCode() ^ this.channel.GetHashCode() ^ this.length.GetHashCode() ^ this.note.GetHashCode() ^ this.tickNumber.GetHashCode() ^ this.track.GetHashCode();
        }

    }

    public class SimpleMidiNote
    {
        // This is just like a regular MidiNote, except the values are all null if they haven't been changed
        // And that it should contain an index, indicating which element it is in its TickNote dictionary for its tick number

        // I guess the tickNumber is definitely required, and the length is meaningless and we can ignore it
        // Any of the others could in theory change, though
        public int tickNumber { get; set; }
        public int index { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? note { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? track { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? channel { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? active { get; set; }

        public SimpleMidiNote(MidiNote original, MidiNote modified, int index)
        {
            this.index = index;
            if (original.note != modified.note)
                this.note = modified.note;
            if (original.track != modified.track)
                this.track = modified.track;
            if (original.channel != modified.channel)
                this.channel = modified.channel;
            if (original.active != modified.active)
                this.active = modified.active;
            this.tickNumber = modified.tickNumber;
        }
    }



    //[DataContract] // This class is for serializing/deserializing, and builds a simplified set of data
    public class SimpleMidiChannelItem // I was doing that on the fly but better to make a specific structure that does it 
    {
        public int Id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Rank { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Active { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? offset { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SimpleMidiNote[] noteList { get; } // For serializing, we don't need them in a dict

        public SimpleMidiChannelItem()
        {
            this.noteList = null;
        }

        public SimpleMidiChannelItem(OldMidiChannelItem from, OldMidiChannelItem original)
        {
            this.Id = from.Id;
            this.Rank = from.Rank ?? original.Rank;
            if (from.Active != original.Active)
                this.Active = from.Active;
            if (from.offset != original.offset)
                this.offset = from.offset;
            List<MidiNote> notelist = new List<MidiNote>();
            List<MidiNote> originalnotelist = new List<MidiNote>();
            foreach (var kvp in from.tickNotes)
            {
                var newNotes = kvp.Value;
                var originalNotes = original.tickNotes[kvp.Key];
                for (int i = 0; i < newNotes.Count; i++)
                {
                    if (!newNotes[i].Equals(originalNotes[i]))
                    {
                        notelist.Add(newNotes[i]);
                        originalnotelist.Add(originalNotes[i]);
                    }
                }
            }
            this.noteList = notelist.Select((n, i) => new SimpleMidiNote(originalnotelist[i], n, i)).ToArray();
            if (this.noteList.Count() == 0)
                this.noteList = null;
        }

        // Should only be used for backwards compatibility, so that old implementations that stored the whole thing can still be read in
        public SimpleMidiChannelItem(OldMidiChannelItem from)
        {
            this.Id = from.Id;
            this.Active = from.Active;
            this.offset = from.offset;
            this.Rank = from.Rank;
        }
    }

    public class ChannelSettings
    {
        public int InstrumentId { get; set; }
        public bool Active { get; set; } = true;
        public int Offset { get; set; }
        public int? Rank { get; set; }
    }

    public class MidiChannelItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Unknown";
        public int NumNotes { get; set; }
        public int HighestNote { get; set; } = 0;
        public int LowestNote { get; set; } = 127;
        public int AverageNote { get; set; }
        public float AvgNoteLength { get; set; }
        public float TotalNoteLength { get; set; }
        public int MaxChordSize { get; set; }
        public Dictionary<int, List<MidiNote>> TickNotes { get; internal set; } = new Dictionary<int, List<MidiNote>>(); // Notes arranged by tick
        public int MidiInstrument { get; set; }
        public float AvgVariation { get; set; }
        public float PercentTimePlaying { get; set; }
        public float PercentSongNotes { get; set; }
        public bool IsTrack { get; protected set; } = false;
        public Dictionary<int, ChannelSettings> Settings { get; private set; } = new Dictionary<int, ChannelSettings>(); // Settings by InstrumentId
        public float FluteRating { get; set; }

        public MidiChannelItem(int Id, bool IsTrack) { this.Id = Id; this.IsTrack = IsTrack; }

        public MidiChannelItem WithOldSettings(Dictionary<int, SimpleMidiChannelItem> oldSettings) // A dictionary of instrumentId to channel, one for each instrument
        {
            if (oldSettings != null)
                foreach (var kvp in oldSettings)
                {
                    var old = kvp.Value;

                    ChannelSettings settings;
                    if (this.Settings.ContainsKey(kvp.Key))
                    {
                        settings = this.Settings[kvp.Key];
                    }
                    else
                    {
                        settings = new ChannelSettings() { InstrumentId = kvp.Key };
                        this.Settings[kvp.Key] = settings;
                    }
                    settings.Active = old.Active ?? true;
                    settings.Offset = old.offset ?? settings.Offset;
                    settings.Rank = old.Rank ?? settings.Rank;

                    // I'm not keeping per-note settings
                }
            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj is MidiChannelItem)
            {
                var other = obj as MidiChannelItem;
                var result = this.IsTrack == other.IsTrack && this.Id == other.Id;
                if (result)
                {
                    // Try to avoid doing this unless we're already a match from the others
                    result = this.Settings == other.Settings || ( other.Settings != null && this.Settings != null && this.Settings.SequenceEqual(other.Settings));
                }

                return result;
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode() ^ this.Settings.GetHashCode() ^ this.TickNotes.GetHashCode();
        }
    }

    public class OldMidiChannelItem
    {
        public int? Rank { get; set; }
        private int id;
        private bool active;
        public int Id { get => id; set => id = value; }
        public string Name { get; set; } = "Unknown";
        public bool Active { get => active; set => active = value; }
        public int numNotes { get; set; }
        public int highestNote { get; set; } = 0;
        public int lowestNote { get; set; } = 127;
        public int averageNote { get; set; }
        public float avgNoteLength { get; set; }
        public float totalNoteLength { get; set; }
        public int maxChordSize { get; set; }
        public Dictionary<int, int> noteTicks { get; } = new Dictionary<int, int>();
        public int offset { get; set; }
        public Dictionary<int, List<MidiNote>> tickNotes { get; internal set; } = new Dictionary<int, List<MidiNote>>(); // For setup and filtering, as well as disabling/enabling specific notes
        public int midiInstrument { get; set; }
        public float avgVariation { get; set; }
        public float percentTimePlaying { get; set; }
        public float percentSongNotes { get; set; }
        public bool isTrack { get; protected set; } = false;

        public OldMidiChannelItem() { }

        public OldMidiChannelItem(OldMidiChannelItem old)
        {
            this.Id = old.Id;
            this.Rank = old.Rank;
            this.Name = old.Name;
            this.Active = old.Active;
            this.numNotes = old.numNotes;
            this.highestNote = old.highestNote;
            this.lowestNote = old.lowestNote;
            this.avgNoteLength = old.avgNoteLength;
            this.offset = old.offset;
            this.maxChordSize = old.maxChordSize;
            this.totalNoteLength = old.totalNoteLength;
            this.averageNote = old.averageNote;
            this.midiInstrument = old.midiInstrument;
            this.avgVariation = old.avgVariation;
            // in tickNotes, the MidiNotes need to be reinstantiated so we don't have them by ref
            this.tickNotes.Clear();
            foreach (var kvp in old.tickNotes)
            {
                var notes = new List<MidiNote>();
                foreach (var note in kvp.Value)
                {
                    notes.Add(new MidiNote(note));
                }
                this.tickNotes[kvp.Key] = notes;
            }
        }

        public OldMidiChannelItem(SimpleMidiChannelItem old)
        {
            this.Id = old.Id;
            this.Rank = old.Rank ?? this.Rank;
            if (old.Active.HasValue)
                this.Active = old.Active.Value;
            if (old.offset.HasValue)
                this.offset = old.offset.Value;
            // If we're creating a new from a simple, we can't process the noteList.  We probably shouldn't allow this at all
            //if(old.noteList != null)
            //{
            //
            //}
        }

        public OldMidiChannelItem WithData(SimpleMidiChannelItem newChannel)
        {
            var result = new OldMidiChannelItem(this);
            result.Rank = newChannel?.Rank ?? result.Rank;
            if (newChannel.Active.HasValue)
                result.Active = newChannel.Active.Value;
            if (newChannel.offset.HasValue)
                result.offset = newChannel.offset.Value;
            if (newChannel.noteList != null)
            {
                foreach (var newNote in newChannel.noteList)
                {
                    if (result.tickNotes.ContainsKey(newNote.tickNumber))
                    {
                        var oldNotes = result.tickNotes[newNote.tickNumber];
                        if (oldNotes.Count > newNote.index)
                        {
                            var oldNote = oldNotes[newNote.index];

                            if (newNote.note.HasValue)
                                oldNote.note = newNote.note.Value;
                            if (newNote.track.HasValue)
                                oldNote.track = newNote.track.Value;
                            if (newNote.channel.HasValue)
                                oldNote.channel = newNote.channel.Value;
                            if (newNote.active.HasValue)
                                oldNote.active = newNote.active.Value;
                        }
                    }
                }
            }

            return result;
        }

        
    }
}
