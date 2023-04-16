using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using Point = WindowsFormsApp1.Dto.Point;

namespace WindowsFormsApp1.GameLogic
{
	public class CodeHistory
	{
		private const int maxx = 15;
		private const int maxy = 10;

		public byte[][] codeHistory = new byte[maxy][];
		public byte[] ptrs = new byte[maxy];
		public int historyPointerY = -1;

		public CodeHistory()
		{
			for (var y = 0; y < maxy; y++)
			{
				codeHistory[y] = new byte[maxx];
			}
		}

		public void SavePtr(int ptr)
		{
			if (ptrs[historyPointerY] == maxx) throw new Exception("PutPtr(byte ptr) ");
			codeHistory[historyPointerY][ptrs[historyPointerY]] = (byte)ptr;
			ptrs[historyPointerY]++;
		}
		public void BeginNewStep()
		{
			historyPointerY++;
			if (historyPointerY == maxy) historyPointerY = 0;
			ptrs[historyPointerY] = 0;
		}

		public (byte[], int) GetLastStepPtrs(int delta)
		{
			if (historyPointerY < 0)
			{
				return (Array.Empty<byte>(), 0);
			}

			var ptr = historyPointerY + delta;

			while (ptr < 0)
			{
				ptr += maxy;
			}

			while (ptr >= maxy)
			{
				ptr -= maxy;
			}


			return (codeHistory[ptr], ptrs[ptr]);
		}
	}

	// Бот с океана foo52
	public class Bot1 : Bot
	{
		public Genom genom;
		public int Pointer;
		public int OldPointer;

		public CodeHistory Hist;




		// Может вызываться только из func.CreateNewBot()
		public Bot1(Point p, Direction dir, uint botNumber, uint botIndex, int en, Genom genom, int pointer, int vx, int vy)
			: base(p, dir, botNumber, botIndex, en, vx, vy)
		{
			Pointer = pointer;
			OldPointer = pointer;
			this.genom = genom;
			Hist = new CodeHistory();
		}

