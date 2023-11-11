using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.Static;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Numerics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace WindowsFormsApp1.GameLogic
{
	public class Genom
	{
		private const int ActListSize = 300;

		// static
		private static long CreateGenomCounter = 0;
		private static long DeleteGenomCounter = 0;
		public static ConcurrentDictionary<Genom, int> GENOMS = new ConcurrentDictionary<Genom, int>();


		// Genom Code
		public byte[,,] CodeForGeneralCmds;
		public byte[,,] CodeForConditionCmds;
		public byte[,,] CodeForReactionCmds;

		public int[] Act; //сколько раз использовалась та или другая команда
		public bool ActCnt; //ведется ли подсчет количества использования команд. завершается если счетчик одной из команд дошел до 230.
		public int[] GeneralCmdsList;  //список где подряд записываются использованные общие команды (номер команды в геноме)
		public int[] ConditionCmdsList;  //список где подряд записываются использованные условные команды (номер команды в геноме)
		public int[] ReactionCmdsList;  //список где подряд записываются использованные реакций команды (номер команды в геноме)
		public int GeneralCmdsListCnt;  //количество команд в списке ActList
		public int ConditionCmdsListCnt;  //количество команд в списке ActList
		public int ReactionCmdsListCnt;  //количество команд в списке ActList

		public bool Plant = false;

		public Guid GenomHash;
		public Guid ParentHash;
		public Guid GrandHash;
		public Guid PraHash;
		public Color Color;
		public Color PraColor;
		public int Level;
		public long PraNum;
		public long Num;
		public uint BeginStep;
		public uint EndStep;
		//private Genom _parent;

		private long _curBots = 0;
		private long _allBots = 0;
		private long _removedBots = 0;
		private long _biteMeCount = 0;
		private long _biteImCount = 0;
		private int _ageBots = 0;


		// Attack-Shield
		public byte[] Shield;
		public byte[] Attack;
		public (byte Type, byte Level)[] AttackTypes;
		public int AttackTypesCnt;

		public long CurBots { get => _curBots; }
		public long AllBots { get => _allBots; }
		public long RemovedBots { get => _removedBots; }
		public int AgeBots { get => _ageBots; }
		public long BiteMeBots { get => _biteMeCount; }
		public long BiteImBots { get => _biteImCount; }

		private Genom()
		{
			//_parent = parent;
		}

		public void IncBot()
		{
			Interlocked.Increment(ref _curBots);
			Interlocked.Increment(ref _allBots);
		}
		public void DecBot(int age, int bitemecount, int biteimcount)
		{
			if (_curBots == 0)
			{
				throw new Exception("if (_curBots == 0)");
			}


			var curBots = Interlocked.Decrement(ref _curBots);
			Interlocked.Increment(ref _removedBots);
			Interlocked.Increment(ref _removedBots);
			Interlocked.Add(ref _biteMeCount, bitemecount);
			Interlocked.Add(ref _biteImCount, biteimcount);


			if (_curBots < 0 || curBots < 0)
			{
				throw new Exception("if (_curBots < 0)");
			}

			// Исчезновение генома
			if (curBots == 0)
			{
				EndStep = Data.CurrentStep;
				Interlocked.Increment(ref DeleteGenomCounter);
			}

			Interlocked.Add(ref _ageBots, age);
		}

		// Создание нового генома
		public static Genom CreateNewGenom()
		{
			var g = new Genom();
			g.CodeForGeneralCmds = new byte[Data.GenomGeneralBranchCnt, Data.MaxCmdInStep, 2];
			g.CodeForConditionCmds = new byte[Data.GenomConditionBranchCnt, Data.MaxCmdInStep, 2];
			g.CodeForReactionCmds = new byte[Data.GenomReactionBranchCnt, Data.MaxCmdInStep, 2];

			g.Act = new int[(Data.GenomGeneralBranchCnt + Data.GenomConditionBranchCnt + Data.GenomReactionBranchCnt) * Data.MaxCmdInStep];
			g.GeneralCmdsList = new int[ActListSize]; g.GeneralCmdsListCnt = 0;
			g.ConditionCmdsList = new int[ActListSize]; g.ConditionCmdsListCnt = 0;
			g.ReactionCmdsList = new int[ActListSize]; g.ReactionCmdsListCnt = 0;
			g.ActCnt = true;
			Array.Clear(g.Act);

			g.GenomHash = Guid.NewGuid();
			g.Color = Func.GetRandomColor();
			g.PraColor = g.Color;
			g.Num = Interlocked.Increment(ref CreateGenomCounter);
			g.BeginStep = Data.CurrentStep;

			g.ParentHash = Guid.Empty;
			g.GrandHash = Guid.Empty;
			g.PraHash = g.GenomHash;
			g.PraNum = g.Num;
			g.Level = 1;

			// Наполнение кода генома (общие команды)
			for (var i = 0; i < Data.GenomGeneralBranchCnt; i++)
			{
				for (var j = 0; i < Data.MaxCmdInStep; j++)
				{
					g.CodeForGeneralCmds[i, j, 0] = Func.GetRandomGeneralCmd();
					g.CodeForGeneralCmds[i, j, 1] = Func.GetRandomBotCode();
				}
			}

			// Наполнение кода генома (условные команды)
			for (var i = 0; i < Data.GenomConditionBranchCnt; i++)
			{
				for (var j = 0; i < Data.MaxCmdInStep; j++)
				{
					g.CodeForConditionCmds[i, j, 0] = Func.GetRandomGeneralCmd();
					g.CodeForConditionCmds[i, j, 1] = Func.GetRandomBotCode();
				}
			}

			// Наполнение кода генома (команды событий)
			for (var i = 0; i < Data.GenomReactionBranchCnt; i++)
			{
				for (var j = 0; j < Data.MaxCmdInStep; j++)
				{
					g.CodeForReactionCmds[i, j, 0] = Func.GetRandomReactionCmd();
					g.CodeForReactionCmds[i, j, 1] = Func.GetRandomBotCode();
				}
			}

			// Attack-Shield
			(g.Shield, g.Attack, g.AttackTypes) = Func.GetRandomAttackShield();
			g.AttackTypesCnt = g.AttackTypes.Length;

			if (!GENOMS.TryAdd(g, 1)) throw new Exception("dfsdfs85");
			return g;
		}

		// Создание генома-потомка
		public static Genom CreateChildGenom(Genom parent)
		{
			var g = new Genom();
			g.CodeForGeneralCmds = new byte[Data.GenomGeneralBranchCnt, Data.MaxCmdInStep, 2];
			g.CodeForConditionCmds = new byte[Data.GenomConditionBranchCnt, Data.MaxCmdInStep, 2];
			g.CodeForReactionCmds = new byte[Data.GenomReactionBranchCnt, Data.MaxCmdInStep, 2];

			g.Act = new int[(Data.GenomGeneralBranchCnt + Data.GenomConditionBranchCnt + Data.GenomReactionBranchCnt) * Data.MaxCmdInStep];
			g.GeneralCmdsList = new int[ActListSize]; g.GeneralCmdsListCnt = 0;
			g.ConditionCmdsList = new int[ActListSize]; g.ConditionCmdsListCnt = 0;
			g.ReactionCmdsList = new int[ActListSize]; g.ReactionCmdsListCnt = 0;
			g.ActCnt = true;
			Array.Clear(g.Act);

			g.GenomHash = Guid.NewGuid();
			g.Color = Func.GetRandomColor();
			g.PraColor = g.Color;
			g.Num = Interlocked.Increment(ref CreateGenomCounter);
			g.BeginStep = Data.CurrentStep;

			g.ParentHash = parent.GenomHash;
			g.GrandHash = parent.ParentHash;
			g.PraHash = parent.PraHash;
			g.PraNum = parent.PraNum;
			g.PraColor = parent.PraColor;
			g.Level = parent.Level + 1;
			Interlocked.Increment(ref Data.MutationCnt);

			// Копирование кода генома (общие команды)
			for (var i = 0; i < Data.GenomGeneralBranchCnt; i++)
			{
				for (var j = 0; i < Data.MaxCmdInStep; j++)
				{
					g.CodeForGeneralCmds[i, j, 0] = parent.CodeForGeneralCmds[i, j, 0];
					g.CodeForGeneralCmds[i, j, 1] = parent.CodeForGeneralCmds[i, j, 1];
				}
			}

			// Копирование кода генома (условные команды)
			for (var i = 0; i < Data.GenomConditionBranchCnt; i++)
			{
				for (var j = 0; i < Data.MaxCmdInStep; j++)
				{
					g.CodeForConditionCmds[i, j, 0] = parent.CodeForConditionCmds[i, j, 0];
					g.CodeForConditionCmds[i, j, 1] = parent.CodeForConditionCmds[i, j, 1];
				}
			}

			// Копирование кода генома (команды событий)
			for (var i = 0; i < Data.GenomReactionBranchCnt; i++)
			{
				for (var j = 0; i < Data.MaxCmdInStep; j++)
				{
					g.CodeForReactionCmds[i, j, 0] = parent.CodeForReactionCmds[i, j, 0];
					g.CodeForReactionCmds[i, j, 1] = parent.CodeForReactionCmds[i, j, 1];
				}
			}


			// Мутация (Data.MutationLenght байт в геноме подменяем)
			for (var i = 0; i < Data.MutationLenght; i++)
			{
				var rnd = Func.GetRandomNext(100);
				var rndpar = Func.GetRandomNext(2);

				// Мутация в основном коде
				int branch;
				int cmdnum;
				if (rnd >= 0 && rnd < 70)
				{
					if (parent.GeneralCmdsListCnt > 0)
					{
						var lim = parent.GeneralCmdsListCnt;
						var indGeneralCmdsList = Func.GetRandomNext(lim > 300 ? 300 : lim);
						var cmd = parent.GeneralCmdsList[indGeneralCmdsList];

						branch = cmd / Data.MaxCmdInStep;
						cmdnum = cmd % Data.MaxCmdInStep;
					}
					else
					{
						branch = Func.GetRandomNext(Data.GenomGeneralBranchCnt);
						cmdnum = Func.GetRandomNext(Data.MaxCmdInStep);
					}

					if (Data.CommandsWithParameter[g.CodeForGeneralCmds[branch, cmdnum, 0]] && rndpar == 1)
					{
						g.CodeForGeneralCmds[branch, cmdnum, 1] = Func.GetRandomBotCode();
					}
					else
					{
						g.CodeForGeneralCmds[branch, cmdnum, 0] = Func.GetRandomGeneralCmd();
					}
				}

				// Мутация в событиях
				if (rnd >= 70 && rnd < 100)
				{
					if (parent.ReactionCmdsListCnt > 0)
					{
						var lim = parent.ReactionCmdsListCnt;
						var indGeneralCmdsList = Func.GetRandomNext(lim);
						var cmd = parent.ReactionCmdsList[indGeneralCmdsList];

						branch = cmd / Data.MaxCmdInStep;
						cmdnum = cmd % Data.MaxCmdInStep;
					}
					else
					{
						branch = Func.GetRandomNext(Data.GenomReactionBranchCnt);
						cmdnum = Func.GetRandomNext(Data.MaxCmdInStep);
					}

					if (Data.CommandsWithParameter[g.CodeForReactionCmds[branch, cmdnum, 0]] && rndpar == 1)
					{
						g.CodeForReactionCmds[branch, cmdnum, 1] = Func.GetRandomBotCode();
					}
					else
					{
						g.CodeForReactionCmds[branch, cmdnum, 0] = Func.GetRandomReactionCmd();
					}
				}
			}


			// Attack-Shield
			g.Shield = parent.Shield;
			g.Attack = parent.Attack;
			g.AttackTypes = parent.AttackTypes;
			g.AttackTypesCnt = parent.AttackTypesCnt;
			//(g.Shield, g.Attack, g.AttackTypes) = Func.GetRandomAttackShield();
			//g.AttackTypesCnt = g.AttackTypes.Length;

			if (!GENOMS.TryAdd(g, 1)) throw new Exception("dfsdfs85");
			return g;
		}

		public (byte, byte) GetGeneralCommandAndSetAct(Pointer p, bool act)
		{
			if (act) SetActGeneralCommand(p);
			return (CodeForGeneralCmds[p.b, p.cmd, 0], CodeForGeneralCmds[p.b, p.cmd, 1]);
		}

		public (byte, byte) GetConditionCommandAndSetAct(Pointer p, bool act)
		{
			if (act) SetActConditionCommand(p);
			return (CodeForConditionCmds[p.b, p.cmd, 0], CodeForConditionCmds[p.b, p.cmd, 1]);
		}

		public (byte, byte) GetReactionCommandAndSetAct(Pointer p, bool act)
		{
			if (act) SetActReactionCommand(p);
			return (CodeForReactionCmds[p.b, p.cmd, 0], CodeForReactionCmds[p.b, p.cmd, 1]);
		}

		private void SetActGeneralCommand(Pointer p)
		{
			if (ActCnt)
			{
				var num = p.b * Data.MaxCmdInStep + p.cmd;

				Interlocked.Increment(ref Act[num]);

				if (Act[num] > 230)
				{
					ActCnt = false;
				}
			}

			if (GeneralCmdsListCnt < ActListSize)
			{
				var curcnt = Interlocked.Increment(ref GeneralCmdsListCnt);
				if (GeneralCmdsListCnt > ActListSize)
				{
					GeneralCmdsListCnt = ActListSize;
				}

				if (curcnt <= ActListSize)
				{
					GeneralCmdsList[curcnt - 1] = p.b * Data.MaxCmdInStep + p.cmd;
				}
			}
		}

		private void SetActConditionCommand(Pointer p)
		{
			if (ActCnt)
			{
				var num = (p.b + Data.GenomGeneralBranchCnt) * Data.MaxCmdInStep + p.cmd;

				Interlocked.Increment(ref Act[num]);

				if (Act[num] > 230)
				{
					ActCnt = false;
				}
			}

			if (ConditionCmdsListCnt < ActListSize)
			{
				var curcnt = Interlocked.Increment(ref ConditionCmdsListCnt);
				if (ConditionCmdsListCnt > ActListSize)
				{
					ConditionCmdsListCnt = ActListSize;
				}

				if (curcnt <= ActListSize)
				{
					ConditionCmdsList[curcnt - 1] = p.b * Data.MaxCmdInStep + p.cmd;
				}
			}
		}

		private void SetActReactionCommand(Pointer p)
		{
			if (ActCnt)
			{
				var num = (p.b + Data.GenomGeneralBranchCnt + Data.GenomConditionBranchCnt) * Data.MaxCmdInStep + p.cmd;

				Interlocked.Increment(ref Act[num]);

				if (Act[num] > 230)
				{
					ActCnt = false;
				}
			}

			if (ReactionCmdsListCnt < ActListSize)
			{
				var curcnt = Interlocked.Increment(ref ReactionCmdsListCnt);
				if (ReactionCmdsListCnt > ActListSize)
				{
					ReactionCmdsListCnt = ActListSize;
				}

				if (curcnt <= ActListSize)
				{
					ReactionCmdsList[curcnt - 1] = p.b * Data.MaxCmdInStep + p.cmd;
				}
			}
		}

		public bool IsRelative(Genom genom2)
		{
			return GenomHash == genom2.GenomHash;
		}

		//=========================================================

		public static string GetText()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"Count: {CreateGenomCounter}");

			var activeGenoms = GENOMS.Keys.Where(g => g.CurBots > 0).Count();
			var activeBigGenoms = GENOMS.Keys.Where(g => g.CurBots > 50).Count();
			sb.AppendLine($"Active: {CreateGenomCounter - DeleteGenomCounter}:{activeGenoms}  Big:{activeBigGenoms}");

			var activePraGenoms = GENOMS.Keys.Where(g => g.CurBots > 0).DistinctBy(g => g.PraNum).Count();
			var activePraBigGenoms = GENOMS.Keys.GroupBy(k => k.PraNum).Select(g => g.Sum(s => s.CurBots)).Count(d => d > 50);

			sb.AppendLine($"PraActive: {activePraGenoms}  Big:{activePraBigGenoms}");

			return sb.ToString();
		}

		public static SortableBindingList<GenomStr> GetSortableBindingList(int minCurBots)
		{
			SortableBindingList<GenomStr> sortableBindingList;

			if (!Data.DgvPra)
			{
				sortableBindingList = new SortableBindingList<GenomStr>(GENOMS.Keys
					.Where(g => Data.DgvOnlyLive ? g.CurBots > minCurBots : g.AllBots > 1)
					.Select(g => new GenomStr
					{
						GenomName = $"{g.PraNum} - {g.Num} - {g.Level}",
						GenomColor = g.Color,
						Live = g.CurBots,
						Total = g.AllBots,
						GAge = (g.CurBots > 0 ? Data.CurrentStep : g.EndStep) - g.BeginStep,
						Age = g.RemovedBots != 0 ? g.AgeBots / g.RemovedBots : 0,
						Me = g.RemovedBots != 0 ? (float)g.BiteMeBots / g.RemovedBots : 0,
						Im = g.RemovedBots != 0 ? (float)g.BiteImBots / g.RemovedBots : 0,
						ActGen = g.Act.Count(g => g > 0),
						Sh_Atc = $"{string.Join("/", g.Shield.Take(Data.AttackShieldTypeCount))} = {string.Join("/", g.Attack.Take(Data.AttackShieldTypeCount))}"
					}).ToList());
			}
			else
			{
				sortableBindingList = new SortableBindingList<GenomStr>(Genom.GENOMS.Keys
					.GroupBy(k => k.PraNum)
					.Select(g =>
					{
						var f = g.First();
						var removed = g.Sum(s => s.RemovedBots);
						var live = g.Where(s => s.CurBots > 0);
						var minl = 0;
						var maxl = 0;
						long botminl = 0;
						long botmaxl = 0;
						if (live.Any())
						{
							minl = live.Min(s => s.Level);
							maxl = live.Max(s => s.Level);
							botminl = live.Where(s => s.Level == minl).Sum(s => s.CurBots);
							botmaxl = live.Where(s => s.Level == maxl).Sum(s => s.CurBots);
						}

						return new GenomStr
						{
							GenomName = $"{f.PraNum} ({minl}({botminl})-{maxl}({botmaxl}))",
							GenomColor = f.PraColor,
							Live = g.Sum(s => s.CurBots),
							Total = g.Sum(s => s.AllBots),
							//Age = (g.CurBots > 0 ? Data.CurrentStep : g.EndStep) - g.BeginStep,
							Age = removed != 0 ? g.Sum(s => s.AgeBots) / removed : 0,
							Me = removed != 0 ? g.Sum(s => s.BiteMeBots) / removed : 0,
							Im = removed != 0 ? g.Sum(s => s.BiteImBots) / removed : 0,
							ActGen = 0
						};
					})
					.Where(g => Data.DgvOnlyLive ? g.Live > minCurBots : g.Total > 1)
					.ToList());
			}

			return sortableBindingList;
		}
	}

	public class GenomStr  // used only in GetSortableBindingList
	{
		public string GenomName { get; set; }
		public Color GenomColor { get; set; }
		public long Live { get; set; }
		public long Total { get; set; }
		public uint GAge { get; set; }
		public float Age { get; set; }
		public float Im { get; set; }
		public float Me { get; set; }
		public int ActGen { get; set; }
		public string Sh_Atc { get; set; }
	}
}

