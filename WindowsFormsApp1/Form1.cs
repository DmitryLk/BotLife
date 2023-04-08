﻿using Newtonsoft.Json;
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
		public GameData _data;

		public Form1()
		{
			InitializeComponent();

			var options = LoadConfig();
			_data = new GameData(options);

			var test = new Tester();
			var painter = new Presenter(_data, pictureBox1, new[] { label1 }, new[] { textBox1, textBox2 }, test);
			game = new Game(painter, test, _data);


		}

		private async void button1_Click(object sender, EventArgs e)
		{
			if (e is MouseEventArgs)
			{
				if (!_data.Started)
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

		public void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (_data.Started)
			{
				if (e.KeyCode == Keys.P)
				{
					_data.PausedMode = !_data.PausedMode;
					if (!_data.PausedMode)
					{
						game.Work();
					}
				}

				if (e.KeyCode == Keys.Space)
				{
					game.Work();
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
					_data.Drawed = !_data.Drawed;
					_data.ReportFrequencyCurrent = _data.Drawed ? _data.ReportFrequencyDrawed : _data.ReportFrequencyNoDrawed;
				}

				if (e.KeyCode == Keys.Up)
				{
				}

			}
		}

		private GameOptions LoadConfig()
		{
			using (StreamReader r = new StreamReader("config.json"))
			{
				string json = r.ReadToEnd();
				//dynamic array = JsonConvert.DeserializeObject(json);
				GameOptions config = JsonConvert.DeserializeObject<GameOptions>(json);
				return config;
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
	}
}
