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
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Label = System.Windows.Forms.Label;
using Point = WindowsFormsApp1.Dto.Point;
using TextBox = System.Windows.Forms.TextBox;

namespace WindowsFormsApp1.GameLogic
{
	public class Func
	{
		private Random _rnd = new Random(Guid.NewGuid().GetHashCode());
		private GameData _data;

		public Func(GameData data)
		{
			_rnd = new Random(Guid.NewGuid().GetHashCode());
			_data = data;
		}


		// Создать нового бота можно только через этот метод им пользуется Seeder и Reproduction
		public void CreateNewBot(Point p, uint botIndex, Genom genom)
		{
			var dir = GetRandomDirection();
			var en = _data.InitialBotEnergy;
			var (vx, vy) = GetRandomSpeed();
			var pointer = 0;
			var botNumber = ++_data.MaxBotNumber;

			genom.Bots++;
			var bot = new Bot1(_data, this, p, dir, botNumber, botIndex, en, genom, pointer, vx, vy);




			_data.Bots[botIndex] = bot;

			_data.World[p.X, p.Y] = botIndex;
			ChangeCell(p.X, p.Y, genom.Color);
		}


		// Запись в буфер измененных ячеек для последующей отрисовки
		public void ChangeCell(int x, int y, Color? color)
		{
			//public uint[,] ChWorld;				- по координатам можно определить перерисовывать ли эту ячейку, там записам индекс массива ChangedCell
			//public ChangedCell[] ChangedCells;	- массив перерисовки, в нем перечислены координаты перерисуеваемых ячеек и их цвета
			//public uint NumberOfChangedCells;		- количество изменившихся ячеек на экране колторые надо перерисовать в следующий раз

			//todo не перерисовывать если на первоначальном экране ячейка такого же цвета
			if (_data.ChWorld[x, y] == 0)
			{
				// В этой клетке еще не было изменений после последнего рисования
				_data.ChangedCells[_data.NumberOfChangedCells] = new ChangedCell
				{
					X = x,
					Y = y,
					Color = color
				};
				_data.NumberOfChangedCells++;
				_data.ChWorld[x, y] = _data.NumberOfChangedCells; // сюда записываем +1 чтобы 0 не записывать
			}
			else
			{
				// В этой клетке уже были изменения после последнего рисования
				_data.ChangedCells[_data.ChWorld[x, y] - 1].Color = color;
				//_data.ChangedCells[_data.ChWorld[x, y] - 1] = new ChangedCell
				//{
				//	X = x,
				//	Y = y,
				//	Color = color
				//};
			}
		}

		#region Random
		public Direction GetRandomDirection()
		{
			return (Direction)_rnd.Next(0, 8);
		}

		public byte GetRandomBotCode()
		{
			return (byte)_rnd.Next(_data.MaxCode + 1);
		}

		public Color GetRandomColor()
		{
			return  Color.FromArgb(_rnd.Next(256), _rnd.Next(256), _rnd.Next(256));
		}

		public int GetRandomBotCodeIndex()
		{
			return _rnd.Next(_data.CodeLength);
		}

		public bool Mutation()
		{
			return _data.Mutation && _rnd.Next(100) < _data.MutationProbabilityPercent;
		}

		public Point GetRandomFreeCell()
		{
			int x;
			int y;
			var i = 0;

			do
			{
				x = _rnd.Next(0, _data.WorldWidth);
				y = _rnd.Next(0, _data.WorldHeight);
			}
			while (CellIsBusy(x, y) && ++i < 100);

			return new Point(x, y);
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

		private bool CellIsBusy(int x, int y)
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
