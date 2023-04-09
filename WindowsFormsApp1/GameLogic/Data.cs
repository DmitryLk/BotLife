using System;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Xml.Linq;
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
		public int ReportFrequencyCurrent;

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


		//VARIABLE CURRENT PARAMETERS
		public uint[,] World; // чтобы можно было узнать по координатам что там находится
		public Bot[] Bots;


		//Для поддержки эффективной перерисовки
		public uint[,] ChWorld;
		public ChangedCell[] ChangedCells;
		public uint NumberOfChangedCells;

		public bool Mutation;
		public uint CurrentNumberOfBots;
		public uint CurrentStep;
		public uint MaxBotNumber;
		public uint DeathCnt;
		public uint ReproductionCnt;
		public uint MutationCnt;

		public GameData(GameOptions options)
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

	}

	public string GetText(double fps)
		{
			var sb = new StringBuilder();

			sb.AppendLine($"Fps: {fps.ToString("#")}");
			sb.AppendLine($"Step: {CurrentStep}");
			sb.AppendLine($"CurrentNumberOfBots: {CurrentNumberOfBots}");
			sb.AppendLine($"NumberOfChangedCells: {NumberOfChangedCells}");

			sb.AppendLine($"deathCnt: {DeathCnt}");
			sb.AppendLine($"reproductionCnt: {ReproductionCnt}");
			sb.AppendLine($"mutationCnt: {MutationCnt}");
			return sb.ToString();
		}
	}
}
