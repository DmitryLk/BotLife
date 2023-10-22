using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.DirectoryServices.ActiveDirectory;
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
using WindowsFormsApp1.Logger;
using WindowsFormsApp1.Static;

namespace WindowsFormsApp1.GameLogic
{
	// Бот с океана foo52
	public class Bot1
	{
		//private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
		//private static readonly object _busy = new();

		const int hueFrom = 200;

		private static readonly object _busyWorld1 = new();
		private static readonly object _busyWorld2 = new();
		private readonly object _busyBotEnergy = new();
		private readonly object _busyInsertedToReproductionList = new();
		private readonly object _busyReceptors = new();
		//private static readonly object _busyTotalEnergy = new();
		//private readonly object _busyBite = new();
		//private readonly object _busyInsertedToDeathList = new();

		// РЕЦЕПТОРЫ
		// меня укусили?
		// увидел рядом чтото?
		private int _recNum;
		private int _recContactDir;
		private int _recPointer;
		private int _procNum;



		//private static long COUNTER1 = 0;
		//private static long COUNTER2 = 0;

		public Genom G;
		public Color Color;
		public int Pointer;
		public int OldPointer;


		public CodeHistory hist;
		//public BotLog Log;

		public double Xd;
		public double Yd;
		public int Xi;
		public int Yi;


		public long Index;         // Индекс бота (может меняться)
		public int Direction;         // Направление бота
		public int Age;
		public int BiteMeCount;
		public int BiteImCount;
		public long Num;         // Номер бота (остается постоянным)

		private int _en;
		private int _size;

		public bool Alive;
		public bool InsertedToDeathList;  // чтобы не вставить бота два раза в этот лист, только для этого
		public bool InsertedToReproductionList;  // чтобы не вставить бота два раза в этот лист, только для этого
		private int _reproductionCycle;

		private int df = 0;

		private long _moved = 0;
		public bool Moved
		{
			get
			{
				return _moved > 10;
			}
		}
		public void ResetMoved()
		{
			_moved = 0;
		}
		public int Energy
		{
			get
			{
				return _en;
				//lock (_busyBotEnergy)
				//{
				//    return _en;
				//}
			}
			//set
			//{
			//    Log.AddLog($"Change Energy from {_en} to {value}");
			//    //if (_en == value)
			//    //{
			//    //}

			//    //df++;
			//    _en = value;
			//    //if (df > 1) throw new Exception();
			//    //df--;
			//    //lock (_busyBotEnergy)
			//    //{
			//    //}

			//    if (Data.BotColorMode == BotColorMode.Energy)
			//    {
			//        Color = GetGraduatedColor(Energy, 0, 6000);
			//        if (Data.DrawType == DrawType.OnlyChangedCells)
			//        {
			//            Func.FixChangeCell(Xi, Yi, Color);
			//        }
			//    }
			//}
		}

		public int Bite(int delta, int contactDir)
		{
			Interlocked.Increment(ref BiteMeCount);
			ActivateReceptor1(contactDir);
			return EnergyChange(delta);
		}

		#region Receptors
		// 1 - укус
		private void ActivateReceptor1(int contactDir)
		{
			if (_recNum != 1)
			{
				lock (_busyReceptors)
				{
					if (_recNum != 1)
					{
						_recNum = 1;
						_recContactDir = contactDir;
						_recPointer = 0;
						_procNum = 0;
					}
				}
			}
		}

		// 2 - рядом бот
		private void ActivateReceptor2(int contactDir, bool rel, int dir) //, bool block, int massa, byte[] Shield, byte[] Attack, int dir, bool mov)
		{
			if (_recNum == 0 || _recNum > 2)
			{
				lock (_busyReceptors)
				{
					if (_recNum == 0 || _recNum > 2)
					{
						_recNum = 2;
						_recContactDir = contactDir;
						_recPointer = 0;

						var needbigrotate = Dir.GetDirDiff(contactDir, dir) < Dir.NumberOfDirections / 4;


						if (rel)
						{
							_procNum = 1;
						}
						else
						{
							if (needbigrotate)
							{
								_procNum = 2;
							}
							else
							{
								_procNum = 3;
							}
						}
					}
				}
			}
		}

