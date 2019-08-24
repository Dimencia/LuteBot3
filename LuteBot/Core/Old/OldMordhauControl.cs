using LuteBot.Logger;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.Old
{
    class RIPMordhauControl
    {

        public enum NoteConversionMode
        {
            FixedOctaves = 0,
            NonFixedOctaves = 1,
            FixedOctavesOld = 2,
            NonFixedOctavesOld = 3,
            Off = 4
        }

        private int lowNoteId = 0;
        private int highNoteId = 24;

        private int lowMidiNoteId = 0;
        private int highMidiNoteId = 127;

        public int LowMidiNoteId { get => lowMidiNoteId; set { lowMidiNoteId = value; UpdateNoteIdBounds(); } }
        public int HighMidiNoteId { get => highMidiNoteId; set { highMidiNoteId = value; UpdateNoteIdBounds(); } }


        public NoteConversionMode ConversionMode { get => conversionMode; set => conversionMode = value; }
        public int LowNoteId { get => lowNoteId; set => lowNoteId = value; }
        public int HighNoteId { get => highNoteId; set => highNoteId = value; }


        private NoteConversionMode conversionMode;

        private SynchronizationContext context;

        private Stopwatch stopWatch;
        private int noteCooldown;

        private bool conversionNeeded = true;

        private ActionManager actionManager;
        private ConfigManager configManager;

        private delegate void NoteMessageCallback(ChannelMessage message);

        private NoteMessageCallback noteOnCallback;


        public RIPMordhauControl(ActionManager actionManager, ConfigManager configManager)
        {
            context = SynchronizationContext.Current;
            this.actionManager = actionManager;

            this.configManager = configManager;
            Enum.TryParse<NoteConversionMode>(configManager.GetProperty("NoteConversionMode").Code, out conversionMode);
            LowNoteId = int.Parse(configManager.GetProperty("LowestNoteId").Code);
            highNoteId = LowNoteId + int.Parse(configManager.GetProperty("AvaliableNoteCount").Code) - 1;
            UpdateNoteIdBounds();
            noteCooldown = int.Parse(configManager.GetProperty("NoteCooldown").Code);
            stopWatch = new Stopwatch();
            noteOnCallback = delegate (ChannelMessage message)
            {
                int tempNote = message.Data1;
                if (conversionMode != NoteConversionMode.Off)
                {
                    tempNote = FinalNoteConversion(message.Data1);
                }
                message = new ChannelMessage(message.Command, message.MidiChannel, tempNote, message.Data2);
                actionManager.PlayNote(message.Data1);
                if (message.Data1 > int.Parse(configManager.GetProperty("LowestNoteId").Code) + int.Parse(configManager.GetProperty("AvaliableNoteCount").Code) - 1 || message.Data1 < int.Parse(configManager.GetProperty("LowestNoteId").Code))
                {
                    GlobalLogger.Error("MordhauControl", LoggerManager.LoggerLevel.Medium, "Note out of bounds !" + message.Data1);
                }
            };
            GlobalLogger.Log("MordhauControl", LoggerManager.LoggerLevel.Essential, "MordhauControl Initialised");
        }

        private int FinalNoteConversion(int noteId)
        {
            int finalNoteId;
            if (conversionMode == NoteConversionMode.FixedOctaves || conversionMode == NoteConversionMode.FixedOctavesOld)
            {
                finalNoteId = noteId - LowNoteId;
            }
            else if (conversionMode == NoteConversionMode.NonFixedOctaves || conversionMode == NoteConversionMode.NonFixedOctavesOld)
            {
                finalNoteId = noteId - (LowNoteId + (LowNoteId % 12));
                if (finalNoteId > int.Parse(configManager.GetProperty("LowestNoteId").Code) + int.Parse(configManager.GetProperty("AvaliableNoteCount").Code) - 1)
                {
                    finalNoteId = finalNoteId - 12;
                }
                if (finalNoteId < int.Parse(configManager.GetProperty("LowestNoteId").Code))
                {
                    finalNoteId = finalNoteId + 12;
                }
            }
            else
            {
                finalNoteId = noteId - LowNoteId;
            }
            return finalNoteId;
        }

        private void UpdateNoteIdBounds()
        {
            Enum.TryParse<NoteConversionMode>(configManager.GetProperty("NoteConversionMode").Code, out conversionMode);
            int tempLowNoteId = int.Parse(configManager.GetProperty("LowestNoteId").Code);
            int tempHighNoteId = tempLowNoteId + int.Parse(configManager.GetProperty("AvaliableNoteCount").Code) - 1;
            LowNoteId = ((highMidiNoteId - lowMidiNoteId) / 2) + lowMidiNoteId - ((tempHighNoteId - tempLowNoteId) / 2);
            if (conversionMode.Equals(NoteConversionMode.FixedOctaves) || conversionMode.Equals(NoteConversionMode.FixedOctavesOld))
            {
                LowNoteId = LowNoteId - LowNoteId % 12;
            }
            highNoteId = LowNoteId + int.Parse(configManager.GetProperty("AvaliableNoteCount").Code) - 1;
            if (conversionMode.Equals(NoteConversionMode.FixedOctaves) || conversionMode.Equals(NoteConversionMode.FixedOctavesOld))
            {
                highNoteId = highNoteId - (highNoteId % 12) - 1;
            }
            conversionNeeded = (highMidiNoteId - lowMidiNoteId) > (tempHighNoteId - tempLowNoteId);
            GlobalLogger.Log("MordhauControl", LoggerManager.LoggerLevel.Basic, "Mordhau Note bounds updated. Lowest note : " + LowNoteId + " Highest note : " + highNoteId);
        }

        public ChannelMessage FilterNote(ChannelMessage message)
        {
            NoteConversionMode tempConversionMode;
            Enum.TryParse<NoteConversionMode>(configManager.GetProperty("NoteConversionMode").Code, out tempConversionMode);
            if (!tempConversionMode.Equals(conversionMode))
            {
                UpdateNoteIdBounds();
            }
            ChannelMessage newMessage = message;

            if (message != null && (message.Command == ChannelCommand.NoteOn))
            {
                if (conversionNeeded)
                {
                    if (!conversionMode.Equals(NoteConversionMode.Off))
                    {
                        int tempNoteId = message.Data1;
                        if (message.Data1 > highNoteId)
                        {
                            tempNoteId = (tempNoteId % 12) + highNoteId - (highNoteId % 12);
                            if (conversionMode.Equals(NoteConversionMode.NonFixedOctaves))
                            {
                                if (tempNoteId > highNoteId)
                                {
                                    tempNoteId = tempNoteId - 12;
                                }
                            }
                        }
                        if (message.Data1 < LowNoteId)
                        {
                            tempNoteId = (tempNoteId % 12) + LowNoteId - (LowNoteId % 12);
                            if (conversionMode.Equals(NoteConversionMode.NonFixedOctaves))
                            {
                                if (tempNoteId < LowNoteId)
                                {
                                    tempNoteId = tempNoteId + 12;
                                }
                            }
                        }
                        if (message.Data1 != tempNoteId)
                        {

                            GlobalLogger.Log("MordhauControl", LoggerManager.LoggerLevel.Medium, "Converted the midi note : " + message.Data1 + " into mordhau note : " + tempNoteId + "using " + conversionMode.ToString());
                            GlobalLogger.Log("MordhauControl", LoggerManager.LoggerLevel.Medium, "Will play the note : " + FinalNoteConversion(tempNoteId));

                        }
                        newMessage = new ChannelMessage(message.Command, message.MidiChannel, tempNoteId, message.Data2);
                    }
                }
            }
            return newMessage;
        }

        public void Send(ChannelMessageEventArgs e)
        {
            Send(e.Message);
        }

        public void Send(ChannelMessage message)
        {
            if (message != null)
            {
                if (message.Command == ChannelCommand.NoteOn &&
                    message.Data1 >= LowMidiNoteId && message.Data1 <= highMidiNoteId)
                {
                    if (!stopWatch.IsRunning)
                    {
                        noteOnCallback(FilterNote(message));
                        stopWatch.Start();
                    }
                    else
                    {
                        if (stopWatch.ElapsedMilliseconds >= noteCooldown)
                        {
                            noteOnCallback(FilterNote(message));
                            stopWatch.Reset();
                        }
                        else
                        {
                            GlobalLogger.Warn("MordhauControl", LoggerManager.LoggerLevel.Medium, "Note overload : Couldn't play the midi note " + message.Data1 + " ms since last note : " + stopWatch.ElapsedMilliseconds);
                        }
                    }
                }
            }
        }
    }
}
