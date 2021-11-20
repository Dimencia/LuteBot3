using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.TrackSelection
{
    [DataContract]
    public class TrackSelectionData
    {
        private List<MidiChannelItem> midiChannels;
        private List<TrackItem> midiTracks;
        [DataMember]
        public List<MidiChannelItem> MidiChannels { get => midiChannels; set => midiChannels = value; }
        [DataMember]
        public Dictionary<int, int> MidiChannelOffsets { get; set; }
        [DataMember]
        public List<TrackItem> MidiTracks { get => midiTracks; set => midiTracks = value; }
        [DataMember]
        public int Offset { get; set; }
        [DataMember]
        public int NumChords { get; set; }

        public TrackSelectionData()
        {

        }

        public TrackSelectionData(TrackSelectionData oldData)
        {
            MidiChannels = oldData.MidiChannels.ConvertAll(channel => new MidiChannelItem(channel));
            MidiTracks = oldData.MidiTracks.ConvertAll(track => new TrackItem(track));
            MidiChannelOffsets = new Dictionary<int, int>(oldData.MidiChannelOffsets);
            Offset = oldData.Offset;
            NumChords = oldData.NumChords;
        }

    }
}
