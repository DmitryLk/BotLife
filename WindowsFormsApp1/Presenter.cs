using System;
using System.Collections.Generic;
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

namespace WindowsFormsApp1
{
    public class Presenter : IDisposable
    {
        private Bitmap _mainBitmap;
        private ImageWrapper _mainImageWrapper;
        private int _cellWidth;
        private int _cellHeight;

        private Bitmap _lensBitmap;
        private ImageWrapper _lensImageWrapper;
        private int _lensCellHeight;
        private int _lensCellWidth;

        private Bitmap _cursorBitmap;
        private ImageWrapper _cursorImageWrapper;
        private int _codeCellWidth;
        private int _codeCellHeight;
        private int _xStartCodeCell;
        private Brush _textBrush;
        private Brush _smallTextBrush;
        private Font _font1;
        private Font _font2;
        private Font _font3;
        private StringFormat _stringFormat;
        private const float CursorPart = 0.9f;

        private Graphics _cursorGraphics;

        private Color _fon;
        private TextBox[] _textBoxes;
        private PictureBox[] _pictureBoxes;

        private int _cnt;
        private DateTime _dt;


        public Presenter(PictureBox[] pictureBoxes, TextBox[] textBoxes)
        {
            _pictureBoxes = pictureBoxes;
            _textBoxes = textBoxes;

            _fon = Color.FromKnownColor(KnownColor.ActiveCaption);
            _cnt = 0;
            _dt = DateTime.Now;


            ConfigureMainBitmap();
            ConfigureLensBitmap();
            ConfigureCursorBitmap();
        }

        public void ConfigureMainBitmap()
        {
            _cellHeight = Data.CellHeight;
            _cellWidth = Data.CellWidth;
            var mainBitmapWidth = Data.WorldWidth * Data.CellWidth;
            var mainBitmapHeight = Data.WorldHeight * Data.CellHeight;
            _pictureBoxes[0].Size = new System.Drawing.Size(mainBitmapWidth, mainBitmapHeight);
            _mainBitmap = new Bitmap(mainBitmapWidth, mainBitmapHeight);
            _pictureBoxes[0].Image = _mainBitmap;
            _mainImageWrapper = new ImageWrapper(_mainBitmap, false);
        }

        public void ConfigureLensBitmap()
        {
            _lensCellHeight = Data.LensCellWidth;
            _lensCellWidth = Data.LensCellHeight;
            var lensBitmapWidth = Data.LensWidth * Data.LensCellWidth;
            var lensBitmapHeight = Data.LensHeight * Data.LensCellHeight;
            _pictureBoxes[1].Size = new System.Drawing.Size(lensBitmapWidth, lensBitmapHeight);
            _lensBitmap = new Bitmap(lensBitmapWidth, lensBitmapHeight);
            _pictureBoxes[1].Image = _lensBitmap;
            _lensImageWrapper = new ImageWrapper(_lensBitmap, false);
        }


        public void ConfigureCursorBitmap()
        {
            var cursorBitmapWidth = 350;
            var cursorBitmapHeight = 300;

            _codeCellWidth = (int)((cursorBitmapWidth * CursorPart) / 8);
            _codeCellHeight = (int)(cursorBitmapHeight / 8);
            _xStartCodeCell = (int)(cursorBitmapWidth * (1 - CursorPart));

            _pictureBoxes[2].Size = new System.Drawing.Size(cursorBitmapWidth, cursorBitmapHeight);
            _cursorBitmap = new Bitmap(cursorBitmapWidth, cursorBitmapHeight);
            _pictureBoxes[2].Image = _cursorBitmap;
            _cursorImageWrapper = new ImageWrapper(_cursorBitmap, true);

            //For text
            _cursorGraphics = Graphics.FromImage(_cursorBitmap);
            //_textBrush = new SolidBrush(Color.Black);
            _smallTextBrush = new SolidBrush(Color.Black);
            _cursorGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            //_cursorGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //_cursorGraphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            _cursorGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            _cursorGraphics.TextContrast = 0;
            //_font = new Font("Arial", 10);
            //_font = new Font("Microsoft Sans Serif", 10);
            //_font = new Font("Times", 10);
            //_font = new Font("Calibri", 10);
            _font1 = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
            _font2 = new Font("Calibri", 10, FontStyle.Regular);
            _font3 = new Font("Arial", 6, FontStyle.Regular);
            _stringFormat = new StringFormat();
            _stringFormat.LineAlignment = StringAlignment.Center;
            _stringFormat.Alignment = StringAlignment.Center;
        }

        //MAIN////////////////////////////////////////////////////////////////////////////////////////////////////
        public void StartNewFrame(BitmapCopyType type)
        {
            _mainImageWrapper.StartEditing(type);
        }

        public void DrawObjectOnFrame(int x, int y, Color? color = null)
        {
            _mainImageWrapper.FillSquare(x * _cellWidth, y * _cellHeight, _cellWidth, color ?? _fon);
        }

        public void DrawLensOnFrame(int x, int y, int sizeX, int sizeY, Color color)
        {
            _mainImageWrapper.EmptySquare1(x * _cellWidth, y * _cellHeight, sizeX * _cellWidth, sizeY * _cellHeight, color);
        }

        public void IntermediateFrameSave()
        {
            _mainImageWrapper.IntervalEditing();
        }

