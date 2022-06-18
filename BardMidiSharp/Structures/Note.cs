using System;
using System.Collections.Generic;
using System.Text;

namespace BardMidiSharp
{
    public class Note
    {
        public int ID { get; set; }
        public int TickNumber { get; set; }
        public int Value { get; set; } // Maybe I should make this more specific and in each one, but it seems appropriate for both
        
    }

    public class MidiNote : Note
    {
        public int Duration { get; set; }
        public int Velocity { get; set; }
        public Channel Channel { get; set; }
        public Track Track { get; set; }
    }

    public class MetaNote : Note
    {

    }

}
