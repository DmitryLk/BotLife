﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WindowsFormsApp1
{
	public enum Direction
	{
		Up = 0,
		UpRight = 1,
		Right = 2,
		DownRight = 3,
		Down = 4,
		DownLeft = 5,
		Left = 6,
		UpLeft = 7
	}


	public enum Content
	{
		Empty = 0,
		Wall = 1,
		Organic = 2,
		Relative = 3,
		Alien = 4,
		Poison = 5,
		Food = 6,
		Edge = 7
	}

	public class Bot
	{
		public int X;
		public int Y;
		public int OldX;
		public int OldY;
		public bool Moved;              // Сдвинулся или нет
		public bool NoDrawed;           // Еще ни разу не рисовался

		private int _vx;
		private int _vy;
		private Direction _dir;         // Направление бота
		private byte[] _code;
		private int _pointer;
		private int _maxX;
		private int _maxY;

		public Bot(Random rnd, int maxX, int maxY)
		{
			// Координаты
			X = rnd.Next(0, maxX);
			Y = rnd.Next(0, maxY);
			OldX = X;
			OldY = Y;

			// Направление бота
			_dir = (Direction)rnd.Next(0, 8);

			// Гены
			_code = new byte[64];
			for (var i = 0; i < 64; i++)
			{
				_code[i] = (byte)rnd.Next(64);
			}
			_pointer = 0;

			// Скорость?
			//do
			//{
			//	_vx = rnd.Next(-1, 2);
			//	_vy = rnd.Next(-1, 2);
			//}
			//while (_vx == 0 && _vy == 0);

			if (rnd.Next(100) > 97)
			{
				_vx = rnd.Next(-1, 2);
				_vy = rnd.Next(-1, 2);
			}
			else
			{
				_vx = 0;
				_vy = 0;
			}

			_maxX = maxX;
			_maxY = maxY;

			Moved = false;
			NoDrawed = true;
		}


		private Content GetContent(int x, int y)
		{
			return Content.Wall;
		}

		private (int dX, int dY) GetDeltaDirection(Direction dir1, Direction dir2)
		{
			return (((int)dir1 + (int)dir2) % 8) switch
			{
				0 => (0, -1),
				1 => (1, -1),
				2 => (1, 0),
				3 => (1, 1),
				4 => (0, 1),
				5 => (-1, 1),
				6 => (-1, 0),
				7 => (-1, -1),
				_ => throw new Exception("return (((int)dir1 + (int)dir2) % 8) switch"),
			};
		}



		public void Live()
		{
			// Получаем команду
			var cmdCode = _code[_pointer];

			// Выполняем команду
			switch (cmdCode)
			{
				//Up = 0,
				//UpRight = 1,
				//Right = 2,
				//DownRight = 3,
				//Down = 4,
				//DownLeft = 5,
				//Left = 6,
				//UpLeft = 7


				case 0: //Движение вперед
					var (dX, dy) = GetDeltaDirection(_dir, Direction.Up);
					var nX = X + dX;
					var nY = Y + dy;
					var cont = GetContent(nX, nY);
					Move();
					break;

				case 1: //Движение вперед-вправо
					Move();
					break;

				case 25:
					_pointer++;
					break;

				default:
					new Exception("switch cmd");
					break;
			};

		}

		public void Move()
		{
			var newX = X + _vx;
			if (newX >= _maxX)
			{
				newX = _maxX - 1;
				_vx = -_vx;
			}
			if (newX < 0)
			{
				newX = 0;
				_vx = -_vx;
			}


			var newY = Y + _vy;
			if (newY >= _maxY)
			{
				newY = _maxY - 1;
				_vy = -_vy;
			}
			if (newY < 0)
			{
				newY = 0;
				_vy = -_vy;
			}

			Moved = X != newX || Y != newY;

			OldX = X;
			OldY = Y;
			X = newX;
			Y = newY;
		}
	}
}

/*
ДВИЖЕНИЕ
Алгоритм:
1. Суммируем направление бота и движения
2. По полученному суммарному направлению вычисляем дельта координаты клетки на которую предполагается передвинуться


ВОЗРАСТ = ЗДОРОВЬЕ = ЭНЕРГИЯ
У бота есть здоровье и каждый ход оно уменьшается на единицу.
Если дойдет до 0 то бот умирает.
Стоит ограничение на здоровье и бот не может иметь более 90 единиц здоровья 

ПИТАНИЕ
Боту нужно найти еду которая добавляет 10 к здоровью.
Еду можно съесть зайдя на клетку с едой или схватив еду из соседней клетки

СТЕНА
бот упирается в стену

ЯД
если бот зайдет на клетку с ядом то он погибнет
Если вначале схватить яд то он преобразуется в еду

БОТ
типа стена
Боты отличают чужого от своего, если код-геном отличается более, чем на один байт. 

РАЗМНОЖЕНИЕ
1. из 64 осталось 8 ботов. каждому добавляем 7 копий в любом месте, одна из которых мутант.
2. бот как набрал достаточно энергии создает свою копию с каким-то процентом мутации.
Когда отпочковывается новый бот, он встраивается в цепочку ботов перед предком.
боты переполнены энергией и они должны отпочкавать потомка, но свободного места нет и они погибают

В стандартном режиме цвет зависит от способа получения энергии. Любители фотосинтеза зеленеют, любители «минералов» синеют, а мясоеды краснеют.
У всеядных может быть промежуточный цвет.

В режиме отображения энергии, чем больше энергии, тем бот краснее, чем меньше энергии, тем бот желтее.


		// КОМАНДЫ 0-63									Завершающая	Указатель
		// 0-7		сделать шаг							З			*	Направление зависит от числа и куда повернут бот в текущий момент.
		// 8-15		схватить еду или нейтрализовать яд	З			*
		// 16-23	посмотреть							Н			*	Бот остается на месте
		// 24-31	поворот								Н			1
		// 32-63	безусловный переход					Н

		// 25 - Фотосинтез		1						З

		поделиться энергией
		сколько энергии?

		// Незавершающая команда может выполняться до 10 раз после чего управлдение будет передано другому боту
		// Неизвестная команда является безусловным переходом



		// (*)Указатель команды перемещается в зависимости от того что было в этом направлении
		// яд - 1
		// стена - 2
		// бот - 3
		// еда - 4
		// пусто - 5

		// пусто - 2
		// стена - 3
		// органика - 4
		// бот - 5
		// свой - 6
		// чужой - 7


		// Поворот бота
		// 24 - 0
		// 25 - 45
		// 26 - 90
		// 27 - 135
		// 28 - 180
		// 29 - 225
		// 30 - 270
		// 31 - 315


 
 
 */
