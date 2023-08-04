using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.GameLogic;

namespace WindowsFormsApp1.Static
{
	public static class Func
	{
		private static readonly object _busyWorld = new object();
		private static readonly object _busyChWorld = new object();
		public static long T123;


		// Список измененных ячеек для последующей отрисовки
		//public long[,] ChWorld;				- по координатам можно определить перерисовывать ли эту ячейку, там записам индекс массива ChangedCell
		//public ChangedCell[] ChangedCells;	- массив перерисовки, в нем перечислены координаты перерисуеваемых ячеек и их цвета
		//public long NumberOfChangedCells;		- количество изменившихся ячеек на экране колторые надо перерисовать в следующий раз
		public static void FixChangeCell(int x, int y, /*int index,*/ Color? color)
		{
			// возможно ли в паралелли одновременное изменение одной клетки? а то приходится локи городить из-за этой возможности
			bool first = false;
			long num;

			if (Data.ChWorld[x, y] == 0)
			{
				lock (_busyChWorld)
				{
					if (Data.ChWorld[x, y] == 0)
					{
						Data.ChWorld[x, y] = Interlocked.Increment(ref Data.NumberOfChangedCells); // сюда записываем +1 чтобы 0 не записывать
						first = true;
					}
				}
			}

			num = Data.ChWorld[x, y];

			if (Data.ChangedCells[num] == null)
			{
				Data.ChangedCells[num] = new ChangedCell
				{
					X = x,
					Y = y,
					//Index = index,
					Color = color
				};
			}
			else
			{
				if (first)
				{
					Data.ChangedCells[num].X = x;
					Data.ChangedCells[num].Y = y;
					//Data.ChangedCells[num].Index = index;
					Data.ChangedCells[num].Color = color;
				}
				else
				{
					//var x1 = Data.ChangedCells[num].X;
					//var y1 = Data.ChangedCells[num].Y;
					//if (Data.ChangedCells[num].X != x || Data.ChangedCells[num].Y != y) throw new Exception("fd546gdf");
					//var rk = Data.ChangedCells[num];
					//var st = Data.CurrentStep;
					//if (st > 0)
					//{
					//    var bl = Data.Bots[index];
					//}

					Data.ChangedCells[num].X = x;
					Data.ChangedCells[num].Y = y;
					//Data.ChangedCells[num].Index = index;
					Data.ChangedCells[num].Color = color;
				}
			}
		}


