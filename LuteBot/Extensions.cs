using LuteBot.TrackSelection;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot
{
    static class Extensions
    {
        // Returns true if any part of targetRect is contained in the rectangle in the x axis
        public static bool LooseContainsX(this Rectangle rect, Rectangle targetRect)
        {
            return targetRect.X + targetRect.Width >= rect.X && targetRect.X <= rect.X + rect.Width;
        }

        public static bool LooseContains(this Rectangle rect, Rectangle targetRect)
        {
            return targetRect.X + targetRect.Width >= rect.X && targetRect.X <= rect.X + rect.Width && targetRect.Y + targetRect.Height >= rect.Y && targetRect.Y <= rect.Y + rect.Height;
        }


        static readonly int[] Empty = new int[0];

        public static int Locate(this byte[] self, params byte[][] candidates)
        {
            foreach (var arr in candidates)
                if (IsEmptyLocate(self, arr))
                    return -1;


            for (int i = 0; i < self.Length; i++)
            {
                foreach (var arr in candidates)
                    if (IsMatch(self, i, arr))
                        return i;
            }

            return -1;
        }

        static bool IsMatch(byte[] array, int position, byte[] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array[position + i] != candidate[i])
                    return false;

            return true;
        }

        static bool IsEmptyLocate(byte[] array, byte[] candidate)
        {
            return array == null
                || candidate == null
                || array.Length == 0
                || candidate.Length == 0
                || candidate.Length > array.Length;
        }

        

        public static float[][] GetRecurrentInput(this MidiChannelItem channel, int noteParams, float maxTickNumber)
        {
            // Got one.  Next, build the neural inputs our way - it accepts a float[][], where each float[] is a note, and each second param is one of the params of it
            var allNotes = channel.tickNotes.SelectMany(kvp => kvp.Value).OrderBy(n => n.tickNumber);

            float[][] inputs = new float[allNotes.Count()][];// This really doesn't need to be jagged tho

            for (int noteCount = 0; noteCount < allNotes.Count(); noteCount++)
            {
                var note = allNotes.ElementAt(noteCount);
                inputs[noteCount] = new float[noteParams];
                inputs[noteCount][0] = note.note;
                inputs[noteCount][1] = note.tickNumber / maxTickNumber; // If this works at all, I should instead get the note's timestamp including tempo, as a percent of the whole
                inputs[noteCount][2] = note.timeLength;
            }
            return inputs;
        }

        public static int numParamsPerChannel = 8;//9;
        public static float[] GetNeuralInputsNew(this MidiChannelItem c)
        {
            float[] inputs = new float[numParamsPerChannel];

            // I need to rethink what we give it and how it's formatted
            // I think 0-1, the 0 being important rather than negatives, so the values can 'scale' appropriately
            
            // And, err on the side of dividing by more instead of less; it's ok to never reach 1, but probably bad to go past it

            // avgNoteLength should be in seconds and maybe /8f and clamp, anything longer than that we don't care
            // maxChordSize maybe /8f or so, and again clamp

            // totalNoteLength should be in seconds... and may not even correlate well when chords double it
            // If we use it, make it a percentage of the song: what percentage of the song is this channel playing at least one note?

            // highest and lowest note are good to /128
            // numNotes would be best as a percentage, of all notes in this song, this channel has x%

            // avgChordSize can be /8
            // avgVariation could be up to 128, so we'll just /128.  That is the average amount of note value change between each consecutive note on this channel

            // And averageNote was a good idea, let's use that

            var channel = c;
            //var channel = song.Values[j];

            //inputs[j * 6] = (maxAvgNoteLength > 0 ? channel.avgNoteLength / maxAvgNoteLength : 0);
            int i = 0;
            inputs[i++] = Math.Min(channel.avgNoteLength / 8f, 1f);
            inputs[i++] = Math.Min(channel.maxChordSize / 8f, 1f);

            inputs[i++] = channel.percentNoteLength;
            //inputi++lNoteLength;
            inputs[i++] = channel.highestNote/128f;
            inputs[i++] = channel.lowestNote/128f;

            inputs[i++] = channel.numNotesPercent;

            //inputi++ 6] = channel.Id / 16f;
            //inputs[i++] = channel.midiInstrument / 64f - 1f;
            inputs[i++] = Math.Min(channel.avgChordSize / 8f, 1f);
            inputs[i++] = channel.avgVariation / 128f;
            //inputs[j * 6 + 5] = channel.numNotes;
            inputs[i++] = channel.averageNote;

            return inputs;
        }

        public static float[] GetNeuralInputs(this MidiChannelItem c)
        {
            // We'll try not normalizing 

            float[] inputs = new float[numParamsPerChannel];


            var channel = c;
            //var channel = song.Values[j];

            //inputs[j * 6] = (maxAvgNoteLength > 0 ? channel.avgNoteLength / maxAvgNoteLength : 0);
            int i = 0;
            inputs[i++] = channel.avgNoteLength; // And this is new but should be mostly the same
            inputs[i++] = channel.maxChordSize;
            inputs[i++] = channel.percentNoteLength; // This is new but should be the same thing, unless it was wrong before...
            //inputi++lNoteLength;
            inputs[i++] = channel.highestNote / 128f;
            inputs[i++] = channel.lowestNote / 128f;
            inputs[i++] = channel.numNotesPercent; // Also new but should be the same thing... 
            //inputi++ 6] = channel.Id / 16f;
            inputs[i++] = channel.midiInstrument / 128f;
            inputs[i++] = channel.avgVariation;
            //inputs[j * 6 + 5] = channel.numNotes;


            return inputs;
        }
    }
}
