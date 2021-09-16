using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteMod.Sequencing
{
    public class Note
    {
        private float tick;
        private int tone;
        private NoteType type;

        public float Tick { get => tick; set => tick = value; }
        public int Tone { get => tone; set => tone = value; }

        public NoteType Type { get => type; set => type = value; }

        public override string ToString()
        {
            string stringType = "";
            switch (type)
            {
                case NoteType.On:
                    stringType = "1";
                    break;
                case NoteType.Off:
                    stringType = "0";
                    break;
                case NoteType.Tempo:
                    stringType = "2";
                    break;
                default:
                    break;
            }
            return tick.ToString() + "-" + tone.ToString() + "-" + stringType;
        }
    }
}
