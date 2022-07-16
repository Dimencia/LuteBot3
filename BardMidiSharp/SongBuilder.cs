using BardMidiSharp.Structures.Reader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BardMidiSharp
{
    internal class SongBuilder
    {
        private List<MidiNote> PendingNotes { get; set; } = new List<MidiNote>();
        private Song Song { get; set; } = new Song();
        private SongData SongData { get; set; } = new SongData();
        private Channel CurrentMetaChannel { get; set; }


        private class ChunkTypes
        {
            internal const string Header = "MThd";
            internal const string Track = "MTrk";
        }

        private class MessageStatuses
        {
            internal const byte NoteOff = 0b10000000;
            internal const byte NoteOn = 0b10010000;
            internal const byte ControlChange = 0b10110000;
            internal const byte ProgramChange = 0b11000000;
            internal const byte Meta = 0b11110000;
        }

        public Song LoadSong(byte[] Data)
        {
            int index = 0;
            Song = new Song();
            Song.Channels = new Channel[16];
            SongData = new SongData();
            PendingNotes = new List<MidiNote>();
            CurrentMetaChannel = null;

            while (index < Data.Length)
            {
                ReadAndAdvanceChunk(Data, ref index);
            }


            return Song;
        }

        private void ReadAndAdvanceChunk(byte[] bytes, ref int index)
        {
            // Each chunk has a 4-character ASCII type, and a 32-bit length, the number of bytes remaining in the chunk after we've read them

            // It does not use a variable length on any of that



            // There are two types of chunks, header chunks and track chunks; the header has some info about the file itself.  The track chunks contain midi data

            // A midi always starts with a header, followed by one or more tracks


            // This function will just read in that preliminary data, and return the bytes to operate on that are contained within that chunk
            // While also moving the index forward

            // An ascii char is only 1 byte each, so the first 4 bytes can turn into those
            string chunkTypeString = null;
            byte[] chunkData = new byte[0];
            uint chunkLength = 0;
            try
            {
                //chunkTypeString = new string(bytes.Skip(index).Take(4).Cast<int>().Cast<char>().ToArray());
                chunkTypeString = Encoding.ASCII.GetString(bytes.Skip(index).Take(4).ToArray());

                // Regardless of the type, the next 32-bits are the length, number of bytes in the chunk (after we read)
                chunkLength = BitConverter.ToUInt32(bytes.Skip(index + 4).Take(4).Reverse().ToArray(), 0);
                // If we can't read a length, we're so off track that we really can't continue, we don't know if we're at a header or not
                chunkData = new byte[chunkLength];
                index += 8;
                Array.Copy(bytes, index, chunkData, 0, chunkLength);
                index += (int)chunkLength;
            }
            catch (Exception ex)
            {
                // If we got an exception here, we don't know the length and have to abort
                // But we can assume it's some EOF data, and still return what we made, if anything
                Console.WriteLine(ex);
                Console.WriteLine("Could not read length of chunk, aborting and returning existing data");
                index = bytes.Length;
                return;
            }
            // Even if we can't process it, we can read the next chunk at least
            try
            {
                switch (chunkTypeString)
                {
                    case ChunkTypes.Header:
                        // This section contains three 16-bit words, most-significant byte first
                        // 2 bytes each
                        // The first 2 bytes specify the file's structure: 
                        // 0: Single multi-channel track
                        // 1: Multiple tracks that play together
                        // 2: Multiple tracks that play separately

                        // I don't really know why that would matter, especially not for my purposes
                        // But might as well
                        Song.Format = BitConverter.ToInt16(chunkData.Take(2).Reverse().ToArray(), 0);

                        // The second 2 bytes is the number of track chunks
                        short numTracks = BitConverter.ToInt16(chunkData.Skip(2).Take(2).Reverse().ToArray(), 0);
                        Song.Tracks = new Track[numTracks];

                        // The third is the division, which can have one of two formats:
                        // If bit 15 is 0, the remaining are all the ticks per quarter-note
                        // If it's a 1, 14-8 are the 'negative SMPTE format', and the second byte is the ticks per frame
                        // SMPTE can be either: -24, -25, -29, -30.  Stored in two's complement
                        // Ticks per frame are typically 4,8, 10, 80, or 100
                        // These all deal with 'standard SMPTE and MIDI Time Code Formats'

                        // I am unsure if by bit 15 they mean, first bit because most significant is first
                        // I will assume that for now

                        var divisionBytes = chunkData.Skip(4).Take(2).ToArray();
                        var firstBitMask = 0b10000000;
                        var firstBit = divisionBytes[0] & firstBitMask;

                        if (firstBit == firstBitMask)
                        {
                            // It's a 1, I don't know how to handle these, abort
                            Console.Write(divisionBytes[0].ToString());
                            throw new NotImplementedException("SMPTE format not currently supported");
                        }
                        else
                        {
                            Song.Division = BitConverter.ToUInt16(divisionBytes.Reverse().ToArray(), 0);
                        }

                        // It is also common that tempo information should always be in the first track chunk, and it must be that way for format1

                        // If there is no tempo/timesig specified, time sig is assumed 4/4 and tempo 120bpm
                        break;
                    case ChunkTypes.Track:
                        // <Track Chunk> = <chunk type><length><MTrk event>+
                        // <MTrk event> = <delta-time><event>

                        // delta-time is a variable-length, and is the amount of time before the event, specified as per the division type
                        // Generally, some fraction of a beat

                        // <event> = <MIDI event> | <sysex event> | <meta-event>
                        // MIDI Messages are a whole other area, but:

                        // <status byte><data (1-2 bytes, depending on specifics)>

                        // Status bytes of MIDI messages may be omitted, if the preceding event is a channel message with the same status
                        // The first event in each track chunk must specify a status

                        // Otherwise, I need to make a big map of the important events we care about...
                        // But generically, all channel messages start with 4 bits of some status, and 4 bits with the channel number

                        // Really, all three of these event types need their own special processing


                        var track = new Track(SongData.CurrentTrack);
                        // chunkData already contains the Events, which each starts with the deltaTime as variable length
                        int chunkIndex = 0;
                        uint currentTick = 0;
                        int numNotes = 0;

                        while (chunkIndex < chunkData.Length)
                        {

                            int deltaTime = GetVariableLengthAndAdvance(chunkData, ref chunkIndex);
                            currentTick += (uint)deltaTime;

                            byte status;
                            if ((chunkData[chunkIndex] & 0b10000000) == 0b10000000)
                            {
                                // Status byte
                                status = ((byte)(chunkData[chunkIndex] & 0b11110000));
                                SongData.CurrentStatus = status;
                            }
                            else
                            {
                                // Status byte was skipped
                                // If there isn't a valid previous status, we can't read this chunk and throw an exception for it
                                status = SongData.CurrentStatus.Value;
                            }
                            Console.WriteLine("Processing status " + status + " (" + chunkData[chunkIndex] + ") at index " + chunkIndex);
                            switch (status)
                            {
                                case MessageStatuses.NoteOn:
                                case MessageStatuses.NoteOff:
                                    short channelNum = (short)(chunkData[chunkIndex] & 0b00001111);
                                    short noteNum = chunkData[chunkIndex + 1];
                                    short velocity = chunkData[chunkIndex + 2];

                                    var channel = Song.Channels[channelNum] ?? new Channel(channelNum);
                                    var note = new MidiNote(numNotes++)
                                    {
                                        Channel = channel,
                                        TickNumber = currentTick,
                                        Value = noteNum,
                                        Velocity = velocity,
                                        Track = track
                                    };
                                    // These handle adding the channels as appropriate
                                    if (status == MessageStatuses.NoteOn)
                                        NoteOn(note, currentTick);
                                    else
                                        NoteOff(note, currentTick);
                                    chunkIndex += 3;
                                    break;
                                case MessageStatuses.ProgramChange:
                                    // For now, just assume there will only be one of these per channel
                                    // Let it overwrite to the latest one if not
                                    short progChangeChannelNum = (short)(chunkData[chunkIndex] & 0b00001111);
                                    var progChangeChannel = Song.Channels[progChangeChannelNum] ?? new Channel(progChangeChannelNum);
                                    progChangeChannel.InstrumentID = chunkData[chunkIndex + 1];
                                    chunkIndex += 2;
                                    break;
                                default: // Right now the only other thing is meta
                                    if (chunkData[chunkIndex] == 0b11111111)
                                    {
                                        // Variable chunkIndex change
                                        // Every MetaMessage has a length as a variable length quantity, in the third byte
                                        int metaStatus = chunkData[++chunkIndex];
                                        chunkIndex++;
                                        int numPieces = GetVariableLengthAndAdvance(chunkData, ref chunkIndex);

                                        if (metaStatus == 0x51) // Set tempo
                                        {
                                            // It's a 3 byte tempo always, but we'll use numPieces for posterity
                                            var metaNote = new MetaNote(numNotes++)
                                            {
                                                TickNumber = currentTick,
                                                Value = BitConverter.ToInt32(new byte[1].Concat(chunkData.Skip(chunkIndex).Take(numPieces)).Reverse().ToArray(), 0),
                                                Track = track
                                            };

                                            AddNote(metaNote);
                                        }
                                        else if (metaStatus == 0x04) // Instrument name
                                        {

                                        }
                                        else if (metaStatus == 0x20) // Midi channel prefix - all following midi events that support it use the specified channel
                                        {// Note that this ends the first time we see any normal midi event with a channel, or another one comes in

                                        }
                                        else if (metaStatus == 0x58) // Time signature
                                        {

                                        }

                                        chunkIndex += numPieces;
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Could not process status code " + status);
                                        // These are variableLength too
                                        chunkIndex++; // Weird.  Sysex messages are supposed to have it in the second byte
                                        // But whatever I'm encountering, a status of 176 (which seems valid by manual tracing)
                                        // Certainly has the length as the third byte still

                                        // Unless it was an extremely long length?  Or a later one does
                                        // Or I'm just fucking something else up somewhere
                                        int numPieces = GetVariableLengthAndAdvance(chunkData, ref chunkIndex);
                                        chunkIndex += numPieces;
                                        break;
                                    }
                            }
                        }
                        Song.Tracks[SongData.CurrentTrack] = track;
                        SongData.CurrentTrack++;
                        break;
                    default:
                        Console.WriteLine("Unknown chunk type: " + chunkTypeString);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("Skipping Chunk " + chunkTypeString + " at index " + (index - 8 - chunkLength));
            }
        }

        private int GetVariableLengthAndAdvance(byte[] bytes, ref int index)
        {
            // In a variable-length, all bytes except the last have bit 7 set, and if it's the last, bit 7 is clear.  
            // So something like 0000000080 might be represented as 8100; the larger number is how it would be in an int with much more room than a byte

            // These are represented with the most significant bits first, which is to say, normal
            // Bit 7 is the most significant, so that should be the left-most bit
            // So I don't understand how 80 turns into 81 00, but am I just getting confused cuz hex is dumb?
            // 80 is 128, which can't be represented in the remaining 7 bits
            // So it turns them all on for the first one, to tell it that there's more coming and it's at least 127
            // And then has to give us a 1 in the next byte...

            // Let's look at a better example.  00003FFF => FF 7F
            // That's 16383.  In our first number, the 7th bit is on, so we know it's not over yet, we have 127 in binary
            // In the second number it's off; the remaining values happen to all be 1's but it doesn't matter, we append them
            // At the end, such that the rightmost of our now 14 bits is the 1's place
            if (index >= bytes.Length)
                return 0;

            byte finishMask = 0b10000000;
            byte valueMask = 0b01111111;

            List<byte> finalValues = new List<byte>();
            do
            {
                finalValues.Add((byte)(bytes[index] & valueMask));
            }
            while (index < bytes.Length-1 && (bytes[index++] & finishMask) == finishMask);

            // Each given byte can be up to 127, can I make that work in decimal instead of per-byte here?
            // It's 2^7*(finalValues.Length-i)*finalValues[i]

            // No no, it's finalValues[i]*2^(7*finalValues.Count-1-i), ?  If there's only one value, it should be *2^0
            // If there's two, it should be *2^7 on the first one and *2^0 on the second
            //int result = 0;
            //for (int i = 0; i < finalValues.Count; i++)
            //    result += finalValues[i] * (int)Math.Pow(2, 7 * finalValues.Count - 1 - i);

            // Using this requires us to have an array of exactly size 4... 
            while (finalValues.Count < 4)
                finalValues.Add(0);
            //finalValues.Reverse();
            int result = (int)BitConverter.ToInt32(finalValues.ToArray(), 0);

            return result;
        }


        internal void NoteOn(MidiNote note, uint currentTick)
        {
            // If there was another note on this channel and value that does not have a duration, set it
            // So we should track non-finished notes, and not add them until they're done

            // Actually, NoteOff does this.
            NoteOff(note, currentTick);
            if(note.Velocity > 0) // If it was a velocity 0, it was really just an off anyway
                PendingNotes.Add(note);
        }

        internal void NoteOff(MidiNote note, uint currentTick)
        {
            var toEnd = PendingNotes.Where(n => n.Channel == note.Channel && n.Track == note.Track && n.Value == note.Value).ToArray();

            foreach (var n in toEnd)
            {
                n.Duration = (int)(currentTick - n.TickNumber);
                AddNote(n);
                PendingNotes.Remove(n);
            }
        }

        private void AddNote(MetaNote note)
        {
            var track = note.Track;

            if (Song.Tracks[track.ID] == null)
                Song.Tracks[track.ID] = track;

            Tick tick;
            if (!track.Ticks.TryGetValue(note.TickNumber, out tick))
            {
                tick = new Tick(note.TickNumber);
                track.Ticks[note.TickNumber] = tick;
            }

            tick.MetaNotes.Add(note);
        }

        private void AddNote(MidiNote note)
        {
            var channel = note.Channel;
            var track = note.Track;


            if (Song.Channels[channel.ID] == null)
                Song.Channels[channel.ID] = channel;
            if (Song.Tracks[track.ID] == null)
                Song.Tracks[track.ID] = track;

            Tick tick;
            if (!track.Ticks.TryGetValue(note.TickNumber, out tick))
            {
                tick = new Tick(note.TickNumber);
                track.Ticks[note.TickNumber] = tick;
            }

            tick.Notes.Add(note);
            // Because of the way Track and Channel are setup, this is all they need

            // The song really ought to also contain the Ticks though, shouldn't it...? All of them?
            // Nah, and if so, it would be by accessing the ticks of each individual thing

            // But, Channel has a separate list of ticks - containing only the ticks that channel runs on
            // The track's tick contains all channels that run in that tick

            // Which means they each need a unique Tick
            if(!channel.Ticks.TryGetValue(note.TickNumber, out tick))
            {
                tick = new Tick(note.TickNumber);
                channel.Ticks[note.TickNumber] = tick;
            }
            tick.Notes.Add(note);
        }
    }
}
