using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WindowsFormsApp1.Enums
{
	public static class CGen
	{
		public const byte RotateAbsolute = 1;
		public const byte RotateRelative = 2;
		public const byte Photosynthesis = 3;
		public const byte StepForward1 = 4;
		public const byte StepForward2 = 5;
		public const byte EatForward1 = 6;
		public const byte EatForward2 = 7;
		public const byte LookForward1 = 8;
		public const byte LookForward2 = 9;
		public const byte LookAround = 10;
		public const byte RotateRandom = 11;
		public const byte AlignHorizontaly = 12;

		public static HashSet<byte> CompleteCommands = new HashSet<byte>()
			{
			CGen.Photosynthesis,
			CGen.StepForward1,
			CGen.StepForward2,
			CGen.EatForward1,
			CGen.EatForward2,
			CGen.LookAround
			};

		public static HashSet<byte> DirectionCommands = new HashSet<byte>()
			{
			CGen.RotateAbsolute,
			CGen.RotateRelative,
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

	}

	public static class CEv
	{
		public const byte RotateRelative = 1;
		public const byte RotateRelativeContact = 2;
		public const byte RotateBackward = 3;
		public const byte RotateBackwardContact = 4;
		public const byte LookAround = 5;
		public const byte StepRelative = 6;
		public const byte StepRelativeContact = 7;
		public const byte StepBackward = 8;
		public const byte StepBackwardContact = 9;
		public const byte EatForward = 10;

		public static HashSet<byte> CompleteCommands = new HashSet<byte>()
			{
			CEv.LookAround,
			CEv.StepRelative,
			CEv.StepRelativeContact,
			CEv.StepBackward,
			CEv.StepBackwardContact,
			CEv.EatForward
			};

		public static HashSet<byte> DirectionCommands = new HashSet<byte>()
			{
			CEv.RotateRelative,
			CEv.RotateRelativeContact,
			CEv.StepRelative,
			CEv.StepRelativeContact,
			};

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
