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
        private Bitmap _mainBitmap;
        private ImageWrapper _mainImageWrapper;
        private int _cellWidth;
        private int _cellHeight;

        private PictureBox _lensPictureBox;
        private Bitmap _lensBitmap;
        private ImageWrapper _lensImageWrapper;
        private int _lensCellHeight;
        private int _lensCellWidth;

        private PictureBox _cursorPictureBox;
        private Bitmap _cursorBitmap;
        private ImageWrapper _cursorImageWrapper;
        private int _codeCellWidth;
        private int _codeCellHeight;
        private int _xStartCodeCell;


        private Graphics _gr;
        private GameData _data;

        private Color _fon;
        private Label[] _labels;
        private TextBox[] _textBoxes;

        private int _cnt;
        private DateTime _dt;

        public Tester _test;

        public Presenter(
            GameData data,
            Tester test,
            PictureBox mainPictureBox,
            PictureBox lensPictureBox,
            PictureBox cursorPictureBox,
            Label[] labels,
            TextBox[] textBoxes)
        {
            _mainPictureBox = mainPictureBox;
            _lensPictureBox = lensPictureBox;
            _cursorPictureBox = cursorPictureBox;

            _labels = labels;
            _textBoxes = textBoxes;

            _fon = Color.FromKnownColor(KnownColor.ActiveCaption);
            _cnt = 0;
            _dt = DateTime.Now;

            _test = test;
            _data = data;

            ConfigureMainBitmap();
            ConfigureLensBitmap();
            ConfigureCursorBitmap();
        }

        public void ConfigureMainBitmap()
        {
            _cellHeight = _data.CellHeight;
            _cellWidth = _data.CellWidth;
            var mainBitmapWidth = _data.WorldWidth * _data.CellWidth;
            var mainBitmapHeight = _data.WorldHeight * _data.CellHeight;
            _mainPictureBox.Size = new System.Drawing.Size(mainBitmapWidth, mainBitmapHeight);
            _mainBitmap = new Bitmap(mainBitmapWidth, mainBitmapHeight);
            _mainPictureBox.Image = _mainBitmap;
            _mainImageWrapper = new ImageWrapper(_mainBitmap);
        }

        public void ConfigureLensBitmap()
        {
            _lensCellHeight = _data.LensCellWidth;
            _lensCellWidth = _data.LensCellHeight;
            var lensBitmapWidth = _data.LensWidth * _data.LensCellWidth;
            var lensBitmapHeight = _data.LensHeight * _data.LensCellHeight;
            _lensPictureBox.Size = new System.Drawing.Size(lensBitmapWidth, lensBitmapHeight);
            _lensBitmap = new Bitmap(lensBitmapWidth, lensBitmapHeight);
            _lensPictureBox.Image = _lensBitmap;
            _lensImageWrapper = new ImageWrapper(_lensBitmap);
        }


        public void ConfigureCursorBitmap()
        {
            var cursorBitmapWidth = 200;
            var cursorBitmapHeight = 200;

            _codeCellWidth = (int)((cursorBitmapWidth * 0.8) / 8);
            _codeCellHeight = (int)(cursorBitmapHeight / 8);
            _xStartCodeCell = (int)(cursorBitmapWidth * 0.2);



            _cursorPictureBox.Size = new System.Drawing.Size(cursorBitmapWidth, cursorBitmapHeight);
            _cursorBitmap = new Bitmap(cursorBitmapWidth, cursorBitmapHeight);
            _cursorPictureBox.Image = _cursorBitmap;
            _cursorImageWrapper = new ImageWrapper(_cursorBitmap);
            _gr = Graphics.FromImage(_cursorBitmap);
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
            _mainImageWrapper.EmptySquare(x * _cellWidth, y * _cellHeight, sizeX * _cellWidth, sizeY * _cellHeight, color);
        }

        public void IntermediateFrameSave()
        {
            _mainImageWrapper.IntervalEditing();
        }

        public void SendFrameToScreen()
        {
            _mainImageWrapper.EndEditing();
            _mainPictureBox.Refresh();
        }

        //LENS////////////////////////////////////////////////////////////////////////////////////////////////////
        public void StartNewLensFrame(BitmapCopyType type)
        {
            _lensImageWrapper.StartEditing(type);
        }

        public void DrawObjectOnLensFrame(int x, int y, Color? color = null)
        {
            _lensImageWrapper.FillSquare(x * _lensCellWidth + 1, y * _lensCellHeight + 1, _lensCellWidth - 2, color ?? _fon);
        }

        public void DrawCursorOnLens(int x, int y, Color? color = null)
        {
            _lensImageWrapper.EmptySquare(x * _lensCellWidth, y * _lensCellHeight, _lensCellWidth, _lensCellHeight, color ?? _fon);
        }

        public void SendLensFrameToScreen()
        {
            _lensImageWrapper.EndEditing();
            _lensPictureBox.Refresh();
        }

        //CURSOR////////////////////////////////////////////////////////////////////////////////////////////////////
        public void StartNewCursorFrame(BitmapCopyType type)
        {
            _cursorImageWrapper.StartEditing(type);
        }

        public void DrawCodeOnCursorFrame(int x, int y, string code, Color? color = null)
        {
            _cursorImageWrapper.EmptySquare(_xStartCodeCell + x * _codeCellWidth + 1, y * _codeCellHeight + 1, _codeCellWidth - 2, _codeCellHeight - 2, color ?? _fon);
        }

        public void SendCursorFrameToScreen()
        {
            _cursorImageWrapper.EndEditing();
            _cursorPictureBox.Refresh();
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////

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

        public void PrintObjectInfo(Bot1 bot)
        {
            //_cnt++;
            //if (_cnt % _data.ReportFrequencyCurrent == 0)
            //{
            //    var tms = (DateTime.Now - _dt).TotalSeconds;
            //    if (tms == 0) throw new Exception("tms == 0");
            //    var fps = _data.ReportFrequencyCurrent / tms;
            //    _dt = DateTime.Now;

            //    _textBoxes[2].Text = _test.GetText();
            //    _textBoxes[2].Update();
            //}
        }

        public void Dispose()
        {
            _mainBitmap.Dispose();
            _gr.Dispose();
            _mainImageWrapper.Dispose();
        }
    }
}
