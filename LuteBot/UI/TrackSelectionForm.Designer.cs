using System;
using System.Windows.Forms;

namespace LuteBot.UI
{
    partial class TrackSelectionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ChannelsListBox = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SongProfileSaveButton = new System.Windows.Forms.Button();
            this.AutoActivateCheckBox = new System.Windows.Forms.CheckBox();
            this.LoadProfileButton = new System.Windows.Forms.Button();
            this.TrackListLabel = new System.Windows.Forms.Label();
            this.TrackListBox = new System.Windows.Forms.CheckedListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textBoxNotesForChords = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.instrumentsBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectNoneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectInverseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new LuteBot.UI.CustomBufferedPanel();
            this.OffsetPanel = new System.Windows.Forms.Panel();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ChannelsListBox
            // 
            this.ChannelsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ChannelsListBox.FormattingEnabled = true;
            this.ChannelsListBox.Location = new System.Drawing.Point(16, 47);
            this.ChannelsListBox.Margin = new System.Windows.Forms.Padding(4);
            this.ChannelsListBox.Name = "ChannelsListBox";
            this.ChannelsListBox.Size = new System.Drawing.Size(359, 208);
            this.ChannelsListBox.TabIndex = 3;
            this.ChannelsListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ChannelListBox_ItemChecked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(153, 20);
            this.label1.TabIndex = 9;
            this.label1.Text = "MIDI Instruments";
            // 
            // SongProfileSaveButton
            // 
            this.SongProfileSaveButton.Location = new System.Drawing.Point(16, 296);
            this.SongProfileSaveButton.Margin = new System.Windows.Forms.Padding(4);
            this.SongProfileSaveButton.Name = "SongProfileSaveButton";
            this.SongProfileSaveButton.Size = new System.Drawing.Size(214, 37);
            this.SongProfileSaveButton.TabIndex = 10;
            this.SongProfileSaveButton.Text = "Save";
            this.SongProfileSaveButton.UseVisualStyleBackColor = true;
            this.SongProfileSaveButton.Click += new System.EventHandler(this.SongProfileSaveButton_Click);
            // 
            // AutoActivateCheckBox
            // 
            this.AutoActivateCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.AutoActivateCheckBox.AutoSize = true;
            this.AutoActivateCheckBox.Checked = true;
            this.AutoActivateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoActivateCheckBox.Location = new System.Drawing.Point(267, 897);
            this.AutoActivateCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.AutoActivateCheckBox.Name = "AutoActivateCheckBox";
            this.AutoActivateCheckBox.Size = new System.Drawing.Size(203, 38);
            this.AutoActivateCheckBox.TabIndex = 11;
            this.AutoActivateCheckBox.Text = "Automatically load a profile \r\non song selection";
            this.AutoActivateCheckBox.UseVisualStyleBackColor = true;
            this.AutoActivateCheckBox.CheckedChanged += new System.EventHandler(this.AutoActivateCheckBox_CheckedChanged);
            // 
            // LoadProfileButton
            // 
            this.LoadProfileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadProfileButton.Location = new System.Drawing.Point(523, 296);
            this.LoadProfileButton.Margin = new System.Windows.Forms.Padding(4);
            this.LoadProfileButton.Name = "LoadProfileButton";
            this.LoadProfileButton.Size = new System.Drawing.Size(201, 37);
            this.LoadProfileButton.TabIndex = 12;
            this.LoadProfileButton.Text = "Reload";
            this.LoadProfileButton.UseVisualStyleBackColor = true;
            this.LoadProfileButton.Click += new System.EventHandler(this.LoadProfileButton_Click);
            // 
            // TrackListLabel
            // 
            this.TrackListLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TrackListLabel.AutoSize = true;
            this.TrackListLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TrackListLabel.Location = new System.Drawing.Point(598, 11);
            this.TrackListLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.TrackListLabel.Name = "TrackListLabel";
            this.TrackListLabel.Size = new System.Drawing.Size(111, 20);
            this.TrackListLabel.TabIndex = 15;
            this.TrackListLabel.Text = "MIDI Tracks";
            // 
            // TrackListBox
            // 
            this.TrackListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TrackListBox.FormattingEnabled = true;
            this.TrackListBox.Location = new System.Drawing.Point(383, 47);
            this.TrackListBox.Margin = new System.Windows.Forms.Padding(4);
            this.TrackListBox.Name = "TrackListBox";
            this.TrackListBox.Size = new System.Drawing.Size(341, 208);
            this.TrackListBox.TabIndex = 13;
            this.TrackListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.TrackListBox_ItemCheck);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(579, 901);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(145, 28);
            this.button1.TabIndex = 19;
            this.button1.Text = "Reset Alignment";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBoxNotesForChords
            // 
            this.textBoxNotesForChords.Location = new System.Drawing.Point(136, 265);
            this.textBoxNotesForChords.Name = "textBoxNotesForChords";
            this.textBoxNotesForChords.Size = new System.Drawing.Size(33, 22);
            this.textBoxNotesForChords.TabIndex = 20;
            this.textBoxNotesForChords.TextChanged += new System.EventHandler(this.textBoxNotesForChords_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 268);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 17);
            this.label2.TabIndex = 21;
            this.label2.Text = "Notes Per Chord";
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.button2.Location = new System.Drawing.Point(263, 296);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(220, 37);
            this.button2.TabIndex = 22;
            this.button2.Text = "Save As...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "mid";
            this.saveFileDialog1.Filter = "Midi files|*.mid";
            // 
            // instrumentsBox
            // 
            this.instrumentsBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.instrumentsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.instrumentsBox.FormattingEnabled = true;
            this.instrumentsBox.Location = new System.Drawing.Point(490, 265);
            this.instrumentsBox.Name = "instrumentsBox";
            this.instrumentsBox.Size = new System.Drawing.Size(230, 24);
            this.instrumentsBox.TabIndex = 23;
            this.instrumentsBox.SelectedIndexChanged += new System.EventHandler(this.instrumentsBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(404, 270);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 17);
            this.label3.TabIndex = 24;
            this.label3.Text = "Instrument";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllToolStripMenuItem,
            this.selectNoneToolStripMenuItem,
            this.selectInverseToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(169, 76);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(168, 24);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // selectNoneToolStripMenuItem
            // 
            this.selectNoneToolStripMenuItem.Name = "selectNoneToolStripMenuItem";
            this.selectNoneToolStripMenuItem.Size = new System.Drawing.Size(168, 24);
            this.selectNoneToolStripMenuItem.Text = "Select None";
            this.selectNoneToolStripMenuItem.Click += new System.EventHandler(this.selectNoneToolStripMenuItem_Click);
            // 
            // selectInverseToolStripMenuItem
            // 
            this.selectInverseToolStripMenuItem.Name = "selectInverseToolStripMenuItem";
            this.selectInverseToolStripMenuItem.Size = new System.Drawing.Size(168, 24);
            this.selectInverseToolStripMenuItem.Text = "Select Inverse";
            this.selectInverseToolStripMenuItem.Click += new System.EventHandler(this.selectInverseToolStripMenuItem_Click);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2,
            this.toolStripMenuItem3});
            this.contextMenuStrip2.Name = "contextMenuStrip1";
            this.contextMenuStrip2.Size = new System.Drawing.Size(169, 76);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(168, 24);
            this.toolStripMenuItem1.Text = "Select All";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(168, 24);
            this.toolStripMenuItem2.Text = "Select None";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(168, 24);
            this.toolStripMenuItem3.Text = "Select Inverse";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.OffsetPanel);
            this.panel1.Location = new System.Drawing.Point(16, 341);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(708, 548);
            this.panel1.TabIndex = 18;
            // 
            // OffsetPanel
            // 
            this.OffsetPanel.AutoScroll = true;
            this.OffsetPanel.Location = new System.Drawing.Point(0, 0);
            this.OffsetPanel.Margin = new System.Windows.Forms.Padding(4);
            this.OffsetPanel.Name = "OffsetPanel";
            this.OffsetPanel.Size = new System.Drawing.Size(684, 382);
            this.OffsetPanel.TabIndex = 16;
            // 
            // TrackSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(737, 939);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.instrumentsBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxNotesForChords);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.TrackListLabel);
            this.Controls.Add(this.TrackListBox);
            this.Controls.Add(this.LoadProfileButton);
            this.Controls.Add(this.AutoActivateCheckBox);
            this.Controls.Add(this.SongProfileSaveButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ChannelsListBox);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TrackSelectionForm";
            this.Text = "Track Filtering";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TrackSelectionForm_Closing);
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckedListBox ChannelsListBox;
        private Label label1;
        private Button SongProfileSaveButton;
        private CheckBox AutoActivateCheckBox;
        private Button LoadProfileButton;
        private Label TrackListLabel;
        private CheckedListBox TrackListBox;
        private Panel OffsetPanel;
        private CustomBufferedPanel panel1;
        private Button button1;
        private TextBox textBoxNotesForChords;
        private Label label2;
        private Button button2;
        private SaveFileDialog saveFileDialog1;
        private ComboBox instrumentsBox;
        private Label label3;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem selectAllToolStripMenuItem;
        private ToolStripMenuItem selectNoneToolStripMenuItem;
        private ToolStripMenuItem selectInverseToolStripMenuItem;
        private ContextMenuStrip contextMenuStrip2;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem3;
    }
}