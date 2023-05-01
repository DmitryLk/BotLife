using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace WindowsFormsApp1.Static
{
	public static class Dir
	{
		public const int NumberOfDirections = 64;
		public static (double, double)[] Directions1 = new (double, double)[64];
		public static (double, double)[] Directions2 = new (double, double)[64];

		public static (int, int)[] NearbyCells = new (int, int)[8]
			{ (-1, -1), (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0) };



		static Dir()
		{
			for (var i = 0; i < NumberOfDirections; i++)
			{
				//var x = Math.Round(Math.Sin(i * 2 * Math.PI / NumberOfDirections));
				//var y = Math.Round(-Math.Cos(i * 2 * Math.PI / NumberOfDirections));
				var x = Math.Sin(i * 2 * Math.PI / NumberOfDirections);
				var y = -Math.Cos(i * 2 * Math.PI / NumberOfDirections);
				Directions1[i] = (x, y);
				Directions2[i] = (1.42 * x, 1.42 * y);
			}
		}



		public static (double, double) GetDeltaDirection(int dir)
		{
			return Directions1[dir];
		}

		public static (double, double) GetDeltaDirection2(int dir)
		{
			return Directions2[dir];
		}

		public static int GetDirectionFromCodeRelative(int curdir, byte code)
		{
			//if (code >= 28 && code <= 30)
			//{
			//	return GetRandomDirection();
			//}
			return (curdir + code) % NumberOfDirections;
		}

		public static int GetRandomDirection()
		{
			return Func.Rnd.Next(NumberOfDirections);
		}

		public static int Round(double val)
		{
			return val >= 0 ? (int)(val + 0.5f) : -(int)(0.5f - val);
		}


		public static int GetDirectionFromCodeAbsolute(byte code)
		{
			//if (code >= 28 && code <= 30)
			//{
			//	return GetRandomDirection();
			//}
			return code % NumberOfDirections;
		}

		public static string GetDirectionStringFromCode(int dir)
		{
			return ((int)(360 * (dir % NumberOfDirections) / NumberOfDirections)).ToString();
		}
	}
}
