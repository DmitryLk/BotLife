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
using Accessibility;
using WindowsFormsApp1.Static;

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
            var (lensPictureBox, cursorPictureBox, objTextBox1, objTextBox2) = Form2.GetControlsLensForm();

            Data.Initialize();

            var pictureBoxes = new[] { pictureBox1, lensPictureBox, cursorPictureBox };
            var textBoxes = new[] { textBox1, textBox2, textBox3, objTextBox1, objTextBox2 };

            var presenter = new Presenter(
                pictureBoxes,
                textBoxes);

            Game = new Game(presenter);

            trackBar1.Value = Data.PhotosynthesisEnergy;

			label2.Text = $@"	S - start
								P - pause mode
								space - step
								M - mutation on/off
								D - draw on/off";
        }

        public async void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!Game.Started)
            {
				switch (e.KeyCode)
				{
					case Keys.P: PauseToggle(); break;
					case Keys.S:
						{
							await Game.Start();
							Game.Work();
							return;
						}
					default: break;
				}
            }


            if (Game.Started)
            {

                switch (e.KeyCode)
                {
                    case Keys.P: PauseToggle(); break;
                    case Keys.Space: StepPause(); break;
                    case Keys.L: LensToggle(); break;
                    case Keys.M: Game.MutationToggle(); break;
                    case Keys.D: Game.DrawedToggle(); break;
                    case Keys.Up: Game.LensUp(); break;
                    case Keys.Down: Game.LensDown(); break;
                    case Keys.Left: Game.LensLeft(); break;
                    case Keys.Right: Game.LensRight(); break;
                    case Keys.Home: Game.CursorUp(); break;
                    case Keys.End: Game.CursorDown(); break;
                    case Keys.Delete: Game.CursorRight(); break;
                    case Keys.PageDown: Game.CursorLeft(); break;
                    case Keys.Z: Game.HistoryDown(); break;
                    case Keys.X: Game.HistoryUp(); break;
                    default: break;
                }
            }
        }

        private void PauseToggle()
        {
            Game.PausedToggle();
            if (!Game.Paused)
            {
                Game.Work();
            }
        }

        private void StepPause()
        {
            if (Game.Paused)
            {
                Game.Work();
            }
        }

        private void LensToggle()
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

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Data.PhotosynthesisEnergy = trackBar1.Value;
        }
    }
}