		public static void DeathBot(int index)
		{
			Interlocked.Increment(ref T123);
			var dBot = Data.BotDeath[index];

			//Func.CheckWorld2();
			if (dBot.Energy > 0)
			{
				dBot.InsertedToDeathList = false;
				Interlocked.Increment(ref Data.Check_QtyFailedDeath);
				//Data.Wlog.LogInfo($"DeathBot {index}-{dBot.Index} Failed Energy > 0");
				return; //бот успел поесть и выжил
			}

			lock (_busyWorld)
			{
				Data.World[dBot.Xi, dBot.Yi] = 0;
			}

			if (Data.DrawType == DrawType.OnlyChangedCells)
			{
				FixChangeCell(dBot.Xi, dBot.Yi, null); // при следующей отрисовке бот стерется с экрана
			}

			dBot.G.DecBot(dBot.Age);

			// Разобраться надо ли обменивать этого бота
			bool replace = false;
			long lastBotIndex = -1;
			if (dBot.Index <= Data.IndexEnclusiveBeforeReplacesBots)  // обменивать надо
			{
				// Найти живого бота для обмена в конце списка (с индексом > Data.IndexEnclusiveBeforeReplacesBots)
				do
				{
					lastBotIndex = Interlocked.Decrement(ref Data.IndexOfLastBotPlusOne);
				}
				while (!Data.Bots[lastBotIndex].Alive);

				//!!!!!!!Переносить только если lastBotIndex > Func.IndexEnclusiveBeforeReplacesBots
				if (lastBotIndex > Data.IndexEnclusiveBeforeReplacesBots)
				{
					replace = true;
				}
				else
				{
					//if (Data.IndexOfLastBotDeathArrayUsedForReproduction < Data.QtyAllBotDeathMinusOne) // еще есть в запасе умирающие боты
					//{
					//	//Data.Wlog.LogInfo($"Data.CurrentNumberOfBots: {Data.CurrentNumberOfBots}");
					//	//Data.Wlog.LogInfo($"Data.LastIndexOfBotDeathArrayUsedForReproduction: {Data.IndexOfLastBotDeathArrayUsedForReproduction}");
					//	//Data.Wlog.LogInfo("Data.CurrentNumberOfBots - Data.NumberOfBotDeathFactCnt + Data.NumberOfBotDeathFactUsedForReproductionCnt: " +
					//	//                  $"{Data.CurrentNumberOfBots - Data.QtyFactBotDeath + Data.QtyFactBotDeathUsedForReproduction}");

					//	Data.IndexEnclusiveBeforeReplacesBots = Data.CurrentNumberOfBots - Data.QtyFactBotDeath + Data.QtyFactBotDeathUsedForReproduction;
					//	Data.QtyRemovedBotsOnStep = 0;
					//	Data.IndexOfLastBotPlusOne = Data.CurrentNumberOfBots + 1;

					//if (Data.Parallel)
					//{
					//	Parallel.For((int)Data.IndexOfLastBotDeathArrayUsedForReproduction + 1, (int)Data.QtyAllBotDeathMinusOne + 1, Func.DeathBot);
					//}
					//else
					//{
					//	for (var i = (int)Data.IndexOfLastBotDeathArrayUsedForReproduction + 1; i < (int)Data.QtyAllBotDeathMinusOne + 1; i++)
					//	{
					//		Func.DeathBot(i);
					//	}
					//}

					var r1 = Data.QtyFactBotDeath - Data.QtyFactBotDeathUsedForReproduction; // сколько реальных смертей осталось после размножения
					var r2 = Data.QtyAllBotDeathMinusOne - Data.IndexOfLastBotDeathArrayUsedForReproduction; // сколько элементов массива BotDeath осталось после размножения
					var r3 = Data.CurrentStep;
					var r4 = Data.Check_QtyFailedReproduction;
					var r5 = Data.Check_QtyFailedDeath;
					var r6 = Data.QtyFactBotDeath;
					var r7 = Data.QtyAllBotDeathMinusOne;
					var r8 = Data.QtyAllBotDeathMinusOne + 1 - Data.QtyFactBotDeath; // всего неуспешных смертей
					var r9 = Data.IndexOfLastBotDeathArrayUsedForReproduction + 1 - Data.QtyFactBotDeathUsedForReproduction; // всего неуспешных смертей при размножении 
																															 // должно быть равно Check_QtyFailedDeath
					var r10 = Data.CurrentNumberOfBots;
					var r11 = Data.IndexOfLastBotDeathArrayUsedForReproduction;
					var r12 = Data.IndexEnclusiveBeforeReplacesBots;
					var r13 = Data.IndexOfLastBotPlusOne;
					var r14 = Data.QtyFactBotDeathUsedForReproduction;
					var r15 = Data.QtyRemovedBotsOnStep;
					var r16 = Data.CurrentNumberOfBots - Data.QtyFactBotDeath + Data.QtyFactBotDeathUsedForReproduction;

					if (r9 != Data.Check_QtyFailedDeath)
					{

					}

					if (r1 != r2 - (r8 - r9))
					{

					}

                    var te = 0;
                    var cnt2 = 0;
                    for (long i = 1; i <= Data.CurrentNumberOfBots; i++)
                    {
                        if (Data.Bots[i] == null) throw new Exception("rtfghrsfd45tssaddfsdfhrt");

                        if (!Data.Bots[i].Alive)
                        {
                            cnt2++;
                        }

                        if (Data.Bots[i].InsertedToReproductionList) throw new Exception("fdgdfg2");

                        te += Data.Bots[i].Energy;
                    }

					//CHECK1();
					//CHECK2();
					//CHECK3();
					//CHECK4();


					throw new Exception("странно");
					//Data.IndexEnclusiveBeforeReplacesBots = Data.CurrentNumberOfBots - Data.QtyFactBotDeath + Data.QtyFactBotDeathUsedForReproduction;
				}
			}

			if (replace)
			{
				// Обмен ботов lastBot и dBot
				var lastBot = Data.Bots[lastBotIndex];
				var dBotIndex = dBot.Index;
				Data.Bots[dBotIndex] = lastBot;
				Data.Bots[lastBotIndex] = dBot;
				lastBot.Index = dBotIndex;

				lock (_busyWorld)
				{
					Data.World[lastBot.Xi, lastBot.Yi] = dBotIndex;
				}
				// конец переноса


				//lastBot.Log.LogInfo($"Index changed from {lastBotIndex}/{Data.CurrentNumberOfBots} to {dBotIndex}");
				//Data.Bots[lastBotIndex] = null;  // todo надо ли это? может лучше оставить и потом просто Update делать
				//Data.Wlog.LogInfo($"DeathBot {index}-{dBot.Index} Replace from {dBotIndex} to {lastBotIndex}");
			}
			else
			{
				//Data.Bots[dBot.Index] = null;   // todo надо ли это? может лучше оставить и потом просто Update делать
				//Data.Wlog.LogInfo($"DeathBot {index}-{dBot.Index}");
			}


			Interlocked.Increment(ref Data.QtyRemovedBotsOnStep);
		}





		//https://symbl.cc/ru/tools/text-to-symbols/
		//      ████─█──█─█───██─██────█───█─█──█─███─█──█────████─████─███─████─███─███─█──█─████────████──████─███─███
		//      █──█─██─█─█────███─────█───█─█──█─█───██─█────█──█─█──█─█───█──█──█───█──██─█─█───────█──██─█──█──█──█──
		//      █──█─█─██─█─────█──────█─█─█─████─███─█─██────█────████─███─████──█───█──█─██─█─██────████──█──█──█──███
		//      █──█─█──█─█─────█──────█████─█──█─█───█──█────█──█─█─█──█───█──█──█───█──█──█─█──█────█──██─█──█──█────█
		//      ████─█──█─███───█───────█─█──█──█─███─█──█────████─█─█──███─█──█──█──███─█──█─████────████──████──█──███