		public override void Step()
		{
			// Некий алгоритм активности бота в рамках одного игорового шага.
			// В результате которого он на основании данных от рецепторов или без них
			// изменит свое состояние (координаты, геном, энергия, здоровье, возраст, направление, цвет, минералы, ...)
			// Также может размножиться

			//Direction dir;
			int shift = 0;
			bool stepComplete = false;
			int cntJump = 0;

			Hist.BeginNewStep();
			do
			{
				// 1. Определяем команду которую будет делать бот
				var cmdCode = genom.GetCurrentCommand(Pointer);
				Hist.SavePtr(Pointer);

				// 2. Выполняем команду
				switch (cmdCode)
				{

					case 23: (shift, stepComplete) = RotateInRelativeDirection(); break;    // ПОВОРОТ относительно								2,               false
					case 24: (shift, stepComplete) = RotateInAbsoluteDirection(); break;    // ПОВОРОТ абсолютно								2,               false
					case 25: (shift, stepComplete) = Photosynthesis(); break;               // ФОТОСИНТЕЗ                                       1,               true
					case 26: (shift, stepComplete) = StepInRelativeDirection(); break;      // ДВИЖЕНИЕ шаг в относительном напралении			(int)refContent, true
					case 27: (shift, stepComplete) = StepInAbsoluteDirection(); break;      // ДВИЖЕНИЕ шаг в абсолютном направлении			(int)refContent, true
					case 28: (shift, stepComplete) = EatInRelativeDirection(); break;       // СЪЕСТЬ в относительном напралении				(int)refContent, true
					case 29: (shift, stepComplete) = EatInAbsoluteDirection(); break;       // СЪЕСТЬ в абсолютном направлении					(int)refContent, true
					case 30: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
					case 31: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 32: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 33: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 34: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 35: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 36: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 37: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 38: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 39: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 40: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 41: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 42: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 43: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 44: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 45: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 46: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 47: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 48: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 49: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 50: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 51: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 52: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 53: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 54: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 55: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
																							//case 56: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
																							//case 57: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
					default: shift = cmdCode; stepComplete = false; break;
				};

				cntJump++;
				// Прибавляем к указателю текущей команды значение команды
				ShiftCodePointer(shift);
			}
			while (!stepComplete && cntJump < Data.MaxUncompleteJump);

			_age++;
			Energy += Data.DeltaEnergyOnStep;
			Data.TotalEnergy += Data.DeltaEnergyOnStep;

			//Death
			if (Energy <= 0)
			{
				Death();
			}

			//Reproduction
			if (Energy >= Data.ReproductionBotEnergy)
			{
				Reproduction();
			}

			// 0-7		движение
			// 8-15		схватить еду или нейтрализовать яд
			// 16-23	посмотреть
			// 24-31	поворот
			// 32-63	безусловный переход
			// 25       фотосинтез

			//...............  сменить направление относительно   ....			if ($command == 23)
			//...............  сменить направление абсолютно   ....			if ($command == 24)
			//...............  фотосинтез ................			if ($command == 25)
			//...............  шаг  в относительном напралении  .................    			if ($command == 26)
			//...............  шаг   в абсолютном направлении     .................  			if ($command == 27)
			//..............   съесть в относительном напралении       ..............			if ($command == 28)
			//..............   съесть  в абсолютном направлении      ...............			if ($command == 29)
			//.............   посмотреть  в относительном напралении ...................................			if ($command == 30) 
			//.............   посмотреть в абсолютном направлении ...................................			if ($command == 31)// пусто - 2 стена - 3 органик - 4 бот -5 родня -  6
			///////////////////////////////////////////
			// делиться - если у бота больше энергии или минералов, чем у соседа, то они распределяются поровну          //.............   делится   в относительном напралении  ........................			if (($command == 32) || ($command == 42))    // здесь я увеличил шансы появления этой команды                   
			//.............   делится  в абсолютном направлении ........................			if (($command == 33) || ($command == 51))     // здесь я увеличил шансы появления этой команды                    
			// отдать - безвозмездно отдать часть энергии и минералов соседу			//.............   отдать   в относительном напралении  ........................			if (($command == 34) || ($command == 50) )     // здесь я увеличил шансы появления этой команды                    
			//.............   отдать  в абсолютном направлении  ........................			if (($command == 35) || ($command == 52) )       // здесь я увеличил шансы появления этой команды                    
			//...................   выравнится по горизонтали  ...............................			if ($command == 36)
			//...................  какой мой уровень (на какой высоте бот)  .........			if ($command == 37)
			//...................  какое моё здоровье  ..............................			if ($command == 38)
			//...................сколько  минералов ...............................			if ($command == 39)
			//...........  многоклеточность ( создание потомка, приклееного к боту )......			if ($command == 40)  
			//...............  деление (создание свободноживущего потомка) ................			if ($command == 41)      
			//...............  окружен ли бот    ................			if ($command == 43)   
			//.............. приход энергии есть? ........................			if ($command == 44)  
			//............... минералы прибавляются? ............................			if ($command == 45)  
			//.............. многоклеточный ли я ? ........................ 			if ($command == 46)  
			//.................. преобразовать минералы в энерию ...................			if ($command == 47) 
			//................      мутировать   ................................... // спорная команда, во время её выполнения меняются случайным образом две случайные команды // читал, что микроорганизмы могут усилить вероятность мутации своего генома в неблагоприятных условиях       			if ($command == 48)
			//................   генная атака  ...................................			if ($command == 49)
		}

