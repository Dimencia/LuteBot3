using LuteBot.TrackSelection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.UI.PianoRoll
{
    public partial class TestPianoRollForm : Form
    {
        public TestPianoRollForm(TrackSelectionManager tsm)
        {
            InitializeComponent();
            var pianoRoll = new PianoRollComponent(tsm);
            pianoRoll.Height = this.Height;
            pianoRoll.Width = this.Width;
            pianoRoll.AddComponents();
            pianoRoll.Visible = true;
            this.Controls.Add(pianoRoll);
            Refresh();
        }
    }
}
