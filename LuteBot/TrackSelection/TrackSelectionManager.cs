using LuteBot.Config;
using LuteBot.Core.Midi;
using LuteBot.IO.Files;

using Sanford.Multimedia.Midi;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.TrackSelection
{
    public class TrackSelectionManager
    {
        private List<MidiChannelItem> midiChannels;
        private List<TrackItem> midiTracks;
        private Dictionary<int, int> midiChannelOffsets = new Dictionary<int, int>();
        private Dictionary<int, int> maxNoteByChannel = new Dictionary<int, int>();
        private Dictionary<int, int> minNoteByChannel = new Dictionary<int, int>();


        public List<MidiChannelItem> MidiChannels { get => midiChannels; set { midiChannels = value; ResetRequest(); } }
        public List<TrackItem> MidiTracks { get => midiTracks; set { midiTracks = value; ResetRequest(); } }
        public Dictionary<int, int> MidiChannelOffsets { get => midiChannelOffsets; set { midiChannelOffsets = value; ResetRequest(); } }
        public Dictionary<int, int> MaxNoteByChannel { get => maxNoteByChannel; set { maxNoteByChannel = value; ResetRequest(); } }
        public Dictionary<int, int> MinNoteByChannel { get => minNoteByChannel; set { minNoteByChannel = value; ResetRequest(); } }
        public bool ActivateAllChannels { get => activateAllChannels; set { activateAllChannels = value; ResetRequest(); } }
        public bool ActivateAllTracks { get => activateAllTracks; set { activateAllTracks = value; ResetRequest(); } }
        public int NoteOffset { get => noteOffset; set { noteOffset = value; ResetRequest(); } }
        public int NumChords { get => numChords; set { numChords = value; ResetRequest(); } }
        public Dictionary<int, TrackSelectionData> DataDictionary { get; set; } = new Dictionary<int, TrackSelectionData>();

        private int numChords;
        private int noteOffset;
        private bool activateAllChannels;
        private bool activateAllTracks;

        public bool autoLoadProfile;
        public string FileName { get; set; }

        public event EventHandler TrackChanged;
        public event EventHandler<TrackItem> ToggleTrackRequest;
        public event EventHandler OutDeviceResetRequest;

        public MidiPlayer Player { get; set; }

        public TrackSelectionManager()
        {
            midiChannels = new List<MidiChannelItem>();
            midiTracks = new List<TrackItem>();
            activateAllChannels = false;
            activateAllTracks = false;
            autoLoadProfile = true;
            numChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
            UpdateTrackSelectionForInstrument(0); // Saves defaults, and also updates us in case lute wasn't the one selected
        }

        public void ToggleChannel(int index, bool active)
        {
            if (index >= 0 && index < midiChannels.Count)
            {
                midiChannels[index].Active = active;
                ResetRequest();
            }
        }

        public void SetTrackSelectionData(TrackSelectionData data)
        {
            // So... Channels and tracks might have changed
            // I think we need to compare current channels and remove any values that aren't in them before storing
            //Dictionary<int, int> newMidiOffsets = new Dictionary<int, int>();
            //foreach (var channel in MidiChannels)
            //{
            //    var newChannel = data.MidiChannels.Where(c => c.Id == channel.Id).FirstOrDefault();
            //    if (newChannel != null)
            //    {
            //        channel.Active = newChannel.Active;
            //        if (data.MidiChannelOffsets.ContainsKey(channel.Id))
            //            newMidiOffsets.Add(channel.Id, data.MidiChannelOffsets[channel.Id]);
            //    }
            //}
            //foreach (var track in MidiTracks)
            //{
            //    var newTrack = data.MidiTracks.Where(t => t.Id == track.Id).FirstOrDefault();
            //    if (newTrack != null)
            //        track.Active = newTrack.Active;
            //}
            //
            //NoteOffset = data.Offset;
            //NumChords = data.NumChords;

            this.MidiChannels = data.MidiChannels.ConvertAll(channel => new MidiChannelItem(channel));
            this.MidiTracks = data.MidiTracks.ConvertAll(track => new TrackItem(track));
            this.NoteOffset = data.Offset;
            this.MidiChannelOffsets = new Dictionary<int, int>(data.MidiChannelOffsets);
            this.NumChords = data.NumChords;

            DataDictionary[ConfigManager.GetIntegerProperty(PropertyItem.Instrument)] = new TrackSelectionData(data);
        }


        public void UpdateTrackSelectionForInstrument(int oldInstrument)
        {
            DataDictionary[oldInstrument] = GetTrackSelectionData();
            int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
            if (DataDictionary.ContainsKey(instrumentId))
                SetTrackSelectionData(DataDictionary[instrumentId]);
            else if (instrumentId == 3 && DataDictionary.ContainsKey(1))
            {
                // If it's duet flute but there is no data for it, and we also have a flute track, copy the flute settings
                SetTrackSelectionData(DataDictionary[1]);
            }
            else if (instrumentId == 2 && DataDictionary.ContainsKey(1))
            {
                // If it's duet lute with no data, and there is a flute track, copy the lute track and disable whatever the flute has active
                SetTrackSelectionData(DataDictionary[0]);
                var fluteData = DataDictionary[1];

                foreach(var channel in MidiChannels)
                {
                    // There's no real situation where they should have any disparity between their channels
                    if (fluteData.MidiChannels.Where(d => d.Id == channel.Id).Single().Active)
                        channel.Active = false;
                }
            }
            else if (DataDictionary.ContainsKey(0))
                SetTrackSelectionData(DataDictionary[0]);
        }

        public TrackSelectionData GetTrackSelectionData()
        {
            TrackSelectionData data = new TrackSelectionData();
            data.MidiChannels = new List<MidiChannelItem>(MidiChannels);
            data.MidiTracks = new List<TrackItem>(MidiTracks);
            data.Offset = NoteOffset;
            data.MidiChannelOffsets = new Dictionary<int, int>(MidiChannelOffsets);
            data.NumChords = NumChords;
            return data;
        }

        public void SaveTrackManager(string filename = null)
        {
            DataDictionary[ConfigManager.GetIntegerProperty(PropertyItem.Instrument)] = GetTrackSelectionData();
            SaveManager.SaveTrackSelectionData(DataDictionary, FileName, filename);
        }

        public void LoadTrackManager()
        {
            var dataDictionary = SaveManager.LoadTrackSelectionData(FileName);
            if (dataDictionary != null)
            {
                DataDictionary = dataDictionary;
                TrackSelectionData data = null;
                int instrumentId = ConfigManager.GetIntegerProperty(PropertyItem.Instrument);
                if (DataDictionary.ContainsKey(instrumentId))
                    data = DataDictionary[instrumentId];
                else if(DataDictionary.ContainsKey(0))
                    data = DataDictionary[0];

                if (data != null)
                {

                    this.MidiChannels = new List<MidiChannelItem>(data.MidiChannels);
                    this.MidiTracks = new List<TrackItem>(data.MidiTracks);
                    this.NoteOffset = data.Offset;
                    this.MidiChannelOffsets = new Dictionary<int, int>(data.MidiChannelOffsets);
                    if (data.NumChords > 0)
                        this.NumChords = data.NumChords;
                    else
                        this.NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);

                    // Restore any channel names that might be null or incorrect, from older data that got saved
                    foreach (var c in midiChannels)
                        c.Name = Player.GetChannelName(c.Id);
                }
                else
                { // Reset these if there's no settings for something
                    DataDictionary.Clear();
                    this.NoteOffset = 0;
                    this.MidiChannelOffsets.Clear();
                    //this.midiChannels.Clear();
                    //this.midiTracks.Clear();
                    this.NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
                    UpdateTrackSelectionForInstrument(0); // Force the settings into both instrument 0 and the current one
                }
                EventHelper();
            }
            else
            {
                DataDictionary.Clear();
                this.NoteOffset = 0;
                this.MidiChannelOffsets.Clear();
                //this.midiChannels.Clear();
                //this.midiTracks.Clear();
                this.NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
                UpdateTrackSelectionForInstrument(0); // Force the settings into both instrument 0 and the current one
            }
        }

        public void LoadTracks(Dictionary<int, string> channels, Dictionary<int,string> tracks, TrackSelectionManager tsm)
        {
            midiChannels.Clear();
            midiTracks.Clear();

            foreach (var kvp in channels)
                if(kvp.Key != 9)
                    midiChannels.Add(new MidiChannelItem() { Id = kvp.Key, Active = true, Name = kvp.Value });
                else // Automatically disable glockenspiel channel
                    midiChannels.Add(new MidiChannelItem() { Id = kvp.Key, Active = false, Name = kvp.Value });
            foreach (var kvp in tracks)
                midiTracks.Add(new TrackItem() { Id = kvp.Key, Name = kvp.Value, Active = true });
            
            NoteOffset = tsm.NoteOffset;
            NumChords = tsm.NumChords;

            ResetRequest();
            EventHelper();
        }

        public ChannelMessage FilterMidiEvent(ChannelMessage message, int trackId)
        {
            ChannelMessage newMessage = message;
            TrackItem track = midiTracks.FirstOrDefault(x => x.Id == trackId);
            if (track != null && track.Active)
            {
                if (message.Command == ChannelCommand.NoteOn)
                {
                    foreach (MidiChannelItem channelItem in midiChannels)
                    {
                        if (channelItem.Id == message.MidiChannel)
                        {
                            // We're filtering out glockenspiel in a super hacky way here... 
                            // And removed it, because we might want them for things sometimes.   || channelItem.Name.ToLower().Equals("glockenspiel")
                            if (!(channelItem.Active || activateAllChannels))
                            {
                                newMessage = new ChannelMessage(ChannelCommand.NoteOn, message.MidiChannel, message.Data1, 0);
                            }
                        }
                    }
                }
            }
            else
            {
                newMessage = new ChannelMessage(ChannelCommand.NoteOn, message.MidiChannel, message.Data1, 0);
            }
            return newMessage;
        }

        public void UnloadTracks()
        {
            midiChannels.Clear();
            midiTracks.Clear();
            NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
            EventHelper();
        }

        public void ToggleTrackActivation(bool active, int index)
        {
            if (index >= 0 && index < midiTracks.Count)
            {
                midiTracks[index].Active = active;
                ToggleTrackRequestHelper(midiTracks[index]);
            }
        }
        public void ToggleChannelActivation(bool active, int index)
        {
            if (index >= 0 && index < midiChannels.Count)
            {
                MidiChannels[index].Active = active;
                ResetRequest();
            }
        }

        private void ResetRequest()
        {
            EventHandler handler = OutDeviceResetRequest;
            handler?.Invoke(this, new EventArgs());
        }

        private void ToggleTrackRequestHelper(TrackItem item)
        {
            EventHandler<TrackItem> handler = ToggleTrackRequest;
            handler?.Invoke(this, item);
        }

        private void EventHelper()
        {
            EventHandler handler = TrackChanged;
            handler?.Invoke(this, new EventArgs());
        }
    }
}
