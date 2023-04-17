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
        public const int NumberOfDirections = 8;
        public static (double, double)[] Directions = new (double, double)[64];
        private static Random _rnd = new Random(Guid.NewGuid().GetHashCode());


        static Dir()
        {
            for (var i = 0; i < NumberOfDirections; i++)
            {
                var x = Math.Sin(i * 2 * Math.PI / NumberOfDirections);
                var y = -Math.Cos(i * 2 * Math.PI / NumberOfDirections);
                Directions[i] = (x, y);
            }
        }



        public static (double, double) GetDeltaDirection(int dir)
        {
            return Directions[dir];
        }

        public static int DirectionPlus(int dir1, int dir2)
        {
            return (dir1 + dir2) % NumberOfDirections;
        }

        public static int GetRandomDirection()
        {
            return _rnd.Next(0, NumberOfDirections);
        }

        public static int GetRandomCardinalDirection()
        {
            return _rnd.Next(0, 8) * NumberOfDirections / 8;
        }

        public static int GetDirectionFromCode(byte code)
        {
            return code % NumberOfDirections;
        }

        public static string GetDirectionStringFromCode(int dir)
        {
            return ((int)(360 * (dir % NumberOfDirections) / NumberOfDirections)).ToString();
        }
    }
}
