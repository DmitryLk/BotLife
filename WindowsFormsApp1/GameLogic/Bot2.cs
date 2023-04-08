using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;

namespace WindowsFormsApp1.GameLogic
{
	// Бот с первой программы foo52
	public class Bot2 : Bot
	{
		private byte[] _code;
		private int _pointer;

		public Bot2(WorldData data, Func func, Point p, Direction dir, uint botNumber, int en, int vx, int vy)
			: base(data, func, p, dir, botNumber, en, vx, vy)

		{
		}

		public bool IsRelative(Bot b2)
		{
			return true;
		}

		public byte GetNextCommand()
		{
			return _code[_pointer];
		}

		public override void Death()
		{ 
		}

		protected override void Reproduction()
		{ 
		}

	public override void Step()
		{
			// Получаем команду
			var cmdCode = GetNextCommand();

			// Выполняем команду
			switch (cmdCode)
			{
				// 0-7		движение
				// 8-15		схватить еду или нейтрализовать яд
				// 16-23	посмотреть
				// 24-31	поворот
				// 32-63	безусловный переход
				// 25       фотосинтез

				//Up = 0,
				//UpRight = 1,
				//Right = 2,
				//DownRight = 3,
				//Down = 4,
				//DownLeft = 5,
				//Left = 6,
				//UpLeft = 7


				case 0: //Движение вперед
					TryToMove(Direction.Up);
					break;

				case 1: //Движение вперед-вправо
					TryToMove(Direction.UpRight);
					break;



				case 25:
					_pointer++;
					break;

				default:
					throw new Exception("switch cmd");
					break;
			};

		}


		private void TryToMove(Direction dir)
		{
			// ДВИЖЕНИЕ
			// Алгоритм:
			// 1. Суммируем направление бота и движения
			// 2. По полученному суммарному направлению вычисляем дельта координаты клетки на которую предполагается передвинуться
			// 3. Узнаем что находится на этой клетке
			// 4.1. Переход на клетку если там free poison
			// 4.2. Не переход на клетку если там  wall edge
			// 4.3. Непонятно переход на клетку если там  food mineral organic

			// 1. Узнаем координаты предполагаемого перемещения
			var (nX, nY) = GetCoordinatesByDirection(dir);


			//var refContent = _data.GetRefContent(nX, nY);
			var refContent = RefContent.Free;

			// надо определить родственник ли бот

			ChangePointerByCellContent(refContent);
		}

		public void ChangePointerByCellContent(RefContent cont)
		{
			//смещение условного перехода 2-пусто  3-стена  4-органика 5-бот 6-родня
			_pointer += cont switch
			{
				RefContent.Free => 2,
				RefContent.Wall => 3,
				RefContent.Organic => 4,
			};

		}

		private (int nX, int nY) GetCoordinatesByDirection(Direction dir)
		{
			var (dX, dY) = dir switch
			{
				Direction.Up => (0, -1),
				Direction.UpRight => (1, -1),
				Direction.Right => (1, 0),
				Direction.DownRight => (1, 1),
				Direction.Down => (0, 1),
				Direction.DownLeft => (-1, 1),
				Direction.Left => (-1, 0),
				Direction.UpLeft => (-1, -1),
				_ => throw new Exception("var (dX, dy) = dir switch"),
			};

			var nX = P.X + dX;
			var nY = P.Y + dY;

			// Проверка перехода сквозь экран
			if (!_data.LeftRightEdge)
			{
				if (nX < 0) nX += _data.WorldWidth;
				if (nX >= _data.WorldWidth) nX -= _data.WorldWidth;
			}
			if (!_data.UpDownEdge)
			{
				if (nY < 0) nY += _data.WorldHeight;
				if (nY >= _data.WorldHeight) nY -= _data.WorldHeight;
			}

			return (nX, nY);
		}
	}
}

