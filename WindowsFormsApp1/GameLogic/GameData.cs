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
using static System.Windows.Forms.Design.AxImporter;

namespace WindowsFormsApp1.GameLogic
{
	public class GameData
	{
		//INITIAL SETTINGS
		public int CellWidth;
		public int CellHeight;
		public int ReportFrequencyDrawed;
		public int ReportFrequencyNoDrawed;

		public uint StartNumberOfBots;
		public uint MaxBotsNumber;
		public int WorldWidth;
		public int WorldHeight;
		public bool UpDownEdge;
		public bool LeftRightEdge;

		public bool SeedFood;
		public bool SeedOrganic;
		public bool SeedMinerals;
		public bool SeedWalls;
		public bool SeedPoison;
		public int SeedFoodNumber;
		public int SeedOrganicNumber;
		public int SeedMineralsNumber;
		public int SeedWallsNumber;
		public int SeedPoisonNumber;

		public int CodeLength;
		public int MaxCode;
		public int MaxUncompleteJump;
		public int MutationProbabilityPercent;

		public int FoodEnergy;
		public int InitialBotEnergy;
		public int ReproductionBotEnergy;
		public int BiteEnergy;
		public int DeltaEnergyOnStep;

		public int LensWidth;
		public int LensHeight;


		//VARIABLE CURRENT PARAMETERS
		public uint[,] World; // чтобы можно было узнать по координатам что там находится
		public Bot[] Bots;

		// Настройки игры
		public bool Started;
		public bool PausedMode;
		public bool Drawed;
		public bool Worked;
		public bool Lens;
		public bool Mutation;


		//Для поддержки эффективной перерисовки
		public uint[,] ChWorld;
		public ChangedCell[] ChangedCells;
		public uint NumberOfChangedCells;
		public uint NumberOfChangedCellsForInfo;

		public uint CurrentNumberOfBots;
		public uint CurrentStep;
		public uint MaxBotNumber;
		public uint DeathCnt;
		public uint ReproductionCnt;
		public uint MutationCnt;
		public int ReportFrequencyCurrent;

		public int LensX;
		public int LensY;


		public GameData()
		{
		}

		public void Initialize()
		{
            var options = LoadConfig();
            MapOptions(options);

			// Создать все игровые массивы
			World = new uint[WorldWidth, WorldHeight];
			Bots = new Bot[MaxBotsNumber];
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
		}

		public string GetText(double fps)
		{
			var sb = new StringBuilder();

			sb.AppendLine($"Fps: {fps.ToString("#")}");
			sb.AppendLine($"Step: {CurrentStep}");
			sb.AppendLine($"CurrentNumberOfBots: {CurrentNumberOfBots}");
			sb.AppendLine($"NumberOfChangedCells: {NumberOfChangedCellsForInfo}");

			sb.AppendLine($"deathCnt: {DeathCnt}");
			sb.AppendLine($"reproductionCnt: {ReproductionCnt}");
			sb.AppendLine($"mutationCnt: {MutationCnt}");
			return sb.ToString();
		}

        private GameOptions LoadConfig()
        {
            using (StreamReader r = new StreamReader("config.json"))
            {
                string json = r.ReadToEnd();
                //dynamic array = JsonConvert.DeserializeObject(json);
                GameOptions config = JsonConvert.DeserializeObject<GameOptions>(json);
                return config;
            }
        }

        public void MapOptions(GameOptions options)
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

            CodeLength = options.CodeLength;
            MaxCode = options.MaxCode;
            MaxUncompleteJump = options.MaxUncompleteJump;
            MutationProbabilityPercent = options.MutationProbabilityPercent;

            InitialBotEnergy = options.InitialBotEnergy;
            FoodEnergy = options.FoodEnergy;
            ReproductionBotEnergy = options.ReproductionBotEnergy;
            BiteEnergy = options.BiteEnergy;
            DeltaEnergyOnStep = options.DeltaEnergyOnStep;

            LensWidth = options.LensWidth;
            LensHeight = options.LensHeight;
        }
	}
}
