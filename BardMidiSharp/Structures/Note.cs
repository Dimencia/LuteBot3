using System;
using System.Collections.Generic;
using System.Text;

namespace BardMidiSharp
{
    public class Note
    {
        public int ID { get; set; }
        public uint TickNumber { get; set; }
        public int Value { get; set; } // Maybe I should make this more specific and in each one, but it seems appropriate for both
        public Track Track { get; set; }
        public Channel Channel { get; set; }

        public Note(int ID)
        {
            this.ID = ID;
        }
    }

    public class MidiNote : Note
    {
        public int Duration { get; set; }
        public int Velocity { get; set; }
        public bool Enabled { get; set; } = true;

        public MidiNote(int ID) : base(ID)
        {

        }
    }

    public class MetaNote : Note
    {
        public MetaNote(int ID) : base(ID)
        {

        }
    }

}
