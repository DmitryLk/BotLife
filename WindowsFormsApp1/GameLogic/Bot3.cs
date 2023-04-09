﻿using System;
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
	// Бот с примитивной нейросетью (берем все показания сенсоров и в зависимости от случайных весов делаем действия)
	public class Bot3 : Bot
	{


		public Bot3(GameData data, Func func, Point p, Direction dir, uint botNumber, uint botIndex, int en, int vx, int vy, Color color)
			: base(data, func, p, dir, botNumber, botIndex, en, color, vx, vy)
		{
		}

		public override void Death()
		{
		}
		protected override void Reproduction()
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
