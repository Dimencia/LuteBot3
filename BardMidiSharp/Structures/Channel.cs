using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BardMidiSharp
{
    public class Channel
    {
        public int ID { get; set; }
        public SortedDictionary<uint, Tick> Ticks { get; set; } = new SortedDictionary<uint, Tick>();
        public MidiNote[] Notes { get => Ticks.SelectMany(t => t.Value.Notes).ToArray(); }
        public bool Enabled { get; set; } = true;
        public int InstrumentID { get; set; }
        public string Name { get; set; }

        public Channel(int ID)
        {
            this.ID = ID;
        }
    }
}
