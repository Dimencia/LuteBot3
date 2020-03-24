using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.Soundboard
{
    [DataContract]
    public class SoundBoardItem
    {
        private Keys hotkey;
        private string path;
        private string name;
        [DataMember(Name = "Hotkey")]
        public int _Hotkey { get => (int)Hotkey; set => Hotkey = (Keys)value; }

        public Keys Hotkey { get => hotkey; set => hotkey = value; }
        [DataMember]
        public string Path { get => path; set => path = value; }
        [DataMember]
        public string Name { get => name; set => name = value; }
    }
}
