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
		public static (double, double)[] Directions1 = new (double, double)[NumberOfDirections];
		public static (double, double)[] Directions2 = new (double, double)[NumberOfDirections];
		public static int[] DirectionsOpposite = new int[NumberOfDirections];

		public static (int, int)[] NearbyCells = new (int, int)[8]
			{ (-1, -1), (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0) };

		public static int[] NearbyCellsDirection = new int[8] { 56, 0, 8, 16, 24, 32, 40, 48 };


		//							y
		//							^
		//							|
		//							|
		//							32
		//							|
		//					40		|
		//							|		24
		//							|
		//							|
		//  ---------48-------------------------16------------> x
		//							|
		//							|
		//					56		|       8
		//							|
		//							0
		//							|

		static Dir()
		{
			for (var i = 0; i < NumberOfDirections; i++)
			{
				//var x = Math.Round(Math.Sin(i * 2 * Math.PI / NumberOfDirections));
				//var y = Math.Round(-Math.Cos(i * 2 * Math.PI / NumberOfDirections));
				var angle = i * 2 * Math.PI / NumberOfDirections;
				var x = Math.Sin(angle);
				var y = -Math.Cos(angle);
				Directions1[i] = (x, y);
				//var dir1 = Dir.Round(Math.Atan2(-x, y) * NumberOfDirections / 2 / Math.PI + NumberOfDirections / 2);
				Directions2[i] = (1.42 * x, 1.42 * y);
				DirectionsOpposite[i] = (i + NumberOfDirections / 2) % NumberOfDirections;
			}
		}

		public static int GetDirDiff(int dir1, int dir2)
		{
			var diff = dir1 - dir2;

			//while (dir1 < 0) dir1 += NumberOfDirections;
			//while (dir1 >= NumberOfDirections) dir1 -= NumberOfDirections;
			//while (dir2 < 0) dir2 += NumberOfDirections;
			//while (dir2 >= NumberOfDirections) dir2 -= NumberOfDirections;
			
			if (diff < 0) diff = -diff;
			while (diff >= NumberOfDirections) diff -= NumberOfDirections;
			if (diff > 32) diff = NumberOfDirections - diff;
			return diff;
		}

		public static int GetOppositeDirection(int dir)
		{
			return DirectionsOpposite[dir];
		}

		public static (double, double) GetDeltaDirection(int dir)
		{
			return Directions1[dir];
		}

		public static (double, double) GetDeltaDirection2(int dir)
		{
			return Directions2[dir];
		}

		public static int Round(double val)
		{
			return val >= 0 ? (int)(val + 0.5f) : -(int)(0.5f - val);
		}

		public static string GetDirectionStringFromCode(int dir)
		{
			return ((int)(360 * (dir % NumberOfDirections) / NumberOfDirections)).ToString();
		}
	}
}