        public void SendFrameToScreen()
        {
            _mainImageWrapper.EndEditing();
            _pictureBoxes[0].Refresh();
        }

        //LENS////////////////////////////////////////////////////////////////////////////////////////////////////
        public void StartNewLensFrame(BitmapCopyType type)
        {
            _lensImageWrapper.StartEditing(type);
        }

        public void DrawObjectOnLensFrame(int x, int y, Color? color, int? dir)
        {


            _lensImageWrapper.FillSquare(x * _lensCellWidth + 1, y * _lensCellHeight + 1, _lensCellWidth - 2, color ?? _fon);

            if (dir.HasValue)
            {
                var (dX, dY) = Dir.GetDeltaDirection(dir.Value);

                _lensImageWrapper.Line(
                    _lensCellWidth * x + _lensCellWidth / 2,
                    _lensCellHeight * y + _lensCellHeight / 2,
                    (int)(_lensCellWidth * x + _lensCellWidth * (1 + dX) / 2),
                    (int)(_lensCellHeight * y + _lensCellHeight * (1 + dY) / 2),
                    Color.Red);
            }
        }

        public void DrawCursorOnLens(int x, int y, Color? color = null)
        {
            _lensImageWrapper.EmptySquare1(x * _lensCellWidth, y * _lensCellHeight, _lensCellWidth, _lensCellHeight, color ?? _fon);
        }

        public void SendLensFrameToScreen()
        {
            _lensImageWrapper.EndEditing();
            _pictureBoxes[1].Refresh();
        }

        //CURSOR////////////////////////////////////////////////////////////////////////////////////////////////////
        public void StartNewCursorFrame(BitmapCopyType type)
        {
            _cursorImageWrapper.StartEditing(type);
        }

        public void ClearGraphicsOnCursorFrame()
        {
            //_cursorGraphics.Clear(_fon);
            //_cursorGraphics.Clear(Color.White);
            _cursorImageWrapper.ClearBitmap();
        }

        public void DrawCodeCellOnCursorFrame(int x, int y, Color? color = null)
        {
            _cursorImageWrapper.EmptySquare2(_xStartCodeCell + x * _codeCellWidth + 1, y * _codeCellHeight + 1, _codeCellWidth - 2, _codeCellHeight - 2, color ?? _fon);
        }

        public void DrawCodeArrowOnCursorFrame(int x1, int y1, int x2, int y2, Color color)
        {
            _cursorImageWrapper.Line(
                _xStartCodeCell+ _codeCellWidth * x1 + 3,
                _codeCellHeight * y1 + 3,
                _xStartCodeCell+ _codeCellWidth * x2 + 3,
                _codeCellHeight * y2 + 3,
                color);
        }

        public void DrawTextOnCursorFrame(int x, int y, string code, Color color)
        {
            var textBrush = new SolidBrush(color);
            _cursorGraphics.DrawString(code, color == Color.Black ? _font2 : _font1, textBrush, _xStartCodeCell + x * _codeCellWidth + 15, y * _codeCellHeight + 12, _stringFormat);
            //_cursorGraphics.Flush();
        }

        public void DrawSmallTextOnCursorFrame1(int x, int y, string code, Color color)
        {
            _cursorGraphics.DrawString(code, _font3, _smallTextBrush, _xStartCodeCell + x * _codeCellWidth + 30, y * _codeCellHeight + 7, _stringFormat);
            //_cursorGraphics.Flush();
        }

        public void DrawSmallTextOnCursorFrame2(int x, int y, string code, Color color)
        {
            _cursorGraphics.DrawString(code, _font3, _smallTextBrush, _xStartCodeCell + x * _codeCellWidth + 19, y * _codeCellHeight + 28, _stringFormat);
            //_cursorGraphics.Flush();
        }

        public void DrawOtherTextOnCursorFrame(int x, int y, string code)
        {
            _cursorGraphics.DrawString(code, _font2, _smallTextBrush, x , y);
            //_cursorGraphics.Flush();
        }

        public void SendCursorFrameToScreen()
        {
            _cursorImageWrapper.EndEditing();
            _pictureBoxes[2].Refresh();
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        public void PrintInfo()
        {
            _cnt++;
            if (_cnt % Data.ReportFrequencyCurrent == 0)
            {
                var tms = (DateTime.Now - _dt).TotalSeconds;
                if (tms == 0) throw new Exception("tms == 0");
                var fps = Data.ReportFrequencyCurrent / tms;
                _dt = DateTime.Now;

                _textBoxes[0].Text = Test.GetText();
                _textBoxes[0].Update();

                _textBoxes[1].Text = Data.GetText(fps);
                _textBoxes[1].Update();

                _textBoxes[2].Text = Genom.GetText();
                _textBoxes[2].Update();
            }
        }

        public void PrintObjectInfo1(Bot1 bot)
        {
            if (bot != null)
            {
                _textBoxes[3].Text = bot.GetText1();
            }
            else
            {
                _textBoxes[3].Text = "";
            }
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

        public void PrintObjectInfo2(Bot1 bot, int delta)
        {
            if (bot != null)
            {
                _textBoxes[4].Text = bot.GetText2(delta);
            }
            else
            {
                _textBoxes[4].Text = "";
            }
            _textBoxes[4].Update();
        }
        public void Dispose()
        {
            _mainBitmap.Dispose();
            _cursorGraphics.Dispose();
            _mainImageWrapper.Dispose();
        }
    }
}
