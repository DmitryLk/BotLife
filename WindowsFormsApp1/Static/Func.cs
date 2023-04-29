using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
		private static readonly object _busy = new object();


		// Создать нового бота можно только через этот метод им пользуется Seeder и Reproduction
		public static void CreateNewBot(int x, int y, uint botIndex, int en, Genom genom)
        {
            var dir = Dir.GetRandomDirection();
            Data.TotalEnergy += en;
            var pointer = 0;
            var botNumber = ++Data.MaxBotNumber;

            genom.AddBot();
            var bot = new Bot1(x,y, dir, botNumber, botIndex, en, genom, pointer);
            bot.RefreshColor();
            Data.Bots[botIndex] = bot;

            Data.World[x, y] = botIndex;
			if (Data.DrawType == DrawType.OnlyChangedCells) FixChangeCell(x, y, bot.Color,"createNewBot");
        }


		private static long COUNTER1 = 0;
		private static long COUNTER2 = 0;
		private static Stack<string> st = new Stack<string>();
		// Список измененных ячеек для последующей отрисовки
		public static void FixChangeCell(int x, int y, Color? color, string stack)
        {
			if (COUNTER1 != COUNTER2)
			{
			}
			st.Push(stack);
			
			Interlocked.Increment(ref COUNTER1);

			//public uint[,] ChWorld;				- по координатам можно определить перерисовывать ли эту ячейку, там записам индекс массива ChangedCell
			//public ChangedCell[] ChangedCells;	- массив перерисовки, в нем перечислены координаты перерисуеваемых ячеек и их цвета
			//public uint NumberOfChangedCells;		- количество изменившихся ячеек на экране колторые надо перерисовать в следующий раз

			if (Data.ChWorld[x, y] == 0)
			{
				// В этой клетке еще не было изменений после последнего рисования
				Data.ChangedCells[Data.NumberOfChangedCells] = new ChangedCell
				{
					X = x,
					Y = y,
					Color = color
				};
				if (Data.NumberOfChangedCells > 900 && Data.ChangedCells[Data.NumberOfChangedCells - 1] == null)
				{
				}
				Data.NumberOfChangedCells++;
				Data.ChWorld[x, y] = Data.NumberOfChangedCells; // сюда записываем +1 чтобы 0 не записывать
			}
			else
			{
				// В этой клетке уже были изменения после последнего рисования
				Data.ChangedCells[Data.ChWorld[x, y] - 1].Color = color;
				//_data.ChangedCells[_data.ChWorld[x, y] - 1] = new ChangedCell
				//{
				//	X = x,
				//	Y = y,
				//	Color = color
				//};
			}
			Interlocked.Increment(ref COUNTER2);
			//lock (_busy)
   //         {
			//}
            if (st.Any()) st.Pop();
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
