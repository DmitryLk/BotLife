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
		public int Level;
		public long PraNum;
		public long Num;
		public uint BeginStep;
		public uint EndStep;
		//private Genom _parent;


		private int _bots;
		public int Bots
		{
			get { return _bots; }
			set
			{
				if (_bots == 1 && value == 0)
				{
					EndStep = Data.CurrentStep;
					Interlocked.Increment(ref ENDCOUNTER);
				}

				if (_bots - value > 1 && _bots - value < -1 || value < 0)
				{
					throw new Exception("if (()_bots - value != 1 && _bots - value != -1) || value<0)");
				}

				_bots = value;
			}
		}

		private Genom()
		{
			//_parent = parent;
			Bots = 0;
		}

		// Создание генома
		public static Genom CreateGenom(Genom parent = null)
		{
			var g = new Genom();

			g.Code = new byte[Data.GenomLength];
			g.GenomHash = Guid.NewGuid();
			g.Color = Func.GetRandomColor();


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
				g.Level = parent.Level + 1;
				Data.MutationCnt++;
			}

			g.Num = Interlocked.Increment(ref BEGINCOUNTER);
			GENOMS.TryAdd(g, 1);

			g.BeginStep = Data.CurrentStep;

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

			var activeGemons = GENOMS.Keys.Where(g => g.Bots > 0).Count();
			sb.AppendLine($"Active: {BEGINCOUNTER - ENDCOUNTER}");

			sb.AppendLine("");

			var genoms = GENOMS.Keys.Where(g => g.Bots > 0).OrderBy(g => g.BeginStep).Take(10);
			foreach (var g in genoms)
			{
				sb.AppendLine($"{g.Bots} - {g.PraNum}({g.Num})  L{g.Level}  ={Data.CurrentStep - g.BeginStep}");
			}
			sb.AppendLine("");

			genoms = GENOMS.Keys.Where(g => g.Bots > 0).OrderByDescending(g => g.Bots).Take(10);
			foreach (var g in genoms)
			{
				sb.AppendLine($"{g.Bots} - {g.PraNum}({g.Num})  L{g.Level}");
			}

			return sb.ToString();
		}
	}
}

