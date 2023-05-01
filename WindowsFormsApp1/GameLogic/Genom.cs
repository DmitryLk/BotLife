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

		private Genom()
		{
			//_parent = parent;
		}

		public void AddBot()
		{
			Interlocked.Increment(ref _curBots);
			Interlocked.Increment(ref _allBots);
		}
		public void RemoveBot(int age)
		{
			if (_curBots < 1)
			{
				throw new Exception("if (_curBots < 1)");
			}

			// Исчезновение генома
			if (_curBots == 1)
			{
				EndStep = Data.CurrentStep;
				Interlocked.Increment(ref ENDCOUNTER);
			}

			Interlocked.Decrement(ref _curBots);
			Interlocked.Increment(ref _removedBots);

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
				Data.MutationCnt++;
			}

			GENOMS.TryAdd(g, 1);
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



		public static string GetText()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"Count: {BEGINCOUNTER}");

			var activeGemons = GENOMS.Keys.Where(g => g.CurBots > 0).Count();
			sb.AppendLine($"Active: {BEGINCOUNTER - ENDCOUNTER}");

			//sb.AppendLine("");
			//sb.AppendLine("По кол-ву живых ботов");
			//var genoms = GENOMS.Keys.Where(g => g.Bots > 0).OrderByDescending(g => g.Bots).Take(10);
			//foreach (var g in genoms)
			//{
			//	sb.AppendLine($"{g.Bots} - {g.PraNum}({g.Num})  L{g.Level}  ={Data.CurrentStep - g.BeginStep}");
			//}

			//sb.AppendLine("");
			//sb.AppendLine("По времени жизни генома");
			//genoms = GENOMS.Keys.Where(g => g.Bots > 0).OrderBy(g => g.BeginStep).Take(10);
			//foreach (var g in genoms)
			//{
			//	sb.AppendLine($"{g.Bots} - {g.PraNum}({g.Num})  L{g.Level}  ={Data.CurrentStep - g.BeginStep}");
			//}

			//sb.AppendLine("");
			//sb.AppendLine("По общему кол-ву ботов");
			//genoms = GENOMS.Keys.OrderByDescending(g => g.AllBots).Take(10);
			//foreach (var g in genoms)
			//{
			//	sb.AppendLine($"{g.AllBots}/{g.Bots} - {g.PraNum}({g.Num})  L{g.Level}  {(g._curBots == 0 ? $"{g.EndStep - g.BeginStep}" : "L")}");
			//}


			sb.AppendLine("");
			sb.AppendLine("По ср.пр.жизни ботов");
			var genoms = GENOMS.Keys.Where(g => g.RemovedBots > 10).OrderByDescending(g => g.AgeBots / g.RemovedBots).Take(10);
			foreach (var g in genoms)
			{
				sb.AppendLine($"{g.AllBots}/{g.CurBots} - {g.PraNum}({g.Num})  L{g.Level}  {g.AgeBots / g.RemovedBots}");
			}

			return sb.ToString();
		}
	}
}

