using LuteBot.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot
{
    public partial class TrackNamingForm : Form
    {
        public TrackNamingForm(string filename, string labelValue = null)
        {
            StartPosition = FormStartPosition.CenterParent;
            InitializeComponent();
            textBoxPartName.Text = filename;
            checkBoxOverwrite.Checked = ConfigManager.GetBooleanProperty(PropertyItem.OverwritePartitions);
            if (labelValue != null)
            {
                this.Text = labelValue;
                label1.Text = labelValue;
            }
            this.BringToFront();
            this.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void checkBoxOverwrite_CheckedChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.OverwritePartitions, checkBoxOverwrite.Checked ? "True" : "False");
        }
    }
}
