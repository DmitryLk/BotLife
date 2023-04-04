using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WindowsFormsApp1.Enums;

namespace WindowsFormsApp1.GameLogic
{
    public class Bot
    {
        public Point P;
        public Point Old;
        public bool Moved;              // Сдвинулся или нет

        public byte[] Code;
        public int Pointer;
        public Direction Dir;         // Направление бота


        public int Vx;
        public int Vy;
        public int MaxX;
        public int MaxY;

        public Bot(int maxX, int maxY)
        {
            MaxX = maxX;
            MaxY = maxY;

            Moved = false;
        }


        public void Move()
        {
            var newX = X + Vx;
            if (newX >= MaxX)
            {
                newX = MaxX - 1;
                Vx = -Vx;
            }
            if (newX < 0)
            {
                newX = 0;
                Vx = -Vx;
            }


            var newY = Y + Vy;
            if (newY >= MaxY)
            {
                newY = MaxY - 1;
                Vy = -Vy;
            }
            if (newY < 0)
            {
                newY = 0;
                Vy = -Vy;
            }

            Moved = X != newX || Y != newY;

            OldX = X;
            OldY = Y;
            X = newX;
            Y = newY;
        }
    }
}

