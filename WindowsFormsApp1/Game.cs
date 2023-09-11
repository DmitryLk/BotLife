using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.Graphic;
using WindowsFormsApp1.Static;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using DrawMode = WindowsFormsApp1.Enums.DrawMode;

namespace WindowsFormsApp1.GameLogic
{
    public class Game
    {
        private readonly World _WORLD;
        private readonly Printer _PRINTER;
        private readonly Drawer _DRAWER;

        private readonly object _sync = new object();
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private int _cnt1;
        private int _cnt2;

        public Game(Presenter presenter, Printer printer)
        {
            _WORLD = new World();
            _DRAWER = new Drawer(presenter, printer);
            _PRINTER = printer;
            _cnt1 = 0;
            _cnt2 = 0;
        }

        public async Task Init()
        {
            Data.Started = true;
            _WORLD.Initialize();
        }

        public async Task Work()
        {
            if (Data.Worked) return;
            await _semaphoreSlim.WaitAsync();
            try
            {
                if (Data.Worked) return;
                Data.Worked = true;

                await Steps();
            }
            finally
            {
                _semaphoreSlim.Release();
                Data.Worked = false;
            }
        }

        private async Task Steps()
        {
            do
            {
                await Step(Data.DrawMode);
            }
            while (!Data.PausedMode);
        }

        private async Task Step(DrawMode mode)
        {
            Test.NextInterval(1, "PrintInfo();");

            IsDrawTypeChanged();
            IsBotColorModeChanged();
            IsDrawModeChanged();

            await Task.Run(() => _WORLD.Step());

            Test.NextInterval(2, "add food");

            if (Data.DrawMode == DrawMode.EachStep ||
                Data.DrawMode == DrawMode.Periodical && Data.CurrentStep % Data.FrequencyOfPeriodicalDraw == 0)
            {
                _DRAWER.DrawGame();
            }
            else
            {
                Test.NextInterval(3, "RedrawWorld();");
                Test.NextInterval(4, "DrawBotOnFrame(bots[botNumber]);");
                Test.NextInterval(5, "PaintFrame();");
            }

            if (++_cnt1 % Data.ReportFrequencyCurrent == 0)
            {
                _PRINTER.Print015();
            }

            if (Data.GenomInfo == GenomInfoMode.OneTime)
            {
                _PRINTER.Print2();
                Data.GenomInfo = GenomInfoMode.None;
            }

            if (Data.GenomInfo == GenomInfoMode.Periodical && ++_cnt2 % Data.GenomInfoPeriodPrint == 0)
            {
                _PRINTER.Print2();
            }

            Test.NextInterval(1, "PrintInfo();");
        }


        private void IsDrawTypeChanged()
        {
            //изменился DrawType
            if (Data.DrawType != Data.NextDrawType)
            {
                Data.DrawType = Data.NextDrawType;
                _PRINTER.Print5();
            }
        }

        private void IsBotColorModeChanged()
        {
            if (Data.BotColorMode != Data.NextBotColorMode)
            {
                // на один шаг полную перерисовку
                Data.NextDrawType = Data.DrawType;
                Data.DrawType = DrawType.AllCells;


                Data.BotColorMode = Data.NextBotColorMode;

                for (long botNumber = 1; botNumber <= Data.CurrentNumberOfBots; botNumber++)
                {
                    Data.Bots[botNumber].RefreshColor();
                }
                _PRINTER.Print5();
            }
        }

        private void IsDrawModeChanged()
        {
            //изменился DrawMode
            if (Data.DrawMode != Data.NextDrawMode)
            {
                // на один шаг полную перерисовку
                Data.NextDrawType = Data.DrawType;
                Data.DrawType = DrawType.AllCells;

                Data.DrawMode = Data.NextDrawMode;
                Data.ReportFrequencyCurrent = GetCurrentReportFrequency();

                _PRINTER.Print5();
            }
        }

        #region for Form
        public void MutationToggle()
        {
            Data.Mutation = !Data.Mutation;
        }

        public void ParallelToggle()
        {
            Data.Parallel = !Data.Parallel;
        }

        public void ChecksToggle()
        {
            Data.Checks = !Data.Checks;
        }

        public void HistToggle()
        {
            Data.Hist = !Data.Hist;
        }

