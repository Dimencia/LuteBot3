using System;
using System.Collections.Generic;
using System.Text;

namespace BardMidiSharp
{
    public class Tick
    {
        public int TickNumber { get; set; }
        public MidiNote[] Notes { get; set; } = new MidiNote[0];    
        public MetaNote[] MetaNotes { get; set; } = new MetaNote[0];
    }
}