		// 3 - рядом еда
		private void ActivateReceptor3(int contactDir)
		{
			if (_recNum == 0 || _recNum > 3)
			{
				lock (_busyReceptors)
				{
					if (_recNum == 0 || _recNum > 3)
					{
						_recNum = 3;
						_recContactDir = contactDir;
						_recPointer = 0;
						_procNum = 4;
					}
				}
			}
		}

		// 4 - рядом минерал
		private void ActivateReceptor4(int contactDir)
		{
			if (_recNum == 0 || _recNum > 4)
			{
				lock (_busyReceptors)
				{
					if (_recNum == 0 || _recNum > 4)
					{
						_recNum = 4;
						_recContactDir = contactDir;
						_recPointer = 0;
						_procNum = 5;
					}
				}
			}
		}

		// 5 - рядом край/стена
		private void ActivateReceptor5(int contactDir)
		{
			if (_recNum == 0 || _recNum > 5)
			{
				lock (_busyReceptors)
				{
					if (_recNum == 0 || _recNum > 5)
					{
						_recNum = 5;
						_recContactDir = contactDir;
						_recPointer = 0;
						_procNum = 6;
					}
				}
			}
		}
		#endregion

		/// <summary>
		/// delta - энергия которая будет добавлена к энергии бота
		/// </summary>
		/// <param name="delta"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public int EnergyChange(int delta)
		{
			if (delta == 0) return 0;

			lock (_busyBotEnergy)
			{
				if (_en + delta < 0) delta = -_en;
				//Log.LogInfo($"Change Energy from {_en} to {_en + delta}");
				_en += delta;


				if (!Alive && _en > 0)
				{
					Alive = true;
					Interlocked.Decrement(ref Data.QtyFactBotDeath);
					//InsertedToDeathList = false;
				}

				if (_en == 0 && Alive)
				{
					Alive = false;
					Interlocked.Increment(ref Data.QtyFactBotDeath);
					if (!InsertedToDeathList)
					{
						InsertedToDeathList = true;
						Data.BotDeath[Interlocked.Increment(ref Data.QtyAllBotDeathMinusOne)] = this;
						//Log.LogInfo("bot inserted to DeathList");
					}
				}
			}

			if (_en < 0)
			{
				throw new Exception("fdgdfgdfg");
			}

			if (Data.BotColorMode == BotColorMode.Energy)
			{
				RefreshColor();
				if (Data.DrawType == DrawType.OnlyChangedCells)
				{
					Func.FixChangeCell(Xi, Yi, Color);
				}
			}

			return -delta;
		}

		public void EnergySet(int en)
		{
			if (en <= 0) throw new Exception("sdfsdf");
			if (_en != 0) throw new Exception("fglkeru84");

			lock (_busyBotEnergy)
			{
				_en = en;
			}
		}


		public Bot1(int x, int y, int dir, long botNumber, long botIndex, int en, Genom genom, int pointer)
		{
			Pointer = pointer;
			OldPointer = pointer;
			this.G = genom;
			hist = new CodeHistory();
			//Log = new BotLog();

			Direction = dir;
			Num = botNumber;
			Index = botIndex;
			_en = en;
			Age = 0;
			BiteMeCount = 0;
			BiteImCount = 0;

			Xd = x;
			Yd = y;
			Xi = x;
			Yi = y;

			InsertedToDeathList = false;
			InsertedToReproductionList = false;
			Alive = true;
			_reproductionCycle = 0;
			//Log.LogInfo($"bot was born. index:{Index}");
		}

		public void RefreshColor()
		{
			Color = Data.BotColorMode switch
			{
				BotColorMode.GenomColor => G.Color,
				BotColorMode.PraGenomColor => G.PraColor,
				BotColorMode.PlantPredator => G.Plant ? Color.Green : Color.Red,
				BotColorMode.Energy => GetGraduatedColor(Energy, 0, 6000),
				BotColorMode.Age => GetGraduatedColor(500 - Age, 0, 500),
				BotColorMode.GenomAge => GetGraduatedColor(6000 - (int)(Data.CurrentStep - G.BeginStep), 0, 6000),
				_ => throw new Exception("Color = Data.BotColorMode switch")
			};
		}


