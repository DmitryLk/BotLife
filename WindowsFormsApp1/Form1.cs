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
using Accessibility;
using WindowsFormsApp1.Static;
using WindowsFormsApp1.Enums;
using DrawMode = WindowsFormsApp1.Enums.DrawMode;
using WindowsFormsApp1.Graphic;
using WindowsFormsApp1.GameLogic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

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
			var (lensPictureBox, cursorPictureBox, reactionsPictureBox, objTextBox1, objTextBox2) = Form2.GetControlsLensForm();

			Data.Initialize();

			var pictureBoxes = new[] { pictureBox1, lensPictureBox, cursorPictureBox, reactionsPictureBox };
			var textBoxes = new[] { textBox1, textBox2, textBox3, objTextBox1, objTextBox2, textBox4, textBox5, textBox6 };

			var presenter = new Presenter(pictureBoxes);
			var printer = new Printer(textBoxes, dataGridView1, this);

			Game = new Game(presenter, printer);

			trackBar1.Value = Data.PhotosynthesisEnergy;
			trackBar2.Value = Data.MovedBiteStrength;
			checkBox3.Checked = Data.DelayForNewbie;


			checkBox4.CheckedChanged -= checkBox4_CheckedChanged;
			if (Data.DeltaEnergyOnStep == -1) checkBox4.Checked = true;
			else if (Data.DeltaEnergyOnStep == 0) checkBox4.Checked = false;
			checkBox4.CheckedChanged += checkBox4_CheckedChanged;


			label2.Text = $@"	Enter - pause mode
								Space - start/step
								M - mutation on/off

								N - NoDraw
								D - EachStep
								T - Periodical
								
								O - OnlyChangedCells
								A - AllCells
								
								F1 - GenomColor
								F2 - PraGenomColor
								F3 - PlantPredator
								F4 - Energy
								F5 - Age
								F6 - GenomAge

								P  - Parallel
								С  - Checks
								F  - Fastest

								H  - History log
								L  - Bot log

								G - Genom info once
								Y - Genom info periodical
			";

			//         dataGridView1.Columns.Add(new DataGridViewColumn
			//         {
			//             DataPropertyName = "GenomName",
			//             Width = 120,
			//             HeaderText = "Геном",
			//             Name = "genom",
			//             CellTemplate = new DataGridViewTextBoxCell()
			//         });

			//         dataGridView1.Columns.Add(new DataGridViewColumn
			//         {
			//             DataPropertyName = "GenomColor",
			//             Width = 20,
			//             HeaderText = "",
			//             Name = "color",
			//             CellTemplate = new DataGridViewTextBoxCell()
			//         });

			//         dataGridView1.Columns.Add(new DataGridViewColumn
			//         {
			//             DataPropertyName = "Live",
			//             Width = 60,
			//             HeaderText = "Живых",
			//             Name = "live",
			//             CellTemplate = new DataGridViewTextBoxCell()
			//         });

			//         dataGridView1.Columns.Add(new DataGridViewColumn
			//         {
			//             DataPropertyName = "Total",
			//             Width = 60,
			//             HeaderText = "Всего",
			//             Name = "total",
			//             CellTemplate = new DataGridViewTextBoxCell()
			//         });

			//         dataGridView1.Columns.Add(new DataGridViewColumn
			//         {
			//             DataPropertyName = "Age",
			//             Width = 60,
			//             HeaderText = "Возраст",
			//             Name = "age",
			//             CellTemplate = new DataGridViewTextBoxCell()
			//         });

			//         dataGridView1.Columns.Add(new DataGridViewColumn
			//         {
			//             DataPropertyName = "AvBotAge",
			//             Width = 80,
			//             HeaderText = "Ср. возр. ботов",
			//             Name = "avBotAge",
			//             CellTemplate = new DataGridViewTextBoxCell()
			//         });

			//dataGridView1.Columns.Add(new DataGridViewColumn
			//{
			//	DataPropertyName = "ActGen",
			//	Width = 60,
			//	HeaderText = "Активных",
			//	Name = "actGen",
			//	CellTemplate = new DataGridViewTextBoxCell()
			//});

			dataGridView1.RowHeadersVisible = false;

			Game.Init().Wait();
			Game.Work();
		}

		public async void Form1_KeyDown(object sender, KeyEventArgs e)
		{
			if (!Game.Started)
			{
				switch (e.KeyCode)
				{
					case Keys.Enter: PauseToggle(); break;
					case Keys.Space:
						{
							await Game.Init();
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
					case Keys.Enter: PauseToggle(); break;
					case Keys.Space: StepPause(); break;

					case Keys.L: LensToggle(); break;
					case Keys.M: Game.MutationToggle(); break;

					case Keys.N: Game.ChangeDrawMode(DrawMode.NoDraw); break;
					case Keys.D: Game.ChangeDrawMode(DrawMode.EachStep); break;
					case Keys.T: Game.ChangeDrawMode(DrawMode.Periodical); break;

					case Keys.O: Game.ChangeDrawType(DrawType.OnlyChangedCells); break;
					case Keys.A: Game.ChangeDrawType(DrawType.AllCells); break;

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

					case Keys.F1: Game.ChangeBotColorMode(BotColorMode.GenomColor); break;
					case Keys.F2: Game.ChangeBotColorMode(BotColorMode.PraGenomColor); break;
					case Keys.F3: Game.ChangeBotColorMode(BotColorMode.PlantPredator); break;
					case Keys.F4: Game.ChangeBotColorMode(BotColorMode.Energy); break;
					case Keys.F5: Game.ChangeBotColorMode(BotColorMode.Age); break;
					case Keys.F6: Game.ChangeBotColorMode(BotColorMode.GenomAge); break;

					case Keys.G: Game.GenomInfo(GenomInfoMode.OneTime); break;
					case Keys.Y: Game.GenomInfo(GenomInfoMode.Periodical); break;

					case Keys.P: Game.ParallelToggle(); break;
					case Keys.C: Game.ChecksToggle(); break;

					case Keys.H: Game.HistToggle(); break;
					case Keys.F: Game.Fastest(); break;
					default: break;
				}
			}
		}

		public void Form1CursorReplace()
		{
			Game.CursorJump();
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
			MouseEventArgs me = (MouseEventArgs)e;
			Point coordinates = me.Location;
			var x = coordinates.X;
			var y = coordinates.Y;

			Data.LensX = x / Data.CellWidth;
			Data.LensY = y / Data.CellHeight;

			Game.LensJump();
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

		private void textBox2_TextChanged(object sender, EventArgs e)
		{

		}

		private void textBox4_TextChanged(object sender, EventArgs e)
		{

		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
		{

		}

		private void dataGridView1_ColumnHeaderMouseClick(
			object sender, DataGridViewCellMouseEventArgs e)
		{


			//public static int DataGridViewColumnIndex = 0;
			//public static ListSortDirection Direction = ListSortDirection.Ascending;


			DataGridViewColumn oldColumn = dataGridView1.Columns[Data.DgvColumnIndex];
			Data.DgvColumnIndex = e.ColumnIndex;
			DataGridViewColumn newColumn = dataGridView1.Columns[Data.DgvColumnIndex];


			if (oldColumn == newColumn)
			{
				Data.DgvDirection = Data.DgvDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
			}
			else
			{
				Data.DgvDirection = ListSortDirection.Descending;
				oldColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
			}

			// Sort the selected column.
			dataGridView1.Sort(newColumn, Data.DgvDirection);
			newColumn.HeaderCell.SortGlyphDirection = Data.DgvDirection == ListSortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending;

			Game.ColorDataGridView();
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			Game.ToggleLiveDataGridView();
		}

		private void textBox5_TextChanged(object sender, EventArgs e)
		{

		}

		private void checkBox2_CheckedChanged(object sender, EventArgs e)
		{
			Game.TogglePraDataGridView();
		}

		private async void button1_Click(object sender, EventArgs e)
		{
			Application.Restart();
			Environment.Exit(0);
		}

		private void trackBar2_Scroll(object sender, EventArgs e)
		{
			Data.MovedBiteStrength = trackBar2.Value;
		}

		private void checkBox3_CheckedChanged(object sender, EventArgs e)
		{
			Game.ToggleDelayForNewbie();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			Game.GenomInfo(GenomInfoMode.OneTime);
		}

		private void button3_Click(object sender, EventArgs e)
		{
			LensToggle();
		}

		private void checkBox4_CheckedChanged(object sender, EventArgs e)
		{
			Game.ToggleReduceEnergyAtStep();
		}

		private void button4_Click(object sender, EventArgs e)
		{
			PauseToggle();
		}

		private void button5_Click(object sender, EventArgs e)
		{
			StepPause();
		}

		private void textBox6_TextChanged(object sender, EventArgs e)
		{

		}
	}
}
