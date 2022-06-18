using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BardMidiSharp
{
    public class MidiReader
    {
        public Song LoadedSong { get; private set; }



        private class ChunkTypes
        {
            private static string Header = "MThd";
            private static string Track = "MTrk";
        }


        public async Task<Song> Load(string filepath)
        {
            // TODO: Consider doing the processing while we read to give it space to breathe...?
            var bytes = await File.ReadAllBytesAsync(filepath);

        }

        private byte[] ReadAndAdvanceChunk(byte[] bytes, ref int index)
        {
            // Each chunk has a 4-character ASCII type, and a 32-bit length, the number of bytes remaining in the chunk after we've read them

            // It does not use a variable length on any of that



            // There are two types of chunks, header chunks and track chunks; the header has some info about the file itself.  The track chunks contain midi data

            // A midi always starts with a header, followed by one or more tracks


            // This function will just read in that preliminary data, and return the bytes to operate on that are contained within that chunk
            // While also moving the index forward

            // An ascii char is only 1 byte each, so the first 4 bytes can turn into those

            string chunkType = new string(bytes.Take(4));
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


            byte finishMask = 0b10000000;
            byte valueMask = 0b01111111;

            List<byte> finalValues = new List<byte>();
            do
            {
                finalValues.Add((byte)(bytes[index] & valueMask));
            }
            while ((bytes[index++] & finishMask) == finishMask);

            // Each given byte can be up to 127, can I make that work in decimal instead of per-byte here?
            // It's 2^7*(finalValues.Length-i)*finalValues[i]

            // No no, it's finalValues[i]*2^(7*finalValues.Count-1-i), ?  If there's only one value, it should be *2^0
            // If there's two, it should be *2^7 on the first one and *2^0 on the second
            int result = 0;
            for (int i = 0; i < finalValues.Count; i++)
                result += finalValues[i] * (int)Math.Pow(2, 7 * finalValues.Count - 1 - i);

            return result;
        }
    }
}
