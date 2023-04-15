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
        public Direction Dir;         // Направление бота

		protected uint _num;         // Номер бота (остается постоянным)
		protected int _age;
		private int _vx;
		private int _vy;

		public Bot(Point p, Direction dir, uint botNumber, uint botIndex, int en, int vx, int vy)
		{
			Dir = dir;
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
	}
}

