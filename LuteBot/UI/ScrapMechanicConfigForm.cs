using LuteBot.Core.Midi;
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

namespace LuteBot.UI
{
    public partial class ScrapMechanicConfigForm : Form
    {

        public Dictionary<int, int> TrackCategoryDict = new Dictionary<int, int>();
        public Dictionary<int, int> TrackTypeDict = new Dictionary<int, int>();
        // It'll be dicts of the track num:Synth/Bass etc, and Type is Dance or Retro (0 or 1)

        private Dictionary<int,ComboBox> categoryBoxes = new Dictionary<int,ComboBox>();
        private Dictionary<int,ComboBox> typeBoxes = new Dictionary<int,ComboBox>();
        public ScrapMechanicConfigForm(MidiPlayer player)
        {
            InitializeComponent();
            tableLayoutPanel.SuspendLayout();

            var channels = player.trackSelectionManager.MidiChannels.Values.Where(c => c.Active);
            if (channels.Count() > 0)
                tableLayoutPanel.RowCount = channels.Count();

            foreach(var channel in channels)
            {
                Label l = new Label();
                l.Text = channel.Name;
                tableLayoutPanel.Controls.Add(l);
                ComboBox categoryBox = new ComboBox();
                categoryBox.Items.Add("Default");
                categoryBox.Items.Add("Bass");
                categoryBox.Items.Add("Synth");
                categoryBox.Items.Add("Percussion");
                categoryBox.SelectedIndex = 0;
                categoryBoxes.Add(channel.Id,categoryBox);
                tableLayoutPanel.Controls.Add(categoryBox);
                ComboBox typeBox = new ComboBox();
                typeBox.Items.Add("Dance");
                typeBox.Items.Add("Retro");
                typeBox.SelectedIndex = 0;
                typeBoxes.Add(channel.Id,typeBox);
                tableLayoutPanel.Controls.Add(typeBox);
            }
            tableLayoutPanel.ResumeLayout();
        }

        private void ButtonExport_Click(object sender, EventArgs e)
        {
            foreach(var catBox in categoryBoxes)
            {
                TrackCategoryDict.Add(catBox.Key, catBox.Value.SelectedIndex);
            }
            foreach (var typeBox in typeBoxes)
            {
                TrackTypeDict.Add(typeBox.Key, typeBox.Value.SelectedIndex);
            }
        }
    }
}
