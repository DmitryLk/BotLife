using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using Point = WindowsFormsApp1.Dto.Point;

namespace WindowsFormsApp1.GameLogic
{
	public abstract class Bot
	{
		public Point P;
		public Point Old;
		public int Energy;
		public uint Index;         // Индекс бота (может меняться)

		protected GameData _data;
		protected Func _func;
		protected Direction _dir;         // Направление бота
		protected uint _num;         // Номер бота (остается постоянным)
		protected int _age;
		private int _vx;
		private int _vy;

		public Bot(GameData data, Func func, Point p, Direction dir, uint botNumber, uint botIndex, int en, int vx, int vy)
		{
			_data = data;
			_func = func;
			_dir = dir;
			_num = botNumber;
			Index = botIndex;
			Energy = en;
			_age = 0;

			_vx = vx;
			_vy = vy;
			P = p;
			Old = new Point(p.X, p.Y);
		}

		public abstract void Step();

		public abstract void Death();

		protected abstract void Reproduction();

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

