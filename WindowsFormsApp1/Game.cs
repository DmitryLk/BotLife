﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WindowsFormsApp1;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.GameLogic;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public class Game
    {
        public Presenter _PRESENTER;
        public World _world;
        public GameData _data;
        public Tester _test;
        public System.Windows.Forms.Timer timer;


        private readonly object _sync = new object();
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public Game(GameData data, Presenter presenter, Tester test)
        {
            _data = data;
            _PRESENTER = presenter;

            //timer = new System.Windows.Forms.Timer();
            //timer.Tick += new System.EventHandler(timer_Tick);
            //timer.Interval = 1;
            //timer.Enabled = false;

            _world = new World(_data);

            _test = test;
            _test.InitInterval(0, "BotsAction();");
            _test.InitInterval(1, "RedrawWorld();");
            _test.InitInterval(2, "DrawBotOnFrame(bots[botNumber]);");
            _test.InitInterval(3, "PaintFrame();");
            _test.InitInterval(4, "PrintInfo();");

        }

        public async Task Start()
        {
            _data.Started = true;
            //timer.Enabled = true;
            _world.Initialize();
        }

        public async Task Work()
        {
            if (_data.Worked) return;
            await _semaphoreSlim.WaitAsync();
            try
            {
                if (_data.Worked) return;
                _data.Worked = true;
                do
                {
                    await Step();
                }
                while (!_data.PausedMode);
                _data.Worked = false;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task Step()
        {
            _test.BeginInterval(0);
            await Task.Run(() => _world.Step());
            _test.EndBeginInterval(0, 1);

            //await Task.Factory.StartNew(() => WorldStep(), TaskCreationOptions.LongRunning);
            //_world.Step();

            if (_data.Drawed)
            {
                RedrawWorld(_data.Lens);
            }
            else
            {
                _test.EndBeginInterval(1, 2);
                _test.EndBeginInterval(2, 3);
                _test.EndBeginInterval(3, 4);
            }
            
            _PRESENTER.PrintInfo();
            
            _test.EndBeginInterval(4, 0);
            //await Task.Delay(5000);
        }

        private void RedrawWorld(bool additionalGraphics)
        {
            _PRESENTER.StartNewFrame(additionalGraphics ? BitmapCopyType.EditCopyScreenBitmapWithAdditionalArray : BitmapCopyType.EditDirectlyScreenBitmap_Fastest);
            
            _test.EndBeginInterval(1, 2);

            // Рисование изменившихся ячеек на основном битмапе экрана (сразу не отображаются)
            //======================================
            _data.NumberOfChangedCellsForInfo = _data.NumberOfChangedCells;
            for (var i = 0; i < _data.NumberOfChangedCells; i++)
            {
                var obj = _data.ChangedCells[i];

                _PRESENTER.DrawObjectOnFrame(obj.X, obj.Y, obj.Color);
                _data.ChWorld[obj.X, obj.Y] = 0;
            }
            _data.NumberOfChangedCells = 0;
            //======================================

            _test.EndBeginInterval(2, 3);

            if (additionalGraphics) AdditionalGraphics();

            _PRESENTER.SendFrameToScreen();
            //await Task.Delay(1);
            _test.EndBeginInterval(3, 4);
        }

        private void AdditionalGraphics()
        {
            _PRESENTER.IntermediateFrameSave();  // сохранить в промежуточный массив экран без дополнительной графики
            if (_data.Lens)
            {
                DrawLens();
                if (_data.PausedMode) DrawCursorInfo();
            }
        }

        private void DrawLens()
        {
            _PRESENTER.DrawLensOnFrame(_data.LensX, _data.LensY, _data.LensWidth, _data.LensHeight, Color.Black);  // рмсование лупы

            _PRESENTER.StartNewLensFrame(BitmapCopyType.EditEmptyArray);
            Color? color;
            // Выберем из _data.World[nX, nY] все что попадет в лупу
            for (var y = _data.LensY; y < _data.LensY + _data.LensHeight; y++)
            {
                for (var x = _data.LensX; x < _data.LensX + _data.LensWidth; x++)
                {

                    var cont = _data.World[x, y];

                    color = cont switch
                    {
                        0 => null,
                        65500 => Color.Green,
                        _ => cont >= 1 && cont <= _data.CurrentNumberOfBots ? ((Bot1)_data.Bots[cont]).Genom.Color : throw new Exception("var color = cont switch")
                    };

                    _PRESENTER.DrawObjectOnLensFrame(x - _data.LensX, y - _data.LensY, color);
                }
            }
            _PRESENTER.DrawCursorOnLens(_data.CursorX, _data.CursorY, Color.Black);  // рмсование курсора.
            _PRESENTER.SendLensFrameToScreen();
        }

        // информация по курсору
        private void DrawCursorInfo()
        {
            var cursorCont = _data.World[_data.LensX + _data.CursorX, _data.LensY + _data.CursorY];

            if (cursorCont >= 1 && cursorCont <= _data.CurrentNumberOfBots)
            {
                var bot = (Bot1)_data.Bots[cursorCont];

                _PRESENTER.StartNewCursorFrame(BitmapCopyType.EditEmptyArray);
                Color? color;

                for (var i = 0; i < _data.GenomLength; i++)
                {
                    var x = i % 8;
                    var y = i / 8;
                    color = i == bot.Pointer ? Color.Aqua : Color.DarkCyan;

                    _PRESENTER.DrawCodeOnCursorFrame(x, y, color);
                }

                _PRESENTER.SendCursorFrameToScreenWithoutRefresh();

                for (var i = 0; i < _data.GenomLength; i++)
                {
                    var code = bot.Genom.Code[i];
                    var x = i % 8;
                    var y = i / 8;
                    color = i == bot.Pointer ? Color.Aqua : Color.DarkCyan;

                    _PRESENTER.DrawTextOnCursorFrame(x, y, code.ToString(), color);
                }

                _PRESENTER.CursorFrameRefreshScreen();

                _PRESENTER.PrintObjectInfo(bot);

            }
            else
            {
                _PRESENTER.StartNewCursorFrame(BitmapCopyType.EditEmptyArray);
                _PRESENTER.SendCursorFrameToScreen();
                _PRESENTER.PrintObjectInfo(null);
            }
        }


        #region for Form
            public void MutationToggle()
        {
            _data.Mutation = !_data.Mutation;
        }

        public void DrawedToggle()
        {
            _data.Drawed = !_data.Drawed;
            _data.ReportFrequencyCurrent = _data.Drawed ? _data.ReportFrequencyDrawed : _data.ReportFrequencyNoDrawed;
        }

        public void PausedToggle()
        {
            _data.PausedMode = !_data.PausedMode;
        }

        public bool Paused
        {
            get { return _data.PausedMode; }
        }

        public bool Started
        {
            get { return _data.Started; }
        }

        public void Lens(bool mode)
        {
            _data.Lens = mode;
        }
        public void LensLeft()
        {
            if (_data.Drawed && _data.Lens && _data.LensX > 0)
            {
                _data.LensX--;
                if (_data.PausedMode) RedrawWorld(_data.Lens);
            }
        }
        public void LensRight()
        {
            if (_data.Drawed && _data.Lens && _data.LensX < _data.WorldWidth - _data.LensWidth)
            {
                _data.LensX++;
                if (_data.PausedMode) RedrawWorld(_data.Lens);
            }
        }
        public void LensUp()
        {
            if (_data.Drawed && _data.Lens && _data.LensY > 0)
            {
                _data.LensY--;
                if (_data.PausedMode) RedrawWorld(_data.Lens);
            }
        }
        public void LensDown()
        {
            if (_data.Lens && _data.LensY < _data.WorldHeight - _data.LensHeight)
            {
                _data.LensY++;
                if (_data.PausedMode) RedrawWorld(_data.Lens);
            }
        }

        public void CursorLeft()
        {
            if (_data.Drawed && _data.Lens && _data.CursorX > 0)
            {
                _data.CursorX--;
                if (_data.PausedMode) RedrawWorld(_data.Lens);
            }
        }
        public void CursorRight()
        {
            if (_data.Drawed && _data.Lens && _data.CursorX < _data.LensWidth - 1)
            {
                _data.CursorX++;
                if (_data.PausedMode) RedrawWorld(_data.Lens);
            }
        }
        public void CursorUp()
        {
            if (_data.Drawed && _data.Lens && _data.CursorY > 0)
            {
                _data.CursorY--;
                if (_data.PausedMode) RedrawWorld(_data.Lens);
            }
        }
        public void CursorDown()
        {
            if (_data.Drawed && _data.Lens && _data.CursorY < _data.LensCellHeight - 1)
            {
                _data.CursorY++;
                if (_data.PausedMode) RedrawWorld(_data.Lens);
            }
        }


        #endregion

        //private void timer_Tick(object sender, EventArgs e)
        //{
        //	Step();
        //}
    }
}
