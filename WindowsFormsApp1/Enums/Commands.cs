using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.Pkcs;
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
		public const byte EatContact = 22;
		// Look
		public const byte LookForward1 = 30;
		public const byte LookForward2 = 31;
		public const byte LookAround = 32;
		// Other
		public const byte Photosynthesis = 40;

		public static string CmdName(byte cmd)
		{
			return cmd switch 
			{
				Cmd.RotateAbsolute => "Поворот абсолютно",
				Cmd.RotateRelative => "Поворот относительно",
				Cmd.RotateRelativeContact => "Поворот относительно контакта",
				Cmd.RotateBackward => "Поворот назад",
				Cmd.RotateBackwardContact => "Поворот назад от контакта",
				Cmd.RotateRandom => "Поворот случайно",
				Cmd.AlignHorizontaly => "Выповняться по горизонтали",
				Cmd.StepForward1 => "Шаг вперед 1",
				Cmd.StepForward2 => "Шаг вперед 2",
				Cmd.StepRelative => "Шаг относительно",
				Cmd.StepRelativeContact => "Шаг относительно контакта",
				Cmd.StepBackward => "Шаг назад",
				Cmd.StepBackwardContact => "Шаг назад от контакта",
				Cmd.EatForward1 => "Есть впереди 1",
				Cmd.EatForward2 => "Есть впереди 2",
				Cmd.EatContact => "Есть контакт",
				Cmd.LookForward1 => "Смотерть вперед 1",
				Cmd.LookForward2 => "Смотреть вперед 2",
				Cmd.LookAround => "Смотреть вокруг",
				Cmd.Photosynthesis => "Фотосинтез",
				_ => $"Cmd_{cmd.ToString()}"
			};
		}

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
			Cmd.EatContact
		};

		public static HashSet<byte> BothCommands = new HashSet<byte>()
		{
			Cmd.RotateRelative,
			Cmd.LookAround,
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
			Cmd.EatContact,
			//Cmd.LookAround
			};

		public static HashSet<byte> DirectionCommands = new HashSet<byte>()
			{
			Cmd.RotateAbsolute,
			Cmd.RotateRelative,
			Cmd.RotateRelativeContact,
			Cmd.StepRelative,
			Cmd.StepRelativeContact,
			};
	}
}
