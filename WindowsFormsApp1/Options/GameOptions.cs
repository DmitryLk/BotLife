using System;

namespace WindowsFormsApp1.Options
{
    public class GameOptions
    {
        public int MaxPictureBoxWidth;
        public int MaxPictureBoxHeight;
        public int CellWidth;
        public int CellHeight;
        public int ReportFrequencyDrawed;
		public int ReportFrequencyNoDrawed;
		public int FrequencyOfPeriodicalDraw;

		public long StartBotsNumber;
        public long MaxBotsNumber;
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

        public int GenomLength;
        public int MaxCode;
        public int MaxUncompleteJump;
        public float MutationProbabilityPercent;
        public int MutationLenght;

        public int FoodEnergy;
        public int SeedBotEnergy;
        public int InitialBotEnergy;
        public int ReproductionBotEnergy;
        public int BiteEnergy;
        public int DeltaEnergyOnStep;
        public int PhotosynthesisEnergy;
        public int PhotosynthesisLayerHeight;

        public int LensWidth;
        public int LensHeight;
        public int LensCellWidth;
        public int LensCellHeight;
    }
}
