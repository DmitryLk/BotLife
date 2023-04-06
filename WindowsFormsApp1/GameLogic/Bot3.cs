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
	// Бот с примитивной нейросетью (берем все показания сенсоров и в зависимости от случайных весов делаем действия)
	public class Bot3 : Bot
	{


		public Bot3(WorldData data, Point p, Direction dir, uint botNumber, int en, int vx, int vy)
			: base(data, p, dir, botNumber, en, vx, vy)
		{
		}

		public override void Step()
		{
		}

		public bool IsRelative(Bot b2)
		{
			return true;
		}
	}
}

