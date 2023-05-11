using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.GameLogic;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using DrawMode = WindowsFormsApp1.Enums.DrawMode;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace WindowsFormsApp1.Static
{
	public static class Func
	{
		public static Random Rnd = new Random(Guid.NewGuid().GetHashCode());
		private static readonly object _busyWorld = new object();
		private static readonly object _busyChWorld = new object();

		// Список измененных ячеек для последующей отрисовки
		//public long[,] ChWorld;				- по координатам можно определить перерисовывать ли эту ячейку, там записам индекс массива ChangedCell
		//public ChangedCell[] ChangedCells;	- массив перерисовки, в нем перечислены координаты перерисуеваемых ячеек и их цвета
		//public long NumberOfChangedCells;		- количество изменившихся ячеек на экране колторые надо перерисовать в следующий раз
		public static void FixChangeCell(int x, int y, Color? color)
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
					Color = color
				};
			}
			else
			{
				if (first)
				{
					Data.ChangedCells[num].X = x;
					Data.ChangedCells[num].Y = y;
					Data.ChangedCells[num].Color = color;
				}
				else
				{
					Data.ChangedCells[num].Color = color;
				}
			}
		}

		public static void Death()
		{
			//SearchDouble();
			//CheckIndex();

			Parallel.For(0, (int)Data.NumberOfBotDeath + 1, DeathBot);

			//Func.CheckWorld2();
			//SearchDouble();
			//CheckIndex();

			int maxdeathindex;
			long maxworldindex;
			while (true)
			{
				maxdeathindex = -1;
				maxworldindex = -1;
				// Выбрать из масива Data.BotDeath[] бота с максимальным world индексом
				for (var i = 0; i <= Data.NumberOfBotDeath; i++)
				{
					var worldindex = Data.BotDeath[i] == null ? -1 : Data.BotDeath[i].Index;

					if (worldindex > maxworldindex)
					{
						maxdeathindex = i;
						maxworldindex = worldindex;
					}
				}


				// все обработаны
				if (maxdeathindex == -1) break;

				Data.BotDeath[maxdeathindex] = null;

				if (maxworldindex > Data.CurrentNumberOfBots)
				{
					throw new Exception("if (_ind > Data.CurrentBotsNumber)");
				}

				if (maxworldindex < Data.CurrentNumberOfBots)
				{
					// Перенос последнего бота в освободившееся отверстие
					// надо его убрать из массива ботов, переставив последнего бота на его место
					// Bots: 0[пусто] 1[бот _ind=1] 2[бот _ind=2]; StartBotsNumber=2 CurrentBotsNumber=2
					for (var x = 0; x < Data.WorldWidth; x++)
					{
						for (var y = 0; y < Data.WorldHeight; y++)
						{
							var cont = Data.World[x, y];
							if (cont == maxworldindex)
							{
								var ls = new List<int>();
								for (var i = 0; i < (int)Data.NumberOfBotDeath + 1; i++)
								{
									var ind = Array.FindIndex(Data.Bots, x => x != null && x.Index == Data.BotDeath[i].Index && x.Num == Data.BotDeath[i].Num);
									ls.Add(ind);
								}
							}
						}
					}

					var lastBot = Data.Bots[Data.CurrentNumberOfBots];
					var lastIndex = Data.Bots[Data.CurrentNumberOfBots].Index;
					Data.Bots[maxworldindex] = lastBot;
					lastBot.Index = maxworldindex;
					//lastBot.Log.AddLog($"Index changed from {lastIndex}/{Data.CurrentNumberOfBots} to {maxworldindex}");
					Data.World[lastBot.Xi, lastBot.Yi] = maxworldindex;

					//Func.CheckWorld2();
					//Func.ChangeCell(P.X, P.Y,  - делать не надо так как по этим координатам ничего не произошло, бот по этим координатам каким был таким и остался, только изменился индекс в двух массивах Bots и World
					//после этого ссылки на текущего бота нигде не останется и он должен будет уничтожен GC

				}

				// Укарачиваем массив
				Data.Bots[Data.CurrentNumberOfBots] = null;
				Interlocked.Decrement(ref Data.CurrentNumberOfBots);
			}


			//CheckIndex();
			//SearchDouble();
			//Func.CheckWorld2();

			Data.DeathCnt += Data.NumberOfBotDeath + 1;

			Data.NumberOfBotDeath = -1;
		}

		private static void DeathBot(int index)
		{
			var bot = Data.BotDeath[index];

			//Func.CheckWorld2();

			lock (_busyWorld)
			{
				if (Data.World[bot.Xi, bot.Yi] != bot.Index)
				{
					Data.World[bot.Xi, bot.Yi] = 2;
				}
				Data.World[bot.Xi, bot.Yi] = 0;

				if (Data.World[bot.Xi, bot.Yi] != 0)
				{
					Data.World[bot.Xi, bot.Yi] = 2;
				}
			}

			//Func.CheckWorld2();

			if (Data.DrawType == DrawType.OnlyChangedCells)
			{
				FixChangeCell(bot.Xi, bot.Yi, null); // при следующей отрисовке бот стерется с экрана
			}

			bot.Genom.RemoveBot(bot.Age);
		}


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


		//public class Asd
		//{
		//	public int Cnt;
		//	public List<long> Nums;
		//	public List<(int X, int Y)> Lst;
		//}

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

		//private static void CheckIndex()
		//{
		//	for (long i = 1; i <= Data.CurrentNumberOfBots; i++)
		//	{
		//		if (Data.Bots[i].Index != i)
		//		{ 
		//		}
		//	}
		//}

		public static void Reproduction()
		{
			//SearchDouble();

			Parallel.For(0, (int)Data.NumberOfBotReproduction + 1, ReproductionBot);

			//SearchDouble();

			Data.NumberOfBotReproduction = -1;
		}


		private static void ReproductionBot(int index)
		{
			var reproductedBot = Data.BotReproduction[index];
			if (!reproductedBot.CanReproduct()) return;

			if (TryOccupyRandomFreeCellNearby(reproductedBot.Xi, reproductedBot.Yi, out var x, out var y, out var newBotIndex))
			{
				var genom = Func.Mutation() ? Genom.CreateGenom(reproductedBot.Genom) : reproductedBot.Genom;

				Bot1.CreateNewBot(x, y, newBotIndex, Data.InitialBotEnergy, genom);
				reproductedBot.Energy -= Data.InitialBotEnergy;

				Interlocked.Increment(ref Data.ReproductionCnt);
			}
		}

		private static bool TryOccupyRandomFreeCellNearby(int Xi, int Yi, out int nXi, out int nYi, out long newBotIndex)
		{
			newBotIndex = 0;
			var n = Func.Rnd.Next(8);
			var i = 0;
			bool ready = false;

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
								newBotIndex = Interlocked.Increment(ref Data.CurrentNumberOfBots);
								Data.World[nXi, nYi] = newBotIndex;
								ready = true;
							}
						}
					}
				}

				i++;
				if (++n >= 8) n -= 8;
			}
			while (!ready && i <= 8);

			return ready;
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



		#region Random

		public static byte GetRandomBotCode()
		{
			return (byte)Rnd.Next(Data.MaxCode + 1);
		}

		public static Color GetRandomColor()
		{
			return Color.FromArgb(Rnd.Next(256), Rnd.Next(256), Rnd.Next(256));
		}

		public static int GetRandomBotCodeIndex()
		{
			return Rnd.Next(Data.GenomLength);
		}

		public static bool Mutation()
		{
			return Data.Mutation && Rnd.NextDouble() * 100 < Data.MutationProbabilityPercent;
		}

		public static bool TryGetRandomFreeCell(out int x, out int y)
		{
			x = 0;
			y = 0;
			var i = 0;

			do
			{
				x = Rnd.Next(0, Data.WorldWidth);
				y = Rnd.Next(0, Data.WorldHeight);
			}
			while (CellIsBusy(x, y) && ++i < 1000);

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

			if (Rnd.Next(100) > 97)
			{
				return (Rnd.Next(-1, 2), Rnd.Next(-1, 2));
			}
			return (0, 0);
		}

		private static bool CellIsBusy(int x, int y)
		{
			return Data.World[x, y] != 0;
		}


		private static unsafe Guid GuidRandomCachedInstance()
		{
			var bytes = stackalloc byte[16];
			var dst = bytes;

			var random = ThreadSafeRandom.ObtainThreadStaticRandom();
			for (var i = 0; i < 4; i++)
			{
				*(int*)dst = random.Next();
				dst += 4;
			}

			return *(Guid*)bytes;
		}

		#endregion
	}

	internal static class ThreadSafeRandom
	{
		[ThreadStatic]
		private static Random random;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Random ObtainThreadStaticRandom() => ObtainRandom();

		private static Random ObtainRandom()
		{
			return random ?? (random = new Random(Guid.NewGuid().GetHashCode()));
		}
	}
}
