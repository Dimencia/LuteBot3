using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.UI.PianoRoll
{
    public class NoteDisplaySettings
    {
        public PianoRollDisplaySettings PianoRollSettings { get; set; }
        public Color NoteColor { get; set; }
        public bool Visible { get; set; } = true;
    }
}
