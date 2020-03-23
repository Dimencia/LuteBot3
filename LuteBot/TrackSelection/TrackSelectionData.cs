using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.TrackSelection
{
    public class TrackSelectionData
    {
        private List<MidiChannelItem> midiChannels;
        private List<TrackItem> midiTracks;

        public List<MidiChannelItem> MidiChannels { get => midiChannels; set => midiChannels = value; }
        public List<TrackItem> MidiTracks { get => midiTracks; set => midiTracks = value; }

        public int Offset { get; set; }


    }
}
