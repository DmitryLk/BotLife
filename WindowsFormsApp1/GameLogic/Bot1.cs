using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.Static;

namespace WindowsFormsApp1.GameLogic
{
	// Бот с океана foo52
	public class Bot1
	{
		//private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
		//private static readonly object _busy = new object();

		//private static long COUNTER1 = 0;
		//private static long COUNTER2 = 0;

		public Genom Genom;
		public Color Color;
		public int Pointer;
		public int OldPointer;

		public CodeHistory Hist;

		public double _Xd;
		public double _Yd;
		public int Xi;
		public int Yi;

		public int Energy
		{
			get { return _en; }
			set
			{
				_en = value;
				if (Data.BotColorMode == BotColorMode.Energy)
				{
					Color = GetGraduatedColor(Energy, 0, 6000);
					if (Data.DrawType == DrawType.OnlyChangedCells)
					{
						Func.FixChangeCell(Xi, Yi, Color);
					}
				}
			}
		}

		public long Index;         // Индекс бота (может меняться)
		public int Direction;         // Направление бота
		public int Age;

		private int _en;
		private long _num;         // Номер бота (остается постоянным)
		private int _size;



		private Bot1(int x, int y, int dir, long botNumber, long botIndex, int en, Genom genom, int pointer)
		{
			Pointer = pointer;
			OldPointer = pointer;
			this.Genom = genom;
			Hist = new CodeHistory();

			Direction = dir;
			_num = botNumber;
			Index = botIndex;
			Energy = en;
			Age = 0;

			_Xd = x;
			_Yd = y;
			Xi = x;
			Yi = y;
		}

		public void RefreshColor()
		{
			Color = Data.BotColorMode switch
			{
				BotColorMode.GenomColor => Genom.Color,
				BotColorMode.PraGenomColor => Genom.PraColor,
				BotColorMode.PlantPredator => Genom.Plant ? Color.Green : Color.Red,
				BotColorMode.Energy => GetGraduatedColor(Energy, 0, 6000),
				BotColorMode.Age => GetGraduatedColor(Age, 0, 500),
				BotColorMode.GenomAge => GetGraduatedColor((int)(Data.CurrentStep - Genom.BeginStep), 0, 10000),
				_ => throw new Exception("Color = Data.BotColorMode switch")
			};
		}


		private Color GetGraduatedColor(int grad, int min, int max)
		{
			const int hueFrom = 200;
			// 0 -		FFFF00
			// 1000 -	FFFFFF
			//0 ->	H = 120 S = 100 V =100
			//20 -> H = 60	S = 100 V =100
			//40 -> H = 0	S = 100 V =100

			var hue = hueFrom - hueFrom * grad / (max - min);
			if (hue < 0) hue = 0;
			if (hue > hueFrom) hue = hueFrom;

			return Data.GrColors[hue];
		}