		private Color GetGraduatedColor(int grad, int min, int max)
		{
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

			//Func.CheckWorld2(Index, Num, Xi, Yi);
			//Func.CheckWorld2(Index, Num, Xi, Yi);

			if (Data.HistoryOn) hist.BeginNewStep();

			CommandCycle();

			Age++;

			var realchange = EnergyChange(Data.DeltaEnergyOnStep);
			if (Data.DeltaEnergyOnStep != 0) Interlocked.Add(ref Data.TotalEnergy, -realchange);

			if (Data.BotColorMode == BotColorMode.GenomAge || Data.BotColorMode == BotColorMode.Age)
			{
				RefreshColor();
				if (Data.DrawType == DrawType.OnlyChangedCells)
				{
					Func.FixChangeCell(Xi, Yi, Color);
				}
			}


			//Func.CheckWorld2(Index, Num, Xi, Yi);
			//Func.CheckWorld2(Index, Num, Xi, Yi);

			if (CanReproduct())
			{
				ToReproductionList();
			}

			//Func.CheckWorld2(Index, Num, Xi, Yi);
		}

		private (bool, byte, byte) GetCommand1()
		{
			var cmd = G.GetCurrentCommandAndSetActGen(Pointer, true);
			if (Data.HistoryOn) hist.SavePtr(Pointer);
			return (false, cmd, 0);
		}

		private (bool, byte, byte) GetCommand2()
		{
			byte cmd;
			byte par;
			if (_recNum > 0)  // Есть сигнал от рецепторов. Цикл по командам конкретного event _recNum.
			{
				lock (_busyReceptors)
				{
					cmd = G.CodeForEvents[_procNum, _recPointer, 0];
					par = G.CodeForEvents[_procNum, _recPointer, 1];
					_recPointer++;
					if (_recPointer >= G.CodeForEventsLenght[_procNum] || _recPointer == Data.GenomEventsLenght) _recNum = 0;
				}

				return (true, cmd, par);
			}
			else
			{
				cmd = G.GetCurrentCommandAndSetActGen(Pointer, true);
				if (Data.HistoryOn) hist.SavePtr(Pointer);
				return (false, cmd, 0);
			}
		}


		private void CommandCycle()
		{
			int cntJump = 0;
			int shift = 0;
			byte cmd;
			byte par;
			bool ev;
			bool realCmd;
			bool bigrotate = false;

			do
			{
				(ev, cmd, par) = GetCommand2();

				if (ev)
				{
					switch (cmd)
					{
						//case Cmd.RotateRelative: (_ , bigrotate) = RotateRelative(par); break;
						case Cmd.RotateRelativeContact: (_, bigrotate) = RotateRelativeContact(par); break;
						case Cmd.RotateBackward: (_, bigrotate) = RotateBackward(); break;
						case Cmd.RotateBackwardContact: (_, bigrotate) = RotateBackwardContact(); break;
						//case Cmd.LookAround: LookAround(); break;
						//case Cmd.StepRelative: StepRelative(par); break;
						case Cmd.StepRelativeContact: StepRelativeContact(par); break;
						case Cmd.StepBackward: StepBackward(); break;
						case Cmd.StepBackwardContact: StepBackwardContact(); break;
						case Cmd.EatForward1: EatForward(); break;
						//case Cmd.EatContact: EatContact(); break;
						default: break;
					};
				}
				else
				{
					realCmd = true;
					switch (cmd)
					{
						//case Cmd.RotateAbsolute: (shift, bigrotate) = RotateAbsolute(G.GetDirectionFromNextCommand(Pointer, true)); break;
						case Cmd.RotateRelative: (shift, bigrotate) = RotateRelative(G.GetDirectionFromNextCommand(Pointer, true)); break;
						case Cmd.StepForward1: shift = StepForward(); break;
						case Cmd.StepForward2: shift = StepForward(); break;
						case Cmd.EatForward1: shift = EatForward(); break;
						case Cmd.EatForward2: shift = EatForward(); break;
						case Cmd.LookForward1: shift = LookForward(); break;
						case Cmd.LookForward2: shift = LookForward(); break;
						case Cmd.Photosynthesis: shift = Photosynthesis(); break;
						case Cmd.LookAround: shift = LookAround(); break;
						//case Cmd.RotateRandom: shift = RotateRandom(); break;
						//case Cmd.AlignHorizontaly: shift = AlignHorizontaly(); break;
						default: shift = cmd; break;
					};

					if (realCmd)
					{
						if (cmd == Cmd.StepForward1 || cmd == Cmd.StepForward2)
						{
							if (_moved < 50)
							{
								_moved += 5;
							}
						}
						else
						{
							if (_moved > 0)
							{
								_moved--;
							}
						}
					}
					ShiftCodePointer(shift);
				}

				cntJump++;
			}
			while (!Data.CompleteCommands[cmd] && !bigrotate && cntJump < Data.MaxUncompleteJump);
		}

