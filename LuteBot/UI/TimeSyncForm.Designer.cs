﻿namespace LuteBot.UI
{
    partial class TimeSyncForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.ServerTimeLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.inputTime = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.serverBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.ntpPingLabel = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.serverPingLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server Time: ";
            // 
            // ServerTimeLabel
            // 
            this.ServerTimeLabel.AutoSize = true;
            this.ServerTimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ServerTimeLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ServerTimeLabel.Location = new System.Drawing.Point(132, 9);
            this.ServerTimeLabel.Name = "ServerTimeLabel";
            this.ServerTimeLabel.Size = new System.Drawing.Size(90, 24);
            this.ServerTimeLabel.TabIndex = 1;
            this.ServerTimeLabel.Text = "12:00.000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 24);
            this.label2.TabIndex = 2;
            this.label2.Text = "Start Playing At: ";
            // 
            // inputTime
            // 
            this.inputTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.inputTime.Location = new System.Drawing.Point(152, 39);
            this.inputTime.Name = "inputTime";
            this.inputTime.Size = new System.Drawing.Size(100, 29);
            this.inputTime.TabIndex = 3;
            this.inputTime.Text = "12:01";
            this.inputTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(258, 43);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Set";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(202, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Start your song, then pause it immediately";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(282, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Drag the time slider back to the very beginning of the song";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 102);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(234, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Agree upon an exact UTC time to start and set it";
            // 
            // serverBox
            // 
            this.serverBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.serverBox.Location = new System.Drawing.Point(152, 125);
            this.serverBox.Name = "serverBox";
            this.serverBox.Size = new System.Drawing.Size(100, 29);
            this.serverBox.TabIndex = 8;
            this.serverBox.Text = "127.0.0.1";
            this.serverBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.serverBox.TextChanged += new System.EventHandler(this.ServerBox_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(12, 128);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 24);
            this.label6.TabIndex = 9;
            this.label6.Text = "Server IP:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 167);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(302, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "Providing the server IP will let the program calculate for latency";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 180);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(244, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "This updates in realtime and checks every second";
            // 
            // ntpPingLabel
            // 
            this.ntpPingLabel.AutoSize = true;
            this.ntpPingLabel.Location = new System.Drawing.Point(295, 17);
            this.ntpPingLabel.Name = "ntpPingLabel";
            this.ntpPingLabel.Size = new System.Drawing.Size(19, 13);
            this.ntpPingLabel.TabIndex = 12;
            this.ntpPingLabel.Text = "10";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(313, 17);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(20, 13);
            this.label10.TabIndex = 13;
            this.label10.Text = "ms";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(313, 136);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(20, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = "ms";
            // 
            // serverPingLabel
            // 
            this.serverPingLabel.AutoSize = true;
            this.serverPingLabel.Location = new System.Drawing.Point(295, 136);
            this.serverPingLabel.Name = "serverPingLabel";
            this.serverPingLabel.Size = new System.Drawing.Size(19, 13);
            this.serverPingLabel.TabIndex = 14;
            this.serverPingLabel.Text = "10";
            // 
            // TimeSyncForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 217);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.serverPingLabel);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.ntpPingLabel);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.serverBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.inputTime);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ServerTimeLabel);
            this.Controls.Add(this.label1);
            this.Name = "TimeSyncForm";
            this.Text = "TimeSyncForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ServerTimeLabel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        public System.Windows.Forms.TextBox inputTime;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox serverBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label ntpPingLabel;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label serverPingLabel;
    }
}