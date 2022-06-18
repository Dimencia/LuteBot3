using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BardMidiSharp
{
    public class Track
    {
        public int ID { get; set; }
        public Tick[] Ticks { get; set; } = new Tick[0];
        public MidiNote[] Notes { get => Ticks.SelectMany(t => t.Notes).ToArray(); }
        public MetaNote[] MetaNotes { get => Ticks.SelectMany(t => t.MetaNotes).ToArray(); }
        public bool Enabled { get; set; }
        public string Name { get; set; }
    }
}
