namespace LuteBot.UI
{
    partial class OnlineSyncForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OnlineSyncForm));
            this.OnlinePlayersList = new System.Windows.Forms.ListBox();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.NicknameLabel = new System.Windows.Forms.Label();
            this.IpTextBox = new System.Windows.Forms.TextBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.HostButton = new System.Windows.Forms.Button();
            this.ReadyButton = new System.Windows.Forms.Button();
            this.RoomIPLabel = new System.Windows.Forms.Label();
            this.ConnectionLabel = new System.Windows.Forms.Label();
            this.HostLabel = new System.Windows.Forms.Label();
            this.RoomIpAdressLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // OnlinePlayersList
            // 
            this.OnlinePlayersList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.OnlinePlayersList.FormattingEnabled = true;
            this.OnlinePlayersList.Location = new System.Drawing.Point(13, 13);
            this.OnlinePlayersList.Name = "OnlinePlayersList";
            this.OnlinePlayersList.Size = new System.Drawing.Size(407, 355);
            this.OnlinePlayersList.TabIndex = 0;
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(426, 25);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(352, 20);
            this.NameTextBox.TabIndex = 1;
            this.NameTextBox.Text = "Anonymous";
            // 
            // NicknameLabel
            // 
            this.NicknameLabel.AutoSize = true;
            this.NicknameLabel.Location = new System.Drawing.Point(426, 9);
            this.NicknameLabel.Name = "NicknameLabel";
            this.NicknameLabel.Size = new System.Drawing.Size(61, 13);
            this.NicknameLabel.TabIndex = 2;
            this.NicknameLabel.Text = "Nickname :";
            // 
            // IpTextBox
            // 
            this.IpTextBox.Location = new System.Drawing.Point(426, 99);
            this.IpTextBox.Name = "IpTextBox";
            this.IpTextBox.Size = new System.Drawing.Size(352, 20);
            this.IpTextBox.TabIndex = 3;
            this.IpTextBox.Text = "Enter an IP Adress";
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(426, 125);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(352, 37);
            this.ConnectButton.TabIndex = 4;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // HostButton
            // 
            this.HostButton.Location = new System.Drawing.Point(426, 280);
            this.HostButton.Name = "HostButton";
            this.HostButton.Size = new System.Drawing.Size(352, 37);
            this.HostButton.TabIndex = 7;
            this.HostButton.Text = "Start a new Room";
            this.HostButton.UseVisualStyleBackColor = true;
            this.HostButton.Click += new System.EventHandler(this.HostButton_Click);
            // 
            // ReadyButton
            // 
            this.ReadyButton.Location = new System.Drawing.Point(426, 168);
            this.ReadyButton.Name = "ReadyButton";
            this.ReadyButton.Size = new System.Drawing.Size(352, 30);
            this.ReadyButton.TabIndex = 8;
            this.ReadyButton.Text = "Ready";
            this.ReadyButton.UseVisualStyleBackColor = true;
            this.ReadyButton.Click += new System.EventHandler(this.ReadyButton_Click);
            // 
            // RoomIPLabel
            // 
            this.RoomIPLabel.AutoSize = true;
            this.RoomIPLabel.Location = new System.Drawing.Point(426, 337);
            this.RoomIPLabel.Name = "RoomIPLabel";
            this.RoomIPLabel.Size = new System.Drawing.Size(89, 13);
            this.RoomIPLabel.TabIndex = 9;
            this.RoomIPLabel.Text = "Room IP Adress :";
            // 
            // ConnectionLabel
            // 
            this.ConnectionLabel.AutoSize = true;
            this.ConnectionLabel.Location = new System.Drawing.Point(423, 83);
            this.ConnectionLabel.Name = "ConnectionLabel";
            this.ConnectionLabel.Size = new System.Drawing.Size(67, 13);
            this.ConnectionLabel.TabIndex = 5;
            this.ConnectionLabel.Text = "Connection :";
            // 
            // HostLabel
            // 
            this.HostLabel.AutoSize = true;
            this.HostLabel.Location = new System.Drawing.Point(426, 264);
            this.HostLabel.Name = "HostLabel";
            this.HostLabel.Size = new System.Drawing.Size(35, 13);
            this.HostLabel.TabIndex = 6;
            this.HostLabel.Text = "Host :";
            // 
            // RoomIpAdressLabel
            // 
            this.RoomIpAdressLabel.AutoSize = true;
            this.RoomIpAdressLabel.Location = new System.Drawing.Point(521, 337);
            this.RoomIpAdressLabel.Name = "RoomIpAdressLabel";
            this.RoomIpAdressLabel.Size = new System.Drawing.Size(64, 13);
            this.RoomIpAdressLabel.TabIndex = 10;
            this.RoomIpAdressLabel.Text = "192.168.1.0";
            // 
            // OnlineSyncForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(790, 375);
            this.Controls.Add(this.RoomIpAdressLabel);
            this.Controls.Add(this.RoomIPLabel);
            this.Controls.Add(this.ReadyButton);
            this.Controls.Add(this.HostButton);
            this.Controls.Add(this.HostLabel);
            this.Controls.Add(this.ConnectionLabel);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.IpTextBox);
            this.Controls.Add(this.NicknameLabel);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.OnlinePlayersList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OnlineSyncForm";
            this.Text = "OnlineSyncForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox OnlinePlayersList;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label NicknameLabel;
        private System.Windows.Forms.TextBox IpTextBox;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.Button HostButton;
        private System.Windows.Forms.Button ReadyButton;
        private System.Windows.Forms.Label RoomIPLabel;
        private System.Windows.Forms.Label ConnectionLabel;
        private System.Windows.Forms.Label HostLabel;
        private System.Windows.Forms.Label RoomIpAdressLabel;
    }
}