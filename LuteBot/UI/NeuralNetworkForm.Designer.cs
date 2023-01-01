namespace LuteBot.UI
{
    partial class NeuralNetworkForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NeuralNetworkForm));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.textBoxSizes = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonTrain = new System.Windows.Forms.Button();
            this.progressBarTraining = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBoxTrainExisting = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(776, 460);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // textBoxSizes
            // 
            this.textBoxSizes.Location = new System.Drawing.Point(112, 498);
            this.textBoxSizes.Name = "textBoxSizes";
            this.textBoxSizes.Size = new System.Drawing.Size(100, 22);
            this.textBoxSizes.TabIndex = 1;
            this.textBoxSizes.Text = "64,32";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 501);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Network Sizes";
            // 
            // buttonTrain
            // 
            this.buttonTrain.Location = new System.Drawing.Point(713, 498);
            this.buttonTrain.Name = "buttonTrain";
            this.buttonTrain.Size = new System.Drawing.Size(75, 23);
            this.buttonTrain.TabIndex = 3;
            this.buttonTrain.Text = "Train";
            this.buttonTrain.UseVisualStyleBackColor = true;
            this.buttonTrain.Click += new System.EventHandler(this.buttonTrain_Click);
            // 
            // progressBarTraining
            // 
            this.progressBarTraining.Location = new System.Drawing.Point(1, 552);
            this.progressBarTraining.Name = "progressBarTraining";
            this.progressBarTraining.Size = new System.Drawing.Size(799, 23);
            this.progressBarTraining.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(233, 501);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(157, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Success after completing";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(512, 498);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(50, 22);
            this.textBox1.TabIndex = 5;
            this.textBox1.Text = "2";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(394, 498);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(36, 22);
            this.textBox2.TabIndex = 7;
            this.textBox2.Text = "100";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(436, 501);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 16);
            this.label3.TabIndex = 8;
            this.label3.Text = "% of tests, ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(568, 501);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 16);
            this.label4.TabIndex = 9;
            this.label4.Text = "times in a row";
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(713, 523);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "Test...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBoxTrainExisting
            // 
            this.checkBoxTrainExisting.AutoSize = true;
            this.checkBoxTrainExisting.Location = new System.Drawing.Point(12, 525);
            this.checkBoxTrainExisting.Name = "checkBoxTrainExisting";
            this.checkBoxTrainExisting.Size = new System.Drawing.Size(109, 20);
            this.checkBoxTrainExisting.TabIndex = 13;
            this.checkBoxTrainExisting.Text = "Train Existing";
            this.checkBoxTrainExisting.UseVisualStyleBackColor = true;
            this.checkBoxTrainExisting.CheckedChanged += new System.EventHandler(this.checkBoxTrainExisting_CheckedChanged);
            // 
            // NeuralNetworkForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 587);
            this.Controls.Add(this.checkBoxTrainExisting);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.progressBarTraining);
            this.Controls.Add(this.buttonTrain);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxSizes);
            this.Controls.Add(this.richTextBox1);
            this.Name = "NeuralNetworkForm";
            this.Text = "Neural Network Training";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox textBoxSizes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonTrain;
        private System.Windows.Forms.ProgressBar progressBarTraining;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBoxTrainExisting;
    }
}