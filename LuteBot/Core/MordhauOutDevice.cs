using LuteBot.Config;
using LuteBot.IO.KB;
using LuteBot.TrackSelection;

using Sanford.Multimedia.Midi;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.Core
{
    public class MordhauOutDevice
    {
        private int lowNoteId = 0;
        private int highNoteId = 24;
        private int luteMin = 0;
        private int numChords = 2;

        private int lowMidiNoteId = 0;
        private int highMidiNoteId = 127;

        private bool conversionNeeded;
        private bool cooldownNeeded = true;
        private bool muteOutOfRange = false;

        public int LowMidiNoteId { get => lowMidiNoteId; set { lowMidiNoteId = value; UpdateNoteIdBounds(); } }
        public int HighMidiNoteId { get => highMidiNoteId; set { highMidiNoteId = value; UpdateNoteIdBounds(); } }

        public int LowNoteId { get => lowNoteId; set { ForceNoteBounds(value, true); } }
        public int HighNoteId { get => highNoteId; set { ForceNoteBounds(value, false); } }

        public bool ConversionNeeded { get => conversionNeeded; }
        public bool CooldownNeeded { get => cooldownNeeded; set => cooldownNeeded = value; }
        public bool MuteOutOfRange { get => muteOutOfRange; set => muteOutOfRange = value; }

        private bool ForbidsChords = false;

        private Stopwatch stopWatch;

        private TrackSelectionManager TSM = null;
        public MordhauOutDevice(TrackSelectionManager tsm)
        {
            stopWatch = new Stopwatch();
            TSM = tsm;
        }

        public void UpdateNoteIdBounds()
        {
            int noteRange = highMidiNoteId - lowMidiNoteId;
            int luteRange = ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount);
            luteMin = ConfigManager.GetIntegerProperty(PropertyItem.LowestNoteId);
            ForbidsChords = ConfigManager.GetBooleanProperty(PropertyItem.ForbidsChords);

            int lowestPlayed = 36;
            try
            {
                lowestPlayed = ConfigManager.GetIntegerProperty(PropertyItem.LowestPlayedNote);
            }
            catch
            {
                ConfigManager.SetProperty(PropertyItem.LowestPlayedNote, lowestPlayed.ToString()); // Enforce a default if they don't have it
                ConfigManager.SaveConfig();
            }
            if (noteRange > luteRange || lowMidiNoteId < luteMin || highMidiNoteId > luteMin + luteRange)
            {
                //lowNoteId = ((noteRange / 2) + lowMidiNoteId) - (luteRange / 2);
                //highNoteId = ((noteRange / 2) + lowMidiNoteId) + (luteRange / 2);
                lowNoteId = lowestPlayed + luteMin;
                highNoteId = lowNoteId + luteRange;
                //lowNoteId = lowNoteId - (lowNoteId % 12);
                //highNoteId = highNoteId - (highNoteId % 12);
                conversionNeeded = true;
            }
            else
            {
                conversionNeeded = false;
                //if the note range of the midi is lower than the lute range
                lowNoteId = lowMidiNoteId;
                highNoteId = highMidiNoteId;
            }
        }

        private void ForceNoteBounds(int value, bool isLower)
        {
            if (isLower)
            {
                lowNoteId = value;
                highNoteId = value + ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount) - 1;
            }
            else
            {
                highNoteId = value;
                lowNoteId = value - ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount) - 1;
            }
        }

        public ChannelMessage FilterNote(ChannelMessage message, int offset)
        {
            if (conversionNeeded && (message.Command == ChannelCommand.NoteOn || message.Command == ChannelCommand.NoteOff))
            {
                bool outOfRange = false;
                int newData1 = 0;
                int oldData1 = message.Data1 + offset;
                int velocity = message.Data2;

                if (oldData1 < lowNoteId)
                {
                    newData1 = lowNoteId + (oldData1 % 12);
                    outOfRange = true;
                }
                else
                {
                    if (oldData1 > highNoteId)
                    {
                        newData1 = (highNoteId - 12) + (oldData1 % 12);
                        outOfRange = true;
                    }
                    else
                    {
                        newData1 = oldData1;
                    }
                }
                if (outOfRange && muteOutOfRange)
                {
                    velocity = 0;
                }
                return new ChannelMessage(message.Command, message.MidiChannel, newData1, velocity);
            }
            else
            {
                return message;
            }
        }



        private List<int> notesThisCooldown = new List<int>();

        public void SendNote(ChannelMessage message, int offset)
        {
            if (TSM != null)
                numChords = TSM.NumChords;
            else
                numChords = ConfigManager.GetIntegerProperty(PropertyItem.NumChords);
            ChannelMessage filterResult;
            if (message.Command == ChannelCommand.NoteOn && message.Data2 > 0)
            {
                int noteCooldown = int.Parse(ConfigManager.GetProperty(PropertyItem.NoteCooldown));
                if (cooldownNeeded)
                {
                    if (!ForbidsChords)
                    {
                        if (!stopWatch.IsRunning)
                        {
                            filterResult = FilterNote(message, offset);
                            int note = filterResult.Data1 - lowNoteId + luteMin;
                            ActionManager.PlayNote(note, message.MidiChannel);

                            stopWatch.Start();
                            notesThisCooldown.Add(note);
                        }
                        else
                        {
                            filterResult = FilterNote(message, offset);
                            int note = filterResult.Data1 - lowNoteId + luteMin;
                            // We use numChords-1 because by default we always allow 2 chords by the way the timer resets.  When they enter 3, we really only want to play two notes during cooldown
                            if (stopWatch.ElapsedMilliseconds < noteCooldown && notesThisCooldown.Count < (numChords - 1) && !notesThisCooldown.Contains(note))
                            {
                                ActionManager.PlayNote(note, message.MidiChannel);
                                notesThisCooldown.Add(note);
                            }
                            else if (stopWatch.ElapsedMilliseconds >= noteCooldown)
                            {
                                ActionManager.PlayNote(note, message.MidiChannel);
                                stopWatch.Reset();
                                notesThisCooldown = new List<int>() { note };
                            }
                        }
                    }
                    else
                    {
                        filterResult = FilterNote(message, offset);
                        int note = filterResult.Data1 - lowNoteId + luteMin;
                        if (!stopWatch.IsRunning)
                        {
                            ActionManager.PlayNote(note, message.MidiChannel);

                            stopWatch.Start();
                        }
                        else
                        {
                            if (stopWatch.ElapsedMilliseconds >= noteCooldown)
                            {
                                ActionManager.PlayNote(note, message.MidiChannel);
                                stopWatch.Restart();
                            }
                        }
                    }
                }
                else
                {
                    filterResult = FilterNote(message, offset);
                    if (message.Data2 > 0)
                    {
                        ActionManager.PlayNote(filterResult.Data1 - lowNoteId + luteMin, message.MidiChannel);
                    }
                }
            }
        }
    }
}
