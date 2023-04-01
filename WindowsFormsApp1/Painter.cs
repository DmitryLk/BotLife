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
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace WindowsFormsApp1
{
	public class Painter : IDisposable
	{
		private PictureBox _pb;
		private Bitmap _btmp;
		private Bitmap _btmp2;
		private Graphics _gr;
		private ImageWrapper _iw;

		private SolidBrush _br;
		private Color _fon;
		private Label _label;
		private TextBox _textBox;

		private int _worldWidth;
		private int _worldHeight;
		private int _botWidth;
		private int _botHeight;
		private int _cnt;
		private DateTime _dt;
		private int _reportFrequency;

		public Tester _test;

		public Painter(PictureBox pb, Label label, TextBox textBox, Tester test)
		{
			_pb = pb;
			_label = label;
			_textBox = textBox;

			_br = new SolidBrush(Color.Red);
			_fon = Color.Gray;
			_cnt = 0;
			_dt = DateTime.Now;

			_test = test;
		}

		public void Configure(int worldWidth, int worldHeight, int botWidth, int botHeight, int reportFrequency)
		{
			_worldHeight = worldHeight;
			_worldWidth = worldWidth;
			_botHeight = botHeight;
			_botWidth = botWidth;
			_reportFrequency = reportFrequency;


			//_btmp = new Bitmap(worldWidth * botWidth, worldHeight * botHeight);
			//_gr = Graphics.FromImage(_btmp);

			// Инициализация при запуске
			_btmp = new Bitmap(worldWidth * botWidth, worldHeight * botHeight);
			_pb.Image = _btmp;
			//_gr = Graphics.FromImage(_pb.Image);

		}

		public void StartNewFrame()
		{
			//_btmp2 = new Bitmap(_worldWidth * _botWidth, _worldHeight * _botHeight);
			//_gr.Clear(_fon);
			_iw = new ImageWrapper(_btmp, false);
			//_gr.Clear(Form.ActiveForm.BackColor);
			//_pb.Invalidate();
		}

		public void DrawBotOnFrame(Bot bot)
		{
			//_gr.FillRectangle(_br, bot.X * _botWidth, bot.Y * _botHeight, _botWidth, _botHeight);
			//_btmp.SetPixel(bot.X * _botWidth, bot.Y * _botHeight, Color.Red);

			//Color.LightGray
			//Form.ActiveForm.BackColor
			//Color.Empty
			//Color.FromArgb(255,128,128,128)
			//Color.FromKnownColor(KnownColor.ActiveCaption)
			if (bot.X != bot.OldX || bot.Y != bot.OldY)
			{
				_iw.FillSquare(bot.OldX * _botWidth, bot.OldY * _botHeight, Color.FromKnownColor(KnownColor.ActiveCaption));
			}

			_iw.FillSquare(bot.X * _botWidth, bot.Y * _botHeight, Color.Red);


			//_iw[bot.X * _botWidth, bot.Y * _botHeight] = Color.Red;
			//_iw[bot.X * _botWidth, bot.Y * _botHeight + 1] = Color.Red;
			//_iw[bot.X * _botWidth + 1, bot.Y * _botHeight] = Color.Red;
			//_iw[bot.X * _botWidth + 1, bot.Y * _botHeight + 1] = Color.Red;
		}

		public void PaintFrame()
		{
			_iw.Dispose();

			//_pb.Image = _btmp2;
			//_pb.Update();
			_pb.Refresh();

			_cnt++;
			if (_cnt % _reportFrequency == 0)
			{
				var tms = (DateTime.Now - _dt).TotalSeconds;
				if (tms == 0) new Exception("tms == 0");
				var fps = _reportFrequency / tms;
				_dt = DateTime.Now;
				_label.Text = "fps: " + fps.ToString("#");
				_label.Update();

				_textBox.Text = _test.GetText();
				_textBox.Update();
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
