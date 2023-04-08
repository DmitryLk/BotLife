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
	public abstract class Bot
	{
		public Point P;
		public Point Old;
		protected WorldData _data;
		protected Direction _dir;         // Направление бота
		protected uint _num;         // Номер бота (остается постоянным)
		protected uint _ind;         // Индекс бота (может меняться)
		protected int _energy;
		protected int _age;
		private int _vx;
		private int _vy;

		public Bot(WorldData data, Point p, Direction dir, uint botNumber, int en, int vx, int vy)
		{
			_data = data;
			P = p;
			_dir = dir;
			_num = botNumber;
			_ind = botNumber;
			_energy = en;
			_age = 0;
			
			_vx = vx;
			_vy = vy;
			Old = new Point { X = p.X, Y = p.Y };
		}

		public abstract void Step();

		public void Move()
		{
			var newX = P.X + _vx;
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


			var newY = P.Y + _vy;
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

			Old.X = P.X;
			Old.Y = P.Y;
			P.X = newX;
			P.Y = newY;
		}
	}
}