		//===================================================================================================
		//// Rotate
		//1 C
		private (int, bool) RotateAbsolute(int dir)
		{
			var bigrotate = Dir.GetDirDiff(Direction, dir) > Dir.NumberOfDirections / 4;

			return (Rotate(dir % Dir.NumberOfDirections), bigrotate);
		}

		//2 CE
		private (int, bool) RotateRelative(int dir)
		{
			var bigrotate = Dir.GetDirDiff(0, dir) > Dir.NumberOfDirections / 4;
			return (Rotate((Direction + dir) % Dir.NumberOfDirections), bigrotate);
		}

		//3 E
		private (int, bool) RotateRelativeContact(int dir)
		{
			var bigrotate = Dir.GetDirDiff(Direction, _recContactDir + dir) > Dir.NumberOfDirections / 4;

			return (Rotate((_recContactDir + dir) % Dir.NumberOfDirections), bigrotate);
		}

		//4 E
		private (int, bool) RotateBackward()
		{
			var bigrotate = true;

			return (Rotate(Dir.GetOppositeDirection(Direction)), bigrotate);
		}

		//5 E
		private (int, bool) RotateBackwardContact()
		{
			var dir = Dir.GetOppositeDirection(_recContactDir);
			var bigrotate = Dir.GetDirDiff(Direction, dir) > Dir.NumberOfDirections / 4;

			return (Rotate(dir), bigrotate);
		}

		//6 C - может быть лучше для E ?
		private (int, bool) RotateRandom()
		{
			var dir = Func.GetRandomDirection();
			var bigrotate = Dir.GetDirDiff(Direction, dir) > Dir.NumberOfDirections / 4;

			return (Rotate(Func.GetRandomDirection()), bigrotate);
		}

		//7 C
		private (int, bool) AlignHorizontaly()
		{
			var dir = 16;
			var bigrotate = Dir.GetDirDiff(Direction, dir) > Dir.NumberOfDirections / 4;

			return (Rotate(Func.GetRandomDirection()), bigrotate);
		}

		//// Step
		//10 11 C
		private int StepForward()
		{
			return Step(GetDirForward());
		}

		//12 E
		private int StepRelative(int dir)
		{
			return Step((Direction + dir) % Dir.NumberOfDirections);
		}

		//13 E
		private int StepRelativeContact(int dir)
		{
			return Step((_recContactDir + dir) % Dir.NumberOfDirections);
		}

		//14 E
		private int StepBackward()
		{
			return Step(Dir.GetOppositeDirection(Direction));
		}

		//15 E
		private int StepBackwardContact()
		{
			return Step(Dir.GetOppositeDirection(_recContactDir));
		}

		//// Eat
		//20 21 CE
		private int EatForward()
		{
			return Eat(GetDirForward());
		}

		//22 E
		private int EatContact()
		{
			return Eat(_recContactDir);
		}

		//// Look
		//30 31 C
		private int LookForward()
		{
			return Look(GetDirForward());
		}

		//32 CE
		private int LookAround()
		{
			LookAroundForEnemy();
			return 1;
		}

		//// Other
		//40 C
		private int Photosynthesis()
		{
			if (Yi < Data.PhotosynthesisLayerHeight)
			{
				EnergyChange(Data.PhotosynthesisEnergy);
				Interlocked.Add(ref Data.TotalEnergy, Data.PhotosynthesisEnergy);
				G.Plant = true;
				//genom.Color = Color.Green;
			}

			return 1;
		}


		//===================================================================================================
		public bool CanReproduct()
		{
			return Energy >= Data.ReproductionBotEnergy;
		}

