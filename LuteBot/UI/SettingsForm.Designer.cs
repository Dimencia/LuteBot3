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
            this.ReturnButton = new System.Windows.Forms.Button();
            this.ApplyButton = new System.Windows.Forms.Button();
            this.TrackSelectionCheckBox = new System.Windows.Forms.CheckBox();
            this.PartitionListCheckBox = new System.Windows.Forms.CheckBox();
            this.SettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.AdvancedGroupBox = new System.Windows.Forms.GroupBox();
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
            this.label3 = new System.Windows.Forms.Label();
            this.guildLabel = new System.Windows.Forms.LinkLabel();
            this.checkBoxCheckUpdates = new System.Windows.Forms.CheckBox();
            this.checkBoxMajorUpdates = new System.Windows.Forms.CheckBox();
            this.checkBoxMinorUpdates = new System.Windows.Forms.CheckBox();
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
            this.VersionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Location = new System.Drawing.Point(-1, 448);
            this.VersionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(277, 16);
            this.VersionLabel.TabIndex = 0;
            this.VersionLabel.Text = "Mordhau Lute Bot [VERSION] made by Monty";
            // 
            // ReturnButton
            // 
            this.ReturnButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ReturnButton.Location = new System.Drawing.Point(13, 407);
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
            this.ApplyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ApplyButton.Location = new System.Drawing.Point(369, 407);
            this.ApplyButton.Margin = new System.Windows.Forms.Padding(4);
            this.ApplyButton.Name = "ApplyButton";
            this.ApplyButton.Size = new System.Drawing.Size(160, 37);
            this.ApplyButton.TabIndex = 5;
            this.ApplyButton.Text = "Apply";
            this.ApplyButton.UseVisualStyleBackColor = true;
            this.ApplyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // TrackSelectionCheckBox
            // 
            this.TrackSelectionCheckBox.AutoSize = true;
            this.TrackSelectionCheckBox.Location = new System.Drawing.Point(5, 23);
            this.TrackSelectionCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.TrackSelectionCheckBox.Name = "TrackSelectionCheckBox";
            this.TrackSelectionCheckBox.Size = new System.Drawing.Size(258, 20);
            this.TrackSelectionCheckBox.TabIndex = 7;
            this.TrackSelectionCheckBox.Text = "Show Track Selection Menu on Startup";
            this.TrackSelectionCheckBox.UseVisualStyleBackColor = true;
            this.TrackSelectionCheckBox.CheckedChanged += new System.EventHandler(this.TrackSelectionCheckBox_CheckedChanged);
            // 
            // PartitionListCheckBox
            // 
            this.PartitionListCheckBox.AutoSize = true;
            this.PartitionListCheckBox.Location = new System.Drawing.Point(5, 51);
            this.PartitionListCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.PartitionListCheckBox.Name = "PartitionListCheckBox";
            this.PartitionListCheckBox.Size = new System.Drawing.Size(199, 20);
            this.PartitionListCheckBox.TabIndex = 8;
            this.PartitionListCheckBox.Text = "Show Partition List on Startup";
            this.PartitionListCheckBox.UseVisualStyleBackColor = true;
            this.PartitionListCheckBox.CheckedChanged += new System.EventHandler(this.PartitionListCheckBox_CheckedChanged);
            // 
            // SettingsGroupBox
            // 
            this.SettingsGroupBox.Controls.Add(this.TrackSelectionCheckBox);
            this.SettingsGroupBox.Controls.Add(this.PartitionListCheckBox);
            this.SettingsGroupBox.Location = new System.Drawing.Point(13, 15);
            this.SettingsGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.SettingsGroupBox.Name = "SettingsGroupBox";
            this.SettingsGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.SettingsGroupBox.Size = new System.Drawing.Size(516, 83);
            this.SettingsGroupBox.TabIndex = 12;
            this.SettingsGroupBox.TabStop = false;
            this.SettingsGroupBox.Text = "Settings";
            // 
            // AdvancedGroupBox
            // 
            this.AdvancedGroupBox.Controls.Add(this.label2);
            this.AdvancedGroupBox.Controls.Add(this.instrumentsBox);
            this.AdvancedGroupBox.Controls.Add(this.panel1);
            this.AdvancedGroupBox.Location = new System.Drawing.Point(13, 106);
            this.AdvancedGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.AdvancedGroupBox.Name = "AdvancedGroupBox";
            this.AdvancedGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.AdvancedGroupBox.Size = new System.Drawing.Size(516, 206);
            this.AdvancedGroupBox.TabIndex = 13;
            this.AdvancedGroupBox.TabStop = false;
            this.AdvancedGroupBox.Text = "Advanced Parameters";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(10, 19);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(194, 20);
            this.label2.TabIndex = 18;
            this.label2.Text = "Select Instrument Prefab";
            // 
            // instrumentsBox
            // 
            this.instrumentsBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.instrumentsBox.FormattingEnabled = true;
            this.instrumentsBox.Location = new System.Drawing.Point(231, 17);
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
            this.panel1.Location = new System.Drawing.Point(10, 47);
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
            this.label4.Size = new System.Drawing.Size(203, 20);
            this.label4.TabIndex = 18;
            this.label4.Text = "Default Notes Per Chord :";
            // 
            // NotesPerChordNumeric
            // 
            this.NotesPerChordNumeric.Location = new System.Drawing.Point(246, 106);
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
            this.LowestNoteNumeric.Location = new System.Drawing.Point(246, 11);
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
            this.NoteCountNumeric.Location = new System.Drawing.Point(246, 43);
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
            this.NoteCooldownNumeric.Location = new System.Drawing.Point(246, 75);
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
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 464);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(133, 16);
            this.label3.TabIndex = 14;
            this.label3.Text = "Modified by D.Mentia";
            // 
            // guildLabel
            // 
            this.guildLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.guildLabel.AutoSize = true;
            this.guildLabel.LinkArea = new System.Windows.Forms.LinkArea(15, 31);
            this.guildLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.guildLabel.Location = new System.Drawing.Point(0, 480);
            this.guildLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.guildLabel.Name = "guildLabel";
            this.guildLabel.Size = new System.Drawing.Size(197, 20);
            this.guildLabel.TabIndex = 15;
            this.guildLabel.TabStop = true;
            this.guildLabel.Text = "With help from THE Bard\'s Guild";
            this.guildLabel.UseCompatibleTextRendering = true;
            this.guildLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // checkBoxCheckUpdates
            // 
            this.checkBoxCheckUpdates.AutoSize = true;
            this.checkBoxCheckUpdates.Checked = true;
            this.checkBoxCheckUpdates.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCheckUpdates.Location = new System.Drawing.Point(13, 320);
            this.checkBoxCheckUpdates.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxCheckUpdates.Name = "checkBoxCheckUpdates";
            this.checkBoxCheckUpdates.Size = new System.Drawing.Size(200, 20);
            this.checkBoxCheckUpdates.TabIndex = 16;
            this.checkBoxCheckUpdates.Text = "Check for updates on Startup";
            this.checkBoxCheckUpdates.UseVisualStyleBackColor = true;
            this.checkBoxCheckUpdates.CheckedChanged += new System.EventHandler(this.checkBoxCheckUpdates_CheckedChanged);
            // 
            // checkBoxMajorUpdates
            // 
            this.checkBoxMajorUpdates.AutoSize = true;
            this.checkBoxMajorUpdates.Checked = true;
            this.checkBoxMajorUpdates.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxMajorUpdates.Location = new System.Drawing.Point(13, 349);
            this.checkBoxMajorUpdates.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxMajorUpdates.Name = "checkBoxMajorUpdates";
            this.checkBoxMajorUpdates.Size = new System.Drawing.Size(211, 20);
            this.checkBoxMajorUpdates.TabIndex = 17;
            this.checkBoxMajorUpdates.Text = "Show popup for major updates";
            this.checkBoxMajorUpdates.UseVisualStyleBackColor = true;
            this.checkBoxMajorUpdates.CheckedChanged += new System.EventHandler(this.checkBoxMajorUpdates_CheckedChanged);
            // 
            // checkBoxMinorUpdates
            // 
            this.checkBoxMinorUpdates.AutoSize = true;
            this.checkBoxMinorUpdates.Location = new System.Drawing.Point(13, 377);
            this.checkBoxMinorUpdates.Margin = new System.Windows.Forms.Padding(4);
            this.checkBoxMinorUpdates.Name = "checkBoxMinorUpdates";
            this.checkBoxMinorUpdates.Size = new System.Drawing.Size(210, 20);
            this.checkBoxMinorUpdates.TabIndex = 18;
            this.checkBoxMinorUpdates.Text = "Show popup for minor updates";
            this.checkBoxMinorUpdates.UseVisualStyleBackColor = true;
            this.checkBoxMinorUpdates.CheckedChanged += new System.EventHandler(this.checkBoxMinorUpdates_CheckedChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 507);
            this.Controls.Add(this.checkBoxCheckUpdates);
            this.Controls.Add(this.checkBoxMajorUpdates);
            this.Controls.Add(this.checkBoxMinorUpdates);
            this.Controls.Add(this.guildLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.AdvancedGroupBox);
            this.Controls.Add(this.SettingsGroupBox);
            this.Controls.Add(this.ApplyButton);
            this.Controls.Add(this.ReturnButton);
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
        private System.Windows.Forms.Button ReturnButton;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.CheckBox TrackSelectionCheckBox;
        private System.Windows.Forms.CheckBox PartitionListCheckBox;
        private System.Windows.Forms.GroupBox SettingsGroupBox;
        private System.Windows.Forms.GroupBox AdvancedGroupBox;
        private System.Windows.Forms.Label LowestNoteLabel;
        private System.Windows.Forms.NumericUpDown LowestNoteNumeric;
        private System.Windows.Forms.NumericUpDown NoteCountNumeric;
        private System.Windows.Forms.Label NoteCountLabel;
        private System.Windows.Forms.Label NoteCooldownLabel;
        private System.Windows.Forms.NumericUpDown NoteCooldownNumeric;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox instrumentsBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.LinkLabel guildLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown NotesPerChordNumeric;
        private System.Windows.Forms.CheckBox checkBoxCheckUpdates;
        private System.Windows.Forms.CheckBox checkBoxMajorUpdates;
        private System.Windows.Forms.CheckBox checkBoxMinorUpdates;
    }
}