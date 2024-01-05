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
		public const byte RotateToContact = 8;
		public const byte RotateParallelContact = 9;

		// Step
		public const byte StepAbsolute = 10;
		public const byte StepForward = 11;
		public const byte StepRelative = 12;
		public const byte StepRelativeContact = 13;
		public const byte StepBackward = 14;
		public const byte StepBackwardContact = 15;
		public const byte StepNearContact = 16;
		public const byte StepToContact = 17;

		// Eat
		public const byte EatForward = 20;
		public const byte EatContact = 21;

		// Look
		public const byte LookForward = 30;
		public const byte LookAround1 = 31;
		public const byte LookAround2 = 32;

		// Other
		public const byte ClingToContact = 40;
		public const byte Nothing = 41;
		public const byte PushContact = 42;

		public static int MaxCmdChance = 50;
		public const byte _ = 0;
		
		public static (string CmdName, Color CmdColor, int CmdChance, byte CmdClass, byte WithParameters, 
			byte General, byte EnemyDangerous, byte EnemyNoDangerous, byte Relative, byte Things) CmdInfo(byte cmd)
		{
			return cmd switch
			{
				Cmd.RotateAbsolute =>			("Поворот_абсолютно",				Color.Orange,	2,		CmdClass.Rotate,	1,		_,_,_,_,_																),
				Cmd.RotateRelative =>			("Поворот_относительно",			Color.Orange,	10,		CmdClass.Rotate,	1,		_,_,_,_,_																),
				Cmd.RotateRelativeContact =>	("Поворот_относительно_контакта",	Color.Orange,	10,		CmdClass.Rotate,	1,		_,_,_,_,_																),
				Cmd.RotateToContact =>			("Поворот_к_контакту",				Color.Orange,	20,		CmdClass.Rotate,	_,		_,_,_,_,_																),
				Cmd.RotateBackward =>			("Поворот_назад",					Color.Orange,	10,		CmdClass.Rotate,	_,		_,_,_,_,_																),
				Cmd.RotateBackwardContact =>	("Поворот_назад_от_контакта",		Color.Orange,	10,		CmdClass.Rotate,	_,		_,_,_,_,_																),
				Cmd.RotateRandom =>				("Поворот_случайно",				Color.Orange,	2,		CmdClass.Rotate,	_,		_,_,_,_,_																),
				Cmd.AlignHorizontaly =>			("Выровняться_по_горизонтали",		Color.Orange,	1,		CmdClass.Rotate,	_,		_,_,_,_,_																),
				Cmd.RotateParallelContact =>	("Поворот_параллельно_контакту",	Color.Orange,	10,		CmdClass.Rotate,	_,		_,_,_,_,_																),
				Cmd.StepAbsolute =>				("Шаг_абсолютно",					Color.Red,		0,		CmdClass.Step,		1,		_,_,_,_,_																),
				Cmd.StepForward =>				("Шаг_вперед",						Color.Red,		20,		CmdClass.Step,		_,		_,_,_,_,_																),
				Cmd.StepRelative =>				("Шаг_относительно",				Color.Red,		10,		CmdClass.Step,		1,		_,_,_,_,_																),
				Cmd.StepRelativeContact =>		("Шаг_относительно_контакта",		Color.Red,		10,		CmdClass.Step,		1,		_,_,_,_,_																),
				Cmd.StepBackward =>				("Шаг_назад",						Color.Red,		10,		CmdClass.Step,		_,		_,_,_,_,_																),
				Cmd.StepBackwardContact =>		("Шаг_назад_от_контакта",			Color.Red,		10,		CmdClass.Step,		_,		_,_,_,_,_																),
				Cmd.StepToContact =>			("Шаг_к_контакту",					Color.Red,		10,		CmdClass.Step,		_,		_,_,_,_,_																),
				Cmd.StepNearContact =>			("Шаг_рядом_к_контакту",			Color.Red,		10,		CmdClass.Step,		1,		_,_,_,_,_																),
				Cmd.EatForward =>				("Есть_впереди",					Color.Green,	20,		CmdClass.Eat,		_,		_,_,_,_,_																),
				Cmd.EatContact =>				("Есть_контакт",					Color.Green,	10,		CmdClass.Eat,		_,		_,_,_,_,_																),
				Cmd.LookForward =>				("Смотреть_вперед",					Color.Blue,		20,		CmdClass.Look,		_,		_,_,_,_,_																),
				Cmd.LookAround1 =>				("Смотреть_вокруг_на_1",			Color.Blue,		40,		CmdClass.Look,		_,		_,_,_,_,_																),
				Cmd.LookAround2 =>				("Смотреть_вокруг_на_2",			Color.Blue,		40,		CmdClass.Look,		_,		_,_,_,_,_																),
				Cmd.ClingToContact =>			("Прицепиться_к_контакту",			Color.Magenta,	10,		CmdClass.Other,		_,		_,_,_,_,_																),
				Cmd.Nothing =>					("Ничего_не_делать",				Color.Magenta,	0,		CmdClass.Other,		_,		_,_,_,_,_																),
				Cmd.PushContact =>				("Оттолкнуть_контакт",				Color.Magenta,	20,		CmdClass.Other,		_,		_,_,_,_,_																),
				_ => throw new Exception()
			};
		}




		public static HashSet<byte> CommandsWithParameter = new HashSet<byte>()
		{
			Cmd.StepAbsolute,
			Cmd.RotateAbsolute,
			Cmd.RotateRelative,
			Cmd.RotateRelativeContact,
			Cmd.StepRelative,
			Cmd.StepRelativeContact,
			Cmd.StepNearContact
		};
	}

	public static class CmdClass
	{
		public const byte Rotate = 1;
		public const byte Step = 2;
		public const byte Eat = 3;
		public const byte Look = 4;
		public const byte Other = 5;
	}

	public static class CmdType
	{
		public const byte Rotate = 1;
		public const byte StepSuccessful = 2;
		public const byte StepNotSuccessful = 3;
		public const byte EatSuccessful = 4;
		public const byte EatNotSuccessful = 5;
		public const byte EatContactSuccessful = 6;
		public const byte EatContactNotSuccessful = 7;
		public const byte Look = 8;
		public const byte LookAround1 = 9;
		public const byte LookAround2 = 10;
		public const byte PhotosynthesisSuccessful = 11;
		public const byte PhotosynthesisNotSuccessful = 12;
		public const byte ClingToSuccessful = 13;
		public const byte ClingToNotSuccessful = 14;
		public const byte PushContactSuccessful = 15;
		public const byte PushContactNotSuccessful = 16;

		public static int CmdTime(byte cmd)
		{
			return cmd switch
			{
				CmdType.Rotate => 3,
				CmdType.StepSuccessful => 100,
				CmdType.StepNotSuccessful => 20,
				CmdType.EatSuccessful => 100,
				CmdType.EatNotSuccessful => 20,
				CmdType.EatContactSuccessful => 200,
				CmdType.EatContactNotSuccessful => 20,
				CmdType.Look => 10,
				CmdType.LookAround1 => 20,
				CmdType.LookAround2 => 30,
				CmdType.PhotosynthesisSuccessful => 60,
				CmdType.PhotosynthesisNotSuccessful => 20,
				CmdType.ClingToSuccessful => 2,
				CmdType.ClingToNotSuccessful => 0,
				CmdType.PushContactSuccessful => 50,
				CmdType.PushContactNotSuccessful => 10,
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
		public const byte React_Bot_Enemy = 11;
		public const byte React_Bot_NoBigrot = 12;
		public const byte React_Bot_Bigrot = 13;
		public const byte React_Bot_LessDigestion = 14;
		public const byte React_Bot_BiggerDigestion = 15;
		public const byte React_Bot_Relat = 16;
		public const byte React_Grass = 17;
		public const byte React_Mineral = 18;
		public const byte React_Wall = 19;

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
				Branch.React_Bot_Enemy => "bot enemy",
				Branch.React_Bot_NoBigrot => "bot nobigrot",
				Branch.React_Bot_Bigrot => "bot bigrot",
				Branch.React_Bot_LessDigestion => "bot less dig",
				Branch.React_Bot_BiggerDigestion => "bot bigger dig",
				Branch.React_Bot_Relat => "bot rel",
				Branch.React_Grass => "grass",
				Branch.React_Mineral => "mineral",
				Branch.React_Wall => "wall",
				_ => throw new NotImplementedException()
			};
		}

		public static byte[] BranchCmds(byte branch)
		{
			var generalCmds = new byte[]
			{
				Cmd.RotateAbsolute,
				Cmd.RotateRelative,
				//Cmd.RotateRelativeContact,
				//Cmd.RotateToContact,
				Cmd.RotateBackward,
				//Cmd.RotateBackwardContact,
				Cmd.RotateRandom,
				Cmd.AlignHorizontaly,
				Cmd.StepForward,
				Cmd.StepRelative,
				//Cmd.StepRelativeContact,
				Cmd.StepBackward,
				//Cmd.StepBackwardContact,
				Cmd.EatForward,
				//Cmd.EatContact,
				//Cmd.LookForward,
				//Cmd.LookAround1,
				Cmd.LookAround2  
			};

			var bot_enemyCmds = new byte[]
			{
				//Cmd.RotateAbsolute,
				Cmd.RotateRelative,
				Cmd.RotateRelativeContact,
				Cmd.RotateToContact,
				Cmd.RotateBackward,
				Cmd.RotateBackwardContact,
				Cmd.RotateRandom,
				//Cmd.RotateParallelContact,
				//Cmd.AlignHorizontaly,
				Cmd.StepForward,
				Cmd.StepRelative,
				Cmd.StepRelativeContact,
				Cmd.StepBackward,
				Cmd.StepBackwardContact,
				Cmd.StepNearContact,
				Cmd.StepToContact,
				Cmd.EatForward,
				//Cmd.EatContact,
				//Cmd.LookForward,
				//Cmd.LookAround1,
				//Cmd.LookAround2,
				//Cmd.ClingToContact
				Cmd.PushContact
			};

			var bot_relCmds = new byte[]
			{
				//Cmd.RotateAbsolute,
				Cmd.RotateRelative,
				Cmd.RotateRelativeContact,
				Cmd.RotateToContact,
				Cmd.RotateBackward,
				Cmd.RotateBackwardContact,
				Cmd.RotateRandom,
				Cmd.RotateParallelContact,
				//Cmd.AlignHorizontaly,
				Cmd.StepForward,
				Cmd.StepRelative,
				Cmd.StepRelativeContact,
				Cmd.StepBackward,
				Cmd.StepBackwardContact,
				Cmd.StepToContact,
				Cmd.StepNearContact,
				Cmd.EatForward,
				//Cmd.EatContact,
				//Cmd.LookForward,
				//Cmd.LookAround1,
				//Cmd.LookAround2,
				Cmd.ClingToContact,
				Cmd.PushContact
			};

			var thingCmds = new byte[]
			{
				//Cmd.RotateAbsolute,
				Cmd.RotateRelative,
				Cmd.RotateRelativeContact,
				Cmd.RotateToContact,
				Cmd.RotateBackward,
				Cmd.RotateBackwardContact,
				Cmd.RotateRandom,
				Cmd.RotateParallelContact,
				//Cmd.AlignHorizontaly,
				Cmd.StepForward,
				Cmd.StepRelative,
				Cmd.StepRelativeContact,
				Cmd.StepBackward,
				Cmd.StepBackwardContact,
				Cmd.StepToContact,
				Cmd.StepNearContact,
				Cmd.EatForward,
				//Cmd.EatContact,
				//Cmd.LookForward,
				//Cmd.LookAround1,
				//Cmd.LookAround2
			};


			#region comment BranchCmds
			/*
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
			*/
			#endregion

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
				Branch.React_Bite => bot_enemyCmds,
				Branch.React_Bot_Enemy => bot_enemyCmds,
				Branch.React_Bot_NoBigrot => bot_enemyCmds,
				Branch.React_Bot_Bigrot => bot_enemyCmds,
				Branch.React_Bot_LessDigestion => bot_enemyCmds,
				Branch.React_Bot_BiggerDigestion => bot_enemyCmds,
				Branch.React_Bot_Relat => bot_relCmds,
				Branch.React_Grass => thingCmds,
				Branch.React_Mineral => thingCmds,
				Branch.React_Wall => thingCmds,
				_ => throw new NotImplementedException()
			};
		}
	}
}
