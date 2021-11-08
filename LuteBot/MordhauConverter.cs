using LuteBot.TrackSelection;

using LuteMod.Sequencing;
using Sanford.Multimedia.Midi;
using System.Collections.Generic;
using Track = LuteMod.Sequencing.Track;

namespace LuteMod.Converter
{
    public class MordhauConverter
    {
        private Partition partition;
        private int range;
        private int lowNote;
        private int division;
        private bool isConversionEnabled;

        public int Range { get => range; set => range = value; }
        public int LowNote { get => lowNote; set => lowNote = value; }
        public int HighNote { get => (lowNote + range); }
        public bool IsConversionEnabled { get => isConversionEnabled; set => isConversionEnabled = value; }

        public MordhauConverter()
        {
            partition = new Partition();
            range = 48;
            lowNote = 36;
        }

        public void ClearPartition()
        {
            partition = new Partition();
        }

        public void SetDivision(int division)
        {
            this.division = division;
        }

        public int GetTrackCount()
        {
            return partition.Tracks.Count;
        }

        public int GetNoteCount(int id)
        {
            return partition.Tracks[id].Notes.Count;
        }

        public void AddTrack()
        {
            partition.Tracks.Add(new Track());
        }

        public void DelTrack(int id)
        {
            partition.Tracks.RemoveAt(id);
        }

        public void ChangePartitionName(string name)
        {
            partition.Name = name;
        }

        public void SetPartitionTempo(int tempo)
        {
            partition.Tempo = tempo;
        }

        public void SetPartitionNotes(int index, List<Note> notes)
        {
            partition.Tracks[index].Notes = notes;
        }

        public bool CanMoveOctave(bool up)
        {
            if (up)
            {
                return HighNote <= 115;
            }
            else
            {
                return lowNote >= 12;
            }
        }

        public void MoveOctave(bool up)
        {
            if (CanMoveOctave(up))
            {
                if (up)
                {
                    lowNote = lowNote + 12;
                }
                else
                {
                    lowNote = lowNote - 12;
                }
            }
        }

        public void SetEnabledTracksInTrack(int id, List<TrackItem> items)
        {
            Track track = GetTrackAt(id);
            if (track != null)
            {
                foreach (TrackItem item in items)
                {
                    if (item.Active)
                    {
                        track.MidiTracks.Add(item);
                    }
                }
            }
        }

        public void SetEnabledMidiChannelsInTrack(int id, List<MidiChannelItem> items)
        {
            Track track = GetTrackAt(id);
            if (track != null)
            {
                foreach (MidiChannelItem item in items)
                {
                    if (item.Active)
                    {
                        track.MidiChannels.Add(item);
                    }
                }
            }
        }

        public List<TrackItem> GetEnabledTracksInTrack(int id)
        {
            Track track = GetTrackAt(id);
            if (track != null)
            {
                return track.MidiTracks;
            }
            return new List<TrackItem>();
        }

        public List<MidiChannelItem> GetEnabledChannelsInTrack(int id)
        {
            Track track = GetTrackAt(id);
            if (track != null)
            {
                return track.MidiChannels;
            }
            return new List<MidiChannelItem>();
        }

        private Track GetTrackAt(int id)
        {
            if (id >= 0 && id < partition.Tracks.Count)
            {
                return partition.Tracks[id];
            }
            return null;
        }

        private Note BuildNote(Note note)
        {
            if (note.Type == NoteType.Tempo)
            {
                partition.Tempo = note.Tone / division;
                return new Note() { Tick = note.Tick, Tone = note.Tone / division, Type = note.Type };
            }
            if (isConversionEnabled)
            {
                if (note.Tone < lowNote)
                {
                    note.Tone = lowNote + (note.Tone % 12);
                }
                else if (note.Tone > HighNote)
                {
                    note.Tone = (HighNote - 12) + (note.Tone % 12);
                }

            }
            return new Note() { Tick = note.Tick, Tone = note.Tone - lowNote, Type = note.Type };
        }

        public void FillTrack(int id, List<Note> notes)
        {
            notes.Reverse();
            Track track = partition.Tracks[id];
            bool found = false;
            int i = 0;
            foreach (Note note in notes)
            {
                if (track.Notes.Count > 0)
                {
                    for (i = 0; !found && i < track.Notes.Count; i++)
                    {
                        found = track.Notes[i].Tick >= note.Tick;
                    }
                }
                if (found)
                {
                    track.Notes.Insert((i - 1 >= 0) ? i - 1 : i, BuildNote(note));
                }
                else
                {
                    track.Notes.Add(BuildNote(note));
                }
                found = false;
            }
        }

        public string GetPartitionToString()
        {
            return partition.ToString();
        }
    }
}
