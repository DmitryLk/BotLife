using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using WindowsFormsApp1.Static;

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
		public const byte LookAround1 = 31;
		public const byte LookAround2 = 32;
		// Other

		public static string CmdName(byte cmd)
		{
			return cmd switch
			{
				Cmd.RotateAbsolute => "Поворот_абсолютно",
				Cmd.RotateRelative => "Поворот_относительно",
				Cmd.RotateRelativeContact => "Поворот_относительно_контакта",
				Cmd.RotateBackward => "Поворот_назад",
				Cmd.RotateBackwardContact => "Поворот_назад_от_контакта",
				Cmd.RotateRandom => "Поворот_случайно",
				Cmd.AlignHorizontaly => "Выповняться_по_горизонтали",
				Cmd.StepForward => "Шаг_вперед",
				Cmd.StepRelative => "Шаг_относительно",
				Cmd.StepRelativeContact => "Шаг_относительно_контакта",
				Cmd.StepBackward => "Шаг_назад",
				Cmd.StepBackwardContact => "Шаг_назад_от_контакта",
				Cmd.EatForward => "Есть_впереди",
				Cmd.EatContact => "Есть_контакт",
				Cmd.LookForward => "Смотреть_вперед",
				Cmd.LookAround1 => "Смотреть_вокруг_на_1",
				Cmd.LookAround2 => "Смотреть_вокруг_на_2",
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
				Cmd.LookAround1 => Color.Blue,
				Cmd.LookAround2 => Color.Blue,
				_ => Color.Black
			};
		}

		public static int MaxCmdChance = 50;
		public static int CmdChance(byte cmd)
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
				Cmd.LookAround1 => 40,
				Cmd.LookAround2 => 40,
				_ => throw new Exception()
			};
		}

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
		public const byte LookAround1 = 7;
		public const byte LookAround2 = 8;
		public const byte PhotosynthesisSuccessful = 9;
		public const byte PhotosynthesisNotSuccessful = 10;

		public static int CmdTime(byte cmd)
		{
			return cmd switch
			{
				CmdType.Rotate => 3,
				CmdType.StepSuccessful => 100,
				CmdType.StepNotSuccessful => 20,
				CmdType.EatSuccessful => 100,
				CmdType.EatNotSuccessful => 20,
				CmdType.Look => 10,
				CmdType.LookAround1 => 20,
				CmdType.LookAround2 => 30,
				CmdType.PhotosynthesisSuccessful => 60,
				CmdType.PhotosynthesisNotSuccessful => 20,
				_ => throw new Exception()
			};
		}
	}

	public static class Branch
	{
		public static int AllBranchCount = 20;
		public static int GeneralBranchCount = 10;

		public const byte General0 = 0;
		public const byte General1 = 1;
		public const byte General2 = 2;
		public const byte General3 = 3;
		public const byte General4 = 4;
		public const byte General5 = 5;
		public const byte General6 = 6;
		public const byte General7 = 7;
		public const byte General8 = 8;
		public const byte General9 = 9;
		public const byte React_Bite = 10;
		public const byte React_Bot_NoBigrot = 11;
		public const byte React_Bot_Bigrot = 12;
		public const byte React_Bot_LessDigestion = 13;
		public const byte React_Bot_BiggerDigestion = 14;
		public const byte React_Bot_Relat = 15;
		public const byte React_Grass = 16;
		public const byte React_Mineral = 17;
		public const byte React_Wall = 18;
		public const byte React9 = 19;

		public static string BranchName(byte cmd)
		{
			return cmd switch
			{
				Branch.General0 => "Gen0",
				Branch.General1 => "Gen1",
				Branch.General2 => "Gen2",
				Branch.General3 => "Gen3",
				Branch.General4 => "Gen4",
				Branch.General5 => "Gen5",
				Branch.General6 => "Gen6",
				Branch.General7 => "Gen7",
				Branch.General8 => "Gen8",
				Branch.General9 => "Gen9",
				Branch.React_Bite => "bite",
				Branch.React_Bot_NoBigrot => "bot nobigrot",
				Branch.React_Bot_Bigrot => "bot bigrot",
				Branch.React_Bot_LessDigestion => "bot less dig",
				Branch.React_Bot_BiggerDigestion => "bot bigger dig",
				Branch.React_Bot_Relat => "bot rel",
				Branch.React_Grass => "grass",
				Branch.React_Mineral => "mineral",
				Branch.React_Wall => "wall",
				Branch.React9 => "-",
				_ => throw new NotImplementedException()
			};
		}

		public static byte[] BranchCmds(byte branch)
		{
			var generalCmds = new byte[]
			{
				Cmd.RotateAbsolute,
				//Cmd.RotateRelative,
				//Cmd.RotateRelativeContact,
				//Cmd.RotateBackward,
				//Cmd.RotateBackwardContact,
				Cmd.RotateRandom,
				//Cmd.AlignHorizontaly,
				Cmd.StepForward,
				Cmd.StepRelative,
				//Cmd.StepRelativeContact,
				//Cmd.StepBackward,
				//Cmd.StepBackwardContact,
				Cmd.EatForward,
				//Cmd.EatContact,
				Cmd.LookForward,
				Cmd.LookAround1,
				Cmd.LookAround2
			};

			var bot_biteCmds = new byte[]
			{
				//Cmd.RotateAbsolute,
				Cmd.RotateRelative,
				Cmd.RotateRelativeContact,
				Cmd.RotateBackward,
				Cmd.RotateBackwardContact,
				//Cmd.RotateRandom,
				//Cmd.AlignHorizontaly,
				//Cmd.StepForward,
				Cmd.StepRelative,
				Cmd.StepRelativeContact,
				Cmd.StepBackward,
				Cmd.StepBackwardContact,
				Cmd.EatForward,
				//Cmd.EatContact,
				//Cmd.LookForward,
				//Cmd.LookAround1,
				//Cmd.LookAround2
			};

			var bot_relCmds = new byte[]
			{
				//Cmd.RotateAbsolute,
				Cmd.RotateRelative,
				Cmd.RotateRelativeContact,
				//Cmd.RotateBackward,
				//Cmd.RotateBackwardContact,
				//Cmd.RotateRandom,
				//Cmd.AlignHorizontaly,
				//Cmd.StepForward,
				Cmd.StepRelative,
				Cmd.StepRelativeContact,
				//Cmd.StepBackward,
				//Cmd.StepBackwardContact,
				//Cmd.EatForward,
				//Cmd.EatContact,
				//Cmd.LookForward,
				Cmd.LookAround1,
				Cmd.LookAround2
			};

			var bot_enemyCmds = new byte[]
			{
				//Cmd.RotateAbsolute,
				Cmd.RotateRelative,
				Cmd.RotateRelativeContact,
				Cmd.RotateBackward,
				Cmd.RotateBackwardContact,
				//Cmd.RotateRandom,
				//Cmd.AlignHorizontaly,
				//Cmd.StepForward,
				Cmd.StepRelative,
				Cmd.StepRelativeContact,
				Cmd.StepBackward,
				Cmd.StepBackwardContact,
				Cmd.EatForward,
				//Cmd.EatContact,
				//Cmd.LookForward,
				Cmd.LookAround1,
				Cmd.LookAround2
			};

			var bot_lessdigCmds = new byte[]
			{
				//Cmd.RotateAbsolute,
				Cmd.RotateRelative,
				Cmd.RotateRelativeContact,
				//Cmd.RotateBackward,
				//Cmd.RotateBackwardContact,
				//Cmd.RotateRandom,
				//Cmd.AlignHorizontaly,
				//Cmd.StepForward,
				Cmd.StepRelative,
				Cmd.StepRelativeContact,
				//Cmd.StepBackward,
				//Cmd.StepBackwardContact,
				Cmd.EatForward,
				//Cmd.EatContact,
				//Cmd.LookForward,
				Cmd.LookAround1,
				Cmd.LookAround2
			};

			var bot_bigdigCmds = new byte[]
			{
				//Cmd.RotateAbsolute,
				Cmd.RotateRelative,
				Cmd.RotateRelativeContact,
				Cmd.RotateBackward,
				Cmd.RotateBackwardContact,
				//Cmd.RotateRandom,
				//Cmd.AlignHorizontaly,
				//Cmd.StepForward,
				Cmd.StepRelative,
				Cmd.StepRelativeContact,
				Cmd.StepBackward,
				Cmd.StepBackwardContact,
				//Cmd.EatForward,
				//Cmd.EatContact,
				//Cmd.LookForward,
				//Cmd.LookAround1,
				//Cmd.LookAround2
			};

			var thingCmds = new byte[]
			{
				//Cmd.RotateAbsolute,
				Cmd.RotateRelative,
				Cmd.RotateRelativeContact,
				Cmd.RotateBackward,
				Cmd.RotateBackwardContact,
				Cmd.RotateRandom,
				//Cmd.AlignHorizontaly,
				//Cmd.StepForward,
				Cmd.StepRelative,
				Cmd.StepRelativeContact,
				Cmd.StepBackward,
				Cmd.StepBackwardContact,
				Cmd.EatForward,
				//Cmd.EatContact,
				Cmd.LookForward,
				Cmd.LookAround1,
				Cmd.LookAround2
			};

			return branch switch
			{
				Branch.General0 => generalCmds,
				Branch.General1 => generalCmds,
				Branch.General2 => generalCmds,
				Branch.General3 => generalCmds,
				Branch.General4 => generalCmds,
				Branch.General5 => generalCmds,
				Branch.General6 => generalCmds,
				Branch.General7 => generalCmds,
				Branch.General8 => generalCmds,
				Branch.General9 => generalCmds,
				Branch.React_Bite => bot_biteCmds,
				Branch.React_Bot_NoBigrot => bot_enemyCmds,
				Branch.React_Bot_Bigrot => bot_enemyCmds,
				Branch.React_Bot_LessDigestion => bot_lessdigCmds,
				Branch.React_Bot_BiggerDigestion => bot_bigdigCmds,
				Branch.React_Bot_Relat => bot_relCmds,
				Branch.React_Grass => thingCmds,
				Branch.React_Mineral => thingCmds,
				Branch.React_Wall => thingCmds,
				Branch.React9 => thingCmds,
				_ => throw new NotImplementedException()
			};
		}
	}
}
