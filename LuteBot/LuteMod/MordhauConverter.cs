using LuteBot.TrackSelection;

using LuteBot.LuteMod.Sequencing;
using Sanford.Multimedia.Midi;
using System.Collections.Generic;
using Track = LuteBot.LuteMod.Sequencing.Track;
using LuteBot.UI.Utils;

namespace LuteBot.LuteMod
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
        public int LowestPlayed { get; set; }
        public int HighNote { get => lowNote + range; }
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

        public int AddTrack(int instrumentId = 0)
        {
            partition.Tracks.Add(new Track() { InstrumentId = instrumentId });
            return partition.Tracks.Count - 1;
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

        public List<MidiChannelItem> GetEnabledTracksInTrack(int id)
        {
            Track track = GetTrackAt(id);
            if (track != null)
            {
                return track.MidiTracks;
            }
            return new List<MidiChannelItem>();
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

        private Note BuildNote(Note note, Instrument instrument)
        {
            if (note.Type == NoteType.Tempo)
            {
                partition.Tempo = note.Tone / division;
                return new Note() { Tick = note.Tick, Tone = note.Tone / division, Type = note.Type };
            }
            if (isConversionEnabled)
            {
                if (note.Tone < lowNote + LowestPlayed)
                {
                    note.Tone = lowNote + LowestPlayed + note.Tone % 12;
                }
                else if (note.Tone > HighNote + LowestPlayed)
                {
                    note.Tone = HighNote + LowestPlayed - 12 + note.Tone % 12;
                }

            }
            return new Note() { Tick = note.Tick, Tone = note.Tone - instrument.LowestPlayedNote + instrument.LowestSentNote, Type = note.Type, duration = note.duration };
        }

        public void FillTrack(int id, Instrument instrument, List<Note> notes)
        {
            //notes.Reverse();
            Track track = partition.Tracks[id];
            foreach (Note note in notes)
            {
                track.Notes.Add(BuildNote(note, instrument));
            }
        }

        public string GetPartitionToString()
        {
            return partition.ToString();
        }
    }
}
