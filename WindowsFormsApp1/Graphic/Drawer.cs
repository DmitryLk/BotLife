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
using WindowsFormsApp1.Logger;
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

			if (Data.Checks) Func.CHECK5();

			Test.NextInterval(4, "DrawBotOnFrame(bots[botNumber]);");


			if (Data.LensOn)
			{
				DrawLens();
				if (Data.PausedMode)
				{
					Data.DeltaHistory = 0;
					DrawCursor();
					DrawReactions();
				}
			}
			_PRESENTER.SendFrameToScreen();


			//await Task.Delay(1);
			Test.NextInterval(5, "PaintFrame();");
		}

		private void DrawCell(int index)
		{
			var obj = Data.ChangedCells[index];

			_PRESENTER.DrawObjectOnFrame(obj.X, obj.Y, obj.Color);
			Data.ChWorld[obj.X, obj.Y] = 0;


			//if (Data.ClWorld[obj.X, obj.Y] == null)
			//{
			//    Data.ClWorld[obj.X, obj.Y] = new BotLog();
			//}
			//Data.ClWorld[obj.X, obj.Y].LogInfo($"Color:{obj.Color}   Index:{obj.Index}");
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

					if (Data.Parallel)
					{
						Parallel.For(1, (int)Data.NumberOfChangedCells + 1, DrawCell);
					}
					else
					{
						for (var i = 1; i <= Data.NumberOfChangedCells; i++)
						{
							DrawCell(i);
						}
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

		public Color GetPixel(int x, int y)
		{
			return _PRESENTER.GetPixel(x, y);
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
					else if (cont == 65503)
					{
						color = Color.Black;
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


		// информация по курсору (работает только на паузе).  Геном бота.
		public void DrawCursor()
		{
			var cursorCont = Data.World[Data.LensX + Data.CursorX, Data.LensY + Data.CursorY];
			if (cursorCont >= 1 && cursorCont <= Data.CurrentNumberOfBots)
			{
				var bot = Data.Bots[cursorCont];

				//TEXT
				_PRESENTER.ClearGraphicsOnCursorFrame();
				var k = 0;
				for (var i = 0; i < Branch.AllBranchCount; i++)
				{
					if (i >= bot.G.ActiveGeneralBranchCnt && i < Branch.GeneralBranchCount) continue;

					_PRESENTER.DrawMediumTextOnCursorFrame(0, k, -9, 23, i.ToString(), i >= Branch.GeneralBranchCount ? Color.Red :  Color.Black);
					for (var j = 0; j < Data.MaxCmdInStep; j++)
					{
						var code = bot.G.Code[i, j, 0];
						var par = bot.G.Code[i, j, 1];
						var absDirStr = Dir.GetDirectionStringFromCode(par);

						var textColor = Cmd.CmdColor(code);

						_PRESENTER.DrawTextOnCursorFrame(j, k, code.ToString(), textColor);
						_PRESENTER.DrawSmallTextOnCursorFrame(j, k, 35, 20, absDirStr, Color.Blue);
						if (bot.G.Act[i * Data.MaxCmdInStep + j] > 0) _PRESENTER.DrawMediumTextOnCursorFrame(j, k, 15, 35, bot.G.Act[i * Data.MaxCmdInStep + j].ToString(), Color.Red);
					}

					var rn = Branch.BranchName((byte)i);

					_PRESENTER.DrawOtherTextOnCursorFrame(Data.MaxCmdInStep, k, rn, Color.Green);

					k++;
				}

				//IMAGES
				_PRESENTER.StartNewCursorFrame(BitmapCopyType.EditDirectlyScreenBitmap_Fastest);
				Color color;
				int x1, y1, x2, y2;
				k = 0;
				for (var i = 0; i < Branch.AllBranchCount; i++)
				{
					if (i >= bot.G.ActiveGeneralBranchCnt && i < Branch.GeneralBranchCount) continue;
					for (var j = 0; j < Data.MaxCmdInStep; j++)
					{
						_PRESENTER.DrawCodeCellOnCursorFrame(j, k, bot.G.Code[i, j, 2] == 1 ? Color.Red : (bot.G.Act[i * Data.MaxCmdInStep + j] > 0 ? Color.Black : Color.LightGray));
					}
					k++;
				}

				if (bot.hist.historyPointerY >= 0)
				{
					var (hist_old, histPtrCnt_old, _, _, _) = bot.hist.GetLastStepPtrs(Data.DeltaHistory - 1);
					var (hist, histPtrCnt, _, _, _) = bot.hist.GetLastStepPtrs(Data.DeltaHistory);


					for (var i = 0; i < histPtrCnt; i++)
					{
						if (i == 0)
						{
							x1 = hist_old[histPtrCnt_old - 1].c;
							y1 = hist_old[histPtrCnt_old - 1].b;
						}
						else
						{
							x1 = hist[i - 1].c;
							y1 = hist[i - 1].b;
						}

						x2 = hist[i].c;
						y2 = hist[i].b;

						if (y1 >= Branch.GeneralBranchCount) y1 -= Branch.GeneralBranchCount - bot.G.ActiveGeneralBranchCnt;
						if (y2 >= Branch.GeneralBranchCount) y2 -= Branch.GeneralBranchCount - bot.G.ActiveGeneralBranchCnt;


						color = Color.DarkOrchid;
						if (i == histPtrCnt - 1) color = Color.Orange;
						if (i == 0) color = Color.Aqua;

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
				_PRESENTER.StartNewReactionsFrame(BitmapCopyType.EditEmptyArray);
				_PRESENTER.SendReactionsFrameToScreen();

				//INFO
				_PRINTER.Print3(null);
				_PRINTER.Print4(null, Data.DeltaHistory);
			}





			//var cursorCont = Data.World[Data.LensX + Data.CursorX, Data.LensY + Data.CursorY];
			//if (cursorCont >= 1 && cursorCont <= Data.CurrentNumberOfBots)
			//{
			//	var bot = Data.Bots[cursorCont];

			//	//TEXT
			//	_PRESENTER.ClearGraphicsOnCursorFrame();
			//	for (var i = 0; i < Data.GenomLength; i++)
			//	{
			//		var code = bot.G.CodeCommon[i];
			//		var x = i % 8;
			//		var y = i / 8;

			//		var textColor = code switch
			//		{
			//			Cmd.RotateAbsolute => Color.Blue,  //поворот
			//			Cmd.RotateRelative => Color.Blue,
			//			Cmd.Photosynthesis => Color.Green,
			//			Cmd.StepForward1 => Color.Brown,  //шаг
			//			Cmd.StepForward2 => Color.Brown,
			//			Cmd.EatForward1 => Color.Red,    //съесть
			//			Cmd.EatForward2 => Color.Red,
			//			Cmd.LookForward1 => Color.Gray,  //посмотреть
			//			Cmd.LookForward2 => Color.Gray,
			//			_ => Color.Black
			//		};

			//		var absDirStr = Dir.GetDirectionStringFromCode(code);
			//		_PRESENTER.DrawTextOnCursorFrame(x, y, code.ToString(), textColor);

			//		_PRESENTER.DrawSmallTextOnCursorFrame(x, y, 30, 7, i.ToString(), textColor);
			//		_PRESENTER.DrawSmallTextOnCursorFrame(x, y, 26, 28, absDirStr, textColor);
			//		_PRESENTER.DrawSmallTextOnCursorFrame(x, y, 10, 28, bot.G.Act[i].ToString(), textColor);
			//	}

			//	_PRESENTER.DrawOtherTextOnCursorFrame(6, 2, Data.DeltaHistory.ToString());

			//	//IMAGES
			//	_PRESENTER.StartNewCursorFrame(BitmapCopyType.EditDirectlyScreenBitmap_Fastest);
			//	Color color;
			//	int x1, y1, x2, y2;
			//	var (hist, histPtrCnt, pointer, oldpointer, _) = bot.hist.GetLastStepPtrs(Data.DeltaHistory);
			//	for (var i = 0; i < Data.GenomLength; i++)
			//	{
			//		x1 = i % 8;
			//		y1 = i / 8;

			//		color = Color.Gray;

			//		if ((byte)i == oldpointer) color = Color.Orange;
			//		if ((byte)i == pointer) color = Color.GreenYellow;

			//		_PRESENTER.DrawCodeCellOnCursorFrame(x1, y1, color);
			//	}

			//	if (histPtrCnt > 0)
			//	{
			//		byte ptr1 = hist[0].ptr;
			//		byte ptr2;
			//		for (var i = 1; i < histPtrCnt; i++)
			//		{
			//			ptr2 = hist[i].ptr;

			//			x1 = ptr1 % 8;
			//			y1 = ptr1 / 8;
			//			x2 = ptr2 % 8;
			//			y2 = ptr2 / 8;

			//			color = Color.DarkOrchid;

			//			if (i == 1) color = Color.Aqua;
			//			if (i == histPtrCnt - 1) color = Color.Orange;

			//			ptr1 = ptr2;

			//			_PRESENTER.DrawCodeArrowOnCursorFrame(x1, y1, x2, y2, color);
			//		}
			//	}

			//	_PRESENTER.SendCursorFrameToScreen();

			//	//INFO
			//	_PRINTER.Print3(bot);
			//	_PRINTER.Print4(bot, Data.DeltaHistory);
			//}
			//else
			//{
			//	_PRESENTER.StartNewCursorFrame(BitmapCopyType.EditEmptyArray);
			//	_PRESENTER.SendCursorFrameToScreen();

			//	//INFO
			//	_PRINTER.Print3(null);
			//	_PRINTER.Print4(null, Data.DeltaHistory);
			//}
		}

		// информация по событиям/реакциям (работает только на паузе)
		public void DrawReactions()
		{
			//var cursorCont = Data.World[Data.LensX + Data.CursorX, Data.LensY + Data.CursorY];
			//if (cursorCont >= 1 && cursorCont <= Data.CurrentNumberOfBots)
			//{
			//	var bot = Data.Bots[cursorCont];

			//	//TEXT
			//	_PRESENTER.ClearGraphicsOnReactionsFrame();
			//	for (var i = 0; i < Data.GenomEvents; i++)
			//	{
			//		for (var j = 0; j < Data.GenomEventsLenght; j++)
			//		{
			//			var code = bot.G.CodeForEvents[i, j, 0];
			//			var par = bot.G.CodeForEvents[i, j, 1];
			//			var absDirStr = Dir.GetDirectionStringFromCode(par);

			//			_PRESENTER.DrawTextOnReactionsFrame(j, i, code.ToString(), Color.Black);
			//			_PRESENTER.DrawSmallTextOnReactionsFrame(j, i, 20, 22, absDirStr, Color.Black);

			//		}

			//		var rn = i switch
			//		{
			//			0 => "1 bite",
			//			1 => "2 bot rel",
			//			2 => "2 bot bigrot",
			//			3 => "2 bot nobigrot",
			//			4 => "3 food",
			//			5 => "4 mineral",
			//			6 => "5 wall",
			//			_ => "xz"
			//		};
			//		_PRESENTER.DrawTextOnReactionsFrame2(Data.GenomEventsLenght , i, rn, Color.Green);
			//	}

			//	//IMAGES
			//	_PRESENTER.StartNewReactionsFrame(BitmapCopyType.EditDirectlyScreenBitmap_Fastest);
			//	Color color;
			//	int x1, y1, x2, y2;
			//	for (var i = 0; i < Data.GenomEvents; i++)
			//	{
			//		for (var j = 0; j < Data.GenomEventsLenght; j++)
			//		{
			//			_PRESENTER.DrawCodeCellOnReactionsFrame(j, i, Color.Black);
			//		}
			//	}

			//	_PRESENTER.SendReactionsFrameToScreen();
			//}
			//else
			//{
			//	_PRESENTER.StartNewReactionsFrame(BitmapCopyType.EditEmptyArray);
			//	_PRESENTER.SendReactionsFrameToScreen();
			//}
		}
	}
}
