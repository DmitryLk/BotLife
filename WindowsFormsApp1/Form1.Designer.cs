namespace WindowsFormsApp1
{
	partial class Form1
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
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.trackBar1 = new System.Windows.Forms.TrackBar();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Location = new System.Drawing.Point(13, 8);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(1166, 1154);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
			// 
			// textBox1
			// 
			this.textBox1.Enabled = false;
			this.textBox1.Location = new System.Drawing.Point(1186, 330);
			this.textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(190, 159);
			this.textBox1.TabIndex = 4;
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// textBox2
			// 
			this.textBox2.Enabled = false;
			this.textBox2.Location = new System.Drawing.Point(1186, 86);
			this.textBox2.Multiline = true;
			this.textBox2.Name = "textBox2";
			this.textBox2.ReadOnly = true;
			this.textBox2.Size = new System.Drawing.Size(186, 238);
			this.textBox2.TabIndex = 5;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(1186, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(38, 15);
			this.label2.TabIndex = 6;
			this.label2.Text = "label2";
			this.label2.Click += new System.EventHandler(this.label2_Click);
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Location = new System.Drawing.Point(1210, 566);
			this.numericUpDown1.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(70, 23);
			this.numericUpDown1.TabIndex = 7;
			// 
			// trackBar1
			// 
			this.trackBar1.Location = new System.Drawing.Point(1210, 515);
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.Size = new System.Drawing.Size(162, 45);
			this.trackBar1.TabIndex = 8;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1398, 1061);
			this.Controls.Add(this.trackBar1);
			this.Controls.Add(this.numericUpDown1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.pictureBox1);
			this.KeyPreview = true;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MaximumSize = new System.Drawing.Size(5000, 5000);
			this.MinimumSize = new System.Drawing.Size(1164, 1038);
			this.Name = "Form1";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "Form1";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.TrackBar trackBar1;
	}
}

