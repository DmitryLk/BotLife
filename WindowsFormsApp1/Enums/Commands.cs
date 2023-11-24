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
		public const byte StepForward = 10;
		public const byte StepRelative = 11;
		public const byte StepRelativeContact = 12;
		public const byte StepBackward = 13;
		public const byte StepBackwardContact = 14;
		// Eat
		public const byte EatForward = 20;
		public const byte EatContact = 21;
		// Look
		public const byte LookForward = 30;
		public const byte LookAround = 31;
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
				Cmd.StepForward => "Шаг вперед",
				Cmd.StepRelative => "Шаг относительно",
				Cmd.StepRelativeContact => "Шаг относительно контакта",
				Cmd.StepBackward => "Шаг назад",
				Cmd.StepBackwardContact => "Шаг назад от контакта",
				Cmd.EatForward => "Есть впереди",
				Cmd.EatContact => "Есть контакт",
				Cmd.LookForward => "Смотреть вперед",
				Cmd.LookAround => "Смотреть вокруг",
				Cmd.Photosynthesis => "Фотосинтез",
				_ => $""
			};
		}

        public static Color CmdColor(byte cmd)
        {
            return cmd switch
            {
                Cmd.RotateAbsolute => Color.Orange,
                Cmd.RotateRelative => Color.Orange,
                Cmd.RotateRelativeContact => Color.Orange,
                Cmd.RotateBackward => Color.Orange,
                Cmd.RotateBackwardContact => Color.Orange,
                Cmd.RotateRandom => Color.Orange,
                Cmd.AlignHorizontaly => Color.Orange,
                Cmd.StepForward => Color.Red,
                Cmd.StepRelative => Color.Red,
                Cmd.StepRelativeContact => Color.Red,
                Cmd.StepBackward => Color.Red,
                Cmd.StepBackwardContact => Color.Red,
                Cmd.EatForward => Color.Green,
                Cmd.EatContact => Color.Green,
                Cmd.LookForward => Color.Blue,
                Cmd.LookAround => Color.Blue,
                Cmd.Photosynthesis => Color.Green,
                _ => Color.Black
			};
        }

		public static int MaxCmdWeight = 50;
		public static int CmdWeight(byte cmd)
		{
			return cmd switch
			{
				Cmd.RotateAbsolute => 2,
				Cmd.RotateRelative => 10,
				Cmd.RotateRelativeContact => 10,
				Cmd.RotateBackward => 10,
				Cmd.RotateBackwardContact => 10,
				Cmd.RotateRandom => 0,
				Cmd.AlignHorizontaly => 0,
				Cmd.StepForward => 20,
				Cmd.StepRelative => 10,
				Cmd.StepRelativeContact => 10,
				Cmd.StepBackward => 10,
				Cmd.StepBackwardContact => 10,
				Cmd.EatForward => 20,
				Cmd.EatContact => 10,
				Cmd.LookForward => 20,
				Cmd.LookAround => 40,
				Cmd.Photosynthesis => 0,
				_ => throw new Exception()
			};
		}

		public static HashSet<byte> GeneralCommands = new HashSet<byte>()
		{
			Cmd.RotateAbsolute,
			//Cmd.RotateRelative,
			//Cmd.Photosynthesis,
			Cmd.StepForward,
			Cmd.EatForward,
			Cmd.LookForward,
			Cmd.LookAround,
			Cmd.RotateRandom,
			//Cmd.AlignHorizontaly,
		};

		public static HashSet<byte> ReactionCommands = new HashSet<byte>()
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
			Cmd.EatForward,
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

		public static int CmdTime(byte cmd)
		{
			return cmd switch
			{
				CmdType.Rotate => 3,
				CmdType.StepSuccessful => 100,
				CmdType.StepNotSuccessful => 20,
				CmdType.EatSuccessful => 100,
				CmdType.EatNotSuccessful => 20,
				CmdType.Look => 20,
				CmdType.LookAround => 60,
				CmdType.PhotosynthesisSuccessful => 60,
				CmdType.PhotosynthesisNotSuccessful => 20,
				_ => throw new Exception()
			};
		}
	}
}
