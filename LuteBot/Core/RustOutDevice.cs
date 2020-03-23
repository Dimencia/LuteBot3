using LuteBot.Config;
using LuteBot.IO.KB;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.Core
{
    public class RustOutDevice
    {
        // To keep things simple...
        // Selecting an instrument should simply set the lowNoteId and highNoteId and assume all else is well


        private int lowNoteId = 0;
        private int highNoteId = 24;

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

        private Stopwatch stopWatch;

        public RustOutDevice()
        {
            stopWatch = new Stopwatch();
        }

        private void UpdateNoteIdBounds()
        {
            // First, for these calcs, get the lowest C and highest C+11
            var highMidiNoteId = HighMidiNoteId - HighMidiNoteId % 12 - 1;
            var lowMidiNoteId = LowMidiNoteId - LowMidiNoteId % 12; // I remember this being more complicated but I guess not
            
            int noteRange = highMidiNoteId - lowMidiNoteId;
            

            int luteRange = ConfigManager.GetIntegerProperty(PropertyItem.AvaliableNoteCount);
            int luteLowestNote = ConfigManager.GetIntegerProperty(PropertyItem.LowestNoteId);
            //if (noteRange > luteRange)
            //{
                //lowNoteId = ((noteRange / 2) + (luteLowestNote - lowMidiNoteId)) - (luteRange / 2);
                //highNoteId = ((noteRange / 2) + (luteLowestNote - lowMidiNoteId)) + (luteRange / 2);
                //lowNoteId = lowNoteId - (lowNoteId % 12);
                //highNoteId = highNoteId - (highNoteId % 12) - 1;
                // I really don't get the above
                lowNoteId = luteLowestNote;
                highNoteId = luteLowestNote + luteRange;
                conversionNeeded = true;
            //}
            // We need conversion either way because of the way we're working with offset
            /*else
            {
                conversionNeeded = false;
                //if the note range of the midi is lower than the lute range
                lowNoteId = lowMidiNoteId;
                highNoteId = highMidiNoteId;
            }
            */
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
                    // We don't know if lowNoteId is a C or not
                    //newData1 = lowNoteId + (oldData1 % 12); // Keeps the same note but moves to be within an octave of the lowest note
                    newData1 = lowNoteId + (12 - lowNoteId % 12) + (oldData1 % 12); // Should get the nearest octave to lownote, then add oldData
                    outOfRange = true;
                }
                else
                {
                    if (oldData1 > highNoteId)
                    {
                        // Don't know if this is C ir not
                        //newData1 = (highNoteId - 11) + (oldData1 % 12);
                        newData1 = highNoteId - highNoteId % 12 + (oldData1 % 12);
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

    }
}