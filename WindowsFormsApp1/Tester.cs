using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
	public class Tester
	{
		public PictureBox _pb;
		public Bitmap _btmp;
		public Graphics _gr;
		public SolidBrush _br;
		public Color _fon;
		public Label _label;

		private int _botWidth;
		private int _botHeight;
		private int _cnt;
		private DateTime _dt;

		public Painter(PictureBox pb, Label label)
		{
			_pb = pb;
			_label = label;
			_br = new SolidBrush(Color.Red);
			_fon = Color.Gray;
			_cnt = 0;
			_dt =	DateTime.Now;
		}

		public void Configure(int worldWidth, int worldHeight, int botWidth, int botHeight)
		{
			_botHeight = botHeight;
			_botWidth = botWidth;
			_btmp = new Bitmap(worldWidth * botWidth, worldHeight * botHeight);
			_gr = Graphics.FromImage(_btmp);
		}

		public void StartNewFrame()
		{
			_gr.Clear(_fon);
		}

		public void DrawBotOnFrame(Bot bot)
		{
			_gr.FillRectangle(_br, bot.X * _botWidth, bot.Y * _botHeight, _botWidth, _botHeight);
		}

		public void PaintFrame() 
		{
			_pb.Image = _btmp;
			_pb.Update();
			//_pb.Refresh();

			_cnt++;
			if (_cnt % 10 == 0)
			{
				var tms = (DateTime.Now - _dt).TotalSeconds;
				if (tms == 0) new Exception("tms == 0");
				var fps = 10/tms;
				_dt = DateTime.Now;
				_label.Text = "fps: " + fps.ToString("#");
				_label.Update();
			}
		}
	}
}