		public void HoldReproduction()
		{
			_reproductionCycle = Data.HoldReproductionTime;
		}

		private void ToReproductionList()
		{
			if (_reproductionCycle == Data.HoldReproductionTime)
			{
				ShareEnergy();
			}

			if (_reproductionCycle-- > 0) return;


			if (!InsertedToReproductionList)
			{
				lock (_busyInsertedToReproductionList)
				{
					if (!InsertedToReproductionList)
					{

						InsertedToReproductionList = true;
						Data.BotReproduction[Interlocked.Increment(ref Data.IndexOfLastBotReproduction)] = this;
					}
				}
			}
		}

		private void ShareEnergy()
		{
			// Передать энергию окружающим ботам
			var n = ThreadSafeRandom.Next(8);
			int nXi, nYi;
			long cont;
			int i = 0;
			var ent = (Energy - Data.ReproductionBotEnergy) / 2;

			if (ent > 0)
			{
				do
				{
					(nXi, nYi) = Func.GetCoordinatesByDelta(Xi, Yi, n);

					if (nYi >= 0 && nYi < Data.WorldHeight && nXi >= 0 && nXi < Data.WorldWidth)
					{
						cont = Data.World[nXi, nYi];

						if (cont >= 1 && cont <= Data.CurrentNumberOfBots && G.IsRelative(Data.Bots[cont].G))
						{
							var targetBot = Data.Bots[cont];

							if (Energy > targetBot.Energy /*&& targetBot.Energy > 0 && !targetBot.InsertedToDeathList*/)
							{
								var transferedEnergy = EnergyChange(-ent);
								targetBot.EnergyChange(transferedEnergy);
								if (transferedEnergy < 0) throw new Exception("dfgdfg");
							}
						}
					}
					if (++n >= 8) n -= 8;
					i++;
				}
				while (CanReproduct() && i <= 8);
			}
		}


		private int Eat(int dir)
		{
			// Алгоритм:
			// 1. Узнаем координаты клетки на которую надо посмотреть
			var (nXi, nYi) = GetCoordinatesByDirectionOnlyDifferent(dir);

			// 2. Узнаем что находится на этой клетке
			if ((Data.UpDownEdge && (nYi < 0 || nYi >= Data.WorldHeight)) ||
			(Data.LeftRightEdge && (nXi < 0 || nXi >= Data.WorldWidth)))
			{
				ActivateReceptor5(dir);
				return 2;
			}


			// Grass
			bool grass = false;
			if (Data.World[nXi, nYi] == 65500)
			{
				lock (_busyWorld1)
				{
					if (Data.World[nXi, nYi] == 65500)
					{
						Data.World[nXi, nYi] = 0;

						grass = true;
					}
				}
			}

			if (grass)
			{
				EnergyChange(Data.FoodEnergy);
				Interlocked.Add(ref Data.TotalEnergy, Data.FoodEnergy);
				Interlocked.Decrement(ref Data.CurrentNumberOfFood);
				if (Data.DrawType == DrawType.OnlyChangedCells) Func.FixChangeCell(nXi, nYi, null);
				return 2;
			}


			var cont = Data.World[nXi, nYi];

			// Bot || Relative
			if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
			{
				BiteBot(cont, dir);
			}

			return 2;
		}


