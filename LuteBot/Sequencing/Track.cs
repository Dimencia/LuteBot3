using LuteBot.TrackSelection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteMod.Sequencing
{
    public class Track
    {
        private List<Note> notes;
        private List<MidiChannelItem> midiChannels;
        private List<TrackItem> midiTracks;

        public List<Note> Notes { get => notes; set => notes = value; }
        public List<MidiChannelItem> MidiChannels { get => midiChannels; set => midiChannels = value; }
        public List<TrackItem> MidiTracks { get => midiTracks; set => midiTracks = value; }

        public Track()
        {
            notes = new List<Note>();
            midiChannels = new List<MidiChannelItem>();
            midiTracks = new List<TrackItem>();
        }

        public override string ToString()
        {
            StringBuilder strbld = new StringBuilder();

            foreach (Note note in notes)
            {
                strbld.Append(note.ToString()).Append(";");
            }
            if (strbld.Length > 0)
            {
                strbld.Remove(strbld.Length - 1, 1);
            }
            return strbld.ToString();
        }
    }
}