		public override void Death()
		{
			//var idx = Data.World[P.X, P.Y];
			//if (idx != _ind)
			//{
			//	throw new Exception("var idx = Data.World[P.X, P.Y];");
			//}

			Data.World[P.X, P.Y] = (uint)CellContent.Free;
			Func.ChangeCell(P.X, P.Y, null); // при следующей отрисовке бот стерется с экрана

			//idx = Data.World[P.X, P.Y];
			//if (idx != 0)
			//{
			//	throw new Exception("var idx = Data.World[P.X, P.Y];");
			//}



			// надо его убрать из массива ботов, переставив последнего бота на его место
			//Bots: 0[пусто] 1[бот _ind=1] 2[бот _ind=2]; StartBotsNumber=2 CurrentBotsNumber=2

			//1
			if (Index > Data.CurrentNumberOfBots)
			{
				throw new Exception("if (_ind > Data.CurrentBotsNumber)");
			}

			//2
			if (Index < Data.CurrentNumberOfBots)
			{
				// Перенос последнего бота в освободившееся отверстие
				if (Data.Bots[Data.CurrentNumberOfBots] == null)
				{
					throw new Exception("if (Data.Bots[Data.CurrentNumberOfBots] == null)");
				}

				var lastBot = Data.Bots[Data.CurrentNumberOfBots];
				Data.Bots[Index] = lastBot;
				lastBot.Index = Index;
				Data.World[lastBot.P.X, lastBot.P.Y] = Index;
				//Func.ChangeCell(P.X, P.Y,  - делать не надо так как по этим координатам ничего не произошло, бот по этим координатам каким был таким и остался, только изменился индекс в двух массивах Bots и World
				//после этого ссылки на текущего бота нигде не останется и он должен будет уничтожен GC
			}

			// Укарачиваем массив
			Data.Bots[Data.CurrentNumberOfBots] = null;
			Data.CurrentNumberOfBots--;

			Data.DeathCnt++;


			genom.Bots--;

			//for (var j = 0; j < Data.WorldHeight; j++)
			//{
			//	for (var i = 0; i < Data.WorldWidth; i++)
			//	{
			//		if (Data.World[j, i] > Data.CurrentNumberOfBots && Data.World[j, i] < 65000)
			//		{
			//			throw new Exception("if (Data.World[j, i] > Data.CurrentNumberOfBots)");
			//		}
			//	}
			//}
		}

		protected override void Reproduction()
		{
			// Создание нового бота
			// Узнать есть ли рядом ячейка свободная
			var p = GetRandomFreeCellNearby();
			if (p != null)
			{
				Data.CurrentNumberOfBots++;
				var botIndex = Data.CurrentNumberOfBots;
				var genom = Func.Mutation() ? Genom.CreateMutatedGenom(this.genom) : this.genom;

				Func.CreateNewBot(p, botIndex, genom);
				Energy -= Data.InitialBotEnergy;
				Data.TotalEnergy -= Data.InitialBotEnergy;
				Data.ReproductionCnt++;
			}

		}

		private (int shift, bool stepComplete) Photosynthesis()
		{

			Energy += Data.PhotosynthesisEnergy;
			Data.TotalEnergy += Data.PhotosynthesisEnergy;

			return (1, true);
		}

		private (int shift, bool stepComplete) EatInRelativeDirection()
		{
			// Алгоритм:
			// 1. Узнаем направление предполагаемой еды
			var dir = GetDirRelative();
			// 2. Узнаем по направлению новые координаты, что там находится, можно ли туда передвинуться, последующее смещение кода
			var (refContent, nX, nY, cont) = GetCellInfo3(dir);

			// 3. Узнаем съедобно ли это
			if (refContent == RefContent.Grass)
			{
				EatGrass(nX, nY);
			}

			if (refContent == RefContent.Bot ||
				refContent == RefContent.Relative)
			{
				EatBot(nX, nY, cont);
			}

			if (refContent == RefContent.Organic ||
				refContent == RefContent.Mineral ||
				refContent == RefContent.Poison)
			{
			}

			return ((int)refContent, true);
		}

		private (int shift, bool stepComplete) EatInAbsoluteDirection()
		{
			// Алгоритм:
			// 1. Узнаем направление предполагаемой еды
			var dir = GetDirAbsolute();
			// 2. Узнаем по направлению новые координаты, что там находится, можно ли туда передвинуться, последующее смещение кода
			var (refContent, nX, nY, cont) = GetCellInfo3(dir);

			// 3. Узнаем съедобно ли это
			if (refContent == RefContent.Grass)
			{
				EatGrass(nX, nY);
			}

			if (refContent == RefContent.Bot ||
				refContent == RefContent.Relative)
			{
				EatBot(nX, nY, cont);
			}

			if (refContent == RefContent.Organic ||
				refContent == RefContent.Mineral ||
				refContent == RefContent.Poison)
			{
			}

			return ((int)refContent, true);
		}

