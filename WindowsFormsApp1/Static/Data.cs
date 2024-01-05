//			var curBots = Interlocked.Decrement(ref _curBots);
// 		private long _curBots = 0;


using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.GameLogic;
using WindowsFormsApp1.Logger;
using WindowsFormsApp1.Options;
using static System.Windows.Forms.Design.AxImporter;
using DrawMode = WindowsFormsApp1.Enums.DrawMode;

namespace WindowsFormsApp1.Static
{
	public static class Data
	{
		//==================CONSTANT INITIAL SETTINGS===========================
		public static int CellWidth;
		public static int CellHeight;
		public static int ReportFrequencyDrawed;
		public static int ReportFrequencyNoDrawed;
		public static int FrequencyOfPeriodicalDraw;


		public static long StartNumberOfBots;
		public static long MaxBotsNumber;
		public static int WorldWidth;
		public static int WorldHeight;
		public static bool UpDownEdge;
		public static bool LeftRightEdge;

		public static bool SeedFood;
		public static bool SeedOrganic;
		public static bool SeedMinerals;
		public static bool SeedWalls;
		public static bool SeedPoison;
		public static int SeedFoodNumber;
		public static int SeedOrganicNumber;
		public static int SeedMineralsNumber;
		public static int SeedWallsNumber;
		public static int SeedPoisonNumber;

		public static int MaxCode;
		public static int MaxCmdInStep;
		public static float MutationProbabilityPercent;
		public static int MutationLenght;
		public static int DigestionTypeCount;

		public static int FoodEnergy;
		public static int SeedBotEnergy;
		public static int InitialBotEnergy;
		public static int ReproductionBotEnergy;
		public static int BiteEnergy;
		public static int DeltaEnergyOnStep;
		public static int PhotosynthesisEnergy;
		public static int PhotosynthesisLayerHeight;

		public static int LensWidth;
		public static int LensHeight;
		public static int LensCellWidth;
		public static int LensCellHeight;

		// Attack-Shield
		public static int AttackShieldSum;
		public static int AttackShieldTypeCount;
		public static int AttackShieldTypeCountMax;
		public static int AttackMax;
		public static int ShieldMax;
		public static int AttackTypeCountMax;


		public static int HoldReproductionTime;
		public static int GenomInfoPeriodPrint;
		public static int KeptTotalEnergy;
		public static int MovedBiteStrength;
		public static bool DelayForNewbie;

		//=================VARIABLE CURRENT PARAMETERS==============================
		public static long[,] World; // чтобы можно было узнать по координатам что там находится
		public static Bot1[] Bots;
		public static long TotalEnergy;
		public static long SeedTotalEnergy;

		// Death Reproduction		
		public static Bot1[] BotDeath;
		public static Bot1[] BotReproduction;
		public static long QtyAllBotDeathMinusOne;  // Количество собирающихся и собиравшихся умереть ботов
		public static long QtyFactBotDeath;         // Фактическое количество умерших ботов на этом шаге
		public static long QtyFactBotDeathUsedForReproduction;
		public static int IndexOfLastBotDeathArrayUsedForReproduction;
		public static int IndexOfLastBotReproduction;  // В массиве Data.BotReproduction последний индекс бота который хочет размножиться
		public static long TotalQtyBotDeath;
		public static long TotalQtyBotReproduction;
		public static long IndexEnclusiveBeforeReplacesBots;  //Последний индекс в массиве Bots не используемый для перемещения
		public static long Check_QtyFailedReproduction;
		public static long Check_QtyFailedDeath;
		public static int QtyRemovedBotsOnStep;
		public static long IndexOfLastBotPlusOne;
		//private static int removedbots2;


