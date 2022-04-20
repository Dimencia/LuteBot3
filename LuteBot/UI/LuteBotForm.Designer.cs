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
            this.installLuteModToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkInstallUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.playListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundBoardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trackFilteringToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.liveInputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.guildLibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.focusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.guildLibraryToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.lutemodPartitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.ReloadButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.setMordhauPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MusicProgressBar)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.guildLibraryToolStripMenuItem1,
            this.lutemodPartitionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(616, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.keyBindingToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.installLuteModToolStripMenuItem,
            this.checkInstallUpdatesToolStripMenuItem,
            this.setMordhauPathToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(75, 24);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // keyBindingToolStripMenuItem
            // 
            this.keyBindingToolStripMenuItem.Name = "keyBindingToolStripMenuItem";
            this.keyBindingToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.keyBindingToolStripMenuItem.Text = "Key Binding";
            this.keyBindingToolStripMenuItem.Click += new System.EventHandler(this.KeyBindingToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // installLuteModToolStripMenuItem
            // 
            this.installLuteModToolStripMenuItem.Name = "installLuteModToolStripMenuItem";
            this.installLuteModToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.installLuteModToolStripMenuItem.Text = "Install LuteMod";
            this.installLuteModToolStripMenuItem.Click += new System.EventHandler(this.installLuteModToolStripMenuItem_Click);
            // 
            // checkInstallUpdatesToolStripMenuItem
            // 
            this.checkInstallUpdatesToolStripMenuItem.Name = "checkInstallUpdatesToolStripMenuItem";
            this.checkInstallUpdatesToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.checkInstallUpdatesToolStripMenuItem.Text = "Check/Install Updates";
            this.checkInstallUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkInstallUpdatesToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.focusToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(78, 24);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.playListToolStripMenuItem,
            this.soundBoardToolStripMenuItem,
            this.trackFilteringToolStripMenuItem,
            this.liveInputToolStripMenuItem,
            this.guildLibraryToolStripMenuItem});
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(129, 26);
            this.openToolStripMenuItem.Text = "Open";
            // 
            // playListToolStripMenuItem
            // 
            this.playListToolStripMenuItem.Name = "playListToolStripMenuItem";
            this.playListToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.playListToolStripMenuItem.Text = "PlayList";
            this.playListToolStripMenuItem.Click += new System.EventHandler(this.PlayListToolStripMenuItem_Click);
            // 
            // soundBoardToolStripMenuItem
            // 
            this.soundBoardToolStripMenuItem.Name = "soundBoardToolStripMenuItem";
            this.soundBoardToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.soundBoardToolStripMenuItem.Text = "SoundBoard";
            this.soundBoardToolStripMenuItem.Click += new System.EventHandler(this.SoundBoardToolStripMenuItem_Click);
            // 
            // trackFilteringToolStripMenuItem
            // 
            this.trackFilteringToolStripMenuItem.Name = "trackFilteringToolStripMenuItem";
            this.trackFilteringToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.trackFilteringToolStripMenuItem.Text = "Track Filtering";
            this.trackFilteringToolStripMenuItem.Click += new System.EventHandler(this.TrackFilteringToolStripMenuItem_Click);
            // 
            // liveInputToolStripMenuItem
            // 
            this.liveInputToolStripMenuItem.Name = "liveInputToolStripMenuItem";
            this.liveInputToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.liveInputToolStripMenuItem.Text = "Live Input";
            this.liveInputToolStripMenuItem.Click += new System.EventHandler(this.liveInputToolStripMenuItem_Click);
            // 
            // guildLibraryToolStripMenuItem
            // 
            this.guildLibraryToolStripMenuItem.Name = "guildLibraryToolStripMenuItem";
            this.guildLibraryToolStripMenuItem.Size = new System.Drawing.Size(184, 26);
            this.guildLibraryToolStripMenuItem.Text = "Guild Library";
            this.guildLibraryToolStripMenuItem.Click += new System.EventHandler(this.GuildLibraryToolStripMenuItem_Click);
            // 
            // focusToolStripMenuItem
            // 
            this.focusToolStripMenuItem.Name = "focusToolStripMenuItem";
            this.focusToolStripMenuItem.Size = new System.Drawing.Size(129, 26);
            this.focusToolStripMenuItem.Text = "Focus";
            this.focusToolStripMenuItem.Click += new System.EventHandler(this.LuteBotForm_Focus);
            // 
            // guildLibraryToolStripMenuItem1
            // 
            this.guildLibraryToolStripMenuItem1.BackColor = System.Drawing.Color.Gold;
            this.guildLibraryToolStripMenuItem1.Name = "guildLibraryToolStripMenuItem1";
            this.guildLibraryToolStripMenuItem1.Size = new System.Drawing.Size(107, 24);
            this.guildLibraryToolStripMenuItem1.Text = "Guild Library";
            this.guildLibraryToolStripMenuItem1.Click += new System.EventHandler(this.GuildLibraryToolStripMenuItem_Click);
            // 
            // lutemodPartitionsToolStripMenuItem
            // 
            this.lutemodPartitionsToolStripMenuItem.Name = "lutemodPartitionsToolStripMenuItem";
            this.lutemodPartitionsToolStripMenuItem.Size = new System.Drawing.Size(147, 24);
            this.lutemodPartitionsToolStripMenuItem.Text = "Lutemod Partitions";
            this.lutemodPartitionsToolStripMenuItem.Click += new System.EventHandler(this.lutemodPartitionsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // MusicProgressBar
            // 
            this.MusicProgressBar.BackColor = System.Drawing.SystemColors.ControlLight;
            this.MusicProgressBar.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.MusicProgressBar.Location = new System.Drawing.Point(16, 70);
            this.MusicProgressBar.Margin = new System.Windows.Forms.Padding(4);
            this.MusicProgressBar.Maximum = 500;
            this.MusicProgressBar.Name = "MusicProgressBar";
            this.MusicProgressBar.Size = new System.Drawing.Size(584, 56);
            this.MusicProgressBar.TabIndex = 1;
            this.MusicProgressBar.Scroll += new System.EventHandler(this.MusicProgressBar_Scroll);
            // 
            // CurrentMusicLabel
            // 
            this.CurrentMusicLabel.AutoSize = true;
            this.CurrentMusicLabel.Location = new System.Drawing.Point(17, 50);
            this.CurrentMusicLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CurrentMusicLabel.Name = "CurrentMusicLabel";
            this.CurrentMusicLabel.Size = new System.Drawing.Size(0, 17);
            this.CurrentMusicLabel.TabIndex = 2;
            // 
            // StartLabel
            // 
            this.StartLabel.AutoSize = true;
            this.StartLabel.Location = new System.Drawing.Point(16, 129);
            this.StartLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.StartLabel.Name = "StartLabel";
            this.StartLabel.Size = new System.Drawing.Size(44, 17);
            this.StartLabel.TabIndex = 3;
            this.StartLabel.Text = "00:00";
            // 
            // PreviousButton
            // 
            this.PreviousButton.Location = new System.Drawing.Point(16, 159);
            this.PreviousButton.Margin = new System.Windows.Forms.Padding(4);
            this.PreviousButton.Name = "PreviousButton";
            this.PreviousButton.Size = new System.Drawing.Size(100, 28);
            this.PreviousButton.TabIndex = 5;
            this.PreviousButton.Text = "Previous";
            this.PreviousButton.UseVisualStyleBackColor = true;
            this.PreviousButton.Click += new System.EventHandler(this.PreviousButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Location = new System.Drawing.Point(124, 158);
            this.StopButton.Margin = new System.Windows.Forms.Padding(4);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(100, 28);
            this.StopButton.TabIndex = 6;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // PlayButton
            // 
            this.PlayButton.Location = new System.Drawing.Point(392, 158);
            this.PlayButton.Margin = new System.Windows.Forms.Padding(4);
            this.PlayButton.Name = "PlayButton";
            this.PlayButton.Size = new System.Drawing.Size(100, 28);
            this.PlayButton.TabIndex = 7;
            this.PlayButton.Text = "Play";
            this.PlayButton.UseVisualStyleBackColor = true;
            this.PlayButton.Click += new System.EventHandler(this.PlayButton_Click);
            // 
            // NextButton
            // 
            this.NextButton.Location = new System.Drawing.Point(500, 158);
            this.NextButton.Margin = new System.Windows.Forms.Padding(4);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(100, 28);
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
            this.LoadFileButton.Location = new System.Drawing.Point(233, 158);
            this.LoadFileButton.Margin = new System.Windows.Forms.Padding(4);
            this.LoadFileButton.Name = "LoadFileButton";
            this.LoadFileButton.Size = new System.Drawing.Size(151, 28);
            this.LoadFileButton.TabIndex = 9;
            this.LoadFileButton.Text = "Load Midi File";
            this.LoadFileButton.UseVisualStyleBackColor = true;
            this.LoadFileButton.Click += new System.EventHandler(this.LoadFileButton_Click);
            // 
            // EndTimeLabel
            // 
            this.EndTimeLabel.AutoSize = true;
            this.EndTimeLabel.Location = new System.Drawing.Point(555, 129);
            this.EndTimeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.EndTimeLabel.Name = "EndTimeLabel";
            this.EndTimeLabel.Size = new System.Drawing.Size(44, 17);
            this.EndTimeLabel.TabIndex = 10;
            this.EndTimeLabel.Text = "00:00";
            // 
            // ReloadButton
            // 
            this.ReloadButton.FlatAppearance.BorderSize = 0;
            this.ReloadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReloadButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReloadButton.Location = new System.Drawing.Point(553, 30);
            this.ReloadButton.Margin = new System.Windows.Forms.Padding(0);
            this.ReloadButton.Name = "ReloadButton";
            this.ReloadButton.Size = new System.Drawing.Size(47, 37);
            this.ReloadButton.TabIndex = 11;
            this.ReloadButton.Text = "♻️";
            this.toolTip1.SetToolTip(this.ReloadButton, "Reload Song, after changing instruments or editing it");
            this.ReloadButton.UseVisualStyleBackColor = true;
            this.ReloadButton.Click += new System.EventHandler(this.ReloadButton_Click);
            // 
            // setMordhauPathToolStripMenuItem
            // 
            this.setMordhauPathToolStripMenuItem.Name = "setMordhauPathToolStripMenuItem";
            this.setMordhauPathToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.setMordhauPathToolStripMenuItem.Text = "Set Mordhau Path";
            this.setMordhauPathToolStripMenuItem.Click += new System.EventHandler(this.setMordhauPathToolStripMenuItem_Click);
            // 
            // LuteBotForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 204);
            this.Controls.Add(this.ReloadButton);
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
            this.Margin = new System.Windows.Forms.Padding(4);
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
        private Button LoadFileButton;
        private Label EndTimeLabel;
        private ToolStripMenuItem trackFilteringToolStripMenuItem;
        private ToolStripMenuItem focusToolStripMenuItem;
        private ToolStripMenuItem liveInputToolStripMenuItem;
        private ToolStripMenuItem guildLibraryToolStripMenuItem;
        private Button ReloadButton;
        private ToolTip toolTip1;
        private ToolStripMenuItem guildLibraryToolStripMenuItem1;
        private ToolStripMenuItem lutemodPartitionsToolStripMenuItem;
        private ToolStripMenuItem installLuteModToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem checkInstallUpdatesToolStripMenuItem;
        private ToolStripMenuItem setMordhauPathToolStripMenuItem;
    }
}

