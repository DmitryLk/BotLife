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
		public static (string CmdName, Color CmdColor, int CmdChance, byte CmdClass, byte WithParameters, 
			byte General, byte EnemyDangerous, byte EnemyNoDangerous, byte Relative, byte Things) CmdInfo(byte cmd)
		{
			return cmd switch
			{
				Cmd.RotateAbsolute =>			("Поворот_абсолютно",				Color.Orange,	2,		CmdClass.Rotate,			Y,		Y,_,_,_,_													),
				Cmd.RotateRelative =>			("Поворот_относительно",			Color.Orange,	10,		CmdClass.Rotate,			Y,		Y,Y,Y,Y,Y													),
				Cmd.RotateBackward =>			("Поворот_назад",					Color.Orange,	10,		CmdClass.Rotate,			_,		Y,Y,_,Y,Y													),
				Cmd.RotateRandom =>				("Поворот_случайно",				Color.Orange,	2,		CmdClass.Rotate,			_,		Y,Y,Y,Y,Y													),
				Cmd.AlignHorizontaly =>			("Выровняться_по_горизонтали",		Color.Orange,	1,		CmdClass.Rotate,			_,		Y,_,_,_,_													),
				Cmd.StepAbsolute =>				("Шаг_абсолютно",					Color.Red,		0,		CmdClass.Step,				Y,		_,_,_,_,_													),
				Cmd.StepForward =>				("Шаг_вперед",						Color.Red,		20,		CmdClass.Step,				_,		Y,Y,Y,Y,Y													),
				Cmd.StepRelative =>				("Шаг_относительно",				Color.Red,		10,		CmdClass.Step,				Y,		Y,Y,Y,Y,Y													),
				Cmd.StepBackward =>				("Шаг_назад",						Color.Red,		10,		CmdClass.Step,				_,		Y,Y,_,Y,Y													),
				Cmd.EatForward =>				("Есть_впереди",					Color.Green,	20,		CmdClass.Eat,				_,		Y,Y,Y,Y,Y													),
				Cmd.LookForward =>				("Смотреть_вперед",					Color.Blue,		20,		CmdClass.Look,				_,		_,_,_,_,_													),
				Cmd.LookAround1 =>				("Смотреть_вокруг_на_1",			Color.Blue,		40,		CmdClass.LookAround1,		_,		_,_,Y,_,_													),
				Cmd.LookAround2 =>				("Смотреть_вокруг_на_2",			Color.Blue,		40,		CmdClass.LookAround2,		_,		Y,_,Y,_,_													),
				Cmd.Nothing =>					("Ничего_не_делать",				Color.Magenta,	0,		CmdClass.Nothing,			_,		_,_,_,_,_													),
				
				Cmd.RotateRelativeContact =>	("Поворот_относительно_контакта",	Color.Orange,	10,		CmdClass.Rotate,			Y,		_,Y,Y,Y,Y													),
				Cmd.RotateToContact =>			("Поворот_к_контакту",				Color.Orange,	20,		CmdClass.Rotate,			_,		_,Y,Y,Y,Y													),
				Cmd.RotateBackwardContact =>	("Поворот_назад_от_контакта",		Color.Orange,	10,		CmdClass.Rotate,			_,		_,Y,_,Y,Y													),
				Cmd.RotateParallelContact =>	("Поворот_параллельно_контакту",	Color.Orange,	10,		CmdClass.Rotate,			_,		_,_,_,Y,Y													),
				Cmd.StepRelativeContact =>		("Шаг_относительно_контакта",		Color.Red,		10,		CmdClass.Step,				Y,		_,Y,Y,Y,Y													),
				Cmd.StepBackwardContact =>		("Шаг_назад_от_контакта",			Color.Red,		10,		CmdClass.Step,				_,		_,Y,_,Y,Y													),
				Cmd.StepToContact =>			("Шаг_к_контакту",					Color.Red,		10,		CmdClass.Step,				_,		_,Y,Y,Y,Y													),
				Cmd.StepNearContact =>			("Шаг_рядом_к_контакту",			Color.Red,		10,		CmdClass.Step,				Y,		_,Y,Y,Y,Y													),
				Cmd.EatContact =>				("Есть_контакт",					Color.Green,	10,		CmdClass.EatContact,		_,		_,_,_,_,_													),
				Cmd.ClingToContact =>			("Прицепиться_к_контакту",			Color.Magenta,	10,		CmdClass.ClingToContact,	_,		_,_,_,Y,_													),
				Cmd.PushContact =>				("Оттолкнуть_контакт",				Color.Magenta,	20,		CmdClass.PushContact,		_,		_,Y,Y,Y,_													),

				_ => throw new Exception()
			};
		}

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
		public const byte Y = 1;
	}

	public static class CmdClass
	{
		public const byte Rotate = 1;
		public const byte Step = 2;
		public const byte Eat = 3;
		public const byte EatContact = 4;
		public const byte Look = 5;
		public const byte LookAround1 = 6;
		public const byte LookAround2 = 7;
		public const byte ClingToContact = 8;
		public const byte PushContact = 9;
		public const byte Nothing = 10;

		public static int CmdSuccTime(byte cmd)
		{
			return cmd switch
			{
				CmdClass.Rotate => 3,
				CmdClass.Step => 100,
				CmdClass.Eat => 100,
				CmdClass.EatContact => 200,
				CmdClass.Look => 10,
				CmdClass.LookAround1 => 20,
				CmdClass.LookAround2 => 30,
				CmdClass.ClingToContact => 2,
				CmdClass.PushContact => 50,
				CmdClass.Nothing => 0,
				_ => throw new Exception()
			};
		}

		public static int CmdNoSuccessTime(byte cmd)
		{
			return cmd switch
			{
				CmdClass.Rotate => 3,
				CmdClass.Step => 20,
				CmdClass.Eat => 20,
				CmdClass.EatContact => 20,
				CmdClass.Look => 10,
				CmdClass.LookAround1 => 20,
				CmdClass.LookAround2 => 30,
				CmdClass.ClingToContact => 0,
				CmdClass.PushContact => 10,
				CmdClass.Nothing => 0,
				_ => throw new Exception()
			};
		}
	}
}
