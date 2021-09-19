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
            InitializeComponent();
            textBoxPartName.Text = filename;
            if (labelValue != null)
            {
                this.Text = labelValue;
                label1.Text = labelValue;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
