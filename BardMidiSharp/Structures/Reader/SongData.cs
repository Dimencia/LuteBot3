using System;
using System.Collections.Generic;
using System.Text;

namespace BardMidiSharp.Structures.Reader
{
    internal class SongData
    {
        internal short Format { get; set; }
        internal int CurrentTrack { get; set; }
        internal byte? CurrentStatus { get; set; }
        internal short? CurrentChannel { get; set; }
    }
}
