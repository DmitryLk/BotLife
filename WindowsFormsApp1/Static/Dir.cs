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

		public static (int, int)[] NearbyCells1 = new (int, int)[8]
			{ (-1, -1), (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0) };

		public static (int, int)[] NearbyCells2 = new (int, int)[24]
			{   (-1, -1), (0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0),
				(-2, -2), (-1, -2), (0, -2), (1, -2), (2, -2), (2, -1), (2, 0), (2, 1),
				(2, 2), (1, 2), (0, 2), (-1, 2), (-2, 2), (-2, 1), (-2, 0), (-2, -1) };

		public static int[] NearbyCellsDirection = new int[8] { 56, 0, 8, 16, 24, 32, 40, 48 };


		//							y
		//							^
		//							|
		//							|
		//							32
		//							|
		//					40		|
		//							|		24
		//				  (-1,1)  (0,1)	 (1,1)
		//							|
		//  ---------48---(-1,0)---------(1,0)---16------------> x
		//							|
		//				(-1,-1)   (0,-1)  (1,-1)
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

		public static (int, int, int, int)[][] TouchedCells = new (int, int, int, int)[8][] {
			new (int, int, int, int)[] { (-2, 0, 56, 24), (-2, -1, 48, 16), (-2, -2, 40, 8), (-1, -2, 32, 0), (0, -2, 24, 56)},		// (-1, -1)
			new (int, int, int, int)[] {(-1, -2, 40, 8), (0, -2, 32, 0), (1, -2, 24, 56) },											// (0, -1)
			new (int, int, int, int)[] {(0, -2, 40, 8), (1, -2, 32, 0), (2, -2, 24, 56), (2, -1, 16, 48), (2, 0, 8, 40)},			// (1, -1)
			new (int, int, int, int)[] {(2, -1, 24, 56), (2, 0, 16, 48), (2, 1, 8, 40) },											// (1, 0)
			new (int, int, int, int)[] {(2, 0, 24, 56), (2, 1, 16, 48), (2, 2, 8, 40), (1, 2, 0, 32), (0, 2, 56, 24) },				// (1, 1)
			new (int, int, int, int)[] {(-1, 2, 8, 40), (0, 2, 0, 32), (1, 2, 56, 24)},												// (0, 1)
			new (int, int, int, int)[] {(0, 2, 8, 40), (-1, 2, 0, 32), (-2, 2, 56, 24), (-2, 1, 48, 16), (-2, 0, 40, 8) },			// (-1, 1)
			new (int, int, int, int)[] { (-2, -1, 56, 24), (-2, 0, 48, 16), (-2, 1, 40, 8) }										// (-1, 0)
		};

		public static (int X, int Y, int Dir, int DirOp)[] GetTouchedCells(int dX, int dY)
		{
			return (dX, dY) switch
			{
				(-1, -1) => TouchedCells[0],
				(0, -1) => TouchedCells[1],
				(1, -1) => TouchedCells[2],
				(1, 0) => TouchedCells[3],
				(1, 1) => TouchedCells[4],
				(0, 1) => TouchedCells[5],
				(-1, 1) => TouchedCells[6],
				(-1, 0) => TouchedCells[7],
				_ => throw new Exception()
			};
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
			if (diff > NumberOfDirections / 2) diff = NumberOfDirections - diff;
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
