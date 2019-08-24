namespace LuteBot
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.VersionLabel = new System.Windows.Forms.Label();
            this.UpdateLinkLabel = new System.Windows.Forms.LinkLabel();
            this.PlaylistCheckBox = new System.Windows.Forms.CheckBox();
            this.SoundBoardCheckBox = new System.Windows.Forms.CheckBox();
            this.ReturnButton = new System.Windows.Forms.Button();
            this.ApplyButton = new System.Windows.Forms.Button();
            this.SoundEffectsCheckBox = new System.Windows.Forms.CheckBox();
            this.TrackSelectionCheckBox = new System.Windows.Forms.CheckBox();
            this.OnlineSyncCheckBox = new System.Windows.Forms.CheckBox();
            this.NoteConversionMode = new System.Windows.Forms.ComboBox();
            this.NoteConversionLabel = new System.Windows.Forms.Label();
            this.SettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.LiveMidiCheckBox = new System.Windows.Forms.CheckBox();
            this.OffAutoConsoleRadio = new System.Windows.Forms.RadioButton();
            this.AutoConsoleLabel = new System.Windows.Forms.Label();
            this.OldAutoConsoleRadio = new System.Windows.Forms.RadioButton();
            this.NewAutoConsoleRadio = new System.Windows.Forms.RadioButton();
            this.AdvancedGroupBox = new System.Windows.Forms.GroupBox();
            this.NoteCooldownLabel = new System.Windows.Forms.Label();
            this.NoteCooldownNumeric = new System.Windows.Forms.NumericUpDown();
            this.NoteCountNumeric = new System.Windows.Forms.NumericUpDown();
            this.NoteCountLabel = new System.Windows.Forms.Label();
            this.LowestNoteLabel = new System.Windows.Forms.Label();
            this.LowestNoteNumeric = new System.Windows.Forms.NumericUpDown();
            this.SettingsGroupBox.SuspendLayout();
            this.AdvancedGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NoteCooldownNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NoteCountNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LowestNoteNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Location = new System.Drawing.Point(-1, 377);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(224, 13);
            this.VersionLabel.TabIndex = 0;
            this.VersionLabel.Text = "Mordhau Lute Bot [VERSION] made by Monty";
            // 
            // UpdateLinkLabel
            // 
            this.UpdateLinkLabel.AutoSize = true;
            this.UpdateLinkLabel.Location = new System.Drawing.Point(229, 377);
            this.UpdateLinkLabel.Name = "UpdateLinkLabel";
            this.UpdateLinkLabel.Size = new System.Drawing.Size(179, 13);
            this.UpdateLinkLabel.TabIndex = 1;
            this.UpdateLinkLabel.TabStop = true;
            this.UpdateLinkLabel.Text = "you have the latest version avaliable";
            this.UpdateLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.UpdateLinkLabel_LinkClicked);
            // 
            // PlaylistCheckBox
            // 
            this.PlaylistCheckBox.AutoSize = true;
            this.PlaylistCheckBox.Location = new System.Drawing.Point(6, 19);
            this.PlaylistCheckBox.Name = "PlaylistCheckBox";
            this.PlaylistCheckBox.Size = new System.Drawing.Size(170, 17);
            this.PlaylistCheckBox.TabIndex = 2;
            this.PlaylistCheckBox.Text = "Show Playlist Menu on Startup";
            this.PlaylistCheckBox.UseVisualStyleBackColor = true;
            this.PlaylistCheckBox.CheckedChanged += new System.EventHandler(this.PlaylistCheckBox_CheckedChanged);
            // 
            // SoundBoardCheckBox
            // 
            this.SoundBoardCheckBox.AutoSize = true;
            this.SoundBoardCheckBox.Location = new System.Drawing.Point(6, 42);
            this.SoundBoardCheckBox.Name = "SoundBoardCheckBox";
            this.SoundBoardCheckBox.Size = new System.Drawing.Size(196, 17);
            this.SoundBoardCheckBox.TabIndex = 3;
            this.SoundBoardCheckBox.Text = "Show Soundboard Menu on Startup";
            this.SoundBoardCheckBox.UseVisualStyleBackColor = true;
            this.SoundBoardCheckBox.CheckedChanged += new System.EventHandler(this.SoundBoardCheckBox_CheckedChanged);
            // 
            // ReturnButton
            // 
            this.ReturnButton.Location = new System.Drawing.Point(10, 344);
            this.ReturnButton.Name = "ReturnButton";
            this.ReturnButton.Size = new System.Drawing.Size(120, 30);
            this.ReturnButton.TabIndex = 4;
            this.ReturnButton.Text = "Cancel";
            this.ReturnButton.UseVisualStyleBackColor = true;
            this.ReturnButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ApplyButton
            // 
            this.ApplyButton.Location = new System.Drawing.Point(277, 344);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Size = new System.Drawing.Size(120, 30);
            this.ApplyButton.TabIndex = 5;
            this.ApplyButton.Text = "Apply";
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // SoundEffectsCheckBox
            // 
            this.SoundEffectsCheckBox.AutoSize = true;
            this.SoundEffectsCheckBox.Location = new System.Drawing.Point(6, 134);
            this.SoundEffectsCheckBox.Name = "SoundEffectsCheckBox";
            this.SoundEffectsCheckBox.Size = new System.Drawing.Size(92, 17);
            this.SoundEffectsCheckBox.TabIndex = 6;
            this.SoundEffectsCheckBox.Text = "Sound effects";
            this.SoundEffectsCheckBox.UseVisualStyleBackColor = true;
            this.SoundEffectsCheckBox.CheckedChanged += new System.EventHandler(this.SoundEffectsCheckBox_CheckedChanged);
            // 
            // TrackSelectionCheckBox
            // 
            this.TrackSelectionCheckBox.AutoSize = true;
            this.TrackSelectionCheckBox.Location = new System.Drawing.Point(6, 65);
            this.TrackSelectionCheckBox.Name = "TrackSelectionCheckBox";
            this.TrackSelectionCheckBox.Size = new System.Drawing.Size(213, 17);
            this.TrackSelectionCheckBox.TabIndex = 7;
            this.TrackSelectionCheckBox.Text = "Show Track Selection Menu on Startup";
            this.TrackSelectionCheckBox.UseVisualStyleBackColor = true;
            this.TrackSelectionCheckBox.CheckedChanged += new System.EventHandler(this.TrackSelectionCheckBox_CheckedChanged);
            // 
            // OnlineSyncCheckBox
            // 
            this.OnlineSyncCheckBox.AutoSize = true;
            this.OnlineSyncCheckBox.Enabled = false;
            this.OnlineSyncCheckBox.Location = new System.Drawing.Point(6, 88);
            this.OnlineSyncCheckBox.Name = "OnlineSyncCheckBox";
            this.OnlineSyncCheckBox.Size = new System.Drawing.Size(195, 17);
            this.OnlineSyncCheckBox.TabIndex = 8;
            this.OnlineSyncCheckBox.Text = "Show Online Sync Menu on Startup";
            this.OnlineSyncCheckBox.UseVisualStyleBackColor = true;
            this.OnlineSyncCheckBox.CheckedChanged += new System.EventHandler(this.OnlineSyncCheckBox_CheckedChanged);
            // 
            // NoteConversionMode
            // 
            this.NoteConversionMode.FormattingEnabled = true;
            this.NoteConversionMode.Items.AddRange(new object[] {
            "2.0",
            "Off"});
            this.NoteConversionMode.Location = new System.Drawing.Point(172, 15);
            this.NoteConversionMode.Name = "NoteConversionMode";
            this.NoteConversionMode.Size = new System.Drawing.Size(209, 21);
            this.NoteConversionMode.TabIndex = 9;
            this.NoteConversionMode.SelectedIndexChanged += new System.EventHandler(this.NoteConversionMode_SelectedIndexChanged);
            // 
            // NoteConversionLabel
            // 
            this.NoteConversionLabel.AutoSize = true;
            this.NoteConversionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NoteConversionLabel.Location = new System.Drawing.Point(6, 16);
            this.NoteConversionLabel.Name = "NoteConversionLabel";
            this.NoteConversionLabel.Size = new System.Drawing.Size(160, 17);
            this.NoteConversionLabel.TabIndex = 10;
            this.NoteConversionLabel.Text = "Note Conversion Mode :";
            // 
            // SettingsGroupBox
            // 
            this.SettingsGroupBox.Controls.Add(this.LiveMidiCheckBox);
            this.SettingsGroupBox.Controls.Add(this.OffAutoConsoleRadio);
            this.SettingsGroupBox.Controls.Add(this.AutoConsoleLabel);
            this.SettingsGroupBox.Controls.Add(this.OldAutoConsoleRadio);
            this.SettingsGroupBox.Controls.Add(this.NewAutoConsoleRadio);
            this.SettingsGroupBox.Controls.Add(this.PlaylistCheckBox);
            this.SettingsGroupBox.Controls.Add(this.SoundBoardCheckBox);
            this.SettingsGroupBox.Controls.Add(this.TrackSelectionCheckBox);
            this.SettingsGroupBox.Controls.Add(this.OnlineSyncCheckBox);
            this.SettingsGroupBox.Controls.Add(this.SoundEffectsCheckBox);
            this.SettingsGroupBox.Location = new System.Drawing.Point(10, 12);
            this.SettingsGroupBox.Name = "SettingsGroupBox";
            this.SettingsGroupBox.Size = new System.Drawing.Size(387, 194);
            this.SettingsGroupBox.TabIndex = 12;
            this.SettingsGroupBox.TabStop = false;
            this.SettingsGroupBox.Text = "Settings";
            // 
            // LiveMidiCheckBox
            // 
            this.LiveMidiCheckBox.AutoSize = true;
            this.LiveMidiCheckBox.Location = new System.Drawing.Point(6, 111);
            this.LiveMidiCheckBox.Name = "LiveMidiCheckBox";
            this.LiveMidiCheckBox.Size = new System.Drawing.Size(207, 17);
            this.LiveMidiCheckBox.TabIndex = 14;
            this.LiveMidiCheckBox.Text = "Show Live Midi Input Menu on Startup\r\n";
            this.LiveMidiCheckBox.UseVisualStyleBackColor = true;
            this.LiveMidiCheckBox.CheckedChanged += new System.EventHandler(this.LiveMidiCheckBox_CheckedChanged);
            // 
            // OffAutoConsoleRadio
            // 
            this.OffAutoConsoleRadio.AutoSize = true;
            this.OffAutoConsoleRadio.Location = new System.Drawing.Point(104, 171);
            this.OffAutoConsoleRadio.Name = "OffAutoConsoleRadio";
            this.OffAutoConsoleRadio.Size = new System.Drawing.Size(39, 17);
            this.OffAutoConsoleRadio.TabIndex = 13;
            this.OffAutoConsoleRadio.TabStop = true;
            this.OffAutoConsoleRadio.Text = "Off";
            this.OffAutoConsoleRadio.UseVisualStyleBackColor = true;
            this.OffAutoConsoleRadio.CheckedChanged += new System.EventHandler(this.OffAutoConsoleRadio_CheckedChanged);
            // 
            // AutoConsoleLabel
            // 
            this.AutoConsoleLabel.AutoSize = true;
            this.AutoConsoleLabel.Location = new System.Drawing.Point(3, 154);
            this.AutoConsoleLabel.Name = "AutoConsoleLabel";
            this.AutoConsoleLabel.Size = new System.Drawing.Size(138, 13);
            this.AutoConsoleLabel.TabIndex = 12;
            this.AutoConsoleLabel.Text = "Automatic Console System :";
            // 
            // OldAutoConsoleRadio
            // 
            this.OldAutoConsoleRadio.AutoSize = true;
            this.OldAutoConsoleRadio.Location = new System.Drawing.Point(3, 171);
            this.OldAutoConsoleRadio.Name = "OldAutoConsoleRadio";
            this.OldAutoConsoleRadio.Size = new System.Drawing.Size(41, 17);
            this.OldAutoConsoleRadio.TabIndex = 11;
            this.OldAutoConsoleRadio.TabStop = true;
            this.OldAutoConsoleRadio.Text = "Old";
            this.OldAutoConsoleRadio.UseVisualStyleBackColor = true;
            this.OldAutoConsoleRadio.CheckedChanged += new System.EventHandler(this.OldAutoConsoleRadio_CheckedChanged);
            // 
            // NewAutoConsoleRadio
            // 
            this.NewAutoConsoleRadio.AutoSize = true;
            this.NewAutoConsoleRadio.Location = new System.Drawing.Point(50, 171);
            this.NewAutoConsoleRadio.Name = "NewAutoConsoleRadio";
            this.NewAutoConsoleRadio.Size = new System.Drawing.Size(47, 17);
            this.NewAutoConsoleRadio.TabIndex = 10;
            this.NewAutoConsoleRadio.TabStop = true;
            this.NewAutoConsoleRadio.Text = "New";
            this.NewAutoConsoleRadio.UseVisualStyleBackColor = true;
            this.NewAutoConsoleRadio.CheckedChanged += new System.EventHandler(this.NewAutoConsoleRadio_CheckedChanged);
            // 
            // AdvancedGroupBox
            // 
            this.AdvancedGroupBox.Controls.Add(this.NoteCooldownLabel);
            this.AdvancedGroupBox.Controls.Add(this.NoteCooldownNumeric);
            this.AdvancedGroupBox.Controls.Add(this.NoteCountNumeric);
            this.AdvancedGroupBox.Controls.Add(this.NoteCountLabel);
            this.AdvancedGroupBox.Controls.Add(this.LowestNoteLabel);
            this.AdvancedGroupBox.Controls.Add(this.LowestNoteNumeric);
            this.AdvancedGroupBox.Controls.Add(this.NoteConversionLabel);
            this.AdvancedGroupBox.Controls.Add(this.NoteConversionMode);
            this.AdvancedGroupBox.Location = new System.Drawing.Point(10, 212);
            this.AdvancedGroupBox.Name = "AdvancedGroupBox";
            this.AdvancedGroupBox.Size = new System.Drawing.Size(387, 126);
            this.AdvancedGroupBox.TabIndex = 13;
            this.AdvancedGroupBox.TabStop = false;
            this.AdvancedGroupBox.Text = "Advanced Parameters";
            // 
            // NoteCooldownLabel
            // 
            this.NoteCooldownLabel.AutoSize = true;
            this.NoteCooldownLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NoteCooldownLabel.Location = new System.Drawing.Point(6, 97);
            this.NoteCooldownLabel.Name = "NoteCooldownLabel";
            this.NoteCooldownLabel.Size = new System.Drawing.Size(111, 17);
            this.NoteCooldownLabel.TabIndex = 16;
            this.NoteCooldownLabel.Text = "Note Cooldown :";
            // 
            // NoteCooldownNumeric
            // 
            this.NoteCooldownNumeric.Location = new System.Drawing.Point(172, 97);
            this.NoteCooldownNumeric.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.NoteCooldownNumeric.Name = "NoteCooldownNumeric";
            this.NoteCooldownNumeric.Size = new System.Drawing.Size(40, 20);
            this.NoteCooldownNumeric.TabIndex = 15;
            this.NoteCooldownNumeric.ValueChanged += new System.EventHandler(this.NoteCooldownNumeric_ValueChanged);
            // 
            // NoteCountNumeric
            // 
            this.NoteCountNumeric.Location = new System.Drawing.Point(172, 71);
            this.NoteCountNumeric.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.NoteCountNumeric.Name = "NoteCountNumeric";
            this.NoteCountNumeric.Size = new System.Drawing.Size(40, 20);
            this.NoteCountNumeric.TabIndex = 14;
            this.NoteCountNumeric.ValueChanged += new System.EventHandler(this.NoteCountNumeric_ValueChanged);
            // 
            // NoteCountLabel
            // 
            this.NoteCountLabel.AutoSize = true;
            this.NoteCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NoteCountLabel.Location = new System.Drawing.Point(6, 71);
            this.NoteCountLabel.Name = "NoteCountLabel";
            this.NoteCountLabel.Size = new System.Drawing.Size(87, 17);
            this.NoteCountLabel.TabIndex = 13;
            this.NoteCountLabel.Text = "Note Count :";
            // 
            // LowestNoteLabel
            // 
            this.LowestNoteLabel.AutoSize = true;
            this.LowestNoteLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LowestNoteLabel.Location = new System.Drawing.Point(6, 45);
            this.LowestNoteLabel.Name = "LowestNoteLabel";
            this.LowestNoteLabel.Size = new System.Drawing.Size(109, 17);
            this.LowestNoteLabel.TabIndex = 12;
            this.LowestNoteLabel.Text = "Lowest Note id :";
            // 
            // LowestNoteNumeric
            // 
            this.LowestNoteNumeric.Location = new System.Drawing.Point(172, 45);
            this.LowestNoteNumeric.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.LowestNoteNumeric.Name = "LowestNoteNumeric";
            this.LowestNoteNumeric.Size = new System.Drawing.Size(40, 20);
            this.LowestNoteNumeric.TabIndex = 11;
            this.LowestNoteNumeric.ValueChanged += new System.EventHandler(this.LowestNoteNumeric_ValueChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 395);
            this.Controls.Add(this.AdvancedGroupBox);
            this.Controls.Add(this.SettingsGroupBox);
            this.Controls.Add(this.ApplyButton);
            this.Controls.Add(this.ReturnButton);
            this.Controls.Add(this.UpdateLinkLabel);
            this.Controls.Add(this.VersionLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.SettingsGroupBox.ResumeLayout(false);
            this.SettingsGroupBox.PerformLayout();
            this.AdvancedGroupBox.ResumeLayout(false);
            this.AdvancedGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NoteCooldownNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NoteCountNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LowestNoteNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.LinkLabel UpdateLinkLabel;
        private System.Windows.Forms.CheckBox PlaylistCheckBox;
        private System.Windows.Forms.CheckBox SoundBoardCheckBox;
        private System.Windows.Forms.Button ReturnButton;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.CheckBox SoundEffectsCheckBox;
        private System.Windows.Forms.CheckBox TrackSelectionCheckBox;
        private System.Windows.Forms.CheckBox OnlineSyncCheckBox;
        private System.Windows.Forms.ComboBox NoteConversionMode;
        private System.Windows.Forms.Label NoteConversionLabel;
        private System.Windows.Forms.GroupBox SettingsGroupBox;
        private System.Windows.Forms.GroupBox AdvancedGroupBox;
        private System.Windows.Forms.Label LowestNoteLabel;
        private System.Windows.Forms.NumericUpDown LowestNoteNumeric;
        private System.Windows.Forms.NumericUpDown NoteCountNumeric;
        private System.Windows.Forms.Label NoteCountLabel;
        private System.Windows.Forms.Label NoteCooldownLabel;
        private System.Windows.Forms.NumericUpDown NoteCooldownNumeric;
        private System.Windows.Forms.RadioButton NewAutoConsoleRadio;
        private System.Windows.Forms.RadioButton OldAutoConsoleRadio;
        private System.Windows.Forms.Label AutoConsoleLabel;
        private System.Windows.Forms.RadioButton OffAutoConsoleRadio;
        private System.Windows.Forms.CheckBox LiveMidiCheckBox;
    }
}