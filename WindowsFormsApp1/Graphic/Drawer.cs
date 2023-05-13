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
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.GameLogic;
using WindowsFormsApp1.Static;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using DrawMode = WindowsFormsApp1.Enums.DrawMode;

namespace WindowsFormsApp1.Graphic
{
	public class Drawer
	{
		private readonly Presenter _PRESENTER;
		private readonly Printer _PRINTER;
		public Drawer(Presenter presenter, Printer printer)
		{
			_PRESENTER = presenter;
			_PRINTER = printer;
		}

		public void DrawGame()
		{

			DrawBotsWorld();

            Test.NextInterval(4, "DrawBotOnFrame(bots[botNumber]);");


			if (Data.LensOn)
			{
				DrawLens();
				if (Data.PausedMode)
				{
					Data.DeltaHistory = 0;
					DrawCursor();
				}
			}
			_PRESENTER.SendFrameToScreen();


			//await Task.Delay(1);
            Test.NextInterval(5, "PaintFrame();");
		}



		// Рисование изменившихся ячеек на основном битмапе экрана (сразу не отображаются)
		private void DrawBotsWorld()
		{
			switch (Data.DrawType)
			{

				case DrawType.OnlyChangedCells:
					// Рисуем только изменившщиеся ячейки. Одновременно постепенно обнуляем массивы измененных ячеек
					_PRESENTER.StartNewFrame(Data.LensOn ? BitmapCopyType.EditCopyScreenBitmapWithAdditionalArray : BitmapCopyType.EditDirectlyScreenBitmap_Fastest);
                    
                    Test.NextInterval(3, "RedrawWorld();");

					for (var i = 1; i < Data.NumberOfChangedCells; i++)
					{
						var obj = Data.ChangedCells[i];

						_PRESENTER.DrawObjectOnFrame(obj.X, obj.Y, obj.Color);
						Data.ChWorld[obj.X, obj.Y] = 0;
					}
					Data.NumberOfChangedCellsForInfo = Data.NumberOfChangedCells;
					Data.NumberOfChangedCells = 0;
					break;

				case DrawType.AllCells:
					// Рисуем все ячейки.
					_PRESENTER.StartNewFrame(BitmapCopyType.EditEmptyArray);

                    Test.NextInterval(3, "RedrawWorld();");

					for (var i = 1; i <= Data.CurrentNumberOfBots; i++)
					{
						var obj = Data.Bots[i];

						_PRESENTER.DrawObjectOnFrame(obj.Xi, obj.Yi, obj.Color);
					}
					break;

				default:
					throw new Exception("switch (Data.DrawMode)");
			}
		}



		private void DrawLens()
		{
			_PRESENTER.IntermediateFrameSave();  // сохранить в промежуточный массив экран без дополнительной графики

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
						color = Data.Bots[cont].Color;
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


		// информация по курсору (работает только на паузе)
		public void DrawCursor()
		{
			var cursorCont = Data.World[Data.LensX + Data.CursorX, Data.LensY + Data.CursorY];
			if (cursorCont >= 1 && cursorCont <= Data.CurrentNumberOfBots)
			{
				var bot = Data.Bots[cursorCont];

				//TEXT
				_PRESENTER.ClearGraphicsOnCursorFrame();
				for (var i = 0; i < Data.GenomLength; i++)
				{
					var code = bot.Genom.Code[i];
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

				_PRESENTER.DrawOtherTextOnCursorFrame(6, 2, Data.DeltaHistory.ToString());

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

				var (hist, histPtrCnt) = bot.Hist.GetLastStepPtrs(Data.DeltaHistory);
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
				_PRINTER.Print3(bot);
				_PRINTER.Print4(bot, Data.DeltaHistory);
			}
			else
			{
				_PRESENTER.StartNewCursorFrame(BitmapCopyType.EditEmptyArray);
				_PRESENTER.SendCursorFrameToScreen();

				//INFO
				_PRINTER.Print3(null);
				_PRINTER.Print4(null, Data.DeltaHistory);
			}
		}
	}
}