		public static void ReproductionBot(int index)
		{
			var reproductedBot = Data.BotReproduction[index];
			reproductedBot.InsertedToReproductionList = false;

			if (!reproductedBot.CanReproduct())
			{
				Interlocked.Increment(ref Data.Check_QtyFailedReproduction);
				//Data.Wlog.LogInfo($"ReproductionBot {index}-{reproductedBot.Index} Failed 1  LIOBDAUFR:{Data.IndexOfLastBotDeathArrayUsedForReproduction}");
				return;
			}

			if (!TryOccupyRandomFreeCellNearby(reproductedBot.Xi, reproductedBot.Yi, reproductedBot.Index, out var x,
					out var y)) // Вставляем в World[x,y] индекс размножающегося бота-родителя !!!
			{
				Interlocked.Increment(ref Data.Check_QtyFailedReproduction);
				reproductedBot.HoldReproduction();

				// Передать энергию окружающим ботам
				var n = ThreadSafeRandom.Next(8);
				int nXi, nYi;
				long cont;
				int i = 0;
				var ent = (reproductedBot.Energy - Data.ReproductionBotEnergy) / 4;

				if (ent > 0)
				{
					do
					{
						(nXi, nYi) = GetCoordinatesByDelta(reproductedBot.Xi, reproductedBot.Yi, n);

						if (nYi >= 0 && nYi < Data.WorldHeight && nXi >= 0 && nXi < Data.WorldWidth)
						{
							cont = Data.World[nXi, nYi];

							if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
							{
								var targetBot = Data.Bots[cont];

								if (reproductedBot.Energy > targetBot.Energy && targetBot.Energy > 0 && !targetBot.InsertedToDeathList)
								{
									if (ent <= 0)
									{
										throw new Exception("if (ent <= 0)");
									}
									var transferedEnergy = reproductedBot.EnergyChange(-ent);
									targetBot.EnergyChange(transferedEnergy);
									if (transferedEnergy < 0) throw new Exception("dfgdfg");
								}
							}
						}
						if (++n >= 8) n -= 8;
						i++;
					}
					while (reproductedBot.CanReproduct() && i <= 20);
				}

				//Data.Wlog.LogInfo($"ReproductionBot {index}-{reproductedBot.Index} Failed 2  LIOBDAUFR:{Data.IndexOfLastBotDeathArrayUsedForReproduction}");
				return;
			}

			//reproductedBot.Log.LogInfo($"{reproductedBot.Index} will reproduct");

			var genom = Mutation() ? Genom.CreateGenom(reproductedBot.G) : reproductedBot.G;



			// Узнать можно ли создать бота на основе умирающего бота
			// Получить номер умирающего бота из BotDeath
			// Data.NumberOfBotDeath - количество умирающих ботов - 1
			// Data.NumberOfBotDeathForReproduction = -1 - первоначально
			// Data.BotDeath - массив умирающих ботов

			bool update = false;
			int ind = -1;
			int en;
			if (Data.IndexOfLastBotDeathArrayUsedForReproduction < Data.QtyAllBotDeathMinusOne) // еще есть в запасе умирающие боты
			{
				do
				{
					ind = Interlocked.Increment(ref Data.IndexOfLastBotDeathArrayUsedForReproduction);  // получаем индекс умирающего бота

					if (ind <= Data.QtyAllBotDeathMinusOne) // ind в пределах списка умирающих ботов?
					{
						en = Data.BotDeath[ind].Energy;
						//если энергия бота >0 то здесь его надо убрать из массива
						if (en > 0)
						{
							//Data.Wlog.LogInfo($"ReproductionBot {index}-{reproductedBot.Index} BotDeath Skipped (en>0)  LIOBDAUFR:{Data.IndexOfLastBotDeathArrayUsedForReproduction}");
							Data.BotDeath[ind].InsertedToDeathList = false;
							Interlocked.Increment(ref Data.Check_QtyFailedDeath);
						}

						if (en == 0) update = true;
					}
					else
					{
						en = 0;
					}
				}
				while (ind < Data.QtyAllBotDeathMinusOne && en > 0);  // можно провернуть еще раз если этот бот как оказалось не умирает и есть еще умирающие боты в запасе
			}


			if (update)
			{
				UpdateBot(ind, x, y, Data.InitialBotEnergy, genom);
				Interlocked.Increment(ref Data.QtyFactBotDeathUsedForReproduction);
				//Data.Wlog.LogInfo($"ReproductionBot {index}-{reproductedBot.Index} UpdateBot  LIOBDAUFR:{Data.IndexOfLastBotDeathArrayUsedForReproduction}");
				//reproductedBot.Log.LogInfo($"ReproductionBot {index}-{reproductedBot.Index} UpdateBot {Data.BotDeath[ind].Index} LIOBDAUFR:{Data.IndexOfLastBotDeathArrayUsedForReproduction}");
			}
			else
			{
				var newBotIndex = Interlocked.Increment(ref Data.CurrentNumberOfBots);
				CreateNewBot(x, y, newBotIndex, Data.InitialBotEnergy, genom);
				//Data.Wlog.LogInfo($"ReproductionBot {index}-{reproductedBot.Index} CreateNewBot 1/2  LIOBDAUFR:{Data.IndexOfLastBotDeathArrayUsedForReproduction}");
				//reproductedBot.Log.LogInfo($"ReproductionBot {index}-{reproductedBot.Index} CreateNewBot 1/2  LIOBDAUFR:{Data.IndexOfLastBotDeathArrayUsedForReproduction}");
			}

			reproductedBot.EnergyChange(-Data.InitialBotEnergy);

			Interlocked.Increment(ref Data.TotalQtyBotReproduction);
		}

		public static void UpdateBot(int ind, int x, int y, int en, Genom genom)
		{
			var dir = GetRandomDirection();
			var pointer = 0;
			var botNumber = Interlocked.Increment(ref Data.MaxBotNumber);
			genom.IncBot();


			var updBot = Data.BotDeath[ind];
			var index = Data.BotDeath[ind].Index;
			updBot.G.DecBot(updBot.Age);
			var xiold = updBot.Xi;
			var yiold = updBot.Yi;
			//Interlocked.Increment(ref Removedbots1);
			//Data.BotDeath[ind] = null;
			//Data.NumberOfBotDeath = -1; - не делаем так как не уменьшаем массив а прореживаем (или сокращаем снизу)

			// Изменение бота
			updBot.Xi = x;
			updBot.Xd = x;
			updBot.Yi = y;
			updBot.Yd = y;
			updBot.Direction = dir;
			updBot.Num = botNumber;
			updBot.G = genom;
			updBot.Pointer = pointer;
			updBot.OldPointer = pointer;
			updBot.Age = 0;
			updBot.Alive = true;
			updBot.Hist = new CodeHistory();
            updBot.InsertedToReproductionList = false;
            updBot.EnergySet(en);

			updBot.RefreshColor();


			lock (_busyWorld)
			{
				Data.World[xiold, yiold] = 0;
				Data.World[x, y] = index;
			}

			if (Data.DrawType == DrawType.OnlyChangedCells)
			{
				FixChangeCell(xiold, yiold, null); // при следующей отрисовке бот стерется с экрана
				FixChangeCell(x, y, updBot.Color);
			}

            updBot.InsertedToDeathList = false;
		}

