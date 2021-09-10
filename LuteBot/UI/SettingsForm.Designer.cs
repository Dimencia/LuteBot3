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
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.LiveMidiCheckBox = new System.Windows.Forms.CheckBox();
            this.OffAutoConsoleRadio = new System.Windows.Forms.RadioButton();
            this.AutoConsoleLabel = new System.Windows.Forms.Label();
            this.OldAutoConsoleRadio = new System.Windows.Forms.RadioButton();
            this.NewAutoConsoleRadio = new System.Windows.Forms.RadioButton();
            this.AdvancedGroupBox = new System.Windows.Forms.GroupBox();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.instrumentsBox = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.NotesPerChordNumeric = new System.Windows.Forms.NumericUpDown();
            this.LowestNoteLabel = new System.Windows.Forms.Label();
            this.LowestNoteNumeric = new System.Windows.Forms.NumericUpDown();
            this.NoteCountLabel = new System.Windows.Forms.Label();
            this.NoteCooldownLabel = new System.Windows.Forms.Label();
            this.NoteCountNumeric = new System.Windows.Forms.NumericUpDown();
            this.NoteCooldownNumeric = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.outputDeviceBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.guildLabel = new System.Windows.Forms.LinkLabel();
            this.SettingsGroupBox.SuspendLayout();
            this.AdvancedGroupBox.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NotesPerChordNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LowestNoteNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NoteCountNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NoteCooldownNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Location = new System.Drawing.Point(-1, 629);
            this.VersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(294, 17);
            this.VersionLabel.TabIndex = 0;
            this.VersionLabel.Text = "Mordhau Lute Bot [VERSION] made by Monty";
            // 
            // UpdateLinkLabel
            // 
            this.UpdateLinkLabel.AutoSize = true;
            this.UpdateLinkLabel.Location = new System.Drawing.Point(305, 629);
            this.UpdateLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.UpdateLinkLabel.Name = "UpdateLinkLabel";
            this.UpdateLinkLabel.Size = new System.Drawing.Size(238, 17);
            this.UpdateLinkLabel.TabIndex = 1;
            this.UpdateLinkLabel.TabStop = true;
            this.UpdateLinkLabel.Text = "you have the latest version avaliable";
            this.UpdateLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.UpdateLinkLabel_LinkClicked);
            // 
            // PlaylistCheckBox
            // 
            this.PlaylistCheckBox.AutoSize = true;
            this.PlaylistCheckBox.Location = new System.Drawing.Point(8, 23);
            this.PlaylistCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.PlaylistCheckBox.Name = "PlaylistCheckBox";
            this.PlaylistCheckBox.Size = new System.Drawing.Size(221, 21);
            this.PlaylistCheckBox.TabIndex = 2;
            this.PlaylistCheckBox.Text = "Show Playlist Menu on Startup";
            this.PlaylistCheckBox.UseVisualStyleBackColor = true;
            this.PlaylistCheckBox.CheckedChanged += new System.EventHandler(this.PlaylistCheckBox_CheckedChanged);
            // 
            // SoundBoardCheckBox
            // 
            this.SoundBoardCheckBox.AutoSize = true;
            this.SoundBoardCheckBox.Location = new System.Drawing.Point(8, 52);
            this.SoundBoardCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.SoundBoardCheckBox.Name = "SoundBoardCheckBox";
            this.SoundBoardCheckBox.Size = new System.Drawing.Size(255, 21);
            this.SoundBoardCheckBox.TabIndex = 3;
            this.SoundBoardCheckBox.Text = "Show Soundboard Menu on Startup";
            this.SoundBoardCheckBox.UseVisualStyleBackColor = true;
            this.SoundBoardCheckBox.CheckedChanged += new System.EventHandler(this.SoundBoardCheckBox_CheckedChanged);
            // 
            // ReturnButton
            // 
            this.ReturnButton.Location = new System.Drawing.Point(13, 588);
            this.ReturnButton.Margin = new System.Windows.Forms.Padding(4);
            this.ReturnButton.Name = "ReturnButton";
            this.ReturnButton.Size = new System.Drawing.Size(160, 37);
            this.ReturnButton.TabIndex = 4;
            this.ReturnButton.Text = "Cancel";
            this.ReturnButton.UseVisualStyleBackColor = true;
            this.ReturnButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ApplyButton
            // 
            this.ApplyButton.Location = new System.Drawing.Point(369, 588);
            this.ApplyButton.Margin = new System.Windows.Forms.Padding(4);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Size = new System.Drawing.Size(160, 37);
            this.ApplyButton.TabIndex = 5;
            this.ApplyButton.Text = "Apply";
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // SoundEffectsCheckBox
            // 
            this.SoundEffectsCheckBox.AutoSize = true;
            this.SoundEffectsCheckBox.Location = new System.Drawing.Point(8, 165);
            this.SoundEffectsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.SoundEffectsCheckBox.Name = "SoundEffectsCheckBox";
            this.SoundEffectsCheckBox.Size = new System.Drawing.Size(328, 21);
            this.SoundEffectsCheckBox.TabIndex = 6;
            this.SoundEffectsCheckBox.Text = "Rust Mode - Requires                 Port as Output";
            this.SoundEffectsCheckBox.UseVisualStyleBackColor = true;
            this.SoundEffectsCheckBox.CheckedChanged += new System.EventHandler(this.SoundEffectsCheckBox_CheckedChanged);
            // 
            // TrackSelectionCheckBox
            // 
            this.TrackSelectionCheckBox.AutoSize = true;
            this.TrackSelectionCheckBox.Location = new System.Drawing.Point(8, 80);
            this.TrackSelectionCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.TrackSelectionCheckBox.Name = "TrackSelectionCheckBox";
            this.TrackSelectionCheckBox.Size = new System.Drawing.Size(275, 21);
            this.TrackSelectionCheckBox.TabIndex = 7;
            this.TrackSelectionCheckBox.Text = "Show Track Selection Menu on Startup";
            this.TrackSelectionCheckBox.UseVisualStyleBackColor = true;
            this.TrackSelectionCheckBox.CheckedChanged += new System.EventHandler(this.TrackSelectionCheckBox_CheckedChanged);
            // 
            // OnlineSyncCheckBox
            // 
            this.OnlineSyncCheckBox.AutoSize = true;
            this.OnlineSyncCheckBox.Enabled = false;
            this.OnlineSyncCheckBox.Location = new System.Drawing.Point(8, 108);
            this.OnlineSyncCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.OnlineSyncCheckBox.Name = "OnlineSyncCheckBox";
            this.OnlineSyncCheckBox.Size = new System.Drawing.Size(253, 21);
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
            this.NoteConversionMode.Location = new System.Drawing.Point(229, 18);
            this.NoteConversionMode.Margin = new System.Windows.Forms.Padding(4);
            this.NoteConversionMode.Name = "NoteConversionMode";
            this.NoteConversionMode.Size = new System.Drawing.Size(277, 24);
            this.NoteConversionMode.TabIndex = 9;
            this.NoteConversionMode.SelectedIndexChanged += new System.EventHandler(this.NoteConversionMode_SelectedIndexChanged);
            // 
            // NoteConversionLabel
            // 
            this.NoteConversionLabel.AutoSize = true;
            this.NoteConversionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NoteConversionLabel.Location = new System.Drawing.Point(8, 20);
            this.NoteConversionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.NoteConversionLabel.Name = "NoteConversionLabel";
            this.NoteConversionLabel.Size = new System.Drawing.Size(189, 20);
            this.NoteConversionLabel.TabIndex = 10;
            this.NoteConversionLabel.Text = "Note Conversion Mode :";
            // 
            // SettingsGroupBox
            // 
            this.SettingsGroupBox.Controls.Add(this.linkLabel1);
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
            this.SettingsGroupBox.Location = new System.Drawing.Point(13, 15);
            this.SettingsGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.SettingsGroupBox.Name = "SettingsGroupBox";
            this.SettingsGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.SettingsGroupBox.Size = new System.Drawing.Size(516, 249);
            this.SettingsGroupBox.TabIndex = 12;
            this.SettingsGroupBox.TabStop = false;
            this.SettingsGroupBox.Text = "Settings";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(172, 166);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(65, 17);
            this.linkLabel1.TabIndex = 15;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "LoopMidi";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked_1);
            // 
            // LiveMidiCheckBox
            // 
            this.LiveMidiCheckBox.AutoSize = true;
            this.LiveMidiCheckBox.Location = new System.Drawing.Point(8, 137);
            this.LiveMidiCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.LiveMidiCheckBox.Name = "LiveMidiCheckBox";
            this.LiveMidiCheckBox.Size = new System.Drawing.Size(267, 21);
            this.LiveMidiCheckBox.TabIndex = 14;
            this.LiveMidiCheckBox.Text = "Show Live Midi Input Menu on Startup\r\n";
            this.LiveMidiCheckBox.UseVisualStyleBackColor = true;
            this.LiveMidiCheckBox.CheckedChanged += new System.EventHandler(this.LiveMidiCheckBox_CheckedChanged);
            // 
            // OffAutoConsoleRadio
            // 
            this.OffAutoConsoleRadio.AutoSize = true;
            this.OffAutoConsoleRadio.Location = new System.Drawing.Point(139, 210);
            this.OffAutoConsoleRadio.Margin = new System.Windows.Forms.Padding(4);
            this.OffAutoConsoleRadio.Name = "OffAutoConsoleRadio";
            this.OffAutoConsoleRadio.Size = new System.Drawing.Size(48, 21);
            this.OffAutoConsoleRadio.TabIndex = 13;
            this.OffAutoConsoleRadio.TabStop = true;
            this.OffAutoConsoleRadio.Text = "Off";
            this.OffAutoConsoleRadio.UseVisualStyleBackColor = true;
            this.OffAutoConsoleRadio.CheckedChanged += new System.EventHandler(this.OffAutoConsoleRadio_CheckedChanged);
            // 
            // AutoConsoleLabel
            // 
            this.AutoConsoleLabel.AutoSize = true;
            this.AutoConsoleLabel.Location = new System.Drawing.Point(4, 190);
            this.AutoConsoleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.AutoConsoleLabel.Name = "AutoConsoleLabel";
            this.AutoConsoleLabel.Size = new System.Drawing.Size(183, 17);
            this.AutoConsoleLabel.TabIndex = 12;
            this.AutoConsoleLabel.Text = "Automatic Console System :";
            // 
            // OldAutoConsoleRadio
            // 
            this.OldAutoConsoleRadio.AutoSize = true;
            this.OldAutoConsoleRadio.Location = new System.Drawing.Point(4, 210);
            this.OldAutoConsoleRadio.Margin = new System.Windows.Forms.Padding(4);
            this.OldAutoConsoleRadio.Name = "OldAutoConsoleRadio";
            this.OldAutoConsoleRadio.Size = new System.Drawing.Size(51, 21);
            this.OldAutoConsoleRadio.TabIndex = 11;
            this.OldAutoConsoleRadio.TabStop = true;
            this.OldAutoConsoleRadio.Text = "Old";
            this.OldAutoConsoleRadio.UseVisualStyleBackColor = true;
            this.OldAutoConsoleRadio.CheckedChanged += new System.EventHandler(this.OldAutoConsoleRadio_CheckedChanged);
            // 
            // NewAutoConsoleRadio
            // 
            this.NewAutoConsoleRadio.AutoSize = true;
            this.NewAutoConsoleRadio.Location = new System.Drawing.Point(67, 210);
            this.NewAutoConsoleRadio.Margin = new System.Windows.Forms.Padding(4);
            this.NewAutoConsoleRadio.Name = "NewAutoConsoleRadio";
            this.NewAutoConsoleRadio.Size = new System.Drawing.Size(56, 21);
            this.NewAutoConsoleRadio.TabIndex = 10;
            this.NewAutoConsoleRadio.TabStop = true;
            this.NewAutoConsoleRadio.Text = "New";
            this.NewAutoConsoleRadio.UseVisualStyleBackColor = true;
            this.NewAutoConsoleRadio.CheckedChanged += new System.EventHandler(this.NewAutoConsoleRadio_CheckedChanged);
            // 
            // AdvancedGroupBox
            // 
            this.AdvancedGroupBox.Controls.Add(this.linkLabel2);
            this.AdvancedGroupBox.Controls.Add(this.label2);
            this.AdvancedGroupBox.Controls.Add(this.instrumentsBox);
            this.AdvancedGroupBox.Controls.Add(this.panel1);
            this.AdvancedGroupBox.Controls.Add(this.label1);
            this.AdvancedGroupBox.Controls.Add(this.NoteConversionLabel);
            this.AdvancedGroupBox.Controls.Add(this.outputDeviceBox);
            this.AdvancedGroupBox.Controls.Add(this.NoteConversionMode);
            this.AdvancedGroupBox.Location = new System.Drawing.Point(13, 272);
            this.AdvancedGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.AdvancedGroupBox.Name = "AdvancedGroupBox";
            this.AdvancedGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.AdvancedGroupBox.Size = new System.Drawing.Size(516, 289);
            this.AdvancedGroupBox.TabIndex = 13;
            this.AdvancedGroupBox.TabStop = false;
            this.AdvancedGroupBox.Text = "Advanced Parameters";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.LinkArea = new System.Windows.Forms.LinkArea(22, 8);
            this.linkLabel2.Location = new System.Drawing.Point(17, 85);
            this.linkLabel2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(267, 20);
            this.linkLabel2.TabIndex = 20;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "Output must be set to LoopMidi port for Rust";
            this.linkLabel2.UseCompatibleTextRendering = true;
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel2_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(8, 110);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(194, 20);
            this.label2.TabIndex = 18;
            this.label2.Text = "Select Instrument Prefab";
            // 
            // instrumentsBox
            // 
            this.instrumentsBox.FormattingEnabled = true;
            this.instrumentsBox.Location = new System.Drawing.Point(229, 108);
            this.instrumentsBox.Margin = new System.Windows.Forms.Padding(4);
            this.instrumentsBox.Name = "instrumentsBox";
            this.instrumentsBox.Size = new System.Drawing.Size(277, 24);
            this.instrumentsBox.TabIndex = 17;
            this.instrumentsBox.SelectedIndexChanged += new System.EventHandler(this.InstrumentsBox_SelectedIndexChanged);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.NotesPerChordNumeric);
            this.panel1.Controls.Add(this.LowestNoteLabel);
            this.panel1.Controls.Add(this.LowestNoteNumeric);
            this.panel1.Controls.Add(this.NoteCountLabel);
            this.panel1.Controls.Add(this.NoteCooldownLabel);
            this.panel1.Controls.Add(this.NoteCountNumeric);
            this.panel1.Controls.Add(this.NoteCooldownNumeric);
            this.panel1.Location = new System.Drawing.Point(8, 138);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(499, 144);
            this.panel1.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(8, 106);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(254, 25);
            this.label4.TabIndex = 18;
            this.label4.Text = "Default Notes Per Chord :";
            // 
            // NotesPerChordNumeric
            // 
            this.NotesPerChordNumeric.Location = new System.Drawing.Point(229, 106);
            this.NotesPerChordNumeric.Margin = new System.Windows.Forms.Padding(4);
            this.NotesPerChordNumeric.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.NotesPerChordNumeric.Name = "NotesPerChordNumeric";
            this.NotesPerChordNumeric.Size = new System.Drawing.Size(53, 22);
            this.NotesPerChordNumeric.TabIndex = 17;
            this.NotesPerChordNumeric.ValueChanged += new System.EventHandler(this.NotesPerChordNumeric_ValueChanged);
            // 
            // LowestNoteLabel
            // 
            this.LowestNoteLabel.AutoSize = true;
            this.LowestNoteLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LowestNoteLabel.Location = new System.Drawing.Point(8, 11);
            this.LowestNoteLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LowestNoteLabel.Name = "LowestNoteLabel";
            this.LowestNoteLabel.Size = new System.Drawing.Size(131, 20);
            this.LowestNoteLabel.TabIndex = 12;
            this.LowestNoteLabel.Text = "Lowest Note id :";
            // 
            // LowestNoteNumeric
            // 
            this.LowestNoteNumeric.Location = new System.Drawing.Point(229, 11);
            this.LowestNoteNumeric.Margin = new System.Windows.Forms.Padding(4);
            this.LowestNoteNumeric.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.LowestNoteNumeric.Name = "LowestNoteNumeric";
            this.LowestNoteNumeric.Size = new System.Drawing.Size(53, 22);
            this.LowestNoteNumeric.TabIndex = 11;
            this.LowestNoteNumeric.ValueChanged += new System.EventHandler(this.LowestNoteNumeric_ValueChanged);
            // 
            // NoteCountLabel
            // 
            this.NoteCountLabel.AutoSize = true;
            this.NoteCountLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NoteCountLabel.Location = new System.Drawing.Point(8, 43);
            this.NoteCountLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.NoteCountLabel.Name = "NoteCountLabel";
            this.NoteCountLabel.Size = new System.Drawing.Size(103, 20);
            this.NoteCountLabel.TabIndex = 13;
            this.NoteCountLabel.Text = "Note Count :";
            // 
            // NoteCooldownLabel
            // 
            this.NoteCooldownLabel.AutoSize = true;
            this.NoteCooldownLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NoteCooldownLabel.Location = new System.Drawing.Point(8, 75);
            this.NoteCooldownLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.NoteCooldownLabel.Name = "NoteCooldownLabel";
            this.NoteCooldownLabel.Size = new System.Drawing.Size(132, 20);
            this.NoteCooldownLabel.TabIndex = 16;
            this.NoteCooldownLabel.Text = "Note Cooldown :";
            // 
            // NoteCountNumeric
            // 
            this.NoteCountNumeric.Location = new System.Drawing.Point(229, 43);
            this.NoteCountNumeric.Margin = new System.Windows.Forms.Padding(4);
            this.NoteCountNumeric.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.NoteCountNumeric.Name = "NoteCountNumeric";
            this.NoteCountNumeric.Size = new System.Drawing.Size(53, 22);
            this.NoteCountNumeric.TabIndex = 14;
            this.NoteCountNumeric.ValueChanged += new System.EventHandler(this.NoteCountNumeric_ValueChanged);
            // 
            // NoteCooldownNumeric
            // 
            this.NoteCooldownNumeric.Location = new System.Drawing.Point(229, 75);
            this.NoteCooldownNumeric.Margin = new System.Windows.Forms.Padding(4);
            this.NoteCooldownNumeric.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.NoteCooldownNumeric.Name = "NoteCooldownNumeric";
            this.NoteCooldownNumeric.Size = new System.Drawing.Size(53, 22);
            this.NoteCooldownNumeric.TabIndex = 15;
            this.NoteCooldownNumeric.ValueChanged += new System.EventHandler(this.NoteCooldownNumeric_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(8, 55);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 20);
            this.label1.TabIndex = 18;
            this.label1.Text = "Output Device :";
            // 
            // outputDeviceBox
            // 
            this.outputDeviceBox.FormattingEnabled = true;
            this.outputDeviceBox.Location = new System.Drawing.Point(229, 55);
            this.outputDeviceBox.Margin = new System.Windows.Forms.Padding(4);
            this.outputDeviceBox.Name = "outputDeviceBox";
            this.outputDeviceBox.Size = new System.Drawing.Size(277, 24);
            this.outputDeviceBox.TabIndex = 17;
            this.outputDeviceBox.SelectedIndexChanged += new System.EventHandler(this.OutputDeviceBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 645);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 17);
            this.label3.TabIndex = 14;
            this.label3.Text = "Modified by D.Mentia";
            // 
            // guildLabel
            // 
            this.guildLabel.AutoSize = true;
            this.guildLabel.LinkArea = new System.Windows.Forms.LinkArea(15, 31);
            this.guildLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.guildLabel.Location = new System.Drawing.Point(0, 661);
            this.guildLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.guildLabel.Name = "guildLabel";
            this.guildLabel.Size = new System.Drawing.Size(197, 20);
            this.guildLabel.TabIndex = 15;
            this.guildLabel.TabStop = true;
            this.guildLabel.Text = "With help from THE Bard\'s Guild";
            this.guildLabel.UseCompatibleTextRendering = true;
            this.guildLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 688);
            this.Controls.Add(this.guildLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.AdvancedGroupBox);
            this.Controls.Add(this.SettingsGroupBox);
            this.Controls.Add(this.ApplyButton);
            this.Controls.Add(this.ReturnButton);
            this.Controls.Add(this.UpdateLinkLabel);
            this.Controls.Add(this.VersionLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.SettingsGroupBox.ResumeLayout(false);
            this.SettingsGroupBox.PerformLayout();
            this.AdvancedGroupBox.ResumeLayout(false);
            this.AdvancedGroupBox.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NotesPerChordNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LowestNoteNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NoteCountNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NoteCooldownNumeric)).EndInit();
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox outputDeviceBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox instrumentsBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel guildLabel;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown NotesPerChordNumeric;
    }
}