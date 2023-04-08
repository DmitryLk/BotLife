using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
	public partial class Form1 : Form
	{
		public Game game;

		public Form1()
		{
			InitializeComponent();

			var test = new Tester();
			var painter = new Presenter(pictureBox1, new[] { label1, label2 }, textBox1, test);
			game = new Game(painter, test);
		}

		private async void button1_Click(object sender, EventArgs e)
		{
			if (e is MouseEventArgs)
			{
				if (!game.Started)
				{
					game.Start();
					button1.Enabled = false;
				}
			}
			else
			{
				// Not mouse click...
			}

			//await Task.Factory.StartNew(() => life.Start(), TaskCreationOptions.LongRunning);
			//var thread = new System.Threading.Thread(() => life.Start());
			//thread.Start();
		}

		private void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (game.Started)
			{
				if (e.KeyCode == Keys.P)
				{
					game.PausedMode = !game.PausedMode;
					if (!game.PausedMode)
					{
						game.Work();
					}
				}

				if (e.KeyCode == Keys.Space)
				{
					game.Work();
				}
			}
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