		private void EatGrass(int nX, int nY)
		{
			Energy += Data.FoodEnergy;

			Data.World[nX, nY] = (uint)CellContent.Free;
			Func.ChangeCell(nX, nY, null);
		}


		private void EatBot(int nX, int nY, uint cont)
		{
			var eatedBot = Data.Bots[cont];
			var energyCanEat = eatedBot.Energy > Data.BiteEnergy ? Data.BiteEnergy : eatedBot.Energy;

			Energy += energyCanEat;
			eatedBot.Energy -= energyCanEat;

			if (eatedBot.Energy <= 0)
			{
				eatedBot.Death();
			}
		}

		private (int shift, bool stepComplete) LookAtRelativeDirection()
		{
			// Алгоритм:
			// 1. Узнаем новое направление
			var dir = GetDirRelative();

			// 2. Узнаем по направлению новые координаты, что там находится и последующее смещение кода
			var refContent = GetCellInfo1(dir);

			return ((int)refContent, false);
		}

		private (int shift, bool stepComplete) LookAtAbsoluteDirection()
		{
			// Алгоритм:
			// 1. Узнаем новое направление
			var dir = GetDirAbsolute();

			// 2. Узнаем по направлению новые координаты, что там находится и последующее смещение кода
			var refContent = GetCellInfo1(dir);

			return ((int)refContent, false);
		}

		private (int shift, bool stepComplete) RotateInRelativeDirection()
		{
			// Алгоритм:
			// 1. Узнаем новое направление
			var dir = GetDirRelative();
			// 2. Меняем направление бота
			Dir = dir;
			return (2, false);
		}

		private (int shift, bool stepComplete) RotateInAbsoluteDirection()
		{
			// Алгоритм:
			// 1. Узнаем новое направление
			var dir = GetDirAbsolute();
			// 2. Меняем направление бота
			Dir = dir;
			return (2, false);
		}

		private (int shift, bool stepComplete) StepInRelativeDirection()
		{
			// Алгоритм:
			// 1. Узнаем направление предполагаемого движения
			var dir = GetDirRelative();
			// 2. Узнаем по направлению новые координаты, что там находится, можно ли туда передвинуться, последующее смещение кода
			var (refContent, nX, nY) = GetCellInfo2(dir);
			// 3. Если есть возможность туда передвинуться , то перемещаем туда бота
			if (refContent == RefContent.Free || refContent == RefContent.Poison)
			{
				Move(nX, nY);
			}
			return ((int)refContent, true);
		}


		private (int shift, bool stepComplete) StepInAbsoluteDirection()
		{
			// Алгоритм:
			// 1. Узнаем направление предполагаемого движения
			var dir = GetDirAbsolute();
			// 2. Узнаем по направлению новые координаты, что там находится, можно ли туда передвинуться, последующее смещение кода
			var (refContent, nX, nY) = GetCellInfo2(dir);
			// 3. Если есть возможность туда передвинуться , то перемещаем туда бота
			if (refContent == RefContent.Free || refContent == RefContent.Poison)
			{
				Move(nX, nY);
			}
			return ((int)refContent, true);
		}

		//////////////////////////////////////////////////////////////////

		private RefContent GetCellInfo1(Direction dir)
		{
			// 1. Узнаем координаты клетки на которую надо посмотреть
			var (nX, nY) = GetCoordinatesByDirection(dir);

			// 2. Узнаем что находится на этой клетке
			var refContent = GetRefContent(nX, nY);

			// 3. Возвращаем полученную информацию
			return refContent;
		}

		private (RefContent refContent, int nX, int nY) GetCellInfo2(Direction dir)
		{
			// 1. Узнаем координаты предполагаемого перемещения
			var (nX, nY) = GetCoordinatesByDirection(dir);

			// 2. Узнаем что находится на этой клетке
			var refContent = GetRefContent(nX, nY);

			// 3. Возвращаем полученную информацию
			return (refContent, nX, nY);
		}

