using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.GameLogic;
using static System.Windows.Forms.Design.AxImporter;

namespace WindowsFormsApp1.Static
{
	public static class Data
	{
		//INITIAL SETTINGS
		public static int CellWidth;
		public static int CellHeight;
		public static int ReportFrequencyDrawed;
		public static int ReportFrequencyNoDrawed;

		public static uint StartNumberOfBots;
		public static uint MaxBotsNumber;
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


		public static int LensWidth;
		public static int LensHeight;
		public static int LensCellWidth;
		public static int LensCellHeight;


		//VARIABLE CURRENT PARAMETERS
		public static uint[,] World; // чтобы можно было узнать по координатам что там находится
		public static Bot1[] Bots;
		public static int TotalEnergy;

		// Настройки игры
		public static bool Started;
		public static bool PausedMode;
		public static bool Drawed;
		public static bool Worked;
		public static bool Lens;
		public static bool Mutation;


		//Для поддержки эффективной перерисовки
		public static uint[,] ChWorld;
		public static ChangedCell[] ChangedCells;
		public static uint NumberOfChangedCells;
		public static uint NumberOfChangedCellsForInfo;

		public static uint CurrentNumberOfBots;
		public static uint CurrentStep;
		public static uint MaxBotNumber;
		public static uint DeathCnt;
		public static uint ReproductionCnt;
		public static uint MutationCnt;
		public static int ReportFrequencyCurrent;

		public static int LensX;
		public static int LensY;
		public static int CursorX;
		public static int CursorY;


		public static void Initialize()
		{
			var options = LoadConfig();
			MapOptions(options);

			// Создать все игровые массивы
			World = new uint[WorldWidth, WorldHeight];
			Bots = new Bot1[MaxBotsNumber];
			ChWorld = new uint[WorldWidth, WorldHeight];
			ChangedCells = new ChangedCell[MaxBotsNumber];

			Mutation = true;
			Started = false;
			PausedMode = false;
			Drawed = true;
			Lens = false;

			NumberOfChangedCells = 0;
			MaxBotNumber = 0;
			CurrentStep = 0;


			DeathCnt = 0;
			ReproductionCnt = 0;
			MutationCnt = 0;

			ReportFrequencyCurrent = ReportFrequencyDrawed;

			LensX = 10;
			LensY = 10;
			CursorX = 10;
			CursorY = 10;
		}

		public static string GetText(double fps)
		{
			var sb = new StringBuilder();

			sb.AppendLine($"Fps: {fps.ToString("#")}");
			sb.AppendLine($"Step: {CurrentStep}");
			sb.AppendLine($"Bots: {CurrentNumberOfBots}");
			sb.AppendLine($"ChangedCells: {NumberOfChangedCellsForInfo}");

			sb.AppendLine($"deathCnt: {DeathCnt}");
			sb.AppendLine($"reproductionCnt: {ReproductionCnt}");
			sb.AppendLine($"mutationCnt: {MutationCnt}");
			sb.AppendLine($"TotalEnergy: {TotalEnergy}");

			var te = 0;
			for (uint botNumber = 1; botNumber <= Data.CurrentNumberOfBots; botNumber++)
			{
				te += Data.Bots[botNumber].Energy;
			}

			sb.AppendLine($"Actual total energy:{te}");

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
			LensWidth = options.LensWidth;
			LensHeight = options.LensHeight;
			LensCellWidth = options.LensCellWidth;
			LensCellHeight = options.LensCellHeight;
		}
	}
}