		public static void CreateNewBot(int x, int y, long botIndex, int en, Genom genom)
		{
			var dir = GetRandomDirection();
			var pointer = 0;
			var botNumber = Interlocked.Increment(ref Data.MaxBotNumber);
			genom.IncBot();


			Bot1 bot;
			if (Data.Bots[botIndex] == null)
			{
				bot = new Bot1(x, y, dir, botNumber, botIndex, en, genom, pointer);

				Data.Bots[botIndex] = bot;
			}
			else
			{
				bot = Data.Bots[botIndex];
				bot.Index = botIndex;
				bot.Xi = x;
				bot.Xd = x;
				bot.Yi = y;
				bot.Yd = y;
				bot.Direction = dir;
				bot.Num = botNumber;
				bot.EnergySet(en);
				bot.G = genom;
				bot.Pointer = pointer;
				bot.OldPointer = pointer;
				bot.Age = 0;
				bot.InsertedToDeathList = false;
				bot.InsertedToReproductionList = false;
				bot.Alive = true;
				bot.Hist = new CodeHistory();
			}

			bot.RefreshColor();


			if (Data.DrawType == DrawType.OnlyChangedCells)
			{
				FixChangeCell(x, y, bot.Color);
			}

            lock (_busyWorld)
            {
                Data.World[x, y] = botIndex;
            }
		}



		private static bool TryOccupyRandomFreeCellNearby(int Xi, int Yi, long reprBotIndex, out int nXi, out int nYi)
		{
			var n = ThreadSafeRandom.Next(8);
			var i = 0;
			bool result = false;

			do
			{
				(nXi, nYi) = GetCoordinatesByDelta(Xi, Yi, n);

				if (nYi >= 0 && nYi < Data.WorldHeight && nXi >= 0 && nXi < Data.WorldWidth)
				{
					if (Data.World[nXi, nYi] == 0)
					{
						lock (_busyWorld)
						{
							if (Data.World[nXi, nYi] == 0)
							{
								Data.World[nXi, nYi] = reprBotIndex;
								result = true;
								break;
							}
						}
					}
				}

				i++;
				if (++n >= 8) n -= 8;
			}
			while (i <= 8);

			return result;
		}

		private static (int nXi, int nYi) GetCoordinatesByDelta(int Xi, int Yi, int nDelta)
		{
			var (nXid, nYid) = Dir.NearbyCells[nDelta];


			var nXi = Xi + nXid;
			var nYi = Yi + nYid;

			// Проверка перехода сквозь экран
			if (!Data.LeftRightEdge)
			{
				if (nXi < 0)
				{
					nXi += Data.WorldWidth;
				}

				if (nXi >= Data.WorldWidth)
				{
					nXi -= Data.WorldWidth;
				}
			}

			if (!Data.UpDownEdge)
			{
				if (nYi < 0)
				{
					nYi += Data.WorldHeight;
				}

				if (nYi >= Data.WorldHeight)
				{
					nYi -= Data.WorldHeight;
				}
			}

			return (nXi, nYi);
		}

		public static (byte[], byte[], (byte, byte)[]) GetRandomAttackShield()
		{
			var fail = false;
			var shield = new byte[Data.AttackShieldTypeCountMax];
			var attack = new byte[Data.AttackShieldTypeCountMax];
			var attackTypes = new List<(byte, byte)>();

			for (var i = 0; i < Data.AttackShieldSum; i++)
			{
				do
				{
					var type = (byte)ThreadSafeRandom.Next(Data.AttackShieldTypeCount);

					if (ThreadSafeRandom.NextDouble() > .5)
					{
						if (shield[type] >= Data.ShieldMax)
						{
							fail = true;
						}
						else
						{
							shield[type]++;
						}
					}
					else
					{
						if (attack[type] >= Data.AttackMax)
						{
							fail = true;
						}
						else
						{
							attack[type]++;
						}
					}
				}
				while (fail);
			}

			for (var i = 0; i < Data.AttackShieldTypeCount; i++)
			{
				if (attack[i] > 0)
				{
					attackTypes.Add(((byte)i, attack[i]));
				}
			}

			return (shield, attack, attackTypes.ToArray());
		}

		public static int GetRandomDirection()
		{
			return ThreadSafeRandom.Next(Dir.NumberOfDirections);
		}

		public static byte GetRandomBotCode()
		{
			return (byte)ThreadSafeRandom.Next(Data.MaxCode + 1);
		}

		public static Color GetRandomColor()
		{
			return Color.FromArgb(ThreadSafeRandom.Next(256), ThreadSafeRandom.Next(256), ThreadSafeRandom.Next(256));
		}

		public static int GetRandomBotCodeIndex()
		{
			return ThreadSafeRandom.Next(Data.GenomLength);
		}

		public static int GetRandomNext(int max)
		{
			return ThreadSafeRandom.Next(max);
		}

		public static bool Mutation()
		{
			return Data.Mutation && ThreadSafeRandom.NextDouble() * 100 < Data.MutationProbabilityPercent;
		}

		//////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////


		public static bool TryGetRandomFreeCell(out int x, out int y)
		{
			x = 0;
			y = 0;
			var i = 0;

			do
			{
				x = ThreadSafeRandom.Next(0, Data.WorldWidth);
				y = ThreadSafeRandom.Next(0, Data.WorldHeight);
			}
			while (Data.World[x, y] != 0 && ++i < 1000);

			if (i >= 1000)
			{
				return false;
			}

			return true;
		}


