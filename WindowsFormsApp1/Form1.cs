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


			var test = new Tester();
			var presenter = new Presenter(pictureBox1, new Label[] { }, new[] { textBox1, textBox2 }, test);
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
					game.PausedMode = !game.PausedMode;
					if (!game.PausedMode)
					{
						needToRunLife = true;
					}
				}

				if (e.KeyCode == Keys.Space)
				{
					if (game.PausedMode)
					{
						needToRunLife = true;
					}
				}

				if (e.KeyCode == Keys.F)
				{
					if (form2 == null)
					{
						form2 = new Form2(this);
						form2.ShowDialog();
					}
					else
					{
						if (false)//form2.Disposing)
						{
							form2 = new Form2(this);
						}
						else
						{
							if (!form2.Visible)
							{
								form2.Visible = true;
							}
							else
							{
								form2.Visible = false;
							}
						}
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
