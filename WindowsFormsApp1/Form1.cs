using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WindowsFormsApp1.GameLogic;

namespace WindowsFormsApp1
{
	public partial class Form1 : Form
	{
		public Game game;
		public Form2 form2;

		public Form1()
		{
			InitializeComponent();
			form2 = new Form2(this);
			var lensPictureBox = form2.GetLensPictureBox();

			var test = new Tester();
			var presenter = new Presenter(pictureBox1, lensPictureBox,   new Label[] { }, new[] { textBox1, textBox2 }, test);
			game = new Game(presenter, test);


			label2.Text = $@"	S - start
								P - pause mode
								space - step
								M - mutation on/off
								D - draw on/off";


		}

		public async void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			bool needToRunLife = false;
			if (!game.Started)
			{
				if (e.KeyCode == Keys.S)
				{
					await game.Start();
					needToRunLife = true;
				}
			}


			if (game.Started)
			{

				if (e.KeyCode == Keys.P)
				{
					game.PausedToggle();
					if (!game.Paused)
					{
						needToRunLife = true;
					}
				}

				if (e.KeyCode == Keys.Space)
				{
					if (game.Paused)
					{
						needToRunLife = true;
					}
				}

				if (e.KeyCode == Keys.L)
				{
					if (!form2.Visible)
					{
						game.Lens(true);
						form2.Visible = true;
					}
					else
					{
						game.Lens(false);
						form2.Visible = false;
					}
				}

				if (e.KeyCode == Keys.M)
				{
					game.MutationToggle();
				}

				if (e.KeyCode == Keys.D)
				{
					game.DrawedToggle();
				}



				if (e.KeyCode == Keys.Up)
				{
					game.LensUp();

				}
				if (e.KeyCode == Keys.Down)
				{
					game.LensDown();

				}
				if (e.KeyCode == Keys.Left)
				{
					game.LensLeft();

				}
				if (e.KeyCode == Keys.Right)
				{
					game.LensRight();
				}
			}


			if (needToRunLife)
			{
				game.Work();
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

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (form2 != null)
			{

				form2.Close();
			}

		}

		private void label2_Click(object sender, EventArgs e)
		{

		}
	}
}
