using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BardMidiSharp
{
    public class Channel
    {
        public int ID { get; set; }
        public Tick[] Ticks { get; set; } = new Tick[0];    
        public Note[] Notes { get => Ticks.SelectMany(t => t.Notes).ToArray(); }
        public bool Enabled { get; set; }
        public int InstrumentID { get; set; }
        public string Name { get; set; }
    }
}
