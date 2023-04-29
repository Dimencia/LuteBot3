using LuteBot.TrackSelection;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public static int numParamsPerChannel = 9;

        public static float[][] GetRecurrentInput(this MidiChannelItem channel, int noteParams, float maxTickNumber)
        {
            // Got one.  Next, build the neural inputs our way - it accepts a float[][], where each float[] is a note, and each second param is one of the params of it
            var allNotes = channel.TickNotes.SelectMany(kvp => kvp.Value).OrderBy(n => n.tickNumber);

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



        public static float[] GetNeuralInputs(this MidiChannelItem c, int channelCount, int numChannels)
        {
            // We'll try not normalizing 

            float[] inputs = new float[numParamsPerChannel];


            var channel = c;
            //var channel = song.Values[j];

            //inputs[j * 6] = (maxAvgNoteLength > 0 ? channel.avgNoteLength / maxAvgNoteLength : 0);
            int i = 0;
            inputs[i++] = channel.AvgNoteLength;
            inputs[i++] = channel.MaxChordSize;
            inputs[i++] = channel.PercentTimePlaying;
            //inputi++lNoteLength;
            inputs[i++] = channel.HighestNote / 128f;
            inputs[i++] = channel.LowestNote / 128f;

            inputs[i++] = channel.PercentSongNotes;

            //inputs[i++] = channel.midiInstrument / 128f;
            inputs[i++] = channel.AvgVariation / 64f; // Doubt this ever gets above 64, which this handles
            //inputs[j * 6 + 5] = channel.numNotes;
            inputs[i++] = channel.AverageNote / 128f;
            inputs[i++] = channelCount / (float)numChannels;

            return inputs;
        }

        public static Task InvokeAsync(this Form form, Action action, bool catchExceptions = true)
        {
            if (form.InvokeRequired)
            {
                var ar = form.BeginInvoke(new MethodInvoker(async () =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        if (!catchExceptions)
                            throw;
                        await LuteBotForm.Instance.HandleErrorAsync(ex, ex.Message).ConfigureAwait(false);
                    }
                }));
                return Task.Factory.FromAsync(ar, form.EndInvoke);
            }
            else
            {
                action();
                return Task.CompletedTask;
            }
        }
    }
}
