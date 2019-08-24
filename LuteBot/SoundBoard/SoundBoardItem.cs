using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.Soundboard
{
    public class SoundBoardItem
    {
        private Keys hotkey;
        private string path;
        private string name;

        public Keys Hotkey { get => hotkey; set => hotkey = value; }
        public string Path { get => path; set => path = value; }
        public string Name { get => name; set => name = value; }
    }
}
