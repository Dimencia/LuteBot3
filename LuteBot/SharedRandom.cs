using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot
{
    public static class SharedRandom
    {
        public static Random Random { get; } = new Random();

        public static int Next(int a, int b)
        {
            return Random.Next(a, b);
        }
    }
}
