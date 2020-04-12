using System;
using System.Windows.Forms;

namespace LuteBot
{
    partial class KeyBindingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeyBindingForm));
            this.HotkeysList = new System.Windows.Forms.ListView();
            this.Action = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Key = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.AutoConfigTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mordhauConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.automaticConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.revertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mordhauConfigurationFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setDefaultInputiniLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDefaultInputiniToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MordhauConfigLabel = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // HotkeysList
            // 
            this.HotkeysList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HotkeysList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Action,
            this.Key});
            this.HotkeysList.FullRowSelect = true;
            this.HotkeysList.GridLines = true;
            this.HotkeysList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.HotkeysList.HideSelection = false;
            this.HotkeysList.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.HotkeysList.Location = new System.Drawing.Point(12, 27);
            this.HotkeysList.Name = "HotkeysList";
            this.HotkeysList.Size = new System.Drawing.Size(600, 167);
            this.HotkeysList.TabIndex = 1;
            this.HotkeysList.UseCompatibleStateImageBehavior = false;
            this.HotkeysList.View = System.Windows.Forms.View.Details;
            this.HotkeysList.SelectedIndexChanged += new System.EventHandler(this.HotkeysList_SelectedIndexChanged);
            // 
            // Action
            // 
            this.Action.Text = "Action";
            this.Action.Width = 295;
            // 
            // Key
            // 
            this.Key.Text = "Key";
            this.Key.Width = 300;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(12, 213);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(120, 30);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(492, 213);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(120, 30);
            this.applyButton.TabIndex = 5;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.ApplyButton_Click);
            // 
            // AutoConfigTooltip
            // 
            this.AutoConfigTooltip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mordhauConfigToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(624, 24);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // mordhauConfigToolStripMenuItem
            // 
            this.mordhauConfigToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.automaticConfigurationToolStripMenuItem,
            this.mordhauConfigurationFilesToolStripMenuItem});
            this.mordhauConfigToolStripMenuItem.Name = "mordhauConfigToolStripMenuItem";
            this.mordhauConfigToolStripMenuItem.Size = new System.Drawing.Size(107, 20);
            this.mordhauConfigToolStripMenuItem.Text = "Mordhau Config";
            // 
            // automaticConfigurationToolStripMenuItem
            // 
            this.automaticConfigurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setUpToolStripMenuItem,
            this.revertToolStripMenuItem});
            this.automaticConfigurationToolStripMenuItem.Name = "automaticConfigurationToolStripMenuItem";
            this.automaticConfigurationToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.automaticConfigurationToolStripMenuItem.Text = "Automatic Configuration";
            // 
            // setUpToolStripMenuItem
            // 
            this.setUpToolStripMenuItem.Name = "setUpToolStripMenuItem";
            this.setUpToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.setUpToolStripMenuItem.Text = "Set up";
            this.setUpToolStripMenuItem.Click += new System.EventHandler(this.AutoConFigButton_Click);
            // 
            // revertToolStripMenuItem
            // 
            this.revertToolStripMenuItem.Name = "revertToolStripMenuItem";
            this.revertToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.revertToolStripMenuItem.Text = "Revert";
            this.revertToolStripMenuItem.Click += new System.EventHandler(this.RevertAutoConfig_Click);
            // 
            // mordhauConfigurationFilesToolStripMenuItem
            // 
            this.mordhauConfigurationFilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setDefaultInputiniLocationToolStripMenuItem,
            this.openDefaultInputiniToolStripMenuItem});
            this.mordhauConfigurationFilesToolStripMenuItem.Name = "mordhauConfigurationFilesToolStripMenuItem";
            this.mordhauConfigurationFilesToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.mordhauConfigurationFilesToolStripMenuItem.Text = "Mordhau Configuration Files";
            // 
            // setDefaultInputiniLocationToolStripMenuItem
            // 
            this.setDefaultInputiniLocationToolStripMenuItem.Name = "setDefaultInputiniLocationToolStripMenuItem";
            this.setDefaultInputiniLocationToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.setDefaultInputiniLocationToolStripMenuItem.Text = "Set DefaultInput.ini Location";
            this.setDefaultInputiniLocationToolStripMenuItem.Click += new System.EventHandler(this.SetConfig_Click);
            // 
            // openDefaultInputiniToolStripMenuItem
            // 
            this.openDefaultInputiniToolStripMenuItem.Name = "openDefaultInputiniToolStripMenuItem";
            this.openDefaultInputiniToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.openDefaultInputiniToolStripMenuItem.Text = "Open DefaultInput.ini";
            this.openDefaultInputiniToolStripMenuItem.Click += new System.EventHandler(this.OpenConfig_Click);
            // 
            // MordhauConfigLabel
            // 
            this.MordhauConfigLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MordhauConfigLabel.AutoSize = true;
            this.MordhauConfigLabel.Location = new System.Drawing.Point(12, 197);
            this.MordhauConfigLabel.Name = "MordhauConfigLabel";
            this.MordhauConfigLabel.Size = new System.Drawing.Size(479, 13);
            this.MordhauConfigLabel.TabIndex = 8;
            this.MordhauConfigLabel.Text = "Make sure to set your in-game Console key to the one listed here.  It cannot be a" +
    " key that sends text";
            // 
            // KeyBindingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 253);
            this.Controls.Add(this.MordhauConfigLabel);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.HotkeysList);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KeyBindingForm";
            this.Text = "KeyBinding";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeyHandler);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView HotkeysList;
        private System.Windows.Forms.ColumnHeader Action;
        private System.Windows.Forms.ColumnHeader Key;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private ToolTip AutoConfigTooltip;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem mordhauConfigToolStripMenuItem;
        private ToolStripMenuItem automaticConfigurationToolStripMenuItem;
        private ToolStripMenuItem setUpToolStripMenuItem;
        private ToolStripMenuItem revertToolStripMenuItem;
        private ToolStripMenuItem mordhauConfigurationFilesToolStripMenuItem;
        private ToolStripMenuItem setDefaultInputiniLocationToolStripMenuItem;
        private ToolStripMenuItem openDefaultInputiniToolStripMenuItem;
        private Label MordhauConfigLabel;
    }
}