		// Настройки игры
		public static bool Started;
		public static bool PausedMode;
		public static bool Worked;
		public static bool LensOn;
		public static bool Mutation;
		public static bool Parallel;
		public static bool Checks;
		public static bool HistoryOn;
		public static DrawMode DrawMode;
		public static DrawMode NextDrawMode;
		public static DrawType DrawType;
		public static DrawType NextDrawType;
		public static BotColorMode BotColorMode;
		public static BotColorMode NextBotColorMode;
		public static GenomInfoMode GenomInfo;


		//Для поддержки эффективной перерисовки
		public static long[,] ChWorld;
		//public static BotLog[,] ClWorld;
		public static ChangedCell[] ChangedCells;
		public static long NumberOfChangedCells;
		public static long NumberOfChangedCellsForInfo;
		public static Color[] GrColors;

		public static long CurrentNumberOfBots;
		public static uint CurrentStep;
		public static long MaxBotNumber;
		public static long MutationCnt;
		public static int ReportFrequencyCurrent;
		public static long CurrentNumberOfFood;

		public static int LensX;
		public static int LensY;
		public static int CursorX;
		public static int CursorY;
		public static int DeltaHistory;

		// DataGridView
		public static int DgvColumnIndex = 0;
		public static ListSortDirection DgvDirection = ListSortDirection.Ascending;
		public static bool DgvOnlyLive;
		public static bool DgvPra;

		// Commands
		public static bool[] CmdWithParameter;
		public static string[] CmdName;
		public static Color[] CmdColor;
		public static int[] CmdChance;
		public static byte[] CmdClass;

		public static bool GridWalls;
		// public static Log.Log Wlog;

		public static void Initialize()
		{
			var options = LoadConfig();
			MapOptions(options);

			// Создать все игровые массивы
			World = new long[WorldWidth, WorldHeight];
			Bots = new Bot1[MaxBotsNumber];
			ChWorld = new long[WorldWidth, WorldHeight];
			Array.Clear(ChWorld, 0, ChWorld.Length);

			//ClWorld = new BotLog[WorldWidth, WorldHeight];
			//Array.Clear(ClWorld, 0, ClWorld.Length);

			ChangedCells = new ChangedCell[MaxBotsNumber + 10];
			GrColors = new Color[361];

			BotDeath = new Bot1[50_000];
			BotReproduction = new Bot1[11_000];
			QtyAllBotDeathMinusOne = -1;
			QtyFactBotDeath = 0;
			QtyFactBotDeathUsedForReproduction = 0;
			IndexOfLastBotReproduction = -1;
			TotalQtyBotDeath = 0;
			TotalQtyBotReproduction = 0;

			Mutation = true;
			Started = false;
			PausedMode = false;
			LensOn = false;
			Parallel = true;
			Checks = false;
			HistoryOn = true;

			DrawMode = DrawMode.EachStep;
			NextDrawMode = DrawMode.EachStep;
			BotColorMode = BotColorMode.GenomColor;
			NextBotColorMode = BotColorMode.GenomColor;
			DrawType = DrawType.OnlyChangedCells;
			NextDrawType = DrawType.OnlyChangedCells;
			GenomInfo = GenomInfoMode.None;

			NumberOfChangedCells = 0;
			MaxBotNumber = 0;
			CurrentStep = 0;
			MutationCnt = 0;

			ReportFrequencyCurrent = ReportFrequencyDrawed;

			LensX = 10;
			LensY = 10;
			CursorX = 10;
			CursorY = 10;

			//Wlog = new Log.Log();
			DgvColumnIndex = 0;
			DgvDirection = ListSortDirection.Ascending;
			DgvOnlyLive = false;
			DgvPra = false;
			GridWalls = false;


			for (var hue = 0; hue <= 360; hue++)
			{
				GrColors[hue] = ColorFromHSV(hue, 1, 1);
			}

			CmdWithParameter = new bool[Data.MaxCode + 1];
			CmdName = new string[Data.MaxCode + 1];
			CmdColor = new Color[Data.MaxCode + 1];
			CmdChance = new int[Data.MaxCode + 1];
			CmdClass = new byte[Data.MaxCode + 1];
			Array.Clear(CmdWithParameter, 0, Data.MaxCode + 1);
			Array.Clear(CmdName, 0, Data.MaxCode + 1);
			Array.Clear(CmdColor, 0, Data.MaxCode + 1);
			Array.Clear(CmdChance, 0, Data.MaxCode + 1);
			Array.Clear(CmdClass, 0, Data.MaxCode + 1);

			foreach (var c in Cmd.CommandsWithParameter)
			{
				CmdWithParameter[c] = true;
			}

			FieldInfo[] fields = typeof(Cmd).GetFields();
			foreach (FieldInfo fi in fields)
			{
				if (fi.FieldType.Name == "Byte" && fi.Name.Length > 1)
				{
					var cmd = (byte)fi.GetValue(null);

					var cmdInfo = Cmd.CmdInfo(cmd);
					CmdName[cmd] = cmdInfo.CmdName;
					CmdColor[cmd] = cmdInfo.CmdColor;
					CmdChance[cmd] = cmdInfo.CmdChance;
					CmdClass[cmd] = cmdInfo.CmdClass;
				}
			}

			//var maxcmd = 0;
			//FieldInfo[] fields = typeof(Cmd).GetFields(BindingFlags.Static | BindingFlags.Public);
			//foreach (FieldInfo fi in fields)
			//{
			//    var val = fi.GetValue(null);
			//    if (val is byte)
			//    {
			//        if ((int)val > maxcmd)
			//        {
			//            maxcmd = (int)val;
			//        }
			//    }
			//}
		}

