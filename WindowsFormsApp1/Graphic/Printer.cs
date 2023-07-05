using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.GameLogic;
using WindowsFormsApp1.Static;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace WindowsFormsApp1.Graphic
{
	public class Printer
	{
		private TextBox[] _textBoxes;
		private DataGridView _dgv;
		private Form _form;
		private DateTime _dt;


		public Printer(TextBox[] textBoxes, DataGridView dgv, Form form)
		{
			_textBoxes = textBoxes;
			_dgv = dgv;
			_form = form;
			_dt = DateTime.Now;
		}

		// 0 - на главной форме посередине
		// 1 - на главной форме сверху
		// 2 - на главной форме снизу
		// 3 - на второй форме сверху               только на паузе
		// 4 - на второй форме снизу                только на паузе
		// 5 - на главной форме различные режимы
		public void Print015()
		{
			var tms = (DateTime.Now - _dt).TotalSeconds;
			if (tms == 0) throw new Exception("tms == 0");
			var fps = Data.ReportFrequencyCurrent / tms;
			_dt = DateTime.Now;

			_textBoxes[0].Text = Test.GetText();
			_textBoxes[0].Update();

			_textBoxes[1].Text = Data.GetText(fps);
			_textBoxes[1].Update();

			_form.Text = Data.GetTextForCaption(fps);


			Print5();
		}

		public void Print2()
		{
			try
			{
				_textBoxes[2].Text = Genom.GetText();
				_textBoxes[2].Update();

                var minCurBots = 0;
                int.TryParse(_textBoxes[6].Text, out minCurBots);

				_dgv.DataSource = Genom.GetSortableBindingList(minCurBots);

				foreach (DataGridViewColumn column in _dgv.Columns)
				{
					column.SortMode = DataGridViewColumnSortMode.Programmatic;
				}
				_dgv.Columns[0].Width = 120;	//Геном
				_dgv.Columns[1].Width = 20;
				_dgv.Columns[2].Width = 60;     //Живых
				_dgv.Columns[3].Width = 60;     //Всего
				_dgv.Columns[4].Width = 40;     //Возраст
				_dgv.Columns[5].Width = 40;     //Ср. возр. ботов
				_dgv.Columns[6].Width = 40;     //Активных
				_dgv.Columns[7].Width = 80;

				//             HeaderText = "Геном",
				//             HeaderText = "",
				//             HeaderText = "Живых",
				//             HeaderText = "Всего",
				//             HeaderText = "Возраст",
				//             HeaderText = "Ср. возр. ботов",
				//	HeaderText = "Активных",


				_dgv.Sort(_dgv.Columns[Data.DgvColumnIndex], Data.DgvDirection);

				ColorDataGridView();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				throw;
			}
		}

		public void ColorDataGridView()
		{
			for (var i = 0; i < 10 && i < _dgv.Rows.Count; i++)
			{
				var cv = (Color)_dgv.Rows[i].Cells[1].Value;
				_dgv.Rows[i].Cells[1].Style.BackColor = Color.FromArgb(cv.A, cv.R, cv.G, cv.B);
				_dgv.Rows[i].Cells[1].Style.ForeColor = Color.FromArgb(cv.A, cv.R, cv.G, cv.B);
			}
		}

		public void Print3(Bot1 bot)
		{
			_textBoxes[3].Text = bot != null ? bot.GetText1() : "";
			_textBoxes[3].Update();

			//_cnt++;
			//if (_cnt % Data.ReportFrequencyCurrent == 0)
			//{
			//    var tms = (DateTime.Now - _dt).TotalSeconds;
			//    if (tms == 0) throw new Exception("tms == 0");
			//    var fps = Data.ReportFrequencyCurrent / tms;
			//    _dt = DateTime.Now;

			//    _textBoxes[2].Text = _test.GetText();
			//    _textBoxes[2].Update();
			//}
		}

		public void Print4(Bot1 bot, int delta)
		{
			_textBoxes[4].Text = bot != null ? bot.GetText2(delta) : "";
			_textBoxes[4].Update();
		}


		public void Print5()
		{
			_textBoxes[5].Text = Data.GetText2();
			_textBoxes[5].Update();
		}
	}
}
