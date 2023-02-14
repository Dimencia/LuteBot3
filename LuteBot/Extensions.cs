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

        public static int numParamsPerChannel = 8;

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


        public static float[] GetNeuralInput(this MidiChannelItem[] song)
        {
            float maxAvgNoteLength = song.Max(c => c.Id == 9 ? 0 : c.avgNoteLength);
            float maxNoteLength = song.Max(c => c.Id == 9 ? 0 : c.totalNoteLength);
            // noteLength is now in a time format, a float in seconds.  We should divide it by total song duration

            // ... but we don't have that... 
            // Welp, nvm then.  It's very hard and annoying to get that here

            float maxNumNotes = song.Max(c => c.Id == 9 ? 0 : c.numNotes);

            float[] inputs = new float[numParamsPerChannel * 16];

            for (int j = 0; j < 16; j++)
            {
                var channel = song.Where(c => c.Id == j && c.Id != 9).SingleOrDefault();
                //var channel = song.Values[j];


                if (channel != null)
                {
                    //inputs[j * 6] = (maxAvgNoteLength > 0 ? channel.avgNoteLength / maxAvgNoteLength : 0);
                    inputs[j * numParamsPerChannel] = channel.avgNoteLength;
                    inputs[j * numParamsPerChannel + 1] = channel.maxChordSize;
                    inputs[j * numParamsPerChannel + 2] = (maxNoteLength > 0 ? channel.totalNoteLength / maxNoteLength : 0);
                    //inputs[j * 6 + 2] = channel.totalNoteLength;
                    inputs[j * numParamsPerChannel + 3] = channel.highestNote;// / 128f;
                    inputs[j * numParamsPerChannel + 4] = channel.lowestNote;// / 128f;
                    inputs[j * numParamsPerChannel + 5] = (maxNumNotes > 0 ? channel.numNotes / maxNumNotes : 0);
                    //inputs[j * numParamsPerChannel + 6] = channel.Id / 16f;
                    inputs[j * numParamsPerChannel + 6] = channel.midiInstrument;// / 128f;
                    inputs[j * numParamsPerChannel + 7] = channel.avgVariation;
                    //inputs[j * 6 + 5] = channel.numNotes;
                }
                else
                {
                    for (int k = 0; k < numParamsPerChannel; k++)
                        inputs[j * numParamsPerChannel + k] = 0;
                }
            }

            return inputs;
        }


        public static float[] GetNeuralInputs(this MidiChannelItem c, float maxAvgNoteLength, float maxNumNotes, float maxTotalNoteLength)
        {
            // We'll try not normalizing 

            float[] inputs = new float[numParamsPerChannel];


            var channel = c;
            //var channel = song.Values[j];

            //inputs[j * 6] = (maxAvgNoteLength > 0 ? channel.avgNoteLength / maxAvgNoteLength : 0);
            int i = 0;
            inputs[i++] = (maxAvgNoteLength > 0 ? channel.avgNoteLength / maxAvgNoteLength : 0);
            inputs[i++] = channel.maxChordSize;
            inputs[i++] = (maxTotalNoteLength > 0 ? channel.totalNoteLength / maxTotalNoteLength : 0);
            //inputi++lNoteLength;
            inputs[i++] = channel.highestNote / 128f;
            inputs[i++] = channel.lowestNote / 128f;
            inputs[i++] = (maxNumNotes > 0 ? channel.numNotes / maxNumNotes : 0);
            //inputi++ 6] = channel.Id / 16f;
            inputs[i++] = channel.midiInstrument / 128f;
            inputs[i++] = channel.avgVariation;
            //inputs[j * 6 + 5] = channel.numNotes;


            return inputs;
        }
    }
}