		public static Color ColorFromHSV(double hue, double saturation, double value)
		{
			int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
			double f = hue / 60 - Math.Floor(hue / 60);

			value = value * 255;
			int v = Convert.ToInt32(value);
			int p = Convert.ToInt32(value * (1 - saturation));
			int q = Convert.ToInt32(value * (1 - f * saturation));
			int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

			if (hi == 0)
				return Color.FromArgb(255, v, t, p);
			else if (hi == 1)
				return Color.FromArgb(255, q, v, p);
			else if (hi == 2)
				return Color.FromArgb(255, p, v, t);
			else if (hi == 3)
				return Color.FromArgb(255, p, q, v);
			else if (hi == 4)
				return Color.FromArgb(255, t, p, v);
			else
				return Color.FromArgb(255, v, p, q);
		}

		public static string GetTextForCaption(double fps)
		{
			return $"Bots: {CurrentNumberOfBots}                Fps: {fps.ToString("#")}";
		}

		public static string GetText(double fps)
		{
			var sb = new StringBuilder();
			var f = new NumberFormatInfo { NumberGroupSeparator = "_" };

			sb.AppendLine($"Fps: {fps.ToString("#")}");
			sb.AppendLine($"Step: {CurrentStep}");
			sb.AppendLine($"Bots: {CurrentNumberOfBots}");
			sb.AppendLine($"ChangedCells: {NumberOfChangedCellsForInfo}");

			sb.AppendLine($"deathCnt: {TotalQtyBotDeath}");
			sb.AppendLine($"reproductionCnt: {TotalQtyBotReproduction}");
			sb.AppendLine($"mutationCnt: {MutationCnt}");
			sb.AppendLine($"TotalEnergy: {TotalEnergy.ToString("n", f)}");
			sb.AppendLine($"FoodCount: {CurrentNumberOfFood}");

			var te = 0;
			for (long botNumber = 1; botNumber <= Data.CurrentNumberOfBots; botNumber++)
			{
				te += Data.Bots[botNumber].Energy;
			}

			sb.AppendLine($"ActualEnergy:{te.ToString("n", f)}");
			//sb.AppendLine($"{(te - TotalEnergy).ToString("n", f)}");
			sb.AppendLine($"{(TotalEnergy + CurrentNumberOfFood * FoodEnergy).ToString("n", f)}");

			return sb.ToString();
		}

		public static string GetText2()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"DrawMode: {DrawMode.ToString()}");
			sb.AppendLine($"DrawType: {DrawType.ToString()}");
			sb.AppendLine($"BotColorMode: {BotColorMode.ToString()}");
			sb.AppendLine($"Parallel: {(Data.Parallel ? "true" : "false")}");
			sb.AppendLine($"Checks: {(Data.Checks ? "true" : "false")}");
			sb.AppendLine($"History: {(Data.HistoryOn ? "true" : "false")}");
			//sb.AppendLine($"Paused Mode: {(Data.PausedMode ? "true" : "false")}");
			return sb.ToString();
		}

