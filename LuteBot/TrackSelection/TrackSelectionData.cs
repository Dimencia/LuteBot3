using Newtonsoft.Json;

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
        public List<TrackItem> MidiTracks { get => midiTracks; set => midiTracks = value; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<int, int> MidiChannelOffsets { get; set; }
        [DataMember]
        public int Offset { get; set; }
        [DataMember]
        public int NumChords { get; set; }
        [DataMember]
        public int InstrumentID { get; set; } // Added to make our save files not a dictionary

        public TrackSelectionData()
        {

        }

        public TrackSelectionData(TrackSelectionData oldData, int instrumentId)
        {
            MidiChannels = oldData.MidiChannels.ConvertAll(channel => new MidiChannelItem(channel));
            MidiTracks = oldData.MidiTracks.ConvertAll(track => new TrackItem(track));
            if (oldData.MidiChannelOffsets != null)
                foreach (var kvp in oldData.MidiChannelOffsets)
                {
                    var channel = MidiChannels.Where(c => c.Id == kvp.Key).FirstOrDefault();
                    if (channel != null)
                        channel.offset = kvp.Value;
                }
            Offset = oldData.Offset;
            NumChords = oldData.NumChords;
            InstrumentID = instrumentId;// What if it didn't have it?  We better take the value in here to ensure it's right
        }

        public TrackSelectionData(SimpleTrackSelectionData oldData)
        {
            MidiChannels = oldData.MidiChannels.ConvertAll(channel => new MidiChannelItem(channel));
            MidiTracks = oldData.MidiTracks.ConvertAll(track => new TrackItem(track));
            if (oldData.MidiChannelOffsets != null)
                foreach (var kvp in oldData.MidiChannelOffsets)
                {
                    var channel = MidiChannels.Where(c => c.Id == kvp.Key).FirstOrDefault();
                    if (channel != null)
                        channel.offset = kvp.Value;
                }
            Offset = oldData.Offset;
            NumChords = oldData.NumChords;
            InstrumentID = oldData.InstrumentID; // This should always have it
        }
    }

    //[DataContract]
    public class SimpleTrackSelectionData : TrackSelectionData
    {
        [DataMember]
        new public List<SimpleMidiChannelItem> MidiChannels { get; set; }

        public SimpleTrackSelectionData() { }

        // InstrumentID must be passed on instantiation; this is usually gotten as the key from the dictionary these are usually stored in
        public SimpleTrackSelectionData(TrackSelectionData oldData, int instrumentId)
        {
            MidiChannels = oldData.MidiChannels.ConvertAll(channel => new SimpleMidiChannelItem(channel)).ToList();
            MidiTracks = oldData.MidiTracks.ConvertAll(track => new TrackItem(track)).ToList();
            if (oldData.MidiChannelOffsets != null)
                foreach (var kvp in oldData.MidiChannelOffsets)
                {
                    var channel = MidiChannels.Where(c => c.Id == kvp.Key).FirstOrDefault();
                    if (channel != null)
                        channel.offset = kvp.Value;
                }
            Offset = oldData.Offset;
            NumChords = oldData.NumChords;
            InstrumentID = instrumentId;
        }
    }
}
