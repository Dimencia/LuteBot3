using LuteBot.Config;
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

        private int numChords;
        private int noteOffset;
        private bool activateAllChannels;
        private bool activateAllTracks;

        public bool autoLoadProfile;
        public string FileName { get; set; }

        public event EventHandler TrackChanged;
        public event EventHandler<TrackItem> ToggleTrackRequest;
        public event EventHandler OutDeviceResetRequest;

        public TrackSelectionManager()
        {
            midiChannels = new List<MidiChannelItem>();
            midiTracks = new List<TrackItem>();
            activateAllChannels = false;
            activateAllTracks = false;
            autoLoadProfile = true;
            numChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
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
            Dictionary<int, int> newMidiOffsets = new Dictionary<int, int>();
            foreach (var channel in MidiChannels)
            {
                var newChannel = data.MidiChannels.Where(c => c.Id == channel.Id).FirstOrDefault();
                if (newChannel != null)
                {
                    channel.Active = newChannel.Active;
                    if (data.MidiChannelOffsets.ContainsKey(channel.Id))
                        newMidiOffsets.Add(channel.Id, data.MidiChannelOffsets[channel.Id]);
                }
            }
            foreach (var track in MidiTracks)
            {
                var newTrack = data.MidiTracks.Where(t => t.Id == track.Id).FirstOrDefault();
                if (newTrack != null)
                    track.Active = newTrack.Active;
            }

            NoteOffset = data.Offset;
            NumChords = data.NumChords;
        }

        public TrackSelectionData GetTrackSelectionData()
        {
            TrackSelectionData data = new TrackSelectionData();
            data.MidiChannels = MidiChannels;
            data.MidiTracks = MidiTracks;
            data.Offset = NoteOffset;
            data.MidiChannelOffsets = MidiChannelOffsets;
            data.NumChords = NumChords;
            return data;
        }

        public void SaveTrackManager()
        {
            TrackSelectionData data = new TrackSelectionData();
            data.MidiChannels = MidiChannels;
            data.MidiTracks = MidiTracks;
            data.Offset = NoteOffset;
            data.MidiChannelOffsets = MidiChannelOffsets;
            data.NumChords = NumChords;
            SaveManager.SaveTrackSelectionData(data, FileName);
        }

        public void LoadTrackManager()
        {
            TrackSelectionData data = SaveManager.LoadTrackSelectionData(FileName);
            if (data != null)
            {
                this.midiChannels = data.MidiChannels;
                this.midiTracks = data.MidiTracks;
                this.noteOffset = data.Offset;
                this.midiChannelOffsets = data.MidiChannelOffsets;
                if (data.NumChords > 0)
                    this.NumChords = data.NumChords;
                else
                    this.NumChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
                EventHelper();
            }
            else
            { // Reset these if there's no settings for something
                this.NoteOffset = 0;
                this.MidiChannelOffsets = new Dictionary<int, int>();
            }
        }

        public void LoadTracks(List<int> channels, List<string> tracks, TrackSelectionManager tsm)
        {
            midiChannels.Clear();
            midiTracks.Clear();
            int length = channels.Count;
            if (length < tracks.Count)
            {
                length = tracks.Count;
            }
            for (int i = 0; i < length; i++)
            {
                if (i < tracks.Count)
                {
                    midiTracks.Add(new TrackItem() { Id = i, Name = tracks[i], Active = true });
                }
                if (i < channels.Count)
                {
                    midiChannels.Add(new MidiChannelItem() { Id = channels[i], Active = true });
                }
            }
            NoteOffset = tsm.NoteOffset;
            NumChords = tsm.NumChords;
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
                midiChannels[index].Active = active;
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