		public static (int, int) GetRandomSpeed()
		{
			//do
			//{
			//	_vx = rnd.Next(-1, 2);
			//	_vy = rnd.Next(-1, 2);
			//}
			//while (_vx == 0 && _vy == 0);

			if (ThreadSafeRandom.Next(100) > 97)
			{
				return (ThreadSafeRandom.Next(-1, 2), ThreadSafeRandom.Next(-1, 2));
			}
			return (0, 0);
		}

		public static void CHECK1()
		{
			if (Data.QtyAllBotDeathMinusOne != -1)
			{
				throw new Exception("fdgdfgds34fg2");
			}

			if (Data.QtyFactBotDeath != 0)
			{
				throw new Exception("fdgdf45646gds34fg2");
			}

			if (Data.IndexOfLastBotReproduction != -1)
			{
				throw new Exception("fdgdfgd3df4fg2");
			}
		}

		public static void CHECK2()
		{

			for (var i = 0; i < (int)Data.QtyAllBotDeathMinusOne + 1; i++)
			{
				var ind = Array.FindIndex(Data.Bots, x => x != null && x.Index == Data.BotDeath[i].Index && x.Num == Data.BotDeath[i].Num);

				if (Data.Bots[ind].Xd != Data.BotDeath[i].Xd || Data.Bots[ind].Yd != Data.BotDeath[i].Yd)
				{
					throw new Exception("fgfdgdfgf");
				}

				if (Data.World[Data.BotDeath[i].Xi, Data.BotDeath[i].Yi] != Data.BotDeath[i].Index)
				{
					throw new Exception("fdgdfgdfg");
				}

				if (!Data.BotDeath[i].InsertedToDeathList && Data.BotDeath[i].Energy == 0)
				{
					throw new Exception("fdgdfgdfg2");
				}

				if (Data.BotDeath[i].Index > Data.CurrentNumberOfBots)
				{
					throw new Exception("if (_ind > Data.CurrentBotsNumber)");
				}


				for (var j = 0; j < (int)Data.QtyAllBotDeathMinusOne + 1; j++)
				{
					if (i != j)
					{
						if (Data.BotDeath[i].Index == Data.BotDeath[j].Index)
						{
							throw new Exception("if (_insdfdfsdfsdd > Data.CurrentBotsNumber)");
						}
					}
				}
			}

			for (var i = 0; i < (int)Data.IndexOfLastBotReproduction + 1; i++)
			{
				var ind = Array.FindIndex(Data.Bots, x => x != null && x.Index == Data.BotReproduction[i].Index && x.Num == Data.BotReproduction[i].Num);

				if (Data.Bots[ind].Xd != Data.BotReproduction[i].Xd || Data.Bots[ind].Yd != Data.BotReproduction[i].Yd)
				{
					throw new Exception("fgfdg34dfgf");
				}

				if (Data.World[Data.BotReproduction[i].Xi, Data.BotReproduction[i].Yi] != Data.BotReproduction[i].Index)
				{
					throw new Exception("fdgdfgd34fg");
				}

				if (!Data.BotReproduction[i].InsertedToReproductionList)
				{
					throw new Exception("fdgdfgd34fg2");
				}
			}

			int cnt0 = 0;
			int cnt1 = 0;
			var cnt2 = 0;
			for (long botNumber = 1; botNumber <= Data.CurrentNumberOfBots; botNumber++)
			{
				if (Data.Bots[botNumber] == null) throw new Exception("rtfghrsfd45tsdfsdfhrt");
				if (Data.Bots[botNumber].Energy < -1) throw new Exception("rtfghrsfd45thrt");

				if (Data.Bots[botNumber].InsertedToDeathList)
				{
					cnt0++;
				}

				if (Data.Bots[botNumber].InsertedToReproductionList)
				{
					cnt1++;
				}

				if (!Data.Bots[botNumber].Alive)
				{
					cnt2++;
				}
			}
			if (cnt0 != Data.QtyAllBotDeathMinusOne + 1) throw new Exception("fdgergg");
			if (cnt1 != Data.IndexOfLastBotReproduction + 1) throw new Exception("fdgergsdsdg");
			if (cnt2 != Data.QtyFactBotDeath) throw new Exception("fdgerdsfsd34gg");
		}

		public static void CHECK3()
		{
			if (Data.QtyFactBotDeath < 0) throw new Exception("rtfg65765");

			for (long i = 1; i <= Data.CurrentNumberOfBots; i++)
			{
				if (Data.Bots[i] == null) throw new Exception("rtfghrsfd45tssaddfsdfhrt");
				if (Data.Bots[i].Index != i)
				{
					throw new Exception("fdgdfgdsdfdf34f435345g");
				}

				if (Data.World[Data.Bots[i].Xi, Data.Bots[i].Yi] != Data.Bots[i].Index)
				{
					throw new Exception("fdgdfgd34f435345g");
				}

				if (Data.Bots[i].InsertedToReproductionList) throw new Exception("fdgdfg23");
			}

			long cont;
			for (var x = 0; x < Data.WorldWidth; x++)
			{
				for (var y = 0; y < Data.WorldHeight; y++)
				{
					cont = Data.World[x, y];
					if (cont < 0) throw new Exception("fgfrgreg45645sdfds7");
					if (cont > 0 && (cont < 65000 || cont > 65504))
					{
						if (cont > Data.CurrentNumberOfBots) throw new Exception("fgfrgreg456457");
					}
				}
			}
		}

