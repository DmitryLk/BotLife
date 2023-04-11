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
        private PictureBox _mainPictureBox;
        private PictureBox _lensPictureBox;
		private Bitmap _mainBitmap;
		private Graphics _gr;
		private ImageWrapper _mainImageWrapper;
		private GameData _data;

		private Color _fon;
		private Label[] _labels;
		private TextBox[] _textBoxes;

		private int _cellWidth;
		private int _cellHeight;
		private int _cnt;
		private DateTime _dt;

		public Tester _test;

		public Presenter(GameData data, PictureBox mainPictureBox, PictureBox lensPictureBox, Label[] labels, TextBox[] textBoxes, Tester test)
		{
            _mainPictureBox = mainPictureBox;
            _lensPictureBox = lensPictureBox;
			_labels = labels;
			_textBoxes = textBoxes;

			_fon = Color.FromKnownColor(KnownColor.ActiveCaption);
			_cnt = 0;
			_dt = DateTime.Now;

            _test = test;
            _data = data;
            
            ConfigureMainBitmap();
        }

		public void ConfigureMainBitmap()
		{
			var bitmapWidth = _data.WorldWidth * _data.CellWidth;
			var bitmapHeight = _data.WorldHeight * _data.CellHeight;
			_mainPictureBox.Size = new System.Drawing.Size(bitmapWidth, bitmapHeight);
            _mainBitmap = new Bitmap(bitmapWidth, bitmapHeight);
            _mainPictureBox.Image = _mainBitmap;
			_mainImageWrapper = new ImageWrapper(_mainBitmap);
		}

        public void StartNewFrame(bool additionalGraphics)
		{
			_mainImageWrapper.StartEditing(additionalGraphics ? BitmapCopyType.EditCopyScreenBitmapWithAdditionalArray : BitmapCopyType.EditDirectlyScreenBitmap_Fastest);
		}

		public void DrawObjectOnFrame(int x, int y, Color? color = null)
		{
			_mainImageWrapper.FillSquare(x * _cellWidth, y * _cellHeight, _cellWidth, color ?? _fon);
		}

		public void DrawLensOnFrame(int x, int y, int sizeX, int sizeY, Color color)
		{
			_mainImageWrapper.Square(x * _cellWidth, y * _cellHeight, sizeX * _cellWidth, sizeY * _cellHeight, color);
		}

		public void IntermediateFrameSave()
		{
			_mainImageWrapper.IntervalEditing();
		}

		public void PaintFrame()
		{
			_mainImageWrapper.EndEditing();
			_mainPictureBox.Refresh();
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

				_textBoxes[0].Text = _test.GetText();
				_textBoxes[0].Update();

				_textBoxes[1].Text = _data.GetText(fps);
				_textBoxes[1].Update();
			}
		}

		public void Dispose()
		{
			_mainBitmap.Dispose();
			_gr.Dispose();
			_mainImageWrapper.Dispose();
		}
	}
}
