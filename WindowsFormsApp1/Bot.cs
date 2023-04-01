using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
	public class Bot
	{
		public int X;
		public int Y;
		public int Vx;
		public int Vy;
		public bool Moved;

		public Bot(Random rnd, int maxX, int maxY)
		{
			X = rnd.Next(1, maxX);
			Y = rnd.Next(1, maxY);
			Vx = rnd.Next(-1, 2);
			Vy = rnd.Next(-1, 2);
			Moved = false;
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

			X = newX;
			Y = newY;
		}
	}
}
