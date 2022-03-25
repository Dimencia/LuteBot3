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
            foreach(var arr in candidates)
                if (IsEmptyLocate(self, arr))
                return -1;


            for (int i = 0; i < self.Length; i++)
            {
                foreach(var arr in candidates)
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
    }
}
