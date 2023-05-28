using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.GameLogic;
using WindowsFormsApp1.Options;
using static System.Windows.Forms.Design.AxImporter;

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

		public static int GenomLength;
		public static int MaxCode;
		public static int MaxUncompleteJump;
		public static float MutationProbabilityPercent;
		public static int MutationLenght;

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


		//=================VARIABLE CURRENT PARAMETERS==============================
		public static long[,] World; // чтобы можно было узнать по координатам что там находится
		public static Bot1[] Bots;
		public static long TotalEnergy;
		public static long SeedTotalEnergy;

		// Death Reproduction		
        public static Bot1[] BotDeath;
		public static Bot1[] BotReproduction;
        public static long NumberOfBotDeath;
        public static long NumberOfBotDeathFactCnt;
        public static long NumberOfBotDeathForReproduction;
		public static long NumberOfBotReproduction;
		public static long DeathCnt;
		public static long ReproductionCnt;

		// Настройки игры
		public static bool Started;
		public static bool PausedMode;
		public static bool Worked;
		public static bool LensOn;
		public static bool Mutation;
		public static bool Parallel;
		public static bool Checks;
		public static DrawMode DrawMode;
		public static DrawMode NextDrawMode;
		public static DrawType DrawType;
		public static DrawType NextDrawType;
		public static BotColorMode BotColorMode;
		public static BotColorMode NextBotColorMode;


		//Для поддержки эффективной перерисовки
		public static long[,] ChWorld;
		public static ChangedCell[] ChangedCells;
		public static long NumberOfChangedCells;
		public static long NumberOfChangedCellsForInfo;
		public static Color[] GrColors;

		public static long CurrentNumberOfBots;
		public static uint CurrentStep;
		public static long MaxBotNumber;
		public static long MutationCnt;
		public static int ReportFrequencyCurrent;

		public static int LensX;
		public static int LensY;
		public static int CursorX;
		public static int CursorY;
		public static int DeltaHistory;


		public static void Initialize()
		{
			var options = LoadConfig();
			MapOptions(options);

			// Создать все игровые массивы
			World = new long[WorldWidth, WorldHeight];
			Bots = new Bot1[MaxBotsNumber];
			ChWorld = new long[WorldWidth, WorldHeight];
			ChangedCells = new ChangedCell[MaxBotsNumber];
			GrColors = new Color[361];

			BotDeath = new Bot1[3000];
			BotReproduction = new Bot1[5000];
			NumberOfBotDeath = -1;
            NumberOfBotDeathFactCnt = 0;
			NumberOfBotReproduction = -1;
			DeathCnt = 0;
			ReproductionCnt = 0;

			Mutation = true;
			Started = false;
			PausedMode = false;
			LensOn = false;
			Parallel = true;
			Checks = true;

			DrawMode = DrawMode.EachStep;
			NextDrawMode = DrawMode.EachStep;
			BotColorMode = BotColorMode.GenomColor;
			NextBotColorMode = BotColorMode.GenomColor;
			DrawType = DrawType.OnlyChangedCells;
			NextDrawType = DrawType.OnlyChangedCells;

			NumberOfChangedCells = 0;
			MaxBotNumber = 0;
			CurrentStep = 0;
			MutationCnt = 0;

			ReportFrequencyCurrent = ReportFrequencyDrawed;

			LensX = 10;
			LensY = 10;
			CursorX = 10;
			CursorY = 10;

			for (var hue = 0; hue <= 360; hue++)
			{
				GrColors[hue] = ColorFromHSV(hue, 1, 1);
			}
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

			sb.AppendLine($"deathCnt: {DeathCnt}");
			sb.AppendLine($"reproductionCnt: {ReproductionCnt}");
			sb.AppendLine($"mutationCnt: {MutationCnt}");
			sb.AppendLine($"TotalEnergy: {TotalEnergy.ToString("n", f)}");

			var te = 0;
			for (long botNumber = 1; botNumber <= Data.CurrentNumberOfBots; botNumber++)
			{
				te += Data.Bots[botNumber].Energy;
			}

			sb.AppendLine($"ActualEnergy:{te.ToString("n", f)}");

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

			GenomLength = options.GenomLength;
			MaxCode = options.MaxCode;
			MaxUncompleteJump = options.MaxUncompleteJump;
			MutationProbabilityPercent = options.MutationProbabilityPercent;
			MutationLenght = options.MutationLenght;

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
		}
	}
}
