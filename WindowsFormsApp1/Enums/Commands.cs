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
				Cmd.StepForward1 => "Шаг вперед",
				Cmd.StepForward2 => "Шаг вперед",
				Cmd.StepRelative => "Шаг относительно",
				Cmd.StepRelativeContact => "Шаг относительно контакта",
				Cmd.StepBackward => "Шаг назад",
				Cmd.StepBackwardContact => "Шаг назад от контакта",
				Cmd.EatForward1 => "Есть впереди",
				Cmd.EatForward2 => "Есть впереди",
				Cmd.EatContact => "Есть контакт",
				Cmd.LookForward1 => "Смотреть вперед",
				Cmd.LookForward2 => "Смотреть вперед",
				Cmd.LookAround => "Смотреть вокруг",
				Cmd.Photosynthesis => "Фотосинтез",
				_ => $""
			};
		}

		public static HashSet<byte> GeneralCommands = new HashSet<byte>()
		{
			//Cmd.RotateAbsolute,
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

		public static HashSet<byte> ReactionCommands = new HashSet<byte>()
		{
			//Cmd.RotateRelative,
			Cmd.RotateRelativeContact,
			Cmd.RotateBackward,
			Cmd.RotateBackwardContact,
			//Cmd.LookAround,
			//Cmd.StepRelative,
			Cmd.StepRelativeContact,
			Cmd.StepBackward,
			Cmd.StepBackwardContact,
			Cmd.EatForward1,
			//Cmd.EatContact
		};

		public static HashSet<byte> CommandsWithParameter = new HashSet<byte>()
			{
			Cmd.RotateAbsolute,
			Cmd.RotateRelative,
			Cmd.RotateRelativeContact,
			Cmd.StepRelative,
			Cmd.StepRelativeContact,
			};
	}

	public static class CmdType
	{
		public const byte Rotate = 1;
		public const byte StepSuccessful = 2;
		public const byte StepNotSuccessful = 3;
		public const byte EatSuccessful = 4;
		public const byte EatNotSuccessful = 5;
		public const byte Look = 6;
		public const byte LookAround = 7;
		public const byte PhotosynthesisSuccessful = 8;
		public const byte PhotosynthesisNotSuccessful = 9;

		public static int CmdWeight(byte cmd)
		{
			return cmd switch
			{
				CmdType.Rotate => 2,
				CmdType.StepSuccessful => 50,
				CmdType.StepNotSuccessful => 10,
				CmdType.EatSuccessful => 50,
				CmdType.EatNotSuccessful => 10,
				CmdType.Look => 10,
				CmdType.LookAround => 60,
				CmdType.PhotosynthesisSuccessful => 60,
				CmdType.PhotosynthesisNotSuccessful => 10,
				_ => throw new Exception()
			};
		}
	}
}
