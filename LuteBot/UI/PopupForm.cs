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
    public partial class PopupForm : Form
    {
        public PopupForm(string windowTitle, string title, string content, Dictionary<string,string> links = null, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            InitializeComponent();
            this.Text = windowTitle;
            titleLabel.Text = title;
            contentLabel.Text = content;
            if (links != null)
            {
                foreach(var kvp in links)
                {
                    var linklabel = new LinkLabel();
                    linklabel.Text = kvp.Key;
                    linklabel.Links.Add(0, kvp.Key.Length);
                    linklabel.Tag = kvp.Value;
                    linklabel.Click += Linklabel_Click;
                    linklabel.AutoSize = true;
                    linkPanel.Controls.Add(linklabel);
                    linkPanel.Height = linkPanel.Height + linklabel.Height;
                }
            }
            switch(buttons)
            {
                case MessageBoxButtons.OK:
                    var button = new Button();
                    button.Text = "OK";
                    button.Tag = DialogResult.OK;
                    button.Click += button_Click;
                    addButton(button);
                    break;
                case MessageBoxButtons.YesNo:
                    var ybutton = new Button();
                    ybutton.Text = "Yes";
                    ybutton.Tag = DialogResult.Yes;
                    ybutton.Click += button_Click;
                    addButton(ybutton);
                    var nbutton = new Button();
                    nbutton.Text = "No";
                    nbutton.Tag = DialogResult.No;
                    nbutton.Click += button_Click;
                    addButton(nbutton);
                    break;
            }

            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;
        }

        private int buttonCount = 0; // Janky but oh well

        private void addButton(Button b)
        {
            b.Anchor = AnchorStyles.None;
            buttonPanel.Controls.Add(b);
            buttonPanel.SetRow(b, 0);
            if (buttonCount > 0)
                buttonPanel.ColumnCount++;
            buttonPanel.SetColumn(b, buttonCount);
            buttonCount++;
        }

        private void Linklabel_Click(object sender, EventArgs e)
        {
            var linklabel = sender as LinkLabel;
            linklabel.LinkVisited = true;
            System.Diagnostics.Process.Start(linklabel.Tag as string);
        }

        private void button_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            this.DialogResult = (DialogResult)button.Tag;
            this.Close();
        }
    }
}
