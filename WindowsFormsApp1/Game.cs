using Newtonsoft.Json;
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
using WindowsFormsApp1.Static;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public class Game
	{
		public Presenter _PRESENTER;
		public World _world;
		public System.Windows.Forms.Timer timer;
        private int deltaHistory = 0;

		private readonly object _sync = new object();
		private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

		public Game(Presenter presenter)
		{
			_PRESENTER = presenter;

			//timer = new System.Windows.Forms.Timer();
			//timer.Tick += new System.EventHandler(timer_Tick);
			//timer.Interval = 1;
			//timer.Enabled = false;

			_world = new World();

			Test.InitInterval(0, "BotsAction();");
			Test.InitInterval(1, "RedrawWorld();");
			Test.InitInterval(2, "DrawBotOnFrame(bots[botNumber]);");
			Test.InitInterval(3, "PaintFrame();");
			Test.InitInterval(4, "PrintInfo();");

		}

		public async Task Start()
		{
			Data.Started = true;
			//timer.Enabled = true;
			_world.Initialize();
		}

		public async Task Work()
		{
			if (Data.Worked) return;
			await _semaphoreSlim.WaitAsync();
			try
			{
				if (Data.Worked) return;
				Data.Worked = true;
				do
				{
					await Step();
				}
				while (!Data.PausedMode);
				Data.Worked = false;
			}
			finally
			{
				_semaphoreSlim.Release();
			}
		}

		private async Task Step()
		{
			Test.BeginInterval(0);
			await Task.Run(() => _world.Step());
			Test.EndBeginInterval(0, 1);

			//await Task.Factory.StartNew(() => WorldStep(), TaskCreationOptions.LongRunning);
			//_world.Step();

			if (Data.Drawed)
			{
				RedrawWorld(Data.Lens);
			}
			else
			{
				Test.EndBeginInterval(1, 2);
				Test.EndBeginInterval(2, 3);
				Test.EndBeginInterval(3, 4);
			}

			_PRESENTER.PrintInfo();

			Test.EndBeginInterval(4, 0);
			//await Task.Delay(5000);
		}

		private void RedrawWorld(bool additionalGraphics)
		{
			_PRESENTER.StartNewFrame(additionalGraphics ? BitmapCopyType.EditCopyScreenBitmapWithAdditionalArray : BitmapCopyType.EditDirectlyScreenBitmap_Fastest);

			Test.EndBeginInterval(1, 2);
			
			MainGraphics();

			Test.EndBeginInterval(2, 3);

			if (additionalGraphics) AdditionalGraphics();

			_PRESENTER.SendFrameToScreen();
			//await Task.Delay(1);
			Test.EndBeginInterval(3, 4);
		}



		// Рисование изменившихся ячеек на основном битмапе экрана (сразу не отображаются)
		private void MainGraphics()
		{
			Data.NumberOfChangedCellsForInfo = Data.NumberOfChangedCells;
			for (var i = 0; i < Data.NumberOfChangedCells; i++)
			{
				var obj = Data.ChangedCells[i];

				_PRESENTER.DrawObjectOnFrame(obj.X, obj.Y, obj.Color);
				Data.ChWorld[obj.X, obj.Y] = 0;
			}
			Data.NumberOfChangedCells = 0;
		}



		private void AdditionalGraphics()
		{
			_PRESENTER.IntermediateFrameSave();  // сохранить в промежуточный массив экран без дополнительной графики
			if (Data.Lens)
			{
				DrawLens();
                if (Data.PausedMode)
                {
                    deltaHistory = 0;
                    DrawCursorInfo();
				}
			}
		}

		private void DrawLens()
		{
			_PRESENTER.DrawLensOnFrame(Data.LensX, Data.LensY, Data.LensWidth, Data.LensHeight, Color.Black);  // рмсование лупы

			_PRESENTER.StartNewLensFrame(BitmapCopyType.EditEmptyArray);
			Color? color;
			int? dir;
			// Выберем из Data.World[nX, nY] все что попадет в лупу
			for (var y = Data.LensY; y < Data.LensY + Data.LensHeight; y++)
			{
				for (var x = Data.LensX; x < Data.LensX + Data.LensWidth; x++)
				{

					var cont = Data.World[x, y];
					color = null;
					dir = null;

					if (cont == 0)
					{
					}
					else if (cont == 65500)
					{
						color = Color.Green;
					}
					else if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
					{
						color = Data.Bots[cont].genom.Color;
						dir = Data.Bots[cont].Direction;
					}
					else
					{
						throw new Exception("var color = cont switch");
					}


					_PRESENTER.DrawObjectOnLensFrame(x - Data.LensX, y - Data.LensY, color, dir);
				}
			}
			_PRESENTER.DrawCursorOnLens(Data.CursorX, Data.CursorY, Color.Black);  // рмсование курсора.
			_PRESENTER.SendLensFrameToScreen();
		}

		// информация по курсору
		public void DrawCursorInfo()
		{
			var cursorCont = Data.World[Data.LensX + Data.CursorX, Data.LensY + Data.CursorY];
			if (cursorCont >= 1 && cursorCont <= Data.CurrentNumberOfBots)
			{
				var bot = Data.Bots[cursorCont];

				//TEXT
				_PRESENTER.ClearGraphicsOnCursorFrame();
				for (var i = 0; i < Data.GenomLength; i++)
				{
					var code = bot.genom.Code[i];
					var x = i % 8;
					var y = i / 8;


					var textColor = code switch
					{
						23 => Color.Blue,  //поворот
						24 => Color.Blue,
						25 => Color.Green,
						26 => Color.Brown,  //шаг
						27 => Color.Brown,
						28 => Color.Red,    //съесть
						29 => Color.Red,
						30 => Color.Gray,  //посмотреть
						31 => Color.Gray,
						_ => Color.Black
					};

					_PRESENTER.DrawTextOnCursorFrame(x, y, code.ToString(), textColor);
                    _PRESENTER.DrawSmallTextOnCursorFrame1(x, y, i.ToString(), textColor);
                    var absDirStr = Dir.GetDirectionStringFromCode(code);
					_PRESENTER.DrawSmallTextOnCursorFrame2(x, y, absDirStr, textColor);
				}

				_PRESENTER.DrawOtherTextOnCursorFrame(6, 2, deltaHistory.ToString());
				
                //IMAGES
				_PRESENTER.StartNewCursorFrame(BitmapCopyType.EditDirectlyScreenBitmap_Fastest);
				Color color;
				int x1, y1, x2, y2;
				for (var i = 0; i < Data.GenomLength; i++)
				{
					x1 = i % 8;
					y1 = i / 8;
					color = i == bot.Pointer
						? Color.Aqua
						: i == bot.OldPointer
							? Color.Red
							: Color.Gray;

					_PRESENTER.DrawCodeCellOnCursorFrame(x1, y1, color);
				}

				var (hist, histPtrCnt) = bot.Hist.GetLastStepPtrs(deltaHistory);
				if (histPtrCnt > 0)
				{
					byte ptr1 = hist[0];
					byte ptr2;
					for (var i = 1; i < histPtrCnt; i++)
					{
						ptr2 = hist[i];

						x1 = ptr1 % 8;
						y1 = ptr1 / 8;
						x2 = ptr2 % 8;
						y2 = ptr2 / 8;
						color = i == 1
							? Color.Blue
							: i == histPtrCnt - 1
								? Color.Red
								: Color.DarkOrchid;
						ptr1 = ptr2;

						_PRESENTER.DrawCodeArrowOnCursorFrame(x1, y1, x2, y2, color);
					}
				}

				_PRESENTER.SendCursorFrameToScreen();
                
                //INFO
                _PRESENTER.PrintObjectInfo1(bot);
                _PRESENTER.PrintObjectInfo2(bot, deltaHistory);
			}
			else
			{
				_PRESENTER.StartNewCursorFrame(BitmapCopyType.EditEmptyArray);

				_PRESENTER.SendCursorFrameToScreen();
                
                //INFO
                _PRESENTER.PrintObjectInfo1(null);
                _PRESENTER.PrintObjectInfo2(null, deltaHistory);
			}
		}


		#region for Form
		public void MutationToggle()
		{
			Data.Mutation = !Data.Mutation;
		}

		public void DrawedToggle()
		{
			Data.Drawed = !Data.Drawed;
			Data.ReportFrequencyCurrent = GetCurrentReportFrequency();
		}

		private static int GetCurrentReportFrequency()
		{
			if (!Data.Drawed) return Data.ReportFrequencyNoDrawed;
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
			Data.Lens = mode;
		}
		public void LensLeft()
		{
			if (Data.Drawed && Data.Lens && Data.LensX > 0)
			{
				Data.LensX--;
				if (Data.PausedMode) RedrawWorld(Data.Lens);
			}
		}
		public void LensRight()
		{
			if (Data.Drawed && Data.Lens && Data.LensX < Data.WorldWidth - Data.LensWidth)
			{
				Data.LensX++;
				if (Data.PausedMode) RedrawWorld(Data.Lens);
			}
		}
		public void LensUp()
		{
			if (Data.Drawed && Data.Lens && Data.LensY > 0)
			{
				Data.LensY--;
				if (Data.PausedMode) RedrawWorld(Data.Lens);
			}
		}
		public void LensDown()
		{
			if (Data.Lens && Data.LensY < Data.WorldHeight - Data.LensHeight)
			{
				Data.LensY++;
				if (Data.PausedMode) RedrawWorld(Data.Lens);
			}
		}

		public void CursorLeft()
		{
			if (Data.Drawed && Data.Lens && Data.CursorX > 0)
			{
				Data.CursorX--;
				if (Data.PausedMode) RedrawWorld(Data.Lens);
			}
		}
		public void CursorRight()
		{
			if (Data.Drawed && Data.Lens && Data.CursorX < Data.LensWidth - 1)
			{
				Data.CursorX++;
				if (Data.PausedMode) RedrawWorld(Data.Lens);
			}
		}
		public void CursorUp()
		{
			if (Data.Drawed && Data.Lens && Data.CursorY > 0)
			{
				Data.CursorY--;
				if (Data.PausedMode) RedrawWorld(Data.Lens);
			}
		}
		public void CursorDown()
		{
			if (Data.Drawed && Data.Lens && Data.CursorY < Data.LensCellHeight - 1)
			{
				Data.CursorY++;
				if (Data.PausedMode) RedrawWorld(Data.Lens);
			}
		}


        public void HistoryUp()
        {
            deltaHistory++;
			if (Data.PausedMode) DrawCursorInfo();
		}
		public void HistoryDown()
        {
            deltaHistory--;
            if (Data.PausedMode) DrawCursorInfo();
		}


		#endregion

		//private void timer_Tick(object sender, EventArgs e)
		//{
		//	Step();
		//}
	}
}