		private void BiteBot(long cont, int dir)
		{
			var eatedBot = Data.Bots[cont];

			// Растение не может есть животное
			if (G.Plant && !eatedBot.G.Plant)
			{
				return;
			}

			// Животное может есть растение, но ни тогда когда его осталось мало
			if (!G.Plant && eatedBot.G.Plant)
			{
				if (eatedBot.G.CurBots < 2)
				{
					return;
				}
			}

			// Не может есть родственника
			if (G.GenomHash == eatedBot.G.GenomHash)
			{
				return;
			}

			// Не может есть нового
			if (Data.DelayForNewbie && Data.CurrentStep - eatedBot.G.BeginStep < 10)
			{
				return;
			}

			//var olden = Energy;
			var atc = 0;
			for (var i = 0; i < G.AttackTypesCnt; i++)
			{
				//1
				if (G.AttackTypes[i].Level > eatedBot.G.Shield[G.AttackTypes[i].Type])
				{
					atc += G.AttackTypes[i].Level - eatedBot.G.Shield[G.AttackTypes[i].Type];
				}

				//2
				//if (G.AttackTypes[i].Level > eatedBot.G.Shield[G.AttackTypes[i].Type]*2)
				//{
				//	atc += G.AttackTypes[i].Level - eatedBot.G.Shield[G.AttackTypes[i].Type]*2;
				//}

				//3
				//if (G.AttackTypes[i].Level > eatedBot.G.Shield[G.AttackTypes[i].Type])
				//{
				//	atc++;
				//}

				//4
				//if (G.AttackTypes[i].Level > eatedBot.G.Shield[G.AttackTypes[i].Type] && eatedBot.G.Shield[G.AttackTypes[i].Type] == 0)
				//{
				//	atc += G.AttackTypes[i].Level - eatedBot.G.Shield[G.AttackTypes[i].Type];
				//}
			}

			if (atc > 0)
			{
				// Data.BiteEnergy / 2 * atc - отрицательное число. возвращается положительное число.

				var k = 20;
				if (!Moved && eatedBot.Moved) k = Data.MovedBiteStrength;

				var requestedEnergy = Data.BiteEnergy * 10 / k * atc;

				Interlocked.Increment(ref BiteImCount);
				var gotEnergyByEating = eatedBot.Bite(requestedEnergy, Dir.GetOppositeDirection(dir));
				EnergyChange(gotEnergyByEating);

				//var gotEnergyByEating = eatedBot.EnergyChange(Data.BiteEnergy);
				if (gotEnergyByEating < 0) throw new Exception("dfgdfg");
			}


			//eatedBot.Log.LogInfo($"bot was bited. energy:{eatedBot.Energy}");
			//Log.LogInfo($"bot{Index} bite bot{cont} and got {gotEnergyByEating} energy. From {olden} to {Energy}.");
		}

		private int Look(int dir)
		{
			// Алгоритм:
			// 1. Узнаем координаты клетки на которую надо посмотреть
			var (nXi, nYi) = GetCoordinatesByDirectionOnlyDifferent(dir);

			// 2. Узнаем что находится на этой клетке

			//смещение условного перехода 2-пусто  3-стена  4-органика 5-бот 6-родня

			// Если координаты попадают за экран то вернуть RefContent.Edge
			if (nYi < 0 || nYi >= Data.WorldHeight || nXi < 0 || nXi >= Data.WorldWidth)
			{
				ActivateReceptor5(dir);
				return 2;
			}

			var cont = Data.World[nXi, nYi];

			if (cont == 65500)
			{
				ActivateReceptor3(dir);
			}

			if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
			{
				ActivateReceptor2(dir, G.IsRelative(Data.Bots[cont].G), Data.Bots[cont].Direction);
			}

			return 2;
		}

		private void LookAroundForEnemy()
		{
			int nXi, nYi, dir;
			Bot1 b;


			for (var n = 0; n < 8; n++)
			{
				(nXi, nYi) = Func.GetCoordinatesByDelta(Xi, Yi, n);

				if (nYi >= 0 && nYi < Data.WorldHeight && nXi >= 0 && nXi < Data.WorldWidth)
				{
					var cont = Data.World[nXi, nYi];

					if (cont >= 1 && cont <= Data.CurrentNumberOfBots && !G.IsRelative(Data.Bots[cont].G))
					{
						b = Data.Bots[cont];
						dir = Dir.Round(Math.Atan2(Xd - b.Xd, b.Yd - Yd) * Dir.NumberOfDirections / 2 / Math.PI + Dir.NumberOfDirections / 2);
						if (dir == 64) dir = 0;
						//var dir1 = Dir.NearbyCellsDirection[n];
						ActivateReceptor2(dir, false, b.Direction);
						break;
					}
				}
			}
		}

		private int Rotate(int dir)
		{
			Direction = dir;
			return 2;
		}


		//https://www.messletters.com/ru/big-text/ banner3
		//////////////////////////////////////////////////////////////////
		//			##     ##  #######  ##     ## ######## 
		//			###   ### ##     ## ##     ## ##       
		//			#### #### ##     ## ##     ## ##       
		//			## ### ## ##     ## ##     ## ######   
		//			##     ## ##     ##  ##   ##  ##       
		//			##     ## ##     ##   ## ##   ##       
		//			##     ##  #######     ###    ######## 
		//////////////////////////////////////////////////////////////////

