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
            this.buttonExport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PlayListBox
            // 
            this.PlayListBox.AllowDrop = true;
            this.PlayListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.PlayListBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PlayListBox.FormattingEnabled = true;
            this.PlayListBox.ItemHeight = 25;
            this.PlayListBox.Location = new System.Drawing.Point(17, 15);
            this.PlayListBox.Margin = new System.Windows.Forms.Padding(4);
            this.PlayListBox.Name = "PlayListBox";
            this.PlayListBox.Size = new System.Drawing.Size(708, 604);
            this.PlayListBox.TabIndex = 0;
            this.PlayListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.List_DrawItem);
            this.PlayListBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.List_DragDrop);
            this.PlayListBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.List_DragEnter);
            this.PlayListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.List_MouseDown);
            this.PlayListBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.List_MouseMove);
            // 
            // LoadButton
            // 
            this.LoadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LoadButton.Location = new System.Drawing.Point(16, 640);
            this.LoadButton.Margin = new System.Windows.Forms.Padding(4);
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(148, 34);
            this.LoadButton.TabIndex = 1;
            this.LoadButton.Text = "Add to PlayList";
            this.LoadButton.UseVisualStyleBackColor = true;
            this.LoadButton.Click += new System.EventHandler(this.LoadButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SaveButton.Location = new System.Drawing.Point(172, 640);
            this.SaveButton.Margin = new System.Windows.Forms.Padding(4);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(167, 34);
            this.SaveButton.TabIndex = 2;
            this.SaveButton.Text = "Save PlayList";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // LoadPlayListButton
            // 
            this.LoadPlayListButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LoadPlayListButton.Location = new System.Drawing.Point(388, 640);
            this.LoadPlayListButton.Margin = new System.Windows.Forms.Padding(4);
            this.LoadPlayListButton.Name = "LoadPlayListButton";
            this.LoadPlayListButton.Size = new System.Drawing.Size(167, 34);
            this.LoadPlayListButton.TabIndex = 3;
            this.LoadPlayListButton.Text = "Load PlayList";
            this.LoadPlayListButton.UseVisualStyleBackColor = true;
            this.LoadPlayListButton.Click += new System.EventHandler(this.LoadPlayListButton_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.Location = new System.Drawing.Point(563, 640);
            this.buttonExport.Margin = new System.Windows.Forms.Padding(4);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(167, 34);
            this.buttonExport.TabIndex = 4;
            this.buttonExport.Text = "Export PlayList";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // PlayListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(743, 689);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.LoadPlayListButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.LoadButton);
            this.Controls.Add(this.PlayListBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PlayListForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Playlist";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PlayListForm_Closing);
            this.ResizeEnd += new System.EventHandler(this.PlayListForm_ResizeEnd);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox PlayListBox;
        private System.Windows.Forms.Button LoadButton;
        private Button SaveButton;
        private Button LoadPlayListButton;
        private Button buttonExport;
    }
}