using System;
using System.Collections.Generic;
using System.Text;

namespace BardMidiSharp
{
    internal static class ByteExtensions
    {
        public static byte Reverse(this byte b)
        {
            int a = 0;
            for (int i = 0; i < 8; i++)
                if ((b & (1 << i)) != 0)
                    a |= 1 << (7 - i);
            return (byte)a;
        }
    }
}
