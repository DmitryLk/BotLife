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

namespace WindowsFormsApp1.Graphic
{
    public class Printer
    {
		private int _cnt;
		private TextBox[] _textBoxes;
		private Form _form;
		private DateTime _dt;


		public Printer(TextBox[] textBoxes, Form form)
        {
			_textBoxes = textBoxes;
			_form = form;
			_cnt = 0;
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
            if (++_cnt % Data.ReportFrequencyCurrent == 0)
            {
                var tms = (DateTime.Now - _dt).TotalSeconds;
                if (tms == 0) throw new Exception("tms == 0");
                var fps = Data.ReportFrequencyCurrent / tms;
                _dt = DateTime.Now;

                _textBoxes[0].Text = Test.GetText();
                _textBoxes[0].Update();

                _textBoxes[1].Text = Data.GetText(fps);
                _textBoxes[1].Update();

                //_textBoxes[2].Text = Genom.GetText();
                //_textBoxes[2].Update();

                _form.Text = $"Bots: {Data.GetTextForCaption()}";


				Print5();
			}
        }

		public void Print2(GenomInfoMode mode)
		{
            _textBoxes[2].Text = Genom.GetText(mode);
            _textBoxes[2].Update();
        }

		public void Print3(Bot1 bot)
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

        public void Print4(Bot1 bot, int delta)
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


		public void Print5()
		{
			_textBoxes[5].Text = Data.GetText2();
			_textBoxes[5].Update();
		}
	}
}