		private (RefContent refContent, int nX, int nY, uint cont) GetCellInfo3(Direction dir)
		{
			// 1. Узнаем координаты клетки на которую надо посмотреть
			var (nX, nY) = GetCoordinatesByDirection(dir);

			// 2. Узнаем что находится на этой клетке
			var (refContent, cont) = GetRefContentAndCont(nX, nY);

			// 3. Возвращаем полученную информацию
			return (refContent, nX, nY, cont);
		}


		//////////////////////////////////////////////////////////////////
		//			##     ##  #######  ##     ## ######## 
		//			###   ### ##     ## ##     ## ##       
		//			#### #### ##     ## ##     ## ##       
		//			## ### ## ##     ## ##     ## ######   
		//			##     ## ##     ##  ##   ##  ##       
		//			##     ## ##     ##   ## ##   ##       
		//			##     ##  #######     ###    ######## 
		//////////////////////////////////////////////////////////////////


		// Перемещение бота
		private void Move(int nX, int nY)
		{

			Old.X = P.X;
			Old.Y = P.Y;
			P.X = nX;
			P.Y = nY;


			Data.World[Old.X, Old.Y] = (uint)CellContent.Free;
			Func.ChangeCell(Old.X, Old.Y, null);

			Data.World[nX, nY] = Index;
			Func.ChangeCell(nX, nY, genom.Color);
		}


		//////////////////////////////////////////////////////////////////

		private RefContent GetRefContent(int x, int y)
		{
			//смещение условного перехода 2-пусто  3-стена  4-органика 5-бот 6-родня

			// Если координаты попадают за экран то вернуть RefContent.Edge
			if (y < 0 || y >= Data.WorldHeight || x < 0 || x >= Data.WorldWidth) return RefContent.Edge;

			var cont = Data.World[x, y];

			return cont switch
			{
				0 => RefContent.Free,
				65500 => RefContent.Grass,
				65501 => RefContent.Organic,
				65502 => RefContent.Mineral,
				65503 => RefContent.Wall,
				65504 => RefContent.Poison,
				_ => cont >= 1 && cont <= Data.CurrentNumberOfBots ? RefContentByBotRelativity(cont) : throw new Exception("return cont switch")
			};
		}

		private (RefContent refContent, uint cont) GetRefContentAndCont(int x, int y)
		{
			if (y < 0 || y >= Data.WorldHeight || x < 0 || x >= Data.WorldWidth) return (RefContent.Edge, 0);

			var cont = Data.World[x, y];

			return (cont switch
			{
				0 => RefContent.Free,
				65500 => RefContent.Grass,
				65501 => RefContent.Organic,
				65502 => RefContent.Mineral,
				65503 => RefContent.Wall,
				65504 => RefContent.Poison,
				_ => cont >= 1 && cont <= Data.CurrentNumberOfBots ? RefContentByBotRelativity(cont) : throw new Exception("return cont switch")
			}, cont);
		}

		private RefContent GetRefContentWithoutRelative(int x, int y)
		{
			if (y < 0 || y >= Data.WorldHeight || x < 0 || x >= Data.WorldWidth) return RefContent.Edge;

			var cont = Data.World[x, y];

			return cont switch
			{
				0 => RefContent.Free,
				65500 => RefContent.Grass,
				65501 => RefContent.Organic,
				65502 => RefContent.Mineral,
				65503 => RefContent.Wall,
				65504 => RefContent.Poison,
				_ => cont >= 1 && cont <= Data.CurrentNumberOfBots ? RefContent.Bot : throw new Exception("return cont switch")
			};
		}

		private RefContent RefContentByBotRelativity(uint cont)
		{
			// надо определить родственник ли бот

			return genom.IsRelative(((Bot1)Data.Bots[cont]).genom)
				? RefContent.Relative
				: RefContent.Bot;
		}

		private Point GetRandomFreeCellNearby()
		{
			RefContent refContent;
			int x;
			int y;
			var i = 0;

			var dir = Func.GetRandomDirection();
			do
			{
				dir = DirIncrement(dir);
				(x, y) = GetCoordinatesByDirection(dir);
				refContent = GetRefContentWithoutRelative(x, y);
				i++;
			}
			while (refContent != RefContent.Free && i <= 8);

			return refContent == RefContent.Free ? new Point(x, y) : null;
		}