		private static GameOptions LoadConfig()
		{
			using (StreamReader r = new StreamReader("config.json"))
			{
				string json = r.ReadToEnd();
				//dynamic array = JsonConvert.DeserializeObject(json);
				GameOptions config = JsonConvert.DeserializeObject<GameOptions>(json);
				return config;
			}
		}

		public static void MapOptions(GameOptions options)
		{
			CellWidth = options.CellWidth;
			CellHeight = options.CellHeight;
			ReportFrequencyDrawed = options.ReportFrequencyDrawed;
			ReportFrequencyNoDrawed = options.ReportFrequencyNoDrawed;
			FrequencyOfPeriodicalDraw = options.FrequencyOfPeriodicalDraw;

			StartNumberOfBots = options.StartBotsNumber;
			MaxBotsNumber = options.MaxBotsNumber;
			WorldWidth = options.WorldWidth;
			WorldHeight = options.WorldHeight;
			UpDownEdge = options.UpDownEdge;
			LeftRightEdge = options.LeftRightEdge;

			SeedFood = options.SeedFood;
			SeedOrganic = options.SeedOrganic;
			SeedMinerals = options.SeedMinerals;
			SeedWalls = options.SeedWalls;
			SeedPoison = options.SeedPoison;
			SeedFoodNumber = options.SeedFoodNumber;
			SeedOrganicNumber = options.SeedOrganicNumber;
			SeedMineralsNumber = options.SeedMineralsNumber;
			SeedWallsNumber = options.SeedWallsNumber;
			SeedPoisonNumber = options.SeedPoisonNumber;

			MaxCode = options.MaxCode;
			MaxCmdInStep = options.MaxCmdInStep;
			MutationProbabilityPercent = options.MutationProbabilityPercent;
			MutationLenght = options.MutationLenght;
			DigestionTypeCount = options.DigestionTypeCount;

			InitialBotEnergy = options.InitialBotEnergy;
			SeedBotEnergy = options.SeedBotEnergy;
			FoodEnergy = options.FoodEnergy;
			ReproductionBotEnergy = options.ReproductionBotEnergy;
			BiteEnergy = options.BiteEnergy;
			DeltaEnergyOnStep = options.DeltaEnergyOnStep;
			PhotosynthesisEnergy = options.PhotosynthesisEnergy;
			PhotosynthesisLayerHeight = options.PhotosynthesisLayerHeight;

			LensWidth = options.LensWidth;
			LensHeight = options.LensHeight;
			LensCellWidth = options.LensCellWidth;
			LensCellHeight = options.LensCellHeight;

			AttackShieldSum = options.AttackShieldSum;
			AttackShieldTypeCount = options.AttackShieldTypeCount;
			AttackShieldTypeCountMax = options.AttackShieldTypeCountMax;
			AttackMax = options.AttackMax;
			ShieldMax = options.ShieldMax;
			AttackTypeCountMax = options.AttackTypeCountMax;

			HoldReproductionTime = options.HoldReproductionTime;
			GenomInfoPeriodPrint = options.GenomInfoPeriodPrint;
			KeptTotalEnergy = options.KeptTotalEnergy;
			MovedBiteStrength = options.MovedBiteStrength;
			DelayForNewbie = options.DelayForNewbie;
		}
	}
}