		#region Move

		private int Step(int dir)
		{
			//Func.CheckWorld2(Index, Num, Xi, Yi);

			// Алгоритм:
			// 1. Узнаем координаты предполагаемого перемещения
			var (nXd, nYd, nXi, nYi, iEqual) = GetCoordinatesByDirectionForMove(dir);

			if (iEqual)
			{
				// ПЕРЕМЕЩЕНИЕ
				Xd = nXd;
				Yd = nYd;
				return 2;
			}


			// Если координаты попадают за экран то вернуть RefContent.Edge
			if (nYi < 0 || nYi >= Data.WorldHeight || nXi < 0 || nXi >= Data.WorldWidth)
			{
				ActivateReceptor5(dir);
				return 2;
			}



			bool move = false;

			if (Data.World[nXi, nYi] == 0) //Free
			{
				lock (_busyWorld2)
				{
					if (Data.World[nXi, nYi] == 0)
					{
						// ПЕРЕМЕЩЕНИЕ A
						Data.World[Xi, Yi] = 0;
						//Thread.MemoryBarrier();
						Data.World[nXi, nYi] = Index;
						move = true;
					}
				}
			}

			if (move)
			{
				// ПЕРЕМЕЩЕНИЕ B

				if (Data.DrawType == DrawType.OnlyChangedCells)
				{
					Func.FixChangeCell(Xi, Yi, null);
					Func.FixChangeCell(nXi, nYi, Color);
				}

				//Log.LogInfo($"bot was moved from {Xi}/{Yi} to {nXi}/{nYi}.");

				Xd = nXd;
				Yd = nYd;
				Xi = nXi;
				Yi = nYi;

				//Func.CheckWorld2(Index, Num, Xi, Yi);


				return 2;
			}
			else
			{
				//Func.CheckWorld2(Index, Num, Xi, Yi);

				var cont = Data.World[nXi, nYi];

				if (cont == 65500)
				{
					ActivateReceptor3(dir);
				}

				if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
				{
					ActivateReceptor2(dir, G.IsRelative(Data.Bots[cont].G), Data.Bots[cont].Direction);
				}

				//Func.CheckWorld2(Index, Num, Xi, Yi);
				return 2;
			}
		}

