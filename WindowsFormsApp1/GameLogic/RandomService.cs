using System;
using System.Collections.Generic;
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
using WindowsFormsApp1.Enums;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace WindowsFormsApp1.GameLogic
{
    public class RandomService
    {
        private Random _rnd = new Random(Guid.NewGuid().GetHashCode());
        private WorldData _data;

        public RandomService(WorldData data)
        {
            _rnd = new Random(Guid.NewGuid().GetHashCode());
			_data = data;
        }

        public Direction GetRandomDirection()
        {
            return (Direction)_rnd.Next(0, 8);
        }

        public byte GetRandomBotCode()
        {
            return (byte)_rnd.Next(_data.MaxCode + 1);
        }

		public Point GetRandomEmptyPoint()
		{
			var p = new Point();
			var i = 0;
			do
			{
				p.X = _rnd.Next(0, _data.WorldWidth);
				p.Y = _rnd.Next(0, _data.WorldHeight);
			}
			while (CellAllreadyOccupied(p.X, p.Y) && ++i < 100);

			return p;
		}

		public (int, int) GetRandomSpeed()
        {
            //do
            //{
            //	_vx = rnd.Next(-1, 2);
            //	_vy = rnd.Next(-1, 2);
            //}
            //while (_vx == 0 && _vy == 0);

            if (_rnd.Next(100) > 97)
            {
                return (_rnd.Next(-1, 2), _rnd.Next(-1, 2));
            }
            return (0, 0);
        }

		private bool CellAllreadyOccupied(int x, int y)
		{
			return _data.World[x, y] != 0;
		}


		private unsafe Guid GuidRandomCachedInstance()
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
