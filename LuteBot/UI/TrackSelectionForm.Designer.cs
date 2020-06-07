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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TrackSelectionForm));
            this.ChannelsListBox = new System.Windows.Forms.CheckedListBox();
            this.SelectAllChannelsCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SongProfileSaveButton = new System.Windows.Forms.Button();
            this.AutoActivateCheckBox = new System.Windows.Forms.CheckBox();
            this.LoadProfileButton = new System.Windows.Forms.Button();
            this.TrackListLabel = new System.Windows.Forms.Label();
            this.SelectAllTracksCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackListBox = new System.Windows.Forms.CheckedListBox();
            this.OffsetPanel = new System.Windows.Forms.Panel();
            this.buttonAdvanced = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ChannelsListBox
            // 
            this.ChannelsListBox.FormattingEnabled = true;
            this.ChannelsListBox.Location = new System.Drawing.Point(16, 64);
            this.ChannelsListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ChannelsListBox.Name = "ChannelsListBox";
            this.ChannelsListBox.Size = new System.Drawing.Size(341, 361);
            this.ChannelsListBox.TabIndex = 3;
            this.ChannelsListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ChannelListBox_ItemChecked);
            // 
            // SelectAllChannelsCheckBox
            // 
            this.SelectAllChannelsCheckBox.AutoSize = true;
            this.SelectAllChannelsCheckBox.Location = new System.Drawing.Point(16, 36);
            this.SelectAllChannelsCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SelectAllChannelsCheckBox.Name = "SelectAllChannelsCheckBox";
            this.SelectAllChannelsCheckBox.Size = new System.Drawing.Size(88, 21);
            this.SelectAllChannelsCheckBox.TabIndex = 7;
            this.SelectAllChannelsCheckBox.Text = "Select All";
            this.SelectAllChannelsCheckBox.UseVisualStyleBackColor = true;
            this.SelectAllChannelsCheckBox.CheckedChanged += new System.EventHandler(this.SelectAllChannelsTextBox_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(168, 20);
            this.label1.TabIndex = 9;
            this.label1.Text = "List of Instruments";
            // 
            // SongProfileSaveButton
            // 
            this.SongProfileSaveButton.Location = new System.Drawing.Point(16, 446);
            this.SongProfileSaveButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SongProfileSaveButton.Name = "SongProfileSaveButton";
            this.SongProfileSaveButton.Size = new System.Drawing.Size(343, 37);
            this.SongProfileSaveButton.TabIndex = 10;
            this.SongProfileSaveButton.Text = "Save Song Profile";
            this.SongProfileSaveButton.UseVisualStyleBackColor = true;
            this.SongProfileSaveButton.Click += new System.EventHandler(this.SongProfileSaveButton_Click);
            // 
            // AutoActivateCheckBox
            // 
            this.AutoActivateCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.AutoActivateCheckBox.AutoSize = true;
            this.AutoActivateCheckBox.Checked = true;
            this.AutoActivateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoActivateCheckBox.Location = new System.Drawing.Point(255, 694);
            this.AutoActivateCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
            this.LoadProfileButton.Location = new System.Drawing.Point(367, 446);
            this.LoadProfileButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.LoadProfileButton.Name = "LoadProfileButton";
            this.LoadProfileButton.Size = new System.Drawing.Size(333, 37);
            this.LoadProfileButton.TabIndex = 12;
            this.LoadProfileButton.Text = "Load Song Profile";
            this.LoadProfileButton.UseVisualStyleBackColor = true;
            this.LoadProfileButton.Click += new System.EventHandler(this.LoadProfileButton_Click);
            // 
            // TrackListLabel
            // 
            this.TrackListLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TrackListLabel.AutoSize = true;
            this.TrackListLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TrackListLabel.Location = new System.Drawing.Point(367, 11);
            this.TrackListLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.TrackListLabel.Name = "TrackListLabel";
            this.TrackListLabel.Size = new System.Drawing.Size(126, 20);
            this.TrackListLabel.TabIndex = 15;
            this.TrackListLabel.Text = "List of Tracks";
            // 
            // SelectAllTracksCheckBox
            // 
            this.SelectAllTracksCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectAllTracksCheckBox.AutoSize = true;
            this.SelectAllTracksCheckBox.Location = new System.Drawing.Point(372, 36);
            this.SelectAllTracksCheckBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SelectAllTracksCheckBox.Name = "SelectAllTracksCheckBox";
            this.SelectAllTracksCheckBox.Size = new System.Drawing.Size(88, 21);
            this.SelectAllTracksCheckBox.TabIndex = 14;
            this.SelectAllTracksCheckBox.Text = "Select All";
            this.SelectAllTracksCheckBox.UseVisualStyleBackColor = true;
            this.SelectAllTracksCheckBox.CheckedChanged += new System.EventHandler(this.SelectAllTracksCheckBox_CheckedChanged);
            // 
            // TrackListBox
            // 
            this.TrackListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TrackListBox.FormattingEnabled = true;
            this.TrackListBox.Location = new System.Drawing.Point(367, 64);
            this.TrackListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TrackListBox.Name = "TrackListBox";
            this.TrackListBox.Size = new System.Drawing.Size(329, 361);
            this.TrackListBox.TabIndex = 13;
            this.TrackListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.TrackListBox_ItemCheck);
            // 
            // OffsetPanel
            // 
            this.OffsetPanel.AutoScroll = true;
            this.OffsetPanel.Location = new System.Drawing.Point(0, 0);
            this.OffsetPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.OffsetPanel.Name = "OffsetPanel";
            this.OffsetPanel.Size = new System.Drawing.Size(659, 196);
            this.OffsetPanel.TabIndex = 16;
            // 
            // buttonAdvanced
            // 
            this.buttonAdvanced.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonAdvanced.Location = new System.Drawing.Point(17, 694);
            this.buttonAdvanced.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonAdvanced.Name = "buttonAdvanced";
            this.buttonAdvanced.Size = new System.Drawing.Size(100, 28);
            this.buttonAdvanced.TabIndex = 17;
            this.buttonAdvanced.Text = "Advanced";
            this.buttonAdvanced.UseVisualStyleBackColor = true;
            this.buttonAdvanced.Click += new System.EventHandler(this.buttonAdvanced_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.OffsetPanel);
            this.panel1.Location = new System.Drawing.Point(16, 490);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(683, 198);
            this.panel1.TabIndex = 18;
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.Location = new System.Drawing.Point(555, 695);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(145, 28);
            this.button1.TabIndex = 19;
            this.button1.Text = "Reset Alignment";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TrackSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(716, 742);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.buttonAdvanced);
            this.Controls.Add(this.TrackListLabel);
            this.Controls.Add(this.SelectAllTracksCheckBox);
            this.Controls.Add(this.TrackListBox);
            this.Controls.Add(this.LoadProfileButton);
            this.Controls.Add(this.AutoActivateCheckBox);
            this.Controls.Add(this.SongProfileSaveButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SelectAllChannelsCheckBox);
            this.Controls.Add(this.ChannelsListBox);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TrackSelectionForm";
            this.Text = "Track Selection";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TrackSelectionForm_Closing);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckedListBox ChannelsListBox;
        private System.Windows.Forms.CheckBox SelectAllChannelsCheckBox;
        private Label label1;
        private Button SongProfileSaveButton;
        private CheckBox AutoActivateCheckBox;
        private Button LoadProfileButton;
        private Label TrackListLabel;
        private CheckBox SelectAllTracksCheckBox;
        private CheckedListBox TrackListBox;
        private Panel OffsetPanel;
        private Button buttonAdvanced;
        private Panel panel1;
        private Button button1;
    }
}