		public void Step()
		{
			// Некий алгоритм активности бота в рамках одного игорового шага.
			// В результате которого он на основании данных от рецепторов или без них
			// изменит свое состояние (координаты, геном, энергия, здоровье, возраст, направление, цвет, минералы, ...)
			// Также может размножиться

			//Direction dir;
			int shift = 0;
			bool stepComplete = false;
			int cntJump = 0;

			//Death
			if (Energy <= 0)
			{
				ToDeathList();
				return;
			}


			Hist.BeginNewStep();
			do
			{
				// 1. Определяем команду которую будет делать бот
				var cmdCode = Genom.GetCurrentCommand(Pointer);
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

			Age++;

			lock (_busy)
			{
				Interlocked.Increment(ref COUNTER1);
				if (COUNTER1 - COUNTER2 > 1)
				{
				}
				Energy += Data.DeltaEnergyOnStep;
				Data.TotalEnergy += Data.DeltaEnergyOnStep;
				Interlocked.Increment(ref COUNTER2);
			}


			//Death
			if (Energy <= 0)
			{
				ToDeathList();
				return;
			}

			//Reproduction
			if (CanReproduct())
			{
				ToReproductionList();
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


		public bool CanReproduct()
		{
			return Energy >= Data.ReproductionBotEnergy;
		}

		public void ToDeathList()
		{
			var num = Interlocked.Increment(ref Data.NumberOfBotDeath);
			Data.BotDeath[num] = this;
		}

		private void ToReproductionList()
		{
			var num = Interlocked.Increment(ref Data.NumberOfBotReproduction);
			Data.BotReproduction[num] = this;
		}

		private (int shift, bool stepComplete) Photosynthesis()
		{
			if (Yi < Data.PhotosynthesisLayerHeight)
			{
				lock (_busy)
				{
					Interlocked.Increment(ref COUNTER1);
					if (COUNTER1 - COUNTER2 > 1)
					{
					}
					Energy += Data.PhotosynthesisEnergy;
					Data.TotalEnergy += Data.PhotosynthesisEnergy;
					Genom.Plant = true;
					Interlocked.Increment(ref COUNTER2);
				}
				//genom.Color = Color.Green;
				return (1, true);
			}
			else
			{
				return (1, false);
			}
		}

		private (int shift, bool stepComplete) EatInRelativeDirection()
		{
			// Алгоритм:
			// 1. Узнаем направление предполагаемой еды
			var dir = GetDirRelative();

			RefContent refContent;
			lock (_busy)
			{
				Interlocked.Increment(ref COUNTER1);
				if (COUNTER1 - COUNTER2 > 1)
				{
				}

				// 2. Узнаем по направлению новые координаты, что там находится, можно ли туда передвинуться, последующее смещение кода
				(refContent, var nXi, var nYi, var cont) = GetCellInfo3(dir);

				// 3. Узнаем съедобно ли это
				if (refContent == RefContent.Grass)
				{
					EatGrass(nXi, nYi);
				}

				if (refContent == RefContent.Bot ||
					refContent == RefContent.Relative)
				{
					EatBot(nXi, nYi, cont);
				}
				Interlocked.Increment(ref COUNTER2);
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

			RefContent refContent;
			lock (_busy)
			{
				Interlocked.Increment(ref COUNTER1);
				if (COUNTER1 - COUNTER2 > 1)
				{
				}

				// 2. Узнаем по направлению новые координаты, что там находится, можно ли туда передвинуться, последующее смещение кода
				(refContent, var nXi, var nYi, var cont) = GetCellInfo3(dir);

				// 3. Узнаем съедобно ли это
				if (refContent == RefContent.Grass)
				{
					EatGrass(nXi, nYi);
				}

				if (refContent == RefContent.Bot ||
					refContent == RefContent.Relative)
				{
					EatBot(nXi, nYi, cont);
				}

				Interlocked.Increment(ref COUNTER2);
			}



			if (refContent == RefContent.Organic ||
				refContent == RefContent.Mineral ||
				refContent == RefContent.Poison)
			{
			}

			return ((int)refContent, true);
		}

		private void EatGrass(int nXi, int nYi)
		{
			Energy += Data.FoodEnergy;

			Data.World[nXi, nYi] = (long)CellContent.Free;
			if (Data.DrawType == DrawType.OnlyChangedCells) Func.FixChangeCell(nXi, nYi, null);
		}


		private void EatBot(int nXi, int nYi, long cont)
		{
			var eatedBot = Data.Bots[cont];

			// Растение не может есть животное
			if (Genom.Plant && !eatedBot.Genom.Plant)
			{
				return;
			}

			// Животное может есть растение, но ни тогда когда его осталось мало
			//if (!genom.Plant && eatedBot.genom.Plant)
			//{
			//	if (eatedBot.genom.Bots < 2)
			//	{
			//		return;
			//	}
			//}

			var energyCanEat = eatedBot.Energy > Data.BiteEnergy ? Data.BiteEnergy : eatedBot.Energy;

			Energy += energyCanEat;
			eatedBot.Energy -= energyCanEat;

			if (eatedBot.Energy <= 0)
			{
				eatedBot.ToDeathList();
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
			Direction = dir;
			return (2, false);
		}

		private (int shift, bool stepComplete) RotateInAbsoluteDirection()
		{
			// Алгоритм:
			// 1. Узнаем новое направление
			var dir = GetDirAbsolute();
			// 2. Меняем направление бота
			Direction = dir;
			return (2, false);
		}


		private static readonly object _busy1 = new object();
		private (int shift, bool stepComplete) StepInRelativeDirection()
		{
			// Алгоритм:
			// 1. Узнаем направление предполагаемого движения
			var dir = GetDirRelative();

			RefContent refContent;
			lock (_busy)
			{
				Interlocked.Increment(ref COUNTER1);
				if (COUNTER1 - COUNTER2 > 1)
				{
				}
				// 2. Узнаем по направлению новые координаты, что там находится, можно ли туда передвинуться, последующее смещение кода
				(var iEqual, refContent, var nXd, var nYd, var nXi, var nYi) = GetCellInfo2(dir);

				// 3. Если есть возможность туда передвинуться , то перемещаем туда бота
				if (iEqual)
				{
					MoveOnlyDouble(nXd, nYd);
				}
				else
				{
					if (refContent == RefContent.Free || refContent == RefContent.Poison)
					{
						Move(nXd, nYd, nXi, nYi);
					}
				}
				Interlocked.Increment(ref COUNTER2);
			}

			return ((int)refContent, true);
		}


		private static readonly object _busy2 = new object();
		private (int shift, bool stepComplete) StepInAbsoluteDirection()
		{
			// Алгоритм:
			// 1. Узнаем направление предполагаемого движения
			var dir = GetDirAbsolute();

			RefContent refContent;
			lock (_busy)
			{
				Interlocked.Increment(ref COUNTER1);
				if (COUNTER1 - COUNTER2 > 1)
				{
				}
				// 2. Узнаем по направлению новые координаты, что там находится, можно ли туда передвинуться, последующее смещение кода
				(var iEqual, refContent, var nXd, var nYd, var nXi, var nYi) = GetCellInfo2(dir);

				// 3. Если есть возможность туда передвинуться , то перемещаем туда бота
				if (iEqual)
				{
					MoveOnlyDouble(nXd, nYd);
				}
				else
				{
					if (refContent == RefContent.Free || refContent == RefContent.Poison)
					{
						Move(nXd, nYd, nXi, nYi);
					}
				}
				Interlocked.Increment(ref COUNTER2);
			}

			return ((int)refContent, true);
		}

		//////////////////////////////////////////////////////////////////


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
		private void Move(double nXd, double nYd, int nXi, int nYi)
		{
			Data.World[Xi, Yi] = (long)CellContent.Free;
			if (Data.DrawType == DrawType.OnlyChangedCells) Func.FixChangeCell(Xi, Yi, null);

			_Xd = nXd;
			_Yd = nYd;
			Xi = nXi;
			Yi = nYi;

			Data.World[Xi, Yi] = Index;
			if (Data.DrawType == DrawType.OnlyChangedCells) Func.FixChangeCell(Xi, Yi, Color);
		}

		private void MoveOnlyDouble(double nXd, double nYd)
		{
			_Xd = nXd;
			_Yd = nYd;
		}

		//////////////////////////////////////////////////////////////////




		private void ShiftCodePointer(int shift)
		{
			OldPointer = Pointer;
			Pointer = (Pointer + shift) % Data.GenomLength;
		}


		// Метод может вернуть координаты равные текущим (используется для движения)
		// и отличные от текущих (используется для посмотреть, есть, схватить, ...)
		private (double newXdouble, double newYdouble, int newXint, int newYint, bool iEqual) GetCoordinatesByDirection(int dir, bool onlyDifferent)
		{
			var (deltaXdouble, deltaYdouble) = Dir.GetDeltaDirection(dir);

			var newXdouble = _Xd + deltaXdouble;
			var newYdouble = _Yd + deltaYdouble;

			var newXint = Dir.Round(newXdouble);
			var newYint = Dir.Round(newYdouble);

			var iEqual = newXint == Xi && newYint == Yi;

			if (onlyDifferent && iEqual)
			{
				// координаты не изменились. надо шагнуть дальше
				var (deltaXdouble2, deltaYdouble2) = Dir.GetDeltaDirection2(dir);

				newXdouble = _Xd + deltaXdouble2;
				newYdouble = _Yd + deltaYdouble2;

				newXint = Dir.Round(newXdouble);
				newYint = Dir.Round(newYdouble);

				// проверка на изменение координат еще раз. такого не может быть
				if (newXint == Xi && newYint == Yi)
				{
					throw new Exception("if (newXint == _Xi && newYint == _Yi)");
				}

				if (newXint - Xi > 1 || newXint - Xi < -1 || newYint - Yi > 1 || newYint - Yi < -1)
				{
					throw new Exception("if (newXint - _Xi > 1 || newXint - _Xi < -1 || newYint - _Yi > 1 || newYint - _Yi < -1)");
				}

				iEqual = false;
			}

			// Проверка перехода сквозь экран
			if (!Data.LeftRightEdge)
			{
				if (newXint < 0)
				{
					newXint += Data.WorldWidth;
					newXdouble += Data.WorldWidth;
				}

				if (newXint >= Data.WorldWidth)
				{
					newXint -= Data.WorldWidth;
					newXdouble -= Data.WorldWidth;
				}
			}

			if (!Data.UpDownEdge)
			{
				if (newYint < 0)
				{
					newYint += Data.WorldHeight;
					newYdouble += Data.WorldHeight;
				}

				if (newYint >= Data.WorldHeight)
				{
					newYint -= Data.WorldHeight;
					newYdouble -= Data.WorldHeight;
				}
			}

			return (newXdouble, newYdouble, newXint, newYint, iEqual);
		}



		#region GetRefContent

		private RefContent GetRefContent(int x, int y)
		{
			//смещение условного перехода 2-пусто  3-стена  4-органика 5-бот 6-родня

			// Если координаты попадают за экран то вернуть RefContent.Edge
			if (y < 0 || y >= Data.WorldHeight || x < 0 || x >= Data.WorldWidth) return RefContent.Edge;

			long cont;
			lock (_busy)
			{
				Interlocked.Increment(ref COUNTER1);
				if (COUNTER1 - COUNTER2 > 1)
				{
				}

				cont = Data.World[x, y];
				if (cont >= 1 && cont <= Data.CurrentNumberOfBots) return RefContentByBotRelativity(cont);
				Interlocked.Increment(ref COUNTER2);
			}

			if (cont < 0 || (cont > 0 && !(cont >= 65500 && cont <= 65504))) throw new Exception("cont = Data.World[x, y];");

			return cont switch
			{
				0 => RefContent.Free,
				65500 => RefContent.Grass,
				65501 => RefContent.Organic,
				65502 => RefContent.Mineral,
				65503 => RefContent.Wall,
				65504 => RefContent.Poison,
				_ => throw new Exception("return cont switch")
			};
		}

		private (RefContent refContent, long cont) GetRefContentAndCont(int x, int y)
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


		private RefContent RefContentByBotRelativity(long cont)
		{
			// надо определить родственник ли бот

			return Genom.IsRelative(Data.Bots[cont].Genom)
				? RefContent.Relative
				: RefContent.Bot;
		}


		#endregion

		#region GetCellInfo
		// For Look
		private RefContent GetCellInfo1(int dir)
		{
			// 1. Узнаем координаты клетки на которую надо посмотреть
			var (_, _, nXi, nYi, _) = GetCoordinatesByDirection(dir, true);

			// 2. Узнаем что находится на этой клетке
			var refContent = GetRefContent(nXi, nYi);

			// 3. Возвращаем полученную информацию
			return refContent;
		}

		// For move
		private (bool Equal, RefContent refContent, double nXd, double nYd, int nXi, int nYi) GetCellInfo2(int dir)
		{
			// 1. Узнаем координаты предполагаемого перемещения
			var (nXd, nYd, nXi, nYi, iEqual) = GetCoordinatesByDirection(dir, false);
			//private (double newXdouble, double newYdouble, int newXint, int newYint, bool iEqual) GetCoordinatesByDirection(int dir, bool onlyDifferent)

			if (iEqual)
			{
				return (true, RefContent.Free, nXd, nYd, nXi, nYi);
			}

			// 2. Узнаем что находится на этой клетке
			var refContent = GetRefContent(nXi, nYi);

			// 3. Возвращаем полученную информацию
			return (false, refContent, nXd, nYd, nXi, nYi);
		}

		// For eat
		private (RefContent refContent, int nX, int nY, long cont) GetCellInfo3(int dir)
		{
			// 1. Узнаем координаты клетки на которую надо посмотреть
			var (_, _, nXi, nYi, _) = GetCoordinatesByDirection(dir, true);

			// 2. Узнаем что находится на этой клетке
			var (refContent, cont) = GetRefContentAndCont(nXi, nYi);

			// 3. Возвращаем полученную информацию
			return (refContent, nXi, nYi, cont);
		}
		#endregion


		#region Direction
		private int GetDirAbsolute()
		{
			return Dir.GetDirectionFromCodeAbsolute(Genom.GetNextCommand(Pointer));
		}
		private int GetDirRelative()
		{
			return Dir.GetDirectionFromCodeRelative(Direction, Genom.GetNextCommand(Pointer));
		}
		#endregion

		#region GetText
		public string GetText1()
		{
			var sb = new StringBuilder();

			sb.AppendLine($"Age: {Age}");
			sb.AppendLine($"Num: {_num}");
			sb.AppendLine($"Index: {Index}");

			sb.AppendLine($"Energy: {Energy}");
			sb.AppendLine($"_dir: {Direction}");


			sb.AppendLine("");
			sb.AppendLine($"Genom {Genom.PraNum} {(Genom.Num != 0 ? $"({Genom.Num})" : "")}Lev{Genom.Level}");
			sb.AppendLine($"Bots: {Genom.CurBots}");

			sb.AppendLine("");
			sb.AppendLine($"Color: R{Color.R} G{Color.G} B{Color.B}");
			sb.AppendLine($"Pra: {Genom.PraHash.ToString().Substring(0, 8)}");
			sb.AppendLine($"Hash: {Genom.GenomHash.ToString().Substring(0, 8)}");
			sb.AppendLine($"Parent: {Genom.ParentHash.ToString().Substring(0, 8)}");
			sb.AppendLine($"Grand: {Genom.GrandHash.ToString().Substring(0, 8)}");

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
					var cmdTxt = Genom.Code[hist[i]] switch
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

					var dirStr = Dir.GetDirectionStringFromCode(Genom.GetNextCommand(hist[i]));
					if (cmdTxt != "")
					{
						sb.AppendLine($"{cmdTxt} {dirStr}");
					}
				}
			}

			sb.AppendLine("");

			return sb.ToString();
		}
		#endregion

		public static void CreateNewBot(int x, int y, long botIndex, int en, Genom genom)
		{
			var dir = Dir.GetRandomDirection();
			var pointer = 0;
			var botNumber = Interlocked.Increment(ref Data.MaxBotNumber);

			genom.AddBot();
			var bot = new Bot1(x, y, dir, botNumber, botIndex, en, genom, pointer);
			bot.RefreshColor();
			Data.Bots[botIndex] = bot;

			Data.World[x, y] = botIndex;
			if (Data.DrawType == DrawType.OnlyChangedCells) Func.FixChangeCell(x, y, bot.Color);
		}


		private async Task<bool> SetBusy()
		{
			try
			{
				await _semaphore.WaitAsync();
			}
			catch
			{
				ReleaseBusy();
				return false;
			}
			return true;
		}

		private void ReleaseBusy()
		{
			try
			{
				_semaphore.Release();
			}
			catch { }
		}

		//private bool TrySetBusyMode()
		//{
		//	lock (_syncBusyMode)
		//	{
		//		if (_busyState) return false;
		//		_busyState = true;
		//		return true;
		//	}
		//}
		//private void ReleaseBusyMode()
		//{
		//	lock (_syncBusyMode)
		//	{
		//		_busyState = false;
		//	}
		//}

		//	private async Task<(SocketAcceptionResult, ISocket)> GetSocket(ISocketManager socketManager, string tokenId)
		//	{
		//		try
		//		{
		//			var suссessBusy = await SetBusy();
		//			if (!suссessBusy)
		//			{
		//				return (SocketAcceptionResult.UnknownError, null);
		//			}
		//		}
		//		// ReleaseBusy() в finally, чтобы перед каждым return его не писать
		//		finally
		//		{
		//			// Метод не возвращает исключений.
		//			ReleaseBusy();
		//		}
		//	}
		//}
	}
}
