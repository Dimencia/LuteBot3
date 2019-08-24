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

        public List<MidiChannelItem> MidiChannels { get => midiChannels; set { midiChannels = value; ResetRequest(); } }
        public List<TrackItem> MidiTracks { get => midiTracks; set { midiTracks = value; ResetRequest(); } }
        public bool ActivateAllChannels { get => activateAllChannels; set { activateAllChannels = value; ResetRequest(); } }
        public bool ActivateAllTracks { get => activateAllTracks; set { activateAllTracks = value; ResetRequest(); } }

        private bool activateAllChannels;
        private bool activateAllTracks;

        public bool autoLoadProfile;
        public string FileName;

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
        }

        public void ToggleChannel(int index, bool active)
        {
            if (index >= 0 && index < midiChannels.Count)
            {
                midiChannels[index].Active = active;
                ResetRequest();
            }
        }

        public void SaveTrackManager()
        {
            TrackSelectionData data = new TrackSelectionData();
            data.MidiChannels = MidiChannels;
            data.MidiTracks = MidiTracks;
            SaveManager.SaveTrackSelectionData(data, FileName);
        }

        public void LoadTrackManager()
        {
            TrackSelectionData data = SaveManager.LoadTrackSelectionData(FileName);
            if (data != null)
            {
                this.midiChannels = data.MidiChannels;
                this.midiTracks = data.MidiTracks;
                EventHelper();
            }
        }

        public void LoadTracks(List<int> channels, List<string> tracks)
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
                            if (!(channelItem.Active || activateAllChannels) || channelItem.Name.ToLower().Equals("glockenspiel"))
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
