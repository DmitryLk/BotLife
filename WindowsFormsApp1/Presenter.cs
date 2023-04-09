using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.GameLogic;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace WindowsFormsApp1
{
	public class Presenter : IDisposable
	{
		private PictureBox _pb;
		private Bitmap _btmp;
		private Graphics _gr;
		private ImageWrapper _iw;
		private GameData _data;

		private SolidBrush _br;
		private Color _fon;
		private Label[] _labels;
		private TextBox[] _textBoxes;

		private int _cellWidth;
		private int _cellHeight;
		private int _cnt;
		private DateTime _dt;

		public Tester _test;

		public Presenter(PictureBox pb, Label[] labels, TextBox[] textBoxes, Tester test)
		{
			_pb = pb;
			_labels = labels;
			_textBoxes = textBoxes;

			_br = new SolidBrush(Color.Red);
			_fon = Color.FromKnownColor(KnownColor.ActiveCaption);
			_cnt = 0;
			_dt = DateTime.Now;

			_test = test;

		}

		public void Configure(GameData data)
		{
			_data = data;

			var bitmapWidth = _data.WorldWidth * _data.CellWidth;
			var bitmapHeight = _data.WorldHeight * _data.CellHeight;


			_pb.Size = new System.Drawing.Size(bitmapWidth, bitmapHeight);

			_cellHeight = _data.CellHeight;
			_cellWidth = _data.CellWidth;

			//_btmp = new Bitmap(worldWidth * botWidth, worldHeight * botHeight);
			//_gr = Graphics.FromImage(_btmp);

			// Инициализация при запуске
			_btmp = new Bitmap(bitmapWidth, bitmapHeight);
			_pb.Image = _btmp;
			//_gr = Graphics.FromImage(_pb.Image);

			_iw = new ImageWrapper(_btmp, false);

		}
		public void StartNewFrame()
		{
			//_btmp2 = new Bitmap(_worldWidth * _botWidth, _worldHeight * _botHeight);
			//_gr.Clear(_fon);
			_iw.StartEditing();
			//_gr.Clear(Form.ActiveForm.BackColor);
			//_pb.Invalidate();
		}

		public void DrawCellOnFrame(int x, int y, Color? color = null)
		{
			//_gr.FillRectangle(_br, bot.X * _botWidth, bot.Y * _botHeight, _botWidth, _botHeight);
			//_btmp.SetPixel(bot.X * _botWidth, bot.Y * _botHeight, Color.Red);

			//Color.LightGray
			//Form.ActiveForm.BackColor
			//Color.Free
			//Color.FromArgb(255,128,128,128)
			//Color.FromKnownColor(KnownColor.ActiveCaption)

			//if (bot.P.X != bot.Old.X || bot.P.Y != bot.Old.Y)
			//{
			//	_iw.FillSquare(bot.P.X * _cellWidth, bot.P.Y * _cellHeight, _cellWidth, Color.FromKnownColor(KnownColor.ActiveCaption));
			//}

			_iw.FillSquare(x * _cellWidth, y * _cellHeight, _cellWidth, color ?? _fon);


			//_iw[bot.X * _botWidth, bot.Y * _botHeight] = Color.Red;
			//_iw[bot.X * _botWidth, bot.Y * _botHeight + 1] = Color.Red;
			//_iw[bot.X * _botWidth + 1, bot.Y * _botHeight] = Color.Red;
			//_iw[bot.X * _botWidth + 1, bot.Y * _botHeight + 1] = Color.Red;
		}

		public void PaintFrame()
		{
			_iw.EndEditing();

			//_pb.Image = _btmp2;
			//_pb.Update();

			//if (_pb.InvokeRequired)
			//{
			//	_pb.BeginInvoke(new Action(() => _pb.Refresh()));
			//}
			//else
			//	_pb.Refresh();


			_pb.Refresh();
			//if (_pb.InvokeRequired)
			//{
			//	_pb.Invoke((MethodInvoker)delegate
			//	{
			//		_pb.Refresh();
			//	});
			//}
			//else
			//	_pb.Refresh();
		}

		public void PrintInfo()
		{
			_cnt++;
			if (_cnt % _data.ReportFrequencyCurrent == 0)
			{
				var tms = (DateTime.Now - _dt).TotalSeconds;
				if (tms == 0) throw new Exception("tms == 0");
				var fps = _data.ReportFrequencyCurrent / tms;
				_dt = DateTime.Now;

				_labels[0].Text = "fps: " + fps.ToString("#");
				_labels[0].Update();

				//_labels[1].Text = "step: " + _data.CurrentStep;
				//_labels[1].Update();

				_textBoxes[0].Text = _test.GetText();
				_textBoxes[0].Update();

				_textBoxes[1].Text = _data.GetText();
				_textBoxes[1].Update();

				//if (_label.InvokeRequired)
				//{
				//	_label.Invoke((MethodInvoker)delegate
				//	{
				//		_label.Text = "fps: " + fps.ToString("#");
				//	});
				//}
				//else
				//	_label.Text = "fps: " + fps.ToString("#");
				////_label.Update();

				//if (_textBox.InvokeRequired)
				//{
				//	_textBox.Invoke((MethodInvoker)delegate
				//	{
				//		_textBox.Text = _test.GetText();
				//	});
				//}
				//else
				//	_textBox.Text = _test.GetText();
				////_textBox.Update();
			}
		}

		public void Dispose()
		{
			_btmp.Dispose();
			_gr.Dispose();
			_iw.Dispose();
			_br.Dispose();
		}
	}
}
