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
	public partial class Form1 : Form
	{
		public Life life;

		public Form1()
		{
			InitializeComponent();
			var test = new Tester();
			test.Start();

			var painter = new Painter(pictureBox1, label1, textBox1, test);
			life = new Life(painter, test);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			life.Start();
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void pictureBox1_Click(object sender, EventArgs e)
		{

		}

		private void label1_Click(object sender, EventArgs e)
		{

		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{

		}
	}
}
