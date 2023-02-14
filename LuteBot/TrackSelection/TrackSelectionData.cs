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
        [DataMember]
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
            {
                foreach (var kvp in oldData.MidiChannelOffsets)
                {
                    var channel = MidiChannels.Where(c => c.Id == kvp.Key).FirstOrDefault();
                    if (channel != null)
                        channel.offset = kvp.Value;
                }
                oldData.MidiChannelOffsets = null;
            }
            Offset = oldData.Offset;
            NumChords = oldData.NumChords;
            InstrumentID = instrumentId;// What if it didn't have it?  We better take the value in here to ensure it's right
        }

        //public TrackSelectionData(SimpleTrackSelectionData oldData)
        //{
        //    MidiChannels = oldData.MidiChannels.ConvertAll(channel => new MidiChannelItem(channel));
        //    MidiTracks = oldData.MidiTracks.ConvertAll(track => new TrackItem(track));
        //    if (oldData.MidiChannelOffsets != null)
        //        foreach (var kvp in oldData.MidiChannelOffsets)
        //        {
        //            var channel = MidiChannels.Where(c => c.Id == kvp.Key).FirstOrDefault();
        //            if (channel != null)
        //                channel.offset = kvp.Value;
        //        }
        //    Offset = oldData.Offset;
        //    NumChords = oldData.NumChords;
        //    InstrumentID = oldData.InstrumentID; // This should always have it
        //}

        public TrackSelectionData WithData(SimpleTrackSelectionData newData)
        {
            // The channels and tracks need to already exist, and then we copy over only: Active, Offset, and anything in the noteList that isn't null
            // Then the other data is OK to overwrite

            //MidiChannels = oldData.MidiChannels.ConvertAll(channel => new MidiChannelItem(channel));
            //MidiTracks = oldData.MidiTracks.ConvertAll(track => new TrackItem(track));

            var result = new TrackSelectionData(this, newData.InstrumentID);

            for(int i = 0; i < result.MidiChannels.Count; i++) 
            {
                var channel = result.MidiChannels[i];
                SimpleMidiChannelItem newChannel = null;
                if(newData.MidiChannels != null && newData.MidiChannels.Count > 0)
                    newChannel = newData.MidiChannels.Where(c => c.Id == channel.Id).SingleOrDefault();
                if (newChannel != null)
                {
                    result.MidiChannels[i] = channel.WithData(newChannel);
                }
                if (newData.MidiChannelOffsets != null)
                {
                    if (newData.MidiChannelOffsets.ContainsKey(channel.Id))
                        channel.offset = newData.MidiChannelOffsets[channel.Id];
                }
            }
            for (int i = 0; i < result.MidiTracks.Count; i++)
            {
                var channel = result.MidiTracks[i];
                SimpleMidiChannelItem newChannel = null;
                if (newData.MidiTracks != null && newData.MidiTracks.Count > 0)
                    newChannel = newData.MidiTracks.Where(c => c.Id == channel.Id).SingleOrDefault();
                if (newChannel != null)
                {
                    result.MidiTracks[i] = new TrackItem(channel.WithData(newChannel));
                }
            }
            result.Offset = newData.Offset;
            result.NumChords = newData.NumChords;
            //result.InstrumentID = newData.InstrumentID; // It should already be right; oldData may have an incorrect one

            return result;
        }
    }

    //[DataContract]
    public class SimpleTrackSelectionData : TrackSelectionData
    {
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        new public List<SimpleMidiChannelItem> MidiChannels { get; set; } = new List<SimpleMidiChannelItem>();
        [DataMember]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        new public List<SimpleMidiChannelItem> MidiTracks { get; set; } = new List<SimpleMidiChannelItem>();

        public SimpleTrackSelectionData() { }

        // InstrumentID must be passed on instantiation; this is usually gotten as the key from the dictionary these are usually stored in
        public SimpleTrackSelectionData(TrackSelectionData newData, int instrumentId, TrackSelectionData originalData)
        {
            // We hopefully handled this before we got here, but just in case
            if (newData.MidiChannelOffsets != null)
            {
                foreach (var kvp in newData.MidiChannelOffsets)
                {
                    var channel = newData.MidiChannels.Where(c => c.Id == kvp.Key).FirstOrDefault();
                    if (channel != null)
                        channel.offset = kvp.Value;
                }
                newData.MidiChannelOffsets = null;
            }
            for (int i = 0; i < newData.MidiChannels.Count; i++)
            {
                var channel = newData.MidiChannels[i];
                if (originalData != null)
                {
                    var originalChannel = originalData.MidiChannels.Where(c => c.Id == channel.Id).SingleOrDefault();
                    if (originalChannel != null && !channel.Equals(originalChannel))
                        MidiChannels.Add(new SimpleMidiChannelItem(newData.MidiChannels[i], originalChannel));
                }
            }
            for (int i = 0; i < newData.MidiTracks.Count; i++)
            {
                var channel = newData.MidiTracks[i];
                if (originalData != null)
                {
                    var originalChannel = originalData.MidiTracks.Where(c => c.Id == channel.Id).SingleOrDefault();
                    if (originalChannel != null && !channel.Equals(originalChannel))
                        MidiTracks.Add(new SimpleMidiChannelItem(newData.MidiTracks[i], originalChannel));
                }
            }

            if (MidiChannels.Count == 0)
                MidiChannels = null;
            if (MidiTracks.Count == 0)
                MidiTracks = null;
            
            Offset = newData.Offset;
            NumChords = newData.NumChords;
            InstrumentID = instrumentId;
        }

        // This is for backwards compatibility, converting full data into fully populated Simple ones
        public SimpleTrackSelectionData(TrackSelectionData newData, int instrumentId)
        {
            MidiChannels = newData.MidiChannels.ConvertAll(c => new SimpleMidiChannelItem(c));
            MidiTracks = newData.MidiTracks.ConvertAll(c => new SimpleMidiChannelItem(c));
            if (newData.MidiChannelOffsets != null)
            {
                foreach (var kvp in newData.MidiChannelOffsets)
                {
                    var channel = MidiChannels.Where(c => c.Id == kvp.Key).FirstOrDefault();
                    if (channel != null)
                        channel.offset = kvp.Value;
                }
                newData.MidiChannelOffsets = null;
            }
            // This really shouldn't happen here, but just in case it does
            if (MidiChannels.Count == 0)
                MidiChannels = null;
            if (MidiTracks.Count == 0)
                MidiTracks = null;
            Offset = newData.Offset;
            NumChords = newData.NumChords;
            InstrumentID = instrumentId;
        }
    }
}
