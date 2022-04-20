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
    public partial class MordhauPathInputForm : Form
    {
        public MordhauPathInputForm()
        {
            InitializeComponent();
        }

        public string path { get; set; }
        public DialogResult result { get; set; }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog(this);
            if (result == DialogResult.OK)
                textBoxPath.Text = openFileDialog1.FileName;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.result = DialogResult.OK;
            this.path = textBoxPath.Text;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.result = DialogResult.Cancel;
            this.path = null;
        }
    }
}
