using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
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
            return this.active.GetHashCode()^this.channel.GetHashCode()^this.length.GetHashCode()^this.note.GetHashCode()^this.tickNumber.GetHashCode()^this.track.GetHashCode();
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
        public bool? Active { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? offset { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public SimpleMidiNote[] noteList { get; } // For serializing, we don't need them in a dict

        public SimpleMidiChannelItem()
        {
            this.noteList = null;
        }

        public SimpleMidiChannelItem(MidiChannelItem from, MidiChannelItem original)
        {
            this.Id = from.Id;
            if (from.Active != original.Active)
                this.Active = from.Active;
            if (from.offset != original.offset)
                this.offset = from.offset;
            List<MidiNote> notelist = new List<MidiNote>();
            List<MidiNote> originalnotelist = new List<MidiNote>();
            foreach(var kvp in from.tickNotes)
            {
                var newNotes = kvp.Value;
                var originalNotes = original.tickNotes[kvp.Key];
                for(int i = 0; i < newNotes.Count; i++)
                {
                    if (!newNotes[i].Equals(originalNotes[i]))
                    {
                        notelist.Add(newNotes[i]);
                        originalnotelist.Add(originalNotes[i]);
                    }
                }
            }
            this.noteList = notelist.Select((n,i) => new SimpleMidiNote(originalnotelist[i], n, i)).ToArray();
            if(this.noteList.Count() == 0)
                this.noteList = null;
        }

        // Should only be used for backwards compatibility, so that old implementations that stored the whole thing can still be read in
        public SimpleMidiChannelItem(MidiChannelItem from)
        {
            this.Id = from.Id;
            this.Active = from.Active;
            this.offset = from.offset;
        }
    }


    public class MidiChannelItem
    {
        private int id;
        private bool active;
        public int Id { get => id; set => id = value; }
        public string Name { get; set; }
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

        public MidiChannelItem() { }

        public MidiChannelItem(MidiChannelItem old)
        {
            this.Id = old.Id;
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
            foreach(var kvp in old.tickNotes)
            {
                var notes = new List<MidiNote>();
                foreach(var note in kvp.Value)
                {
                    notes.Add(new MidiNote(note));
                }
                this.tickNotes[kvp.Key] = notes;
            }
        }

        public MidiChannelItem(SimpleMidiChannelItem old)
        {
            this.Id = old.Id;
            if(old.Active.HasValue)
                this.Active = old.Active.Value;
            if(old.offset.HasValue)
                this.offset = old.offset.Value;
            // If we're creating a new from a simple, we can't process the noteList.  We probably shouldn't allow this at all
            //if(old.noteList != null)
            //{
            //
            //}
        }

        public MidiChannelItem WithData(SimpleMidiChannelItem newChannel)
        {
            var result = new MidiChannelItem(this);
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

        public override bool Equals(object obj)
        {
            if (obj is MidiChannelItem)
            {
                var other = obj as MidiChannelItem;
                var result = this.Active == other.Active && this.Id == other.Id && this.offset == other.offset;
                if(result)
                {
                    // Try to avoid doing this unless we're already a match from the others
                    result = this.tickNotes.All(kvp => other.tickNotes[kvp.Key].SequenceEqual(kvp.Value));
                }

                return result;
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode()^this.Active.GetHashCode()^this.offset.GetHashCode()^this.tickNotes.GetHashCode();
        }
    }
}
