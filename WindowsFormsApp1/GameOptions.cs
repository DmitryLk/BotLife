using System;

namespace WindowsFormsApp1
{
	public class GameOptions
	{
		public int MaxPictureBoxWidth;
		public int MaxPictureBoxHeight;
		public int CellWidth;
		public int CellHeight;
		public int ReportFrequencyDrawed;
		public int ReportFrequencyNoDrawed;

		public uint StartBotsNumber;
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
	}
}
