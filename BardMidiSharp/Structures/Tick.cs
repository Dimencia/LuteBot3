using System;
using System.Collections.Generic;
using System.Text;

namespace BardMidiSharp
{
    public class Tick
    {
        public uint TickNumber { get; set; }
        public List<MidiNote> Notes { get; set; } = new List<MidiNote>();    
        public List<MetaNote> MetaNotes { get; set; } = new List<MetaNote>();

        public Tick(uint tickNumber)
        {
            this.TickNumber = tickNumber;
        }
    }
}
