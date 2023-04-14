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
		public int historyPointerX = 0;
		public int historyPointerY = -1;

		public CodeHistory()
		{
			for (var y = 0; y < maxy; y++)
			{
				codeHistory[y] = new byte[maxx];
			}
		}

		public void SavePtr(byte ptr)
		{
			if (historyPointerX == maxx) throw new Exception("PutPtr(byte ptr) ");
			codeHistory[historyPointerY][historyPointerX] = ptr;
			historyPointerX++;
		}
		public void BeginNewStep()
		{
			historyPointerY++;
			if (historyPointerY == maxy) historyPointerY = 0;
		}

		public (byte[], int) GetLastStepPtrs()
		{
			return (codeHistory[historyPointerY], historyPointerX);
		}
	}

	// Бот с океана foo52
	public class Bot1 : Bot
	{
		public Genom Genom;
		public int Pointer;

		public CodeHistory Hist;




		// Может вызываться только из func.CreateNewBot()
		public Bot1(GameData data, Func func, Point p, Direction dir, uint botNumber, uint botIndex, int en, Genom genom, int pointer, int vx, int vy)
			: base(data, func, p, dir, botNumber, botIndex, en, vx, vy)
		{
			Pointer = pointer;
			Genom = genom;
			Hist = new CodeHistory();
		}

		public override void Step()
		{
			// Некий алгоритм активности бота в рамках одного игорового шага.
			// В результате которого он на основании данных от рецепторов или без них
			// изменит свое состояние (координаты, геном, энергия, здоровье, возраст, направление, цвет, минералы, ...)
			// Также может размножиться

			//Direction dir;
			int shift;
			bool stepComplete = false;
			int cntJump = 0;

			Hist.BeginNewStep();
			do
			{
				// 1. Определяем команду которую будет делать бот
				var cmdCode = Genom.GetCurrentCommand(Pointer);
				Hist.SavePtr(cmdCode);

				// 2. Выполняем команду
				switch (cmdCode)
				{
					// ПОВОРОТ
					case 23: (shift, stepComplete) = RotateInRelativeDirection(); break;  //поворот относительно
					case 24: (shift, stepComplete) = RotateInAbsoluteDirection(); break;  //поворот абсолютно
																						  // ФОТОСИНТЕЗ
																						  //case 25: (shift, stepComplete) = Photosynthesis(); break;  //фотосинтез
																						  // ДВИЖЕНИЕ
					case 26: (shift, stepComplete) = StepInRelativeDirection(); break;  //шаг в относительном напралении
					case 27: (shift, stepComplete) = StepInAbsoluteDirection(); break;  //шаг в абсолютном направлении
																						// СЪЕСТЬ
					case 28: (shift, stepComplete) = EatInRelativeDirection(); break;  //съесть в относительном напралении
					case 29: (shift, stepComplete) = EatInAbsoluteDirection(); break;  //съесть  в абсолютном направлении
																					   // ПОСМОТРЕТЬ
					case 30: (shift, stepComplete) = LookAtRelativeDirection(); break;  //посмотреть  в относительном напралении
					case 31: (shift, stepComplete) = LookAtAbsoluteDirection(); break;  //посмотреть  в абсолютном напралении
					default: shift = cmdCode; stepComplete = false; break;
				};

				cntJump++;
				// Прибавляем к указателю текущей команды значение команды
				ShiftCodePointer(shift);
			}
			while (!stepComplete && cntJump < _data.MaxUncompleteJump);

			_age++;
			Energy += _data.DeltaEnergyOnStep;

			// todo обработка деления и смерти
			//Die
			if (Energy <= 0)
			{
				Death();
			}

			//Reproduction
			if (Energy >= _data.ReproductionBotEnergy)
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
			//var idx = _data.World[P.X, P.Y];
			//if (idx != _ind)
			//{
			//	throw new Exception("var idx = _data.World[P.X, P.Y];");
			//}

			_data.World[P.X, P.Y] = (uint)CellContent.Free;
			_func.ChangeCell(P.X, P.Y, null); // при следующей отрисовке бот стерется с экрана

			//idx = _data.World[P.X, P.Y];
			//if (idx != 0)
			//{
			//	throw new Exception("var idx = _data.World[P.X, P.Y];");
			//}



			// надо его убрать из массива ботов, переставив последнего бота на его место
			//Bots: 0[пусто] 1[бот _ind=1] 2[бот _ind=2]; StartBotsNumber=2 CurrentBotsNumber=2

			//1
			if (Index > _data.CurrentNumberOfBots)
			{
				throw new Exception("if (_ind > _data.CurrentBotsNumber)");
			}

			//2
			if (Index < _data.CurrentNumberOfBots)
			{
				// Перенос последнего бота в освободившееся отверстие
				if (_data.Bots[_data.CurrentNumberOfBots] == null)
				{
					throw new Exception("if (_data.Bots[_data.CurrentNumberOfBots] == null)");
				}

				var lastBot = _data.Bots[_data.CurrentNumberOfBots];
				_data.Bots[Index] = lastBot;
				lastBot.Index = Index;
				_data.World[lastBot.P.X, lastBot.P.Y] = Index;
				//_func.ChangeCell(P.X, P.Y,  - делать не надо так как по этим координатам ничего не произошло, бот по этим координатам каким был таким и остался, только изменился индекс в двух массивах Bots и World
				//после этого ссылки на текущего бота нигде не останется и он должен будет уничтожен GC
			}

			// Укарачиваем массив
			_data.Bots[_data.CurrentNumberOfBots] = null;
			_data.CurrentNumberOfBots--;

			_data.DeathCnt++;


			Genom.Bots--;
			//todo если геном больше не используется (Bots=0) то удалять геном чтоб память не забивал

			//for (var j = 0; j < _data.WorldHeight; j++)
			//{
			//	for (var i = 0; i < _data.WorldWidth; i++)
			//	{
			//		if (_data.World[j, i] > _data.CurrentNumberOfBots && _data.World[j, i] < 65000)
			//		{
			//			throw new Exception("if (_data.World[j, i] > _data.CurrentNumberOfBots)");
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
				_data.CurrentNumberOfBots++;
				var botIndex = _data.CurrentNumberOfBots;
				var genom = _func.Mutation() ? new Genom(_data, _func, Genom) : Genom;

				_func.CreateNewBot(p, botIndex, genom);
				Energy -= _data.InitialBotEnergy;
				_data.ReproductionCnt++;
			}

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

			// todo если релатив возвращать что просто бот как у foo52 ?
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
			Energy += _data.FoodEnergy;

			_data.World[nX, nY] = (uint)CellContent.Free;
			_func.ChangeCell(nX, nY, null);
		}


		private void EatBot(int nX, int nY, uint cont)
		{
			var eatedBot = _data.Bots[cont];
			var energyCanEat = eatedBot.Energy > _data.BiteEnergy ? _data.BiteEnergy : eatedBot.Energy;

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
			// todo добавить обработку если наступил на яд


			_data.World[Old.X, Old.Y] = (uint)CellContent.Free;
			_func.ChangeCell(Old.X, Old.Y, null);

			_data.World[nX, nY] = Index;
			_func.ChangeCell(nX, nY, Genom.Color);
		}


		//////////////////////////////////////////////////////////////////

		private RefContent GetRefContent(int x, int y)
		{
			//смещение условного перехода 2-пусто  3-стена  4-органика 5-бот 6-родня

			// Если координаты попадают за экран то вернуть RefContent.Edge
			if (y < 0 || y >= _data.WorldHeight || x < 0 || x >= _data.WorldWidth) return RefContent.Edge;

			var cont = _data.World[x, y];

			return cont switch
			{
				0 => RefContent.Free,
				65500 => RefContent.Grass,
				65501 => RefContent.Organic,
				65502 => RefContent.Mineral,
				65503 => RefContent.Wall,
				65504 => RefContent.Poison,
				_ => cont >= 1 && cont <= _data.CurrentNumberOfBots ? RefContentByBotRelativity(cont) : throw new Exception("return cont switch")
			};
		}

		private (RefContent refContent, uint cont) GetRefContentAndCont(int x, int y)
		{
			if (y < 0 || y >= _data.WorldHeight || x < 0 || x >= _data.WorldWidth) return (RefContent.Edge, 0);

			var cont = _data.World[x, y];

			return (cont switch
			{
				0 => RefContent.Free,
				65500 => RefContent.Grass,
				65501 => RefContent.Organic,
				65502 => RefContent.Mineral,
				65503 => RefContent.Wall,
				65504 => RefContent.Poison,
				_ => cont >= 1 && cont <= _data.CurrentNumberOfBots ? RefContentByBotRelativity(cont) : throw new Exception("return cont switch")
			}, cont);
		}

		private RefContent GetRefContentWithoutRelative(int x, int y)
		{
			if (y < 0 || y >= _data.WorldHeight || x < 0 || x >= _data.WorldWidth) return RefContent.Edge;

			var cont = _data.World[x, y];

			return cont switch
			{
				0 => RefContent.Free,
				65500 => RefContent.Grass,
				65501 => RefContent.Organic,
				65502 => RefContent.Mineral,
				65503 => RefContent.Wall,
				65504 => RefContent.Poison,
				_ => cont >= 1 && cont <= _data.CurrentNumberOfBots ? RefContent.Bot : throw new Exception("return cont switch")
			};
		}

		private RefContent RefContentByBotRelativity(uint cont)
		{
			// надо определить родственник ли бот

			return Genom.IsRelative(((Bot1)_data.Bots[cont]).Genom)
				? RefContent.Relative
				: RefContent.Bot;
		}

		private Point GetRandomFreeCellNearby()
		{
			RefContent refContent;
			int x;
			int y;
			var i = 0;

			var dir = _func.GetRandomDirection();
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
			var (dX, dY) = _func.GetDeltaDirection(dir);

			var nX = P.X + dX;
			var nY = P.Y + dY;

			// Проверка перехода сквозь экран
			if (!_data.LeftRightEdge)
			{
				if (nX < 0) nX += _data.WorldWidth;
				if (nX >= _data.WorldWidth) nX -= _data.WorldWidth;
			}
			if (!_data.UpDownEdge)
			{
				if (nY < 0) nY += _data.WorldHeight;
				if (nY >= _data.WorldHeight) nY -= _data.WorldHeight;
			}

			return (nX, nY);
		}

		private void ShiftCodePointer(int shift)
		{
			Pointer = (Pointer + shift) % _data.GenomLength;
		}

		private Direction GetDirAbsolute()
		{
			return (Direction)(((int)Genom.GetNextCommand(Pointer)) % 8);
		}
		private Direction GetDirRelative()
		{
			return (Direction)(((int)Genom.GetNextCommand(Pointer) + (int)Dir) % 8);
		}
		private Direction DirIncrement(Direction dir)
		{
			return (Direction)(((int)dir + 1) % 8);
		}

		public string GetText()
		{
			var sb = new StringBuilder();

			sb.AppendLine($"Age: {_age}");
			sb.AppendLine($"Num: {_num}");
			sb.AppendLine($"Index: {Index}");

			sb.AppendLine($"Pointer: {Pointer}");
			sb.AppendLine($"Energy: {Energy}");
			sb.AppendLine($"_dir: {Dir}");

			sb.AppendLine("");
			sb.AppendLine($"Genom {Genom.Level}");
			sb.AppendLine($"Bots: {Genom.Bots}");

			sb.AppendLine("");
			sb.AppendLine($"Color: R{Genom.Color.R} G{Genom.Color.G} B{Genom.Color.B} A{Genom.Color.A}");
			sb.AppendLine($"Hash: {Genom.GenomHash.ToString().Substring(0, 8)}");
			sb.AppendLine($"Parent: {Genom.ParentGenomHash.ToString().Substring(0, 8)}");
			sb.AppendLine($"Grand: {Genom.GrandParentGenomHash.ToString().Substring(0, 8)}");

			return sb.ToString();
		}
	}
}

