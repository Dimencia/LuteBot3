using System;
using System.Collections.Generic;
using System.Linq;

namespace BardMidiSharp
{
    public class Song
    {
        public Channel[] Channels { get; set; } = new Channel[0];
        public Track[] Tracks { get; set; } = new Track[0];
        public string FilePath { get; set; }
        public string Name { get; set; } // Do we get this from anything?
        public uint Division { get; set; }
        public short Format { get; set; }
        public uint StartTempo { get; set; }
    }
}
