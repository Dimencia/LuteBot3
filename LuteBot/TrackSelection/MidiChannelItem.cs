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
    }


    //[DataContract] // This class is for serializing/deserializing, and builds a simplified set of data
    public class SimpleMidiChannelItem // I was doing that on the fly but better to make a specific structure that does it 
    {
        public int Id { get; set; }
        public bool Active { get; set; } = true;
        public int offset { get; set; }
        public MidiNote[] noteList { get; } // For serializing, we don't need them in a dict

        public SimpleMidiChannelItem()
        {

        }

        public SimpleMidiChannelItem(MidiChannelItem original)
        {
            this.Id = original.Id;
            this.Active = original.Active;
            this.offset = original.offset;
            this.noteList = original.tickNotes.SelectMany(kvp => kvp.Value.Where(n => !n.active)).ToArray();
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
        public Dictionary<int, List<MidiNote>> tickNotes { get; } = new Dictionary<int, List<MidiNote>>(); // For setup and filtering, as well as disabling/enabling specific notes
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
            this.Active = old.Active;
            this.offset = old.offset;
        }
    }
}
