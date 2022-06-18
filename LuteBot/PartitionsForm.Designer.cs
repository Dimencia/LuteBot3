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
            this.listBoxPartitions = new LuteBot.UI.Utils.CustomListBox();
            this.savePartitionButton = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importPartitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSavFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMultipleSongsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMidiFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxPartitions
            // 
            this.listBoxPartitions.AllowDrop = true;
            this.listBoxPartitions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxPartitions.FormattingEnabled = true;
            this.listBoxPartitions.ItemHeight = 16;
            this.listBoxPartitions.Location = new System.Drawing.Point(12, 65);
            this.listBoxPartitions.Name = "listBoxPartitions";
            this.listBoxPartitions.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxPartitions.Size = new System.Drawing.Size(497, 484);
            this.listBoxPartitions.TabIndex = 0;
            this.listBoxPartitions.DragDrop += new System.Windows.Forms.DragEventHandler(this.PartitionIndexBox_DragDrop);
            this.listBoxPartitions.DragOver += new System.Windows.Forms.DragEventHandler(this.PartitionIndexBox_DragOver);
            this.listBoxPartitions.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PartitionIndexBox_MouseDown);
            // 
            // savePartitionButton
            // 
            this.savePartitionButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.savePartitionButton.Location = new System.Drawing.Point(173, 33);
            this.savePartitionButton.Name = "savePartitionButton";
            this.savePartitionButton.Size = new System.Drawing.Size(155, 26);
            this.savePartitionButton.TabIndex = 2;
            this.savePartitionButton.Text = "Save Current Song";
            this.savePartitionButton.UseVisualStyleBackColor = true;
            this.savePartitionButton.Click += new System.EventHandler(this.savePartitionsButton_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(524, 28);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importPartitionsToolStripMenuItem,
            this.openSavFolderToolStripMenuItem,
            this.saveMultipleSongsToolStripMenuItem,
            this.trainToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(75, 24);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // importPartitionsToolStripMenuItem
            // 
            this.importPartitionsToolStripMenuItem.Name = "importPartitionsToolStripMenuItem";
            this.importPartitionsToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.importPartitionsToolStripMenuItem.Text = "Import Partitions";
            this.importPartitionsToolStripMenuItem.Click += new System.EventHandler(this.button1_Click);
            // 
            // openSavFolderToolStripMenuItem
            // 
            this.openSavFolderToolStripMenuItem.Name = "openSavFolderToolStripMenuItem";
            this.openSavFolderToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.openSavFolderToolStripMenuItem.Text = "Open Sav Folder";
            this.openSavFolderToolStripMenuItem.Click += new System.EventHandler(this.button3_Click);
            // 
            // saveMultipleSongsToolStripMenuItem
            // 
            this.saveMultipleSongsToolStripMenuItem.Name = "saveMultipleSongsToolStripMenuItem";
            this.saveMultipleSongsToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.saveMultipleSongsToolStripMenuItem.Text = "Save Multiple Songs...";
            this.saveMultipleSongsToolStripMenuItem.Click += new System.EventHandler(this.saveMultipleSongsToolStripMenuItem_Click);
            // 
            // trainToolStripMenuItem
            // 
            this.trainToolStripMenuItem.Name = "trainToolStripMenuItem";
            this.trainToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.trainToolStripMenuItem.Text = "Train...";
            this.trainToolStripMenuItem.Click += new System.EventHandler(this.trainToolStripMenuItem_Click);
            // 
            // openMidiFileDialog
            // 
            this.openMidiFileDialog.Filter = "MIDI files|*.mid";
            this.openMidiFileDialog.Multiselect = true;
            // 
            // PartitionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 557);
            this.Controls.Add(this.savePartitionButton);
            this.Controls.Add(this.listBoxPartitions);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "PartitionsForm";
            this.Text = "LuteMod Partitions";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PartitionsForm_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CustomListBox listBoxPartitions;
        private System.Windows.Forms.Button savePartitionButton;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importPartitionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSavFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMultipleSongsToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openMidiFileDialog;
        private System.Windows.Forms.ToolStripMenuItem trainToolStripMenuItem;
    }
}