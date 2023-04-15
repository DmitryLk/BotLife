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

namespace WindowsFormsApp1.GameLogic
{
	public class Genom
    {
        private static long COUNTER = 0;
        public static ConcurrentDictionary<Genom, int> GENOMS = new ConcurrentDictionary<Genom, int>();

        public byte[] Code;
		public Guid GenomHash;
		public Guid ParentHash;
        public Guid GrandHash;
        public Guid PraHash;
		public Color Color;
        public int Bots;
        public int Level;
        public long PraNum;
        public long Num;

        //private Genom _parent;

        private Genom()
		{
			//_parent = parent;
			Bots = 0;
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


		// Создание нового генома
        public static Genom CreateNewGenom()
        {
            var g = new Genom();

            g.Code = new byte[Data.GenomLength];
            g.GenomHash = Guid.NewGuid();
            g.Color = Func.GetRandomColor();

            for (var i = 0; i < Data.GenomLength; i++)
            {
                g.Code[i] = Func.GetRandomBotCode();
            }
            g.ParentHash = Guid.Empty;
            g.GrandHash = Guid.Empty;
            g.PraHash = g.GenomHash;
            g.Level = 1;

            g.Num = Interlocked.Increment(ref COUNTER);
            g.PraNum = g.Num;
            GENOMS.TryAdd(g, 1);
            return g;
        }

        // Создание мутировавшего генома

        public static Genom CreateMutatedGenom(Genom parent)
        {
            var g = new Genom();

            g.Code = new byte[Data.GenomLength];
            g.GenomHash = Guid.NewGuid();
            g.Color = Func.GetRandomColor();

            for (var i = 0; i < Data.GenomLength; i++)
            {
                g.Code[i] = parent.Code[i];
            }

            // один байт в геноме подменяем
            g.Code[Func.GetRandomBotCodeIndex()] = Func.GetRandomBotCode();

            g.ParentHash = parent.GenomHash;
            g.GrandHash = parent.ParentHash;
            g.PraHash = parent.PraHash;
            Data.MutationCnt++;
            g.Level = parent.Level + 1;

            g.Num = Interlocked.Increment(ref COUNTER);
            g.PraNum = parent.PraNum;
            GENOMS.TryAdd(g, 1);
            return g;
        }

        public static string GetText()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Count: {COUNTER}");
            sb.AppendLine("");

            var gemons = GENOMS.Keys.Where(g => g.Bots > 0).OrderByDescending(g => g.Bots).Take(20);

            foreach (var g in gemons)
            {
                sb.AppendLine($"{g.Bots} - {g.PraNum}({g.Num})  L{g.Level}");

            }

            return sb.ToString();
        }
    }
}

