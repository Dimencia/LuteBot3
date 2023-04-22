using LuteBot.UI.Utils;

namespace LuteBot
{
    partial class PartitionsForm
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
            this.savePartitionButton = new System.Windows.Forms.Button();
            this.openMidiFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.editSelectedButton = new System.Windows.Forms.Button();
            this.reloadSelectedButton = new System.Windows.Forms.Button();
            this.exportSelectedButton = new System.Windows.Forms.Button();
            this.renameSelectedButton = new System.Windows.Forms.Button();
            this.listBoxPartitions = new LuteBot.UI.Utils.CustomListBox();
            this.SuspendLayout();
            // 
            // savePartitionButton
            // 
            this.savePartitionButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.savePartitionButton.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.savePartitionButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.savePartitionButton.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ButtonFace;
            this.savePartitionButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonShadow;
            this.savePartitionButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.savePartitionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.savePartitionButton.Location = new System.Drawing.Point(12, 6);
            this.savePartitionButton.Name = "savePartitionButton";
            this.savePartitionButton.Size = new System.Drawing.Size(448, 26);
            this.savePartitionButton.TabIndex = 2;
            this.savePartitionButton.Text = "Add Midis";
            this.savePartitionButton.UseVisualStyleBackColor = false;
            this.savePartitionButton.Click += new System.EventHandler(this.saveMultipleSongsToolStripMenuItem_Click);
            // 
            // openMidiFileDialog
            // 
            this.openMidiFileDialog.Filter = "MIDI files|*.mid";
            this.openMidiFileDialog.Multiselect = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 16);
            this.label1.TabIndex = 3;
            this.label1.Text = "LuteMod Songs";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Location = new System.Drawing.Point(12, 39);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(448, 4);
            this.panel1.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label2.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label2.Location = new System.Drawing.Point(331, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 16);
            this.label2.TabIndex = 14;
            this.label2.Text = "Or Drag and Drop";
            // 
            // editSelectedButton
            // 
            this.editSelectedButton.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.editSelectedButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.editSelectedButton.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ButtonFace;
            this.editSelectedButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonShadow;
            this.editSelectedButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.editSelectedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.editSelectedButton.Location = new System.Drawing.Point(154, 47);
            this.editSelectedButton.Name = "editSelectedButton";
            this.editSelectedButton.Size = new System.Drawing.Size(53, 39);
            this.editSelectedButton.TabIndex = 15;
            this.editSelectedButton.Text = "Edit";
            this.editSelectedButton.UseVisualStyleBackColor = false;
            this.editSelectedButton.Click += new System.EventHandler(this.EditItem_Click);
            // 
            // reloadSelectedButton
            // 
            this.reloadSelectedButton.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.reloadSelectedButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.reloadSelectedButton.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ButtonFace;
            this.reloadSelectedButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonShadow;
            this.reloadSelectedButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.reloadSelectedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.reloadSelectedButton.Location = new System.Drawing.Point(213, 47);
            this.reloadSelectedButton.Name = "reloadSelectedButton";
            this.reloadSelectedButton.Size = new System.Drawing.Size(79, 39);
            this.reloadSelectedButton.TabIndex = 16;
            this.reloadSelectedButton.Text = "Resave";
            this.reloadSelectedButton.UseVisualStyleBackColor = false;
            this.reloadSelectedButton.Click += new System.EventHandler(this.reloadSelectedButton_Click);
            // 
            // exportSelectedButton
            // 
            this.exportSelectedButton.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.exportSelectedButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.exportSelectedButton.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ButtonFace;
            this.exportSelectedButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonShadow;
            this.exportSelectedButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.exportSelectedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exportSelectedButton.Location = new System.Drawing.Point(298, 47);
            this.exportSelectedButton.Name = "exportSelectedButton";
            this.exportSelectedButton.Size = new System.Drawing.Size(79, 39);
            this.exportSelectedButton.TabIndex = 17;
            this.exportSelectedButton.Text = "Export";
            this.exportSelectedButton.UseVisualStyleBackColor = false;
            this.exportSelectedButton.Click += new System.EventHandler(this.ExportItem_Click);
            // 
            // renameSelectedButton
            // 
            this.renameSelectedButton.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.renameSelectedButton.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.renameSelectedButton.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.ButtonFace;
            this.renameSelectedButton.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.ButtonShadow;
            this.renameSelectedButton.FlatAppearance.MouseOverBackColor = System.Drawing.SystemColors.ControlLight;
            this.renameSelectedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.renameSelectedButton.Location = new System.Drawing.Point(383, 47);
            this.renameSelectedButton.Name = "renameSelectedButton";
            this.renameSelectedButton.Size = new System.Drawing.Size(79, 39);
            this.renameSelectedButton.TabIndex = 18;
            this.renameSelectedButton.Text = "Rename";
            this.renameSelectedButton.UseVisualStyleBackColor = false;
            this.renameSelectedButton.Click += new System.EventHandler(this.renameSelectedButton_Click);
            // 
            // listBoxPartitions
            // 
            this.listBoxPartitions.AllowDrop = true;
            this.listBoxPartitions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxPartitions.FormattingEnabled = true;
            this.listBoxPartitions.ItemHeight = 16;
            this.listBoxPartitions.Location = new System.Drawing.Point(12, 93);
            this.listBoxPartitions.Name = "listBoxPartitions";
            this.listBoxPartitions.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxPartitions.Size = new System.Drawing.Size(448, 484);
            this.listBoxPartitions.TabIndex = 0;
            this.listBoxPartitions.DragDrop += new System.Windows.Forms.DragEventHandler(this.PartitionIndexBox_DragDrop);
            this.listBoxPartitions.DragOver += new System.Windows.Forms.DragEventHandler(this.PartitionIndexBox_DragOver);
            this.listBoxPartitions.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PartitionIndexBox_MouseDown);
            // 
            // PartitionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(475, 588);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.renameSelectedButton);
            this.Controls.Add(this.exportSelectedButton);
            this.Controls.Add(this.reloadSelectedButton);
            this.Controls.Add(this.editSelectedButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.savePartitionButton);
            this.Controls.Add(this.listBoxPartitions);
            this.Name = "PartitionsForm";
            this.Text = "LuteMod Partitions";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PartitionsForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CustomListBox listBoxPartitions;
        private System.Windows.Forms.OpenFileDialog openMidiFileDialog;
        public System.Windows.Forms.Button savePartitionButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button editSelectedButton;
        private System.Windows.Forms.Button reloadSelectedButton;
        private System.Windows.Forms.Button exportSelectedButton;
        private System.Windows.Forms.Button renameSelectedButton;
    }
}