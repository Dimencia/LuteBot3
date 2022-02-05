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
        public Control extraControl;
        public PopupForm(string windowTitle, string title, string content, Dictionary<string,string> links = null, MessageBoxButtons buttons = MessageBoxButtons.OK, string cancelLabel = "Cancel")
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
                    addButton(button, "OK", DialogResult.OK);
                    break;
                case MessageBoxButtons.YesNo:
                    var ybutton = new Button();
                    addButton(ybutton, "Yes", DialogResult.Yes);
                    var nbutton = new Button();
                    addButton(nbutton, "No", DialogResult.No);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    // This is a bit of a jank, because 'Cancel' isn't necessarily going to be labelled Cancel... 
                    // But it gives us an extra option to allow users to click "Don't show this again" for update prompts
                    var ybutton2 = new Button();
                    addButton(ybutton2, "Yes", DialogResult.Yes);
                    var nbutton2 = new Button();
                    addButton(nbutton2, "No", DialogResult.No);
                    var cbutton = new Button();
                    addButton(cbutton, cancelLabel, DialogResult.Cancel);
                    break;
            }

            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowOnly;
            this.Shown += PopupForm_Shown;
        }

        private void PopupForm_Shown(object sender, EventArgs e)
        {
            using (var g = Graphics.FromHwnd(this.Handle))
            {
                var titleWidth = g.MeasureString(titleLabel.Text, titleLabel.Font).Width * g.DpiX/96;
                this.Width = Math.Max(this.Width, (int)titleWidth + 20);
                this.Height = (int)((titleLabel.PreferredHeight + contentLabel.PreferredHeight + linkPanel.PreferredSize.Height + buttonPanel.PreferredSize.Height + 200) * g.DpiX/96);
            }
            
        }

        private int buttonCount = 0; // Janky but oh well

        private void addButton(Button b, string text, DialogResult tag)
        {
            b.Text = text;
            b.Tag = tag;
            b.Click += button_Click;
            b.AutoSize = true;
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
