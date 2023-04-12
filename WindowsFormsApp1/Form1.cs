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
		public Game Game;
		public Form2 Form2;

		public Form1()
		{
			InitializeComponent();


			Form2 = new Form2(this);
			var (lensPictureBox, cursorPictureBox) = Form2.GetLensFormPictureBox();
			var test = new Tester();
            var data = new GameData();
            data.Initialize();

			var presenter = new Presenter(
                data,
                test,
				pictureBox1, 
                lensPictureBox, 
                cursorPictureBox,   
                new Label[] { }, 
                new[] { textBox1, textBox2 });

			Game = new Game(data, presenter, test);


			label2.Text = $@"	S - start
								P - pause mode
								space - step
								M - mutation on/off
								D - draw on/off";
		}

		public async void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			bool needToRunLife = false;
			if (!Game.Started)
			{
				if (e.KeyCode == Keys.S)
				{
					await Game.Start();
					needToRunLife = true;
				}
			}


			if (Game.Started)
			{

				if (e.KeyCode == Keys.P)
				{
					Game.PausedToggle();
					if (!Game.Paused)
					{
						needToRunLife = true;
					}
				}

				if (e.KeyCode == Keys.Space)
				{
					if (Game.Paused)
					{
						needToRunLife = true;
					}
				}

				if (e.KeyCode == Keys.L)
				{
					if (!Form2.Visible)
					{
						Game.Lens(true);
						Form2.Visible = true;
					}
					else
					{
						Game.Lens(false);
						Form2.Visible = false;
					}
				}

				if (e.KeyCode == Keys.M)
				{
					Game.MutationToggle();
				}

				if (e.KeyCode == Keys.D)
				{
					Game.DrawedToggle();
				}



				if (e.KeyCode == Keys.Up)
				{
					Game.LensUp();

				}
				if (e.KeyCode == Keys.Down)
				{
					Game.LensDown();

				}
				if (e.KeyCode == Keys.Left)
				{
					Game.LensLeft();

				}
				if (e.KeyCode == Keys.Right)
				{
					Game.LensRight();
				}



				if (e.KeyCode == Keys.Home)
				{
					Game.CursorUp();

				}
				if (e.KeyCode == Keys.End)
				{
					Game.CursorDown();

				}
				if (e.KeyCode == Keys.Delete)
				{
					Game.CursorLeft();

				}
				if (e.KeyCode == Keys.PageDown)
				{
					Game.CursorRight();
				}
			}


			if (needToRunLife)
			{
				Game.Work();
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
			if (Form2 != null)
			{

				Form2.Close();
			}

		}

		private void label2_Click(object sender, EventArgs e)
		{

		}
	}
}
