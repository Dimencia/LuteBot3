using System;
using System.Windows.Forms;
using Sanford.Multimedia.Midi;

namespace LuteBot
{
    partial class LuteBotForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LuteBotForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keyBindingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.onlineSyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trackFilteringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.liveInputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.guildLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timeSyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.focusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MusicProgressBar = new System.Windows.Forms.TrackBar();
            this.CurrentMusicLabel = new System.Windows.Forms.Label();
            this.StartLabel = new System.Windows.Forms.Label();
            this.PreviousButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.PlayButton = new System.Windows.Forms.Button();
            this.NextButton = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.LoadFileButton = new System.Windows.Forms.Button();
            this.EndTimeLabel = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MusicProgressBar)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.windowToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(462, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.keyBindingToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // keyBindingToolStripMenuItem
            // 
            this.keyBindingToolStripMenuItem.Name = "keyBindingToolStripMenuItem";
            this.keyBindingToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.keyBindingToolStripMenuItem.Text = "Key Binding";
            this.keyBindingToolStripMenuItem.Click += new System.EventHandler(this.KeyBindingToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.focusToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playListToolStripMenuItem,
            this.soundBoardToolStripMenuItem,
            this.onlineSyncToolStripMenuItem,
            this.trackFilteringToolStripMenuItem,
            this.liveInputToolStripMenuItem,
            this.guildLibraryToolStripMenuItem,
            this.timeSyncToolStripMenuItem});
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Open";
            // 
            // playListToolStripMenuItem
            // 
            this.playListToolStripMenuItem.Name = "playListToolStripMenuItem";
            this.playListToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.playListToolStripMenuItem.Text = "PlayList";
            this.playListToolStripMenuItem.Click += new System.EventHandler(this.PlayListToolStripMenuItem_Click);
            // 
            // soundBoardToolStripMenuItem
            // 
            this.soundBoardToolStripMenuItem.Name = "soundBoardToolStripMenuItem";
            this.soundBoardToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.soundBoardToolStripMenuItem.Text = "SoundBoard";
            this.soundBoardToolStripMenuItem.Click += new System.EventHandler(this.SoundBoardToolStripMenuItem_Click);
            // 
            // onlineSyncToolStripMenuItem
            // 
            this.onlineSyncToolStripMenuItem.Enabled = false;
            this.onlineSyncToolStripMenuItem.Name = "onlineSyncToolStripMenuItem";
            this.onlineSyncToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.onlineSyncToolStripMenuItem.Text = "Online Sync";
            this.onlineSyncToolStripMenuItem.Click += new System.EventHandler(this.OnlineSyncToolStripMenuItem_Click);
            // 
            // trackFilteringToolStripMenuItem
            // 
            this.trackFilteringToolStripMenuItem.Name = "trackFilteringToolStripMenuItem";
            this.trackFilteringToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.trackFilteringToolStripMenuItem.Text = "Track Filtering";
            this.trackFilteringToolStripMenuItem.Click += new System.EventHandler(this.TrackFilteringToolStripMenuItem_Click);
            // 
            // liveInputToolStripMenuItem
            // 
            this.liveInputToolStripMenuItem.Name = "liveInputToolStripMenuItem";
            this.liveInputToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.liveInputToolStripMenuItem.Text = "Live Input";
            this.liveInputToolStripMenuItem.Click += new System.EventHandler(this.liveInputToolStripMenuItem_Click);
            // 
            // guildLibraryToolStripMenuItem
            // 
            this.guildLibraryToolStripMenuItem.Enabled = false;
            this.guildLibraryToolStripMenuItem.Name = "guildLibraryToolStripMenuItem";
            this.guildLibraryToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.guildLibraryToolStripMenuItem.Text = "Guild Library";
            this.guildLibraryToolStripMenuItem.Click += new System.EventHandler(this.GuildLibraryToolStripMenuItem_Click);
            // 
            // timeSyncToolStripMenuItem
            // 
            this.timeSyncToolStripMenuItem.Name = "timeSyncToolStripMenuItem";
            this.timeSyncToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.timeSyncToolStripMenuItem.Text = "Time Sync";
            this.timeSyncToolStripMenuItem.Click += new System.EventHandler(this.TimeSyncToolStripMenuItem_Click);
            // 
            // focusToolStripMenuItem
            // 
            this.focusToolStripMenuItem.Name = "focusToolStripMenuItem";
            this.focusToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.focusToolStripMenuItem.Text = "Focus";
            this.focusToolStripMenuItem.Click += new System.EventHandler(this.LuteBotForm_Focus);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // MusicProgressBar
            // 
            this.MusicProgressBar.BackColor = System.Drawing.SystemColors.ControlLight;
            this.MusicProgressBar.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.MusicProgressBar.Location = new System.Drawing.Point(12, 57);
            this.MusicProgressBar.Maximum = 500;
            this.MusicProgressBar.Name = "MusicProgressBar";
            this.MusicProgressBar.Size = new System.Drawing.Size(438, 45);
            this.MusicProgressBar.TabIndex = 1;
            this.MusicProgressBar.Scroll += new System.EventHandler(this.MusicProgressBar_Scroll);
            // 
            // CurrentMusicLabel
            // 
            this.CurrentMusicLabel.AutoSize = true;
            this.CurrentMusicLabel.Location = new System.Drawing.Point(13, 41);
            this.CurrentMusicLabel.Name = "CurrentMusicLabel";
            this.CurrentMusicLabel.Size = new System.Drawing.Size(0, 13);
            this.CurrentMusicLabel.TabIndex = 2;
            // 
            // StartLabel
            // 
            this.StartLabel.AutoSize = true;
            this.StartLabel.Location = new System.Drawing.Point(12, 105);
            this.StartLabel.Name = "StartLabel";
            this.StartLabel.Size = new System.Drawing.Size(34, 13);
            this.StartLabel.TabIndex = 3;
            this.StartLabel.Text = "00:00";
            // 
            // PreviousButton
            // 
            this.PreviousButton.Location = new System.Drawing.Point(12, 129);
            this.PreviousButton.Name = "PreviousButton";
            this.PreviousButton.Size = new System.Drawing.Size(75, 23);
            this.PreviousButton.TabIndex = 5;
            this.PreviousButton.Text = "Previous";
            this.PreviousButton.UseVisualStyleBackColor = true;
            this.PreviousButton.Click += new System.EventHandler(this.PreviousButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Location = new System.Drawing.Point(93, 128);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(75, 23);
            this.StopButton.TabIndex = 6;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // PlayButton
            // 
            this.PlayButton.Location = new System.Drawing.Point(294, 128);
            this.PlayButton.Name = "PlayButton";
            this.PlayButton.Size = new System.Drawing.Size(75, 23);
            this.PlayButton.TabIndex = 7;
            this.PlayButton.Text = "Play";
            this.PlayButton.UseVisualStyleBackColor = true;
            this.PlayButton.Click += new System.EventHandler(this.PlayButton_Click);
            // 
            // NextButton
            // 
            this.NextButton.Location = new System.Drawing.Point(375, 128);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 23);
            this.NextButton.TabIndex = 8;
            this.NextButton.Text = "Next";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // LoadFileButton
            // 
            this.LoadFileButton.Location = new System.Drawing.Point(175, 128);
            this.LoadFileButton.Name = "LoadFileButton";
            this.LoadFileButton.Size = new System.Drawing.Size(113, 23);
            this.LoadFileButton.TabIndex = 9;
            this.LoadFileButton.Text = "Load Midi File";
            this.LoadFileButton.UseVisualStyleBackColor = true;
            this.LoadFileButton.Click += new System.EventHandler(this.LoadFileButton_Click);
            // 
            // EndTimeLabel
            // 
            this.EndTimeLabel.AutoSize = true;
            this.EndTimeLabel.Location = new System.Drawing.Point(416, 105);
            this.EndTimeLabel.Name = "EndTimeLabel";
            this.EndTimeLabel.Size = new System.Drawing.Size(34, 13);
            this.EndTimeLabel.TabIndex = 10;
            this.EndTimeLabel.Text = "00:00";
            // 
            // LuteBotForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(462, 164);
            this.Controls.Add(this.EndTimeLabel);
            this.Controls.Add(this.LoadFileButton);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.PlayButton);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.PreviousButton);
            this.Controls.Add(this.StartLabel);
            this.Controls.Add(this.CurrentMusicLabel);
            this.Controls.Add(this.MusicProgressBar);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LuteBotForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "LuteBot";
            this.Click += new System.EventHandler(this.LuteBotForm_Focus);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MusicProgressBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem keyBindingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.TrackBar MusicProgressBar;
        private System.Windows.Forms.Label CurrentMusicLabel;
        private System.Windows.Forms.Label StartLabel;
        private System.Windows.Forms.Button PreviousButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Button PlayButton;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Timer timer1;
        private ToolStripMenuItem windowToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem playListToolStripMenuItem;
        private ToolStripMenuItem soundBoardToolStripMenuItem;
        private ToolStripMenuItem onlineSyncToolStripMenuItem;
        private Button LoadFileButton;
        private Label EndTimeLabel;
        private ToolStripMenuItem trackFilteringToolStripMenuItem;
        private ToolStripMenuItem focusToolStripMenuItem;
        private ToolStripMenuItem liveInputToolStripMenuItem;
        private ToolStripMenuItem guildLibraryToolStripMenuItem;
        private ToolStripMenuItem timeSyncToolStripMenuItem;
    }
}