        public void Fastest()
        {
            Data.Hist = false;
            //Data.Logs = false;
            Data.Checks = false;
            Data.Parallel = true;
            Data.NextDrawMode = DrawMode.NoDraw;
            Data.NextDrawType = DrawType.OnlyChangedCells;
            Data.GenomInfo = GenomInfoMode.None;
        }

        public void GenomInfo(GenomInfoMode mode)
        {
            Data.GenomInfo = mode;
        }


        public void ChangeDrawMode(DrawMode mode)
        {
            Data.NextDrawMode = mode;
        }

        public void ChangeDrawType(DrawType type)
        {
            Data.NextDrawType = type;
        }

        public void ChangeBotColorMode(BotColorMode mode)
        {
            Data.NextBotColorMode = mode;
        }

        private static int GetCurrentReportFrequency()
        {
            if (Data.DrawMode == DrawMode.NoDraw) return Data.ReportFrequencyNoDrawed;
            if (Data.PausedMode) return 1;
            return Data.ReportFrequencyDrawed;
        }
        public void PausedToggle()
        {
            Data.PausedMode = !Data.PausedMode;
            Data.ReportFrequencyCurrent = GetCurrentReportFrequency();
        }

        public bool Paused
        {
            get { return Data.PausedMode; }
        }

        public bool Started
        {
            get { return Data.Started; }
        }

        public void Lens(bool mode)
        {
            Data.LensOn = mode;
        }
        public void LensLeft()
        {
            if (Data.DrawMode != DrawMode.NoDraw && Data.LensOn && Data.LensX > 0)
            {
                Data.LensX--;
                if (Data.PausedMode) _DRAWER.DrawGame();
            }
        }
        public void LensRight()
        {
            if (Data.DrawMode != DrawMode.NoDraw && Data.LensOn && Data.LensX < Data.WorldWidth - Data.LensWidth)
            {
                Data.LensX++;
                if (Data.PausedMode) _DRAWER.DrawGame();
            }
        }
        public void LensUp()
        {
            if (Data.DrawMode != DrawMode.NoDraw && Data.LensOn && Data.LensY > 0)
            {
                Data.LensY--;
                if (Data.PausedMode) _DRAWER.DrawGame();
            }
        }
        public void LensDown()
        {
            if (Data.LensOn && Data.LensY < Data.WorldHeight - Data.LensHeight)
            {
                Data.LensY++;
                if (Data.PausedMode) _DRAWER.DrawGame();
            }
        }

        public void CursorLeft()
        {
            if (Data.DrawMode != DrawMode.NoDraw && Data.LensOn && Data.CursorX > 0)
            {
                Data.CursorX--;
                if (Data.PausedMode) _DRAWER.DrawGame();
            }
        }
        public void CursorRight()
        {
            if (Data.DrawMode != DrawMode.NoDraw && Data.LensOn && Data.CursorX < Data.LensWidth - 1)
            {
                Data.CursorX++;
                if (Data.PausedMode) _DRAWER.DrawGame();
            }
        }
        public void CursorUp()
        {
            if (Data.DrawMode != DrawMode.NoDraw && Data.LensOn && Data.CursorY > 0)
            {
                Data.CursorY--;
                if (Data.PausedMode) _DRAWER.DrawGame();
            }
        }
        public void CursorDown()
        {
            if (Data.DrawMode != DrawMode.NoDraw && Data.LensOn && Data.CursorY < Data.LensCellHeight - 1)
            {
                Data.CursorY++;
                if (Data.PausedMode) _DRAWER.DrawGame();
            }
        }


        public void HistoryUp()
        {
            Data.DeltaHistory++;
            if (Data.PausedMode) _DRAWER.DrawCursor();
        }
        public void HistoryDown()
        {
            Data.DeltaHistory--;
            if (Data.PausedMode) _DRAWER.DrawCursor();
        }

        public void ColorDataGridView()
        {
            _PRINTER.ColorDataGridView();
        }

        public void ToggleLiveDataGridView()
        {
            Data.DgvOnlyLive = !Data.DgvOnlyLive;
        }

        public void TogglePraDataGridView()
        {
            Data.DgvPra = !Data.DgvPra;
        }

        public void ToggleDelayForNewbie()
        {
            Data.DelayForNewbie = !Data.DelayForNewbie;
        }
        #endregion
    }
}

//await Task.Factory.StartNew(() => WorldStep(), TaskCreationOptions.LongRunning);
//_world.Step();
//await Task.Delay(5000);
