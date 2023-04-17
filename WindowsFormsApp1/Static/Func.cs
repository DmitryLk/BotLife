﻿using System;
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
using WindowsFormsApp1.GameLogic;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace WindowsFormsApp1.Static
{
    public static class Func
    {
        private static Random _rnd = new Random(Guid.NewGuid().GetHashCode());



        // Создать нового бота можно только через этот метод им пользуется Seeder и Reproduction
        public static void CreateNewBot(int x, int y, uint botIndex, Genom genom)
        {
            var dir = Dir.GetRandomDirection();
            var en = Data.InitialBotEnergy;
            Data.TotalEnergy += Data.InitialBotEnergy;
            var pointer = 0;
            var botNumber = ++Data.MaxBotNumber;

            genom.Bots++;
            var bot = new Bot1(x,y, dir, botNumber, botIndex, en, genom, pointer);

            Data.Bots[botIndex] = bot;

            Data.World[x, y] = botIndex;
            ChangeCell(x, y, genom.Color);
        }


        // Список измененных ячеек для последующей отрисовки
        public static void ChangeCell(int x, int y, Color? color)
        {
            //public uint[,] ChWorld;				- по координатам можно определить перерисовывать ли эту ячейку, там записам индекс массива ChangedCell
            //public ChangedCell[] ChangedCells;	- массив перерисовки, в нем перечислены координаты перерисуеваемых ячеек и их цвета
            //public uint NumberOfChangedCells;		- количество изменившихся ячеек на экране колторые надо перерисовать в следующий раз

            if (Data.ChWorld[x, y] == 0)
            {
                // В этой клетке еще не было изменений после последнего рисования
                Data.ChangedCells[Data.NumberOfChangedCells] = new ChangedCell
                {
                    X = x,
                    Y = y,
                    Color = color
                };
                Data.NumberOfChangedCells++;
                Data.ChWorld[x, y] = Data.NumberOfChangedCells; // сюда записываем +1 чтобы 0 не записывать
            }
            else
            {
                // В этой клетке уже были изменения после последнего рисования
                Data.ChangedCells[Data.ChWorld[x, y] - 1].Color = color;
                //_data.ChangedCells[_data.ChWorld[x, y] - 1] = new ChangedCell
                //{
                //	X = x,
                //	Y = y,
                //	Color = color
                //};
            }
        }

        #region Random

        public static byte GetRandomBotCode()
        {
            return (byte)_rnd.Next(Data.MaxCode + 1);
        }

        public static Color GetRandomColor()
        {
            return Color.FromArgb(_rnd.Next(256), _rnd.Next(256), _rnd.Next(256));
        }

        public static int GetRandomBotCodeIndex()
        {
            return _rnd.Next(Data.GenomLength);
        }

        public static bool Mutation()
        {
            return Data.Mutation && _rnd.NextDouble() * 100 < Data.MutationProbabilityPercent;
        }

        public static (int x, int y) GetRandomFreeCell()
        {
            int x;
            int y;
            var i = 0;

            do
            {
                x = _rnd.Next(0, Data.WorldWidth);
                y = _rnd.Next(0, Data.WorldHeight);
            }
            while (CellIsBusy(x, y) && ++i < 1000);

            return (x, y);
        }

        public static (int, int) GetRandomSpeed()
        {
            //do
            //{
            //	_vx = rnd.Next(-1, 2);
            //	_vy = rnd.Next(-1, 2);
            //}
            //while (_vx == 0 && _vy == 0);

            if (_rnd.Next(100) > 97)
            {
                return (_rnd.Next(-1, 2), _rnd.Next(-1, 2));
            }
            return (0, 0);
        }

        private static bool CellIsBusy(int x, int y)
        {
            return Data.World[x, y] != 0;
        }


        private static unsafe Guid GuidRandomCachedInstance()
        {
            var bytes = stackalloc byte[16];
            var dst = bytes;

            var random = ThreadSafeRandom.ObtainThreadStaticRandom();
            for (var i = 0; i < 4; i++)
            {
                *(int*)dst = random.Next();
                dst += 4;
            }

            return *(Guid*)bytes;
        }

        #endregion
    }

    internal static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random random;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Random ObtainThreadStaticRandom() => ObtainRandom();

        private static Random ObtainRandom()
        {
            return random ?? (random = new Random(Guid.NewGuid().GetHashCode()));
        }
    }
}
