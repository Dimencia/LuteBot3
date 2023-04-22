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
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkInstallUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setMordhauPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lutemodPartitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installLuteModToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.importPartitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportPartitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportMidisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSavFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMidiFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trainToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.focusToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.guildLibraryToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.buttonConsoleToggle = new System.Windows.Forms.Button();
            this.consoleDisplayPanel = new System.Windows.Forms.Panel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.partitionPanel = new System.Windows.Forms.Panel();
            this.menuStrip1.SuspendLayout();
            this.consoleDisplayPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem,
            this.lutemodPartitionsToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.guildLibraryToolStripMenuItem1,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(626, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem,
            this.checkInstallUpdatesToolStripMenuItem,
            this.setMordhauPathToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(75, 24);
            this.optionsToolStripMenuItem.Text = "Options";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // checkInstallUpdatesToolStripMenuItem
            // 
            this.checkInstallUpdatesToolStripMenuItem.Name = "checkInstallUpdatesToolStripMenuItem";
            this.checkInstallUpdatesToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.checkInstallUpdatesToolStripMenuItem.Text = "Check/Install Updates";
            this.checkInstallUpdatesToolStripMenuItem.Click += new System.EventHandler(this.checkInstallUpdatesToolStripMenuItem_Click);
            // 
            // setMordhauPathToolStripMenuItem
            // 
            this.setMordhauPathToolStripMenuItem.Name = "setMordhauPathToolStripMenuItem";
            this.setMordhauPathToolStripMenuItem.Size = new System.Drawing.Size(235, 26);
            this.setMordhauPathToolStripMenuItem.Text = "Set Mordhau Path";
            this.setMordhauPathToolStripMenuItem.Click += new System.EventHandler(this.setMordhauPathToolStripMenuItem_Click);
            // 
            // lutemodPartitionsToolStripMenuItem
            // 
            this.lutemodPartitionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.installLuteModToolStripMenuItem,
            this.toolStripSeparator1,
            this.importPartitionsToolStripMenuItem,
            this.exportPartitionsToolStripMenuItem,
            this.exportMidisToolStripMenuItem,
            this.openSavFolderToolStripMenuItem,
            this.openMidiFolderToolStripMenuItem,
            this.trainToolStripMenuItem});
            this.lutemodPartitionsToolStripMenuItem.Name = "lutemodPartitionsToolStripMenuItem";
            this.lutemodPartitionsToolStripMenuItem.Size = new System.Drawing.Size(82, 24);
            this.lutemodPartitionsToolStripMenuItem.Text = "LuteMod";
            this.lutemodPartitionsToolStripMenuItem.Click += new System.EventHandler(this.lutemodPartitionsToolStripMenuItem_Click);
            // 
            // installLuteModToolStripMenuItem
            // 
            this.installLuteModToolStripMenuItem.Name = "installLuteModToolStripMenuItem";
            this.installLuteModToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.installLuteModToolStripMenuItem.Text = "Install LuteMod";
            this.installLuteModToolStripMenuItem.Click += new System.EventHandler(this.installLuteModToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(221, 6);
            // 
            // importPartitionsToolStripMenuItem
            // 
            this.importPartitionsToolStripMenuItem.Name = "importPartitionsToolStripMenuItem";
            this.importPartitionsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.importPartitionsToolStripMenuItem.Text = "Import Songs";
            this.importPartitionsToolStripMenuItem.Click += new System.EventHandler(this.importPartitionsToolStripMenuItem_Click);
            // 
            // exportPartitionsToolStripMenuItem
            // 
            this.exportPartitionsToolStripMenuItem.Name = "exportPartitionsToolStripMenuItem";
            this.exportPartitionsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.exportPartitionsToolStripMenuItem.Text = "Export Songs";
            this.exportPartitionsToolStripMenuItem.Click += new System.EventHandler(this.exportPartitionsToolStripMenuItem_Click);
            // 
            // exportMidisToolStripMenuItem
            // 
            this.exportMidisToolStripMenuItem.Name = "exportMidisToolStripMenuItem";
            this.exportMidisToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.exportMidisToolStripMenuItem.Text = "Export Midis";
            this.exportMidisToolStripMenuItem.Click += new System.EventHandler(this.exportMidisToolStripMenuItem_Click);
            // 
            // openSavFolderToolStripMenuItem
            // 
            this.openSavFolderToolStripMenuItem.Name = "openSavFolderToolStripMenuItem";
            this.openSavFolderToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.openSavFolderToolStripMenuItem.Text = "Open Sav Folder";
            this.openSavFolderToolStripMenuItem.Click += new System.EventHandler(this.openSavFolderToolStripMenuItem_Click);
            // 
            // openMidiFolderToolStripMenuItem
            // 
            this.openMidiFolderToolStripMenuItem.Name = "openMidiFolderToolStripMenuItem";
            this.openMidiFolderToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.openMidiFolderToolStripMenuItem.Text = "Open Midi Folder";
            this.openMidiFolderToolStripMenuItem.Click += new System.EventHandler(this.openMidiFolderToolStripMenuItem_Click);
            // 
            // trainToolStripMenuItem
            // 
            this.trainToolStripMenuItem.Name = "trainToolStripMenuItem";
            this.trainToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.trainToolStripMenuItem.Text = "Train...";
            this.trainToolStripMenuItem.Click += new System.EventHandler(this.trainToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem1,
            this.focusToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(78, 24);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(224, 26);
            this.toolStripMenuItem2.Text = "Guild Library";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.GuildLibraryToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(224, 26);
            this.toolStripMenuItem1.Text = "Track Filtering";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.TrackFilteringToolStripMenuItem_Click);
            // 
            // focusToolStripMenuItem
            // 
            this.focusToolStripMenuItem.Name = "focusToolStripMenuItem";
            this.focusToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
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
            // timer1
            // 
            this.timer1.Interval = 250;
            // 
            // buttonConsoleToggle
            // 
            this.buttonConsoleToggle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonConsoleToggle.FlatAppearance.BorderSize = 0;
            this.buttonConsoleToggle.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Gray;
            this.buttonConsoleToggle.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.buttonConsoleToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonConsoleToggle.Location = new System.Drawing.Point(0, 582);
            this.buttonConsoleToggle.Name = "buttonConsoleToggle";
            this.buttonConsoleToggle.Size = new System.Drawing.Size(626, 23);
            this.buttonConsoleToggle.TabIndex = 14;
            this.buttonConsoleToggle.Text = "Console";
            this.buttonConsoleToggle.UseVisualStyleBackColor = true;
            this.buttonConsoleToggle.Click += new System.EventHandler(this.buttonConsoleToggle_Click);
            // 
            // consoleDisplayPanel
            // 
            this.consoleDisplayPanel.Controls.Add(this.richTextBox1);
            this.consoleDisplayPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.consoleDisplayPanel.Location = new System.Drawing.Point(0, 0);
            this.consoleDisplayPanel.Name = "consoleDisplayPanel";
            this.consoleDisplayPanel.Size = new System.Drawing.Size(150, 46);
            this.consoleDisplayPanel.TabIndex = 0;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBox1.Size = new System.Drawing.Size(150, 46);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 28);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.partitionPanel);
            this.splitContainer1.Panel1.Controls.Add(this.buttonConsoleToggle);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.consoleDisplayPanel);
            this.splitContainer1.Panel2Collapsed = true;
            this.splitContainer1.Size = new System.Drawing.Size(626, 605);
            this.splitContainer1.SplitterDistance = 447;
            this.splitContainer1.TabIndex = 0;
            // 
            // partitionPanel
            // 
            this.partitionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.partitionPanel.Location = new System.Drawing.Point(0, 0);
            this.partitionPanel.Name = "partitionPanel";
            this.partitionPanel.Size = new System.Drawing.Size(626, 582);
            this.partitionPanel.TabIndex = 15;
            // 
            // LuteBotForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 633);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 500);
            this.Name = "LuteBotForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Text = "LuteBot";
            this.Click += new System.EventHandler(this.LuteBotForm_Focus);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.consoleDisplayPanel.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private ToolStripMenuItem windowToolStripMenuItem;
        private ToolStripMenuItem focusToolStripMenuItem;
        private ToolTip toolTip1;
        private ToolStripMenuItem guildLibraryToolStripMenuItem1;
        private ToolStripMenuItem lutemodPartitionsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem checkInstallUpdatesToolStripMenuItem;
        private ToolStripMenuItem setMordhauPathToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem importPartitionsToolStripMenuItem;
        private ToolStripMenuItem exportPartitionsToolStripMenuItem;
        private ToolStripMenuItem exportMidisToolStripMenuItem;
        private ToolStripMenuItem openSavFolderToolStripMenuItem;
        private ToolStripMenuItem trainToolStripMenuItem;
        private Button buttonConsoleToggle;
        private Panel consoleDisplayPanel;
        private SplitContainer splitContainer1;
        private Panel partitionPanel;
        private RichTextBox richTextBox1;
        private ToolStripMenuItem installLuteModToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem openMidiFolderToolStripMenuItem;
    }
}