		public static void CHECK4()
		{
			//var l234324234 = Data.Wlog.GetLogString();

			if (Data.IndexOfLastBotDeathArrayUsedForReproduction < Data.QtyAllBotDeathMinusOne)
			{
				if (Data.QtyFactBotDeathUsedForReproduction + Data.QtyRemovedBotsOnStep != Data.QtyFactBotDeath) throw new Exception("dfgdfgf");

				if (Data.IndexEnclusiveBeforeReplacesBots != 0)
				{
					if (Data.CurrentNumberOfBots != Data.IndexEnclusiveBeforeReplacesBots) throw new Exception("grtgrtrtg");
				}
			}

			//if (Data.QtyFactBotDeathUsedForReproduction + Data.Check_QtyFailedReproduction !=Data.IndexOfLastBotReproduction +1 ) throw new Exception("dfgdfgf5435");

			var te = 0;
			var cnt2 = 0;
			for (long i = 1; i <= Data.CurrentNumberOfBots; i++)
			{
				if (Data.Bots[i] == null) throw new Exception("rtfghrsfd45tssaddfsdfhrt");
				if (Data.Bots[i].Index != i)
				{
					throw new Exception("fdgdfgdsdfdf34f435345g");
				}

				if (Data.World[Data.Bots[i].Xi, Data.Bots[i].Yi] != Data.Bots[i].Index) 
				{
					throw new Exception("fdgdfgd34f435345g");
				}

				if (!Data.Bots[i].Alive)
				{
					cnt2++;
				}

				if (Data.Bots[i].InsertedToDeathList) throw new Exception("fdgdfgd34f43545345g");
				if (Data.Bots[i].InsertedToReproductionList) throw new Exception("fdgdfg2");

				te += Data.Bots[i].Energy;
			}

			if (cnt2 != 0) throw new Exception("fdge34sd34gg");

			//if(te != Data.TotalEnergy) throw new Exception("fdgdfgsdfd34f435345g");

			var cnt = 0;
			var dct = new Dictionary<long, int>();
			long cont;

			for (var x = 0; x < Data.WorldWidth; x++)
			{
				for (var y = 0; y < Data.WorldHeight; y++)
				{
					cont = Data.World[x, y];
					if (cont < 0) throw new Exception("fgfrgreg45645sdfds7");
					if (cont > 0 && (cont < 65000 || cont > 65504))
					{
						if (cont > Data.CurrentNumberOfBots)
						{
							//var st = Data.CurrentStep;
							//var t = Data.BotDeath;
							//var log222 = Data.Wlog.GetLogString();
							throw new Exception("fgfrgreg456457");
						}
						cnt++;

						if (dct.ContainsKey(cont))
						{
							dct[cont]++;
						}
						else
						{
							dct.Add(cont, 1);
						}
					}
				}
			}

			if (cnt != Data.CurrentNumberOfBots)
			{
				throw new Exception("fdfdgfdgd");
			}
			if (dct.Any(d => d.Value > 1)) throw new Exception("fdfdgf654dgd");
		}

		public static void CHECK5()
		{
			//for (long i = 1; i <= Data.CurrentNumberOfBots; i++)
			//{
			//    var bot = Data.Bots[i];
			//    var x = bot.Xi;
			//    var y = bot.Yi;
			//    var color = _DRAWER.GetPixel(x, y);
			//    var lllll = bot.Log.GetLog();
			//    var lllll2 = Data.ClWorld[x, y].GetLog();


			//    if (color != bot.Color)
			//    {
			//        //throw new Exception("fger675");
			//    }
			//}

			var st = Data.CurrentStep;
			if (st != 0)
			{
				var cnt0 = 0;
				for (var x = 0; x < Data.WorldWidth; x++)
				{
					for (var y = 0; y < Data.WorldHeight; y++)
					{
						var cont = Data.ChWorld[x, y];
						if (cont != 0)
						{
							cnt0++;
							throw new Exception("fgs7565667657");
						}
					}
				}

				if (cnt0 != 0)
				{
					throw new Exception("fgs75656676574");
				}
			}
		}

		public static (int, Dictionary<long, int>) GetAllBotsEnergy()
		{
			var te = 0;
			var dct = new Dictionary<long, int>();
			int en;

			for (long botIndex = 1; botIndex <= Data.CurrentNumberOfBots; botIndex++)
			{
				en = Data.Bots[botIndex].Energy;
				te += en;
				dct.Add(botIndex, en);
			}

			return (te, dct);
		}


		public static void CheckBotsEnergy(Dictionary<long, int> dct, int te1)
		{
			var te2 = 0;
			int en;
			var dct2 = new Dictionary<long, (int, int)>();

			for (long botIndex = 1; botIndex <= Data.CurrentNumberOfBots; botIndex++)
			{
				en = Data.Bots[botIndex].Energy;
				te2 += en;
				if (dct.ContainsKey(botIndex))
				{
					if (dct[botIndex] != en)
					{
						dct2.Add(botIndex, (dct[botIndex], en));
					}
				}
				else
				{
					dct2.Add(botIndex, (0, en));
				}
			}

			if (te1 != te2 && dct2.Count > 0)
			{
				var bts = Data.Bots;
				var st = Data.CurrentStep;
				var bc2 = Data.CurrentNumberOfBots;
				var indttt = dct2.First().Key;
				var bttt = Data.Bots[indttt];
				//var log = bttt.Log.GetLog();
			}
		}
	}
}

//              ███─███─████─███─█───█
//              ─█──█───█──█──█──█───█
//              ─█──███─█─────█──███─█
//              ─█──█───█──█──█──█─█─█
//              ─█──███─████──█──███─█


