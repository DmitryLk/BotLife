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

namespace WindowsFormsApp1.GameLogic
{
    public class Genom
    {
        private const int ActListSize = 1000;
        private static long BEGINCOUNTER = 0;
        private static long ENDCOUNTER = 0;
        public static ConcurrentDictionary<Genom, int> GENOMS = new ConcurrentDictionary<Genom, int>();

        public byte[] Code;
        public int[] Act;
        public bool ActCnt;
        public int[] ActList;
        public int ActListCnt;
        //public List<(int, int)> Sorted1;
        //public HashSet<int> Sorted2;

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
        public string Test = "owdiheiofhweoif";
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
        public byte[] Attack;
        public (byte Type, byte Level)[] AttackTypes;
        public int AttackTypesCnt;

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
            g.Act = new int[Data.GenomLength];
            //g.Sorted1 = new List<(int, int)>();
            //g.Sorted2 = new HashSet<int>();
            g.ActList = new int[ActListSize];
            g.ActListCnt = -1;

            g.ActCnt = true;
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
                    g.Act[i] = 0;
                    //g.Code[i] = 25;
                }
                g.ParentHash = Guid.Empty;
                g.GrandHash = Guid.Empty;
                g.PraHash = g.GenomHash;
                g.PraNum = g.Num;
                g.Level = 1;

                // Attack-Shield
                (g.Shield, g.Attack, g.AttackTypes) = Func.GetRandomAttackShield();
                g.AttackTypesCnt = g.AttackTypes.Length;
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
                    if (parent.ActListCnt > -1)
                    {
                        var lim = parent.ActListCnt + 1;
                        var indActList = Func.GetRandomNext(lim > 1000 ? 1000 : lim);
                        g.Code[parent.ActList[indActList]] = Func.GetRandomUsefulBotCode();
                    }
                    else
                    {
                        g.Code[Func.GetRandomBotCodeIndex()] = Func.GetRandomUsefulBotCode();
                    }
                }

                g.ParentHash = parent.GenomHash;
                g.GrandHash = parent.ParentHash;
                g.PraHash = parent.PraHash;
                g.PraNum = parent.PraNum;
                g.PraColor = parent.PraColor;
                g.Level = parent.Level + 1;
                Interlocked.Increment(ref Data.MutationCnt);

                // Attack-Shield
                g.Shield = parent.Shield;
                g.Attack = parent.Attack;
                g.AttackTypes = parent.AttackTypes;
                g.AttackTypesCnt = parent.AttackTypesCnt;
                //(g.Shield, g.Attack, g.AttackTypes) = Func.GetRandomAttackShield();
                //g.AttackTypesCnt = g.AttackTypes.Length;
            }

            if (!GENOMS.TryAdd(g, 1)) throw new Exception("dfsdfs85");
            return g;
        }

		
        public byte GetCurrentCommandAndSetActGen(int pointer, bool act)
        {
            if (act) SetActCommand(pointer);
            return Code[pointer];
        }

        public byte GetNextCommand(int pointer, bool act)
        {
            var nextpointer = pointer + 1 >= Data.GenomLength ? 0 : pointer + 1;
			if (act) SetActCommand(nextpointer);
			return Code[nextpointer];
        }

		private void SetActCommand(int pointer)
		{
			if (ActCnt)
			{
				Interlocked.Increment(ref Act[pointer]);

				//Sorted2.Add(pointer);
				if (Act[pointer] > 230)
				{
					ActCnt = false;
					//Sorted1 = Act
					//    .Select((x, i) => (Value: x, OriginalIndex: i))
					//    .OrderByDescending(x => x.Value)
					//    .ToList();
				}

				if (ActListCnt < ActListSize)
				{
					var indActList = Interlocked.Increment(ref ActListCnt);

					if (indActList < ActListSize)
					{
						ActList[indActList] = pointer;
					}
				}
			}
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

            var activeGenoms = GENOMS.Keys.Where(g => g.CurBots > 0).Count();
            var activeBigGenoms = GENOMS.Keys.Where(g => g.CurBots > 50).Count();
            sb.AppendLine($"Active: {BEGINCOUNTER - ENDCOUNTER}:{activeGenoms}  Big:{activeBigGenoms}");

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
                sortableBindingList = new SortableBindingList<GenomStr>(Genom.GENOMS.Keys
                    .Where(g => Data.DgvOnlyLive ? g.CurBots > minCurBots : g.AllBots > 1)
                    .Select(g => new GenomStr
                    {
                        GenomName = $"{g.PraNum} - {g.Num} - {g.Level}",
                        GenomColor = g.Color,
                        Live = g.CurBots,
                        Total = g.AllBots,
                        Age = (g.CurBots > 0 ? Data.CurrentStep : g.EndStep) - g.BeginStep,
                        AvBotAge = g.RemovedBots != 0 ? g.AgeBots / g.RemovedBots : 0,
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
                            AvBotAge = removed != 0 ? g.Sum(s => s.AgeBots) / removed : 0,
                            ActGen = 0
                        };
                    })
                    .Where(g => Data.DgvOnlyLive ? g.Live > minCurBots : g.Total > 1)
                    .ToList());
            }

            return sortableBindingList;
        }
    }

    public class GenomStr
    {
        public string GenomName { get; set; }
        public Color GenomColor { get; set; }
        public long Live { get; set; }
        public long Total { get; set; }
        public uint Age { get; set; }
        public float AvBotAge { get; set; }
        public int ActGen { get; set; }
        public string Sh_Atc { get; set; }
    }
}

