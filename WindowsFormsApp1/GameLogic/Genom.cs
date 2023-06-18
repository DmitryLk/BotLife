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
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.Static;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1.GameLogic
{
	public class Genom
	{
		private static long BEGINCOUNTER = 0;
		private static long ENDCOUNTER = 0;
		public static ConcurrentDictionary<Genom, int> GENOMS = new ConcurrentDictionary<Genom, int>();

		public byte[] Code;
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
		private int _ageBots = 0;
		public bool Plant = false;

		public long CurBots { get => _curBots; }
		public long AllBots { get => _allBots; }
		public long RemovedBots { get => _removedBots; }
		public int AgeBots { get => _ageBots; }

		// Attack-Shield
		public byte[] Shield;
		public byte AttackType;
		public byte AttackLevel;

		private Genom()
		{
			//_parent = parent;
		}

		public void IncBot()
		{
			Interlocked.Increment(ref _curBots);
			Interlocked.Increment(ref _allBots);
		}
		public void DecBot(int age)
		{
			if (_curBots == 0)
			{
				throw new Exception("if (_curBots == 0)");
			}


			var curBots = Interlocked.Decrement(ref _curBots);
			Interlocked.Increment(ref _removedBots);

			if (_curBots < 0 || curBots < 0)
			{
				throw new Exception("if (_curBots < 0)");
			}

			// Исчезновение генома
			if (curBots == 0)
			{
				EndStep = Data.CurrentStep;
				Interlocked.Increment(ref ENDCOUNTER);
			}

			_ageBots += age;
		}

		// Создание генома
		public static Genom CreateGenom(Genom parent = null)
		{
			var g = new Genom();

			g.Code = new byte[Data.GenomLength];
			g.GenomHash = Guid.NewGuid();
			//g.Color = Color.Red;
			g.Color = Func.GetRandomColor();
			g.PraColor = g.Color;
			g.Num = Interlocked.Increment(ref BEGINCOUNTER);
			g.BeginStep = Data.CurrentStep;

			if (parent == null)
			{
				for (var i = 0; i < Data.GenomLength; i++)
				{
					g.Code[i] = Func.GetRandomBotCode();
					//g.Code[i] = 25;
				}
				g.ParentHash = Guid.Empty;
				g.GrandHash = Guid.Empty;
				g.PraHash = g.GenomHash;
				g.PraNum = g.Num;
				g.Level = 1;

				// Attack-Shield
				g.Shield = Func.GetRandomShield(Data.ShieldSum, Data.ShieldMax, Data.ShieldTypeCount, Data.ShieldTypeCountMax);
				g.AttackType = Func.GetRandomAttackType(Data.AttackTypeCount);
				g.AttackLevel = Func.GetRandomAttackLevel(Data.AttackMax);
			}
			else
			{
				for (var i = 0; i < Data.GenomLength; i++)
				{
					g.Code[i] = parent.Code[i];
				}
				// Data.MutationLenght байт в геноме подменяем
				for (var i = 0; i < Data.MutationLenght; i++)
				{
					g.Code[Func.GetRandomBotCodeIndex()] = Func.GetRandomBotCode();
				}
				g.ParentHash = parent.GenomHash;
				g.GrandHash = parent.ParentHash;
				g.PraHash = parent.PraHash;
				g.PraNum = parent.PraNum;
				g.PraColor = parent.PraColor;
				g.Level = parent.Level + 1;
				Interlocked.Increment(ref Data.MutationCnt);

				// Attack-Shield
				//g.Shield = parent.Shield;
				//g.AttackType = parent.AttackType;
				//g.AttackLevel = parent.AttackLevel;
				g.Shield = Func.GetRandomShield(Data.ShieldSum, Data.ShieldMax, Data.ShieldTypeCount, Data.ShieldTypeCountMax);
				g.AttackType = Func.GetRandomAttackType(Data.AttackTypeCount);
				g.AttackLevel = Func.GetRandomAttackLevel(Data.AttackMax);
			}

			if (!GENOMS.TryAdd(g, 1)) throw new Exception("dfsdfs85");
			return g;
		}

		public byte GetCurrentCommand(int pointer)
		{
			return Code[pointer];
		}

		public byte GetNextCommand(int pointer)
		{
			return Code[pointer + 1 >= Data.GenomLength ? 0 : pointer + 1];
		}

		public bool IsRelative(Genom genom2)
		{
			if (GenomHash == genom2.GenomHash || GenomHash == genom2.ParentHash || GenomHash == genom2.GrandHash) return true;
			if (ParentHash == genom2.GenomHash || ParentHash == genom2.ParentHash || ParentHash == genom2.GrandHash) return true;
			if (GrandHash == genom2.GenomHash || GrandHash == genom2.ParentHash || GrandHash == genom2.GrandHash) return true;
			return false;
		}



		public static string GetText(GenomInfoMode mode)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"Count: {BEGINCOUNTER}");

			var activeGenoms = GENOMS.Keys.Where(g => g.CurBots > 0).Count();
			var activeBigGenoms = GENOMS.Keys.Where(g => g.CurBots > 50).Count();
			sb.AppendLine($"Active: {BEGINCOUNTER - ENDCOUNTER}:{activeGenoms}  Big:{activeBigGenoms}");

			var activePraGenoms = GENOMS.Keys.Where(g => g.CurBots > 0).DistinctBy(g => g.PraNum).Count();
			var activePraBigGenoms = GENOMS.Keys.Where(g => g.CurBots > 50).DistinctBy(g => g.PraNum).Count();
			sb.AppendLine($"PraActive: {activePraGenoms}  Big:{activePraBigGenoms}");

			sb.AppendLine("");

			IEnumerable<Genom> genoms;
			switch (mode)
			{
				case GenomInfoMode.LiveBotsNumber:
					sb.AppendLine("ПО КОЛ-ВУ ЖИВЫХ БОТОВ");
					genoms = GENOMS.Keys.Where(g => g.CurBots > 0).OrderByDescending(g => g.CurBots).Take(10);
					break;

				case GenomInfoMode.GenomLifetime:
					sb.AppendLine("ПО ВРЕМЕНИ ЖИЗНИ ГЕНОМА");
					genoms = GENOMS.Keys.Where(g => g.CurBots > 0).OrderBy(g => g.BeginStep).Take(10);
					break;

				case GenomInfoMode.AllBotsNumber:
					sb.AppendLine("ПО ОБЩЕМУ КОЛ-ВУ БОТОВ");
					genoms = GENOMS.Keys.OrderByDescending(g => g.AllBots).Take(10);
					break;

				case GenomInfoMode.AverageBotsLifetime:
					sb.AppendLine("По СР.ВОЗРАСТУ БОТОВ");
					genoms = GENOMS.Keys.Where(g => g.RemovedBots > 10).OrderByDescending(g => g.AgeBots / g.RemovedBots).Take(10);
					break;

				default: throw new Exception("fgfdgfdg");

			}

			sb.AppendLine("Живых|Всего |Первый|Текущий|Поколение |Возраст |Ср.возраст|");
			sb.AppendLine("ботов  |ботов  |геном   |геном    |генома        |генома   |ботов        |");

			foreach (var g in genoms)
			{
				var genomAge = g._curBots == 0 ? $"{g.EndStep - g.BeginStep}N" : $"{Data.CurrentStep - g.BeginStep}L";

				sb.AppendLine($"|  {g.CurBots,-8}|  {g.AllBots,-8}|  {g.PraNum,-10}|  {g.Num,-11}|  {g.Level,-15}|  {genomAge,-11}|  {(g.RemovedBots != 0 ? g.AgeBots / g.RemovedBots : 0),-14}|");
			}


			return sb.ToString();
		}
	}
}