// НАЙТИ В WORLD КООРДИНАТЫ УМИРАЮЩЕГО БОТА С МАКСИМАЛЬНЫМ ИНДЕКСОМ И НАЙТИ ЕГО РЕАЛЬНЫЙ ИНДЕКС В BOTS
//for (var x = 0; x < Data.WorldWidth; x++)
//{
//	for (var y = 0; y < Data.WorldHeight; y++)
//	{
//		var cont = Data.World[x, y];
//		if (cont == maxworlddeathindex)
//		{
//			var ls = new List<int>();
//			for (var i = 0; i < (int)Data.NumberOfBotDeath + 1; i++)
//			{
//				var ind = Array.FindIndex(Data.Bots, x => x != null && x.Index == Data.BotDeath[i].Index && x.Num == Data.BotDeath[i].Num);
//				ls.Add(ind);
//			}
//		}
//	}
//}


// НАЙТИ ЕСТЬ ЛИ В МАССИВЕ WORLD ИНДЕКС БОЛЬШИЙ ЧЕМ Data.CurrentNumberOfBots
//public static bool CheckWorld()
//{
//	var res = true;

//	for (var x = 0; x < Data.WorldWidth; x++)
//	{
//		for (var y = 0; y < Data.WorldHeight; y++)
//		{
//			var cont = Data.World[x, y];
//			var step = Data.CurrentStep;
//			if (cont > Data.CurrentNumberOfBots && Data.World[x, y] != 65500)
//			{
//				res = false;
//			}
//		}
//	}

//	return res;
//}


// НАЙТИ СКОЛЬКО РАЗ ВСТРЕЧАЕКТСЯ ИНДЕКС cont В МАССИВЕ WORLD И КООРДИНАТЫ
//public static (int Cnt, List<(int, int)> Lst) CheckWorld(long cont)
//{
//	var cnt = 0;
//	var lst = new List<(int, int)>();

//	for (var x = 0; x < Data.WorldWidth; x++)
//	{
//		for (var y = 0; y < Data.WorldHeight; y++)
//		{
//			var step = Data.CurrentStep;
//			if (cont == Data.World[x, y])
//			{
//				cnt++;
//				lst.Add((x, y));
//			}
//		}
//	}

//	return (cnt, lst);
//}

// ПРОВЕРИТЬ ЧТО У ВСЕХ БОТОВ Index СООТВЕТСТВУЕТ РЕАЛЬНОМУ ИНДЕКСУ В МАССИВЕ Data.Bots
//private static void CheckIndex()
//{
//	for (long i = 1; i <= Data.CurrentNumberOfBots; i++)
//	{
//		if (Data.Bots[i].Index != i)
//		{ 
//		}
//	}
//}


//public class Asd
//{
//	public int Cnt;
//	public List<long> Nums;
//	public List<(int X, int Y)> Lst;
//}


// ПРОЙТИ ПО ВСЕМУ МАССИВУ WORLD НАЙТИ ВСЕ ДУБЛИКАТЫ С КООРДИНАТАМИ И NUMS И ВЫДАТЬ ПОДРОБНУЮ ИНФОРМАЦИЮ ПО ДУБЛИКАТАМ
//public static bool CheckWorld2(long botindex, long botnum, int botx, int boty)
//{
//	var dct = new Dictionary<long, Asd>();

//	for (var x = 0; x < Data.WorldWidth; x++)
//	{
//		for (var y = 0; y < Data.WorldHeight; y++)
//		{
//			var cont = Data.World[x, y];

//			if (cont > 0 && (cont < 65000 || cont > 65504))
//			{
//				if (dct.ContainsKey(cont))
//				{
//					// а осталась ли первая точка? проверить. может просто произошло перемещение
//					if (dct[cont].Cnt == 1 && Data.World[dct[cont].Lst[0].X, dct[cont].Lst[0].Y] != cont)
//					{
//						dct[cont].Lst[0] = (x, y);
//						dct[cont].Nums[0] = Data.Bots[cont].Num;
//					}
//					else
//					{
//						dct[cont].Cnt++;
//						dct[cont].Lst.Add((x, y));
//						dct[cont].Nums.Add(Data.Bots[cont].Num);
//					}
//				}
//				else
//				{
//					dct.Add(cont, new Asd
//					{
//						Cnt = 1,
//						Lst = new List<(int, int)> { (x, y) },
//						Nums = new List<long> { Data.Bots[cont].Num }
//					});
//				}
//			}
//		}
//	}

//	if (dct.Count(d => d.Value.Cnt > 1) > 0)
//	{
//		var conts = dct.Where(d => d.Value.Cnt > 1);
//		var contscnt = dct.Count(d => d.Value.Cnt > 1);
//		var cont1 = conts.First();
//		var cont1idx = cont1.Key;
//		var cont1cnt = cont1.Value.Cnt;
//		var cont1lst = cont1.Value.Lst;
//		var cont1nums = cont1.Value.Nums;
//		var st = Data.CurrentStep;
//		var numb = Data.CurrentNumberOfBots;
//		return false;
//	}

//	return true;
//}



// ПРОЙТИ ПО ВСЕМУ МАССИВУ WORLD НАЙТИ ВСЕ ДУБЛИКАТЫ С КООРДИНАТАМИ И NUMS И ВЫДАТЬ ПОДРОБНУЮ ИНФОРМАЦИЮ ПО ДУБЛИКАТАМ (ТОЖЕ САМОЕ НО ВЫЗЫВАЛОСЬ ИЗ ДРУГОГО МЕСТА)
//public static bool CheckWorld2()
//{
//	var dct = new Dictionary<long, Asd>();

//	for (var x = 0; x < Data.WorldWidth; x++)
//	{
//		for (var y = 0; y < Data.WorldHeight; y++)
//		{
//			var cont = Data.World[x, y];

