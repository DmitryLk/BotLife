using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WindowsFormsApp1.Enums;

namespace WindowsFormsApp1.GameLogic
{
	public abstract class Bot
	{
		public bool NoDrawed = true;           // Еще ни разу не рисовался
		protected WorldData _data;
		protected Point _p;
		protected Point _old;
		protected Direction _dir;         // Направление бота
		protected uint _num;         // Номер бота
		private int _vx;
		private int _vy;
		private bool _moved;              // Сдвинулся или нет

		public Bot(WorldData data, Point p, Direction dir, uint botNumber, int vx, int vy)
		{
			_data = data;
			_moved = false;
			_p = p;
			_dir = dir;
			_num = botNumber;
			_vx = vx;
			_vy = vy;
			_old = new Point(p.X, p.Y);
		}


		public abstract void Step();

		public bool Moved()
		{ 
			return _moved;
		}

		public void Move()
		{
			var newX = _p.X + _vx;
			if (newX >= _data.WorldWidth)
			{
				newX = _data.WorldWidth - 1;
				_vx = -_vx;
			}
			if (newX < 0)
			{
				newX = 0;
				_vx = -_vx;
			}


			var newY = _p.Y + _vy;
			if (newY >= _data.WorldHeight)
			{
				newY = _data.WorldHeight - 1;
				_vy = -_vy;
			}
			if (newY < 0)
			{
				newY = 0;
				_vy = -_vy;
			}

			_moved = _p.X != newX || _p.Y != newY;

			_old.X = _p.X;
			_old.Y = _p.Y;
			_p.X = newX;
			_p.Y = newY;
		}
	}
}

