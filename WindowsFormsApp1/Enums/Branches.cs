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
	public static class Branches
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
				Branches.General0 => "Gen0",
				Branches.General1 => "Gen1",
				Branches.General2 => "Gen2",
				Branches.General3 => "Gen3",
				Branches.General4 => "Gen4",
				Branches.General5 => "Gen5",
				Branches.General6 => "Gen6",
				Branches.General7 => "Gen7",
				Branches.General8 => "Gen8",
				Branches.General9 => "Gen9",
				Branches.React_Bite => "bite",
				Branches.React_Bot_Enemy => "bot enemy",
				Branches.React_Bot_NoBigrot => "bot nobigrot",
				Branches.React_Bot_Bigrot => "bot bigrot",
				Branches.React_Bot_LessDigestion => "bot less dig",
				Branches.React_Bot_BiggerDigestion => "bot bigger dig",
				Branches.React_Bot_Relat => "bot rel",
				Branches.React_Grass => "grass",
				Branches.React_Mineral => "mineral",
				Branches.React_Wall => "wall",
				_ => throw new NotImplementedException()
			};
		}

		public static byte[] BranchCmds(byte branch)
		{
			return branch switch
			{
				Branches.General0 => Data.CmdGeneralCmds,
				Branches.General1 => Data.CmdGeneralCmds,
				Branches.General2 => Data.CmdGeneralCmds,
				Branches.General3 => Data.CmdGeneralCmds,
				Branches.General4 => Data.CmdGeneralCmds,
				Branches.General5 => Data.CmdGeneralCmds,
				Branches.General6 => Data.CmdGeneralCmds,
				Branches.General7 => Data.CmdGeneralCmds,
				Branches.General8 => Data.CmdGeneralCmds,
				Branches.General9 => Data.CmdGeneralCmds,
				Branches.React_Bite => Data.CmdEnemyDangerousCmds,
				Branches.React_Bot_Enemy => Data.CmdEnemyDangerousCmds,
				Branches.React_Bot_NoBigrot => Data.CmdEnemyDangerousCmds,
				Branches.React_Bot_Bigrot => Data.CmdEnemyDangerousCmds,
				Branches.React_Bot_LessDigestion => Data.CmdEnemyDangerousCmds,
				Branches.React_Bot_BiggerDigestion => Data.CmdEnemyDangerousCmds,
				Branches.React_Bot_Relat => Data.CmdRelativeCmds,
				Branches.React_Grass => Data.CmdThingsCmds,
				Branches.React_Mineral => Data.CmdThingsCmds,
				Branches.React_Wall => Data.CmdThingsCmds,
				_ => throw new NotImplementedException()
			};
		}
	}
}
