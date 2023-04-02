using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
	public enum Direction
	{
		Unknown = 0,
		Up = 1,
		UpRight = 2,
		Right = 3,
		DownRight = 4,
		Down = 5,
		DownLeft = 6,
		Left = 7,
		UpLeft = 8
	}

	public class Bot
	{
		public int X;
		public int Y;
		public int OldX;
		public int OldY;
		public int Vx;
		public int Vy;
		public bool Moved;              // Сдвинулся или нет
		public bool NoDrawed;           // Еще ни разу не рисовался
		public Direction Direction;         // Направление бота
		public byte[] Genes;
		public int GenPointer;

		public int MaxX;
		public int MaxY;

		public Bot(Random rnd, int maxX, int maxY)
		{
			// Координаты
			X = rnd.Next(1, maxX);
			Y = rnd.Next(1, maxY);
			OldX = X;
			OldY = Y;

			// Направление бота
			Direction = (Direction)rnd.Next(1, 9);

			// Гены
			Genes = new byte[64];
			for (var i = 0; i < 64; i++)
			{
				Genes[i] = (byte)rnd.Next(65);
			}
			GenPointer = 1;

			// Скорость?
			//Vx = rnd.Next(-1, 2);
			//Vy = rnd.Next(-1, 2);
			if (rnd.Next(100) > 97)
			{
				Vx = rnd.Next(-1, 2);
				Vy = rnd.Next(-1, 2);
			}
			else
			{
				Vx = 0;
				Vy = 0;
			}

			MaxX = maxX;
			MaxY = maxY;

			Moved = false;
			NoDrawed = true;
		}

		public void Live()
		{
			var cmd = Genes[GenPointer + 1];

			switch (cmd)
			{
				case 0: 
					Move(); 
					break;
				
				default: 
					new Exception("switch cmd"); 
					break;
			};

		}

		public void Move()
		{
			var newX = X + Vx;
			if (newX >= 500)
			{
				newX = 499;
				Vx = -Vx;
			}
			if (newX < 0)
			{
				newX = 0;
				Vx = -Vx;
			}


			var newY = Y + Vy;
			if (newY >= 500)
			{
				newY = 499;
				Vy = -Vy;
			}
			if (newY < 0)
			{
				newY = 0;
				Vy = -Vy;
			}

			Moved = X != newX || Y != newY;

			OldX = X;
			OldY = Y;
			X = newX;
			Y = newY;
		}
	}
}