		private (double newXdouble, double newYdouble, int newXint, int newYint, bool iEqual) GetCoordinatesByDirectionForMove(int dir)
		{
			var (deltaXdouble, deltaYdouble) = Dir.Directions1[dir];

			var newXdouble = Xd + deltaXdouble;
			var newYdouble = Yd + deltaYdouble;

			var newXint = Dir.Round(newXdouble);
			var newYint = Dir.Round(newYdouble);

			var iEqual = newXint == Xi && newYint == Yi;


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
		#endregion

		//////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////

		private void ShiftCodePointer(int shift)
		{
			OldPointer = Pointer;
			Pointer = (Pointer + shift) % Data.GenomLength;
		}

		private (int newXint, int newYint) GetCoordinatesByDirectionOnlyDifferent(int dir)
		{
			var (deltaXdouble, deltaYdouble) = Dir.Directions1[dir];

			var newXint = Dir.Round(Xd + deltaXdouble);
			var newYint = Dir.Round(Yd + deltaYdouble);

			if (newXint == Xi && newYint == Yi)
			{
				// координаты не изменились. надо шагнуть дальше
				(deltaXdouble, deltaYdouble) = Dir.Directions2[dir];

				newXint = Dir.Round(Xd + deltaXdouble);
				newYint = Dir.Round(Yd + deltaYdouble);

				// проверка на изменение координат еще раз. такого не может быть
				if (newXint == Xi && newYint == Yi)
				{
					throw new Exception("if (newXint == _Xi && newYint == _Yi)");
				}

				if (newXint - Xi > 1 || newXint - Xi < -1 || newYint - Yi > 1 || newYint - Yi < -1)
				{
					throw new Exception("if (newXint - _Xi > 1 || newXint - _Xi < -1 || newYint - _Yi > 1 || newYint - _Yi < -1)");
				}
			}

			// Проверка перехода сквозь экран
			if (!Data.LeftRightEdge)
			{
				if (newXint < 0)
				{
					newXint += Data.WorldWidth;
				}

				if (newXint >= Data.WorldWidth)
				{
					newXint -= Data.WorldWidth;
				}
			}

			if (!Data.UpDownEdge)
			{
				if (newYint < 0)
				{
					newYint += Data.WorldHeight;
				}

				if (newYint >= Data.WorldHeight)
				{
					newYint -= Data.WorldHeight;
				}
			}

			return (newXint, newYint);
		}

		//private bool IdentifyRelativity(long cont)
		//{
		//	return Genom.IsRelative(Data.Bots[cont].Genom);
		//}

		#region Direction
		private int GetDirAbsolute()
		{
			return G.GetDirectionFromNextCommand(Pointer, true) % Dir.NumberOfDirections;
		}

		private int GetDirRelative()
		{
			return (Direction + G.GetDirectionFromNextCommand(Pointer, true)) % Dir.NumberOfDirections;
		}

		private int GetDirForward()
		{
			return Direction;
		}

		private int GetDirRelativeWithRandom()
		{
			var rand = ThreadSafeRandom.Next(100);

			var shift = rand switch
			{
				99 => ThreadSafeRandom.Next(256),
				98 => ThreadSafeRandom.Next(11) - 5,
				_ => 0
			};

			if (shift < 0) shift += Dir.NumberOfDirections;
			return (Direction + G.GetDirectionFromNextCommand(Pointer, true) + shift) % Dir.NumberOfDirections;
		}
		#endregion

		#region GetText
		public string GetText1()
		{
			var sb = new StringBuilder();

			sb.AppendLine($"Age: {Age}");
			sb.AppendLine($"Num: {Num}");
			sb.AppendLine($"Index: {Index}");

			sb.AppendLine($"Energy: {Energy}");
			sb.AppendLine($"_dir: {Direction}");


			sb.AppendLine("");
			sb.AppendLine($"Genom {G.PraNum} {(G.Num != 0 ? $"({G.Num})" : "")}Lev{G.Level}");
			sb.AppendLine($"ActGen: {G.Act.Count(g => g > 0)}");

			sb.AppendLine("");
			sb.AppendLine($"Color: R{Color.R} G{Color.G} B{Color.B}");
			sb.AppendLine($"Pra: {G.PraHash.ToString().Substring(0, 8)}");
			sb.AppendLine($"Hash: {G.GenomHash.ToString().Substring(0, 8)}");
			sb.AppendLine($"Parent: {G.ParentHash.ToString().Substring(0, 8)}");
			sb.AppendLine($"Grand: {G.GrandHash.ToString().Substring(0, 8)}");

			return sb.ToString();
		}

		public string GetText2(int delta)
		{
			var sb = new StringBuilder();

			//sb.AppendLine($"23r,24a - rotate; 26r,27a - step");
			//sb.AppendLine($"28r,29a - eat; 30r,31a - look");

			sb.AppendLine($"OldPointer: {OldPointer}");
			sb.AppendLine($"Pointer: {Pointer}");

			if (hist.historyPointerY >= 0)
			{
				var (hist, histPtrCnt) = this.hist.GetLastStepPtrs(delta);

				sb.AppendLine($"jumps cnt: {histPtrCnt - 1}");
				sb.AppendLine($"jumps: {string.Join(", ", hist.Take(histPtrCnt))}");

				for (var i = 0; i < histPtrCnt; i++)
				{
					var cmdTxt = Cmd.CmdName(G.CodeCommon[hist[i]]);

					var dirStr = Dir.GetDirectionStringFromCode(G.GetDirectionFromNextCommand(hist[i], false));
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



		//private async Task<bool> SetBusy()
		//{
		//	try
		//	{
		//		await _semaphore.WaitAsync();
		//	}
		//	catch
		//	{
		//		ReleaseBusy();
		//		return false;
		//	}
		//	return true;
		//}

		//private void ReleaseBusy()
		//{
		//	try
		//	{
		//		_semaphore.Release();
		//	}
		//	catch { }
		//}

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
