using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BardMidiSharp
{
    public class Track
    {
        public int ID { get; set; }
        public SortedDictionary<uint, Tick> Ticks { get; set; } = new SortedDictionary<uint, Tick>();
        public MidiNote[] Notes { get => Ticks.SelectMany(t => t.Value.Notes).ToArray(); }
        public MetaNote[] MetaNotes { get => Ticks.SelectMany(t => t.Value.MetaNotes).ToArray(); }
        public bool Enabled { get; set; } = true;
        public string Name { get; set; }

        public Track(int ID)
        {
            this.ID = ID;
        }
    }
}
