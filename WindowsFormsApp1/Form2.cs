using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
	public partial class Form2 : Form
	{
		public Form1 form1;
		public Form2(Form1 form)
		{
			form1 = form;
			InitializeComponent();
		}

		public (PictureBox lens, PictureBox cursorInfo, PictureBox reactions, TextBox objInfo1, TextBox objInfo2) GetControlsLensForm()
		{
			return (pictureBox1, pictureBox2, pictureBox3, textBox1, textBox2);
		}

		private void Form2_Load(object sender, EventArgs e)
		{

		}

		private void Form2_KeyDown(object sender, KeyEventArgs e)
		{
			form1.Form1_KeyDown(sender, e);
		}

		private void Form2_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = true;
			this.Hide();
		}

		private void pictureBox1_Click(object sender, EventArgs e)
		{

		}

		private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
		{

		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void groupBox1_Enter(object sender, EventArgs e)
		{

		}

		private void panel1_Paint(object sender, PaintEventArgs e)
		{

		}

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

		private void pictureBox3_Click(object sender, EventArgs e)
		{

		}
	}
}
