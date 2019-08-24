using System;
using System.Windows.Forms;

namespace LuteBot
{
    partial class PlayListForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlayListForm));
            this.PlayListBox = new System.Windows.Forms.ListBox();
            this.LoadButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.LoadPlayListButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PlayListBox
            // 
            this.PlayListBox.AllowDrop = true;
            this.PlayListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.PlayListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold);
            this.PlayListBox.FormattingEnabled = true;
            this.PlayListBox.ItemHeight = 50;
            this.PlayListBox.Location = new System.Drawing.Point(13, 12);
            this.PlayListBox.Name = "PlayListBox";
            this.PlayListBox.Size = new System.Drawing.Size(532, 504);
            this.PlayListBox.TabIndex = 0;
            this.PlayListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.List_DrawItem);
            this.PlayListBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.List_DragDrop);
            this.PlayListBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.List_DragEnter);
            this.PlayListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.List_MouseDown);
            this.PlayListBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.List_MouseMove);
            // 
            // LoadButton
            // 
            this.LoadButton.Location = new System.Drawing.Point(12, 520);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(150, 28);
            this.LoadButton.TabIndex = 1;
            this.LoadButton.Text = "Add to PlayList";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(200, 520);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(150, 28);
            this.SaveButton.TabIndex = 2;
            this.SaveButton.Text = "Save PlayList";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // LoadPlayListButton
            // 
            this.LoadPlayListButton.Location = new System.Drawing.Point(395, 520);
            this.LoadPlayListButton.Name = "LoadPlayListButton";
            this.LoadPlayListButton.Size = new System.Drawing.Size(150, 28);
            this.LoadPlayListButton.TabIndex = 3;
            this.LoadPlayListButton.Text = "Load PlayList";
            this.LoadPlayListButton.UseVisualStyleBackColor = true;
            this.LoadPlayListButton.Click += new System.EventHandler(this.LoadPlayListButton_Click);
            // 
            // PlayListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(557, 560);
            this.Controls.Add(this.LoadPlayListButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.LoadButton);
            this.Controls.Add(this.PlayListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PlayListForm";
            this.Text = "Playlist";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PlayListForm_Closing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox PlayListBox;
        private System.Windows.Forms.Button LoadButton;
        private Button SaveButton;
        private Button LoadPlayListButton;
    }
}