//			if (cont > 0 && (cont < 65000 || cont > 65504))
//			{
//				if (dct.ContainsKey(cont))
//				{
//					// а осталась ли первая точка? проверить. может просто произошло перемещение
//					if (dct[cont].Cnt == 1 && Data.World[dct[cont].Lst[0].X, dct[cont].Lst[0].Y] != cont)
//					{
//						dct[cont].Lst[0] = (x, y);
//						dct[cont].Nums[0] = Data.Bots[cont].Num;
//					}
//					else
//					{
//						dct[cont].Cnt++;
//						dct[cont].Lst.Add((x, y));
//						dct[cont].Nums.Add(Data.Bots[cont].Num);
//					}
//				}
//				else
//				{
//					dct.Add(cont, new Asd
//					{
//						Cnt = 1,
//						Lst = new List<(int, int)> { (x, y) },
//						Nums = new List<long> { Data.Bots[cont].Num }
//					});
//				}

//			}
//		}
//	}

//	if (dct.Count(d => d.Value.Cnt > 1) > 0)
//	{
//		var conts = dct.Where(d => d.Value.Cnt > 1);
//		var contscnt = dct.Count(d => d.Value.Cnt > 1);
//		var cont1 = conts.First();
//		var cont1idx = cont1.Key;
//		var cont1cnt = cont1.Value.Cnt;
//		var cont1lst = cont1.Value.Lst;
//		var cont1nums = cont1.Value.Nums;
//		var st = Data.CurrentStep;
//		var numb = Data.CurrentNumberOfBots;
//		return false;
//	}

//	return true;
//}

// ПРОЙТИ ПО ВСЕМУ МАССИВУ WORLD НАЙТИ ВСЕ ДУБЛИКАТЫ С КООРДИНАТАМИ И NUMS И ВЫДАТЬ ПОДРОБНУЮ ИНФОРМАЦИЮ ПО ДУБЛИКАТАМ
//private static void SearchDouble()
//{
//	var dct = new Dictionary<long, Asd>();

//	for (var x = 0; x < Data.WorldWidth; x++)
//	{
//		for (var y = 0; y < Data.WorldHeight; y++)
//		{
//			var cont = Data.World[x, y];

//			if (cont > 0 && (cont < 65000 || cont > 65504))
//			{
//				if (dct.ContainsKey(cont))
//				{
//					// а осталась ли первая точка? проверить. может просто произошло перемещение
//					if (dct[cont].Cnt == 1 && Data.World[dct[cont].Lst[0].X, dct[cont].Lst[0].Y] != cont)
//					{
//						dct[cont].Lst[0] = (x, y);
//						dct[cont].Nums[0] = Data.Bots[cont].Num;
//					}
//					else
//					{
//						dct[cont].Cnt++;
//						dct[cont].Lst.Add((x, y));
//						dct[cont].Nums.Add(Data.Bots[cont].Num);
//					}
//				}
//				else
//				{
//					dct.Add(cont, new Asd
//					{
//						Cnt = 1,
//						Lst = new List<(int, int)> { (x, y) },
//						Nums = new List<long> { Data.Bots[cont].Num }
//					});
//				}

//			}
//		}
//	}

//	if (dct.Count(d => d.Value.Cnt > 1) > 0)
//	{
//		var conts = dct.Where(d => d.Value.Cnt > 1);
//		var contscnt = dct.Count(d => d.Value.Cnt > 1);
//		var cont1 = conts.First();
//		var cont1idx = cont1.Key;
//		var cont1cnt = cont1.Value.Cnt;
//		var cont1lst = cont1.Value.Lst;
//		var cont1nums = cont1.Value.Nums;
//		var stp = Data.CurrentStep;
//		var numb = Data.CurrentNumberOfBots;
//	}
//}


// ПРОВЕРИТЬ ЧТО КОЛИЧЕТВО ТОЧЕК В WORLD СОВПАДАЕТ С Data.CurrentNumberOfBots, ЕСЛИ НЕТ ТО
// ВЫДАТЬ ИНФОРМАЦИЮ О ДУБЛЕ ИЛИ ОТСУТСТВУЮЩЕМ С ЕГО ЛОГОМ
//public static bool CheckWorld3()
//{
//	var cnt = 0;
//	long sum = 0;
//	for (var x = 0; x < Data.WorldWidth; x++)
//	{
//		for (var y = 0; y < Data.WorldHeight; y++)
//		{
//			var cont = Data.World[x, y];

//			if (cont > 0 && (cont < 65000 || cont > 65504))
//			{
//				sum += cont;
//				cnt++;
//			}
//		}
//	}

//	long ttt;
//	Bot1 bttt;
//	List<LogRecord> log;
//	long contttt;


//	if (cnt != Data.CurrentNumberOfBots)
//	{
//		var st = Data.CurrentStep;
//		var fcd = Data.BotDeath;

//		if (Data.CurrentNumberOfBots - cnt == 1)
//		{
//			ttt = Data.CurrentNumberOfBots * (Data.CurrentNumberOfBots + 1) / 2 - sum;
//			bttt = Data.Bots[ttt];
//			log = bttt.Log.GetLog();
//			contttt = Data.World[bttt.Xi, bttt.Yi];
//		}

//		if (Data.CurrentNumberOfBots - cnt == -1)
//		{
//			ttt = sum - Data.CurrentNumberOfBots * (Data.CurrentNumberOfBots + 1) / 2;
//			bttt = Data.Bots[ttt];
//			log = bttt.Log.GetLog();
//			contttt = Data.World[bttt.Xi, bttt.Yi];
//			SearchDouble();

//		}

//		var numberOfBotDeath = Data.NumberOfBotDeath;
//		return false;
//	}

//	return true;
//}


// ПОЛУЧИТЬ ОБЩУЮ ЭНЕРГИЮ ВСЕХ БОТОВ
//public static int GetTotalEnergy()
//{
//    var te = 0;
//    for (long botNumber = 1; botNumber <= Data.CurrentNumberOfBots; botNumber++)
//    {
//        te += Data.Bots[botNumber].Energy;
//    }

//    return te;
//}



//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