		// Метод вернет всегда координаты отличные от текущих
		private (int nX, int nY) GetCoordinatesByDirection(Direction dir)
		{
			var (dX, dY) = Func.GetDeltaDirection(dir);

			var nX = P.X + dX;
			var nY = P.Y + dY;

			// Проверка перехода сквозь экран
			if (!Data.LeftRightEdge)
			{
				if (nX < 0) nX += Data.WorldWidth;
				if (nX >= Data.WorldWidth) nX -= Data.WorldWidth;
			}
			if (!Data.UpDownEdge)
			{
				if (nY < 0) nY += Data.WorldHeight;
				if (nY >= Data.WorldHeight) nY -= Data.WorldHeight;
			}

			return (nX, nY);
		}

		private void ShiftCodePointer(int shift)
		{
			OldPointer = Pointer;
			Pointer = (Pointer + shift) % Data.GenomLength;
		}

		private Direction GetDirAbsolute()
		{
			return (Direction)(((int)genom.GetNextCommand(Pointer)) % 8);
		}
		private Direction GetDirRelative()
		{
			return (Direction)(((int)genom.GetNextCommand(Pointer) + (int)Dir) % 8);
		}
		private Direction DirIncrement(Direction dir)
		{
			return (Direction)(((int)dir + 1) % 8);
		}

		public string GetText1()
		{
			var sb = new StringBuilder();

			sb.AppendLine($"Age: {_age}");
			sb.AppendLine($"Num: {_num}");
			sb.AppendLine($"Index: {Index}");

			sb.AppendLine($"Energy: {Energy}");
			sb.AppendLine($"_dir: {Dir}");


			sb.AppendLine("");
			sb.AppendLine($"Genom {genom.PraNum} {(genom.Num != 0 ? $"({genom.Num})" : "")}Lev{genom.Level}");
			sb.AppendLine($"Bots: {genom.Bots}");

			sb.AppendLine("");
			sb.AppendLine($"Color: R{genom.Color.R} G{genom.Color.G} B{genom.Color.B}");
			sb.AppendLine($"Pra: {genom.PraHash.ToString().Substring(0, 8)}");
			sb.AppendLine($"Hash: {genom.GenomHash.ToString().Substring(0, 8)}");
			sb.AppendLine($"Parent: {genom.ParentHash.ToString().Substring(0, 8)}");
			sb.AppendLine($"Grand: {genom.GrandHash.ToString().Substring(0, 8)}");

			return sb.ToString();
		}

		public string GetText2(int delta)
		{
			var sb = new StringBuilder();

			//sb.AppendLine($"23r,24a - rotate; 26r,27a - step");
			//sb.AppendLine($"28r,29a - eat; 30r,31a - look");

			sb.AppendLine($"OldPointer: {OldPointer}");
			sb.AppendLine($"Pointer: {Pointer}");

			if (Hist.historyPointerY >= 0)
			{
				var (hist, histPtrCnt) = Hist.GetLastStepPtrs(delta);

				sb.AppendLine($"jumps cnt: {histPtrCnt - 1}");
				sb.AppendLine($"jumps: {string.Join(", ", hist.Take(histPtrCnt))}");

				for (var i = 0; i < histPtrCnt; i++)
				{
					var cmdTxt = genom.Code[hist[i]] switch
					{
						23 => "Поворот относительно",
						24 => "Поворот абсолютно",
						25 => "Фотосинтез",
						26 => "Шаг относительно",  //шаг
						27 => "Шаг абсолютно",
						28 => "Есть относительно",    //съесть
						29 => "Есть абсолютно",
						30 => "Посмотерть относительно",  //посмотреть
						31 => "Посмотерть абсолютно",
						_ => ""
					};

					var dirStr = ((Direction)(((int)genom.GetNextCommand(hist[i])) % 8)).ToString();

					if (cmdTxt != "")
					{
						sb.AppendLine($"{cmdTxt} {dirStr}");
					}
				}
			}

			sb.AppendLine("");

			return sb.ToString();
		}
	}
}

