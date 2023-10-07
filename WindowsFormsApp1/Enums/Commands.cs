using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WindowsFormsApp1.Enums
{
    public static class Cmd
    {
        // Rotate
        public const byte RotateAbsolute = 1;
        public const byte RotateRelative = 2;
        public const byte RotateRelativeContact = 3;
        public const byte RotateBackward = 4;
        public const byte RotateBackwardContact = 5;
        public const byte RotateRandom = 6;
        public const byte AlignHorizontaly = 7;
        // Step
        public const byte StepForward1 = 10;
        public const byte StepForward2 = 11;
        public const byte StepRelative = 12;
        public const byte StepRelativeContact = 13;
        public const byte StepBackward = 14;
        public const byte StepBackwardContact = 15;
        // Eat
        public const byte EatForward1 = 20;
        public const byte EatForward2 = 21;
        // Look
        public const byte LookForward1 = 30;
        public const byte LookForward2 = 31;
        public const byte LookAround = 32;
        // Other
        public const byte Photosynthesis = 19;


        public static HashSet<byte> GeneralCommands = new HashSet<byte>()
        {
            Cmd.RotateAbsolute,
            Cmd.RotateRelative,
            Cmd.Photosynthesis,
            Cmd.StepForward1,
            Cmd.StepForward2,
            Cmd.EatForward1,
            Cmd.EatForward2,
            Cmd.LookForward1,
            Cmd.LookForward2,
            Cmd.LookAround,
            Cmd.RotateRandom,
            Cmd.AlignHorizontaly,
        };

        public static HashSet<byte> EventCommands = new HashSet<byte>()
        {
            Cmd.RotateRelative,
            Cmd.RotateRelativeContact,
            Cmd.RotateBackward,
            Cmd.RotateBackwardContact,
            Cmd.LookAround,
            Cmd.StepRelative,
            Cmd.StepRelativeContact,
            Cmd.StepBackward,
            Cmd.StepBackwardContact,
            Cmd.EatForward1,
        };

        public static HashSet<byte> CompleteCommands = new HashSet<byte>()
            {
            Cmd.Photosynthesis,
            Cmd.StepForward1,
            Cmd.StepForward2,
            Cmd.StepRelative,
            Cmd.StepRelativeContact,
            Cmd.StepBackward,
            Cmd.StepBackwardContact,
            Cmd.EatForward1,
            Cmd.EatForward2,
            Cmd.LookAround
            };

        public static HashSet<byte> DirectionCommands = new HashSet<byte>()
            {
            Cmd.RotateAbsolute,
            Cmd.RotateRelative,
            Cmd.RotateRelativeContact,
            Cmd.StepRelative,
            Cmd.StepRelativeContact,
            };

        //	КОМАНДЫ ДЕЙСТВИЙ ОБЫЧНЫХ
        //+поворот абсолютно
        //+поворот относительно
        //+фотосинтез
        //+шаг вперед
        //+укусить впереди
        //+посмотреть вперед
        //посмотреть вокруг
        //команда периодической смены направления при определенной вероятности направление менятеся случайно или нет
        //выравнится по горизонтали
        //случайное направление , допустим цифра 8 (0-7 это обычные направления)
        //команда перехода на случайное количество шагов в программе

        //EatBackward = 6,
        //EatReverse = 7,
        //StepBackward = 5,
        //RotateRight = 4,
        //RotateUp = 5,
        //RotateDown = 6,
        //RotateLeftUp = 7,
        //RotateRightUp = 8,

        //КОМАНДЫ ДЕЙСТВИЙ НА СОБЫТИЯ
        //поворот относительно
        //поворот относительно раздражителя
        //? поворот на 180
        //? поворт влево
        //? поворот вправо
        //посмотреть вокруг
        //шаг относительно
        //шаг относительно раздражителя
        //? шаг влево
        //? шаг вправо
        //? шаг назад
        //укусить впереди
    }
}
