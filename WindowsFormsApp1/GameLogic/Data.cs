using System;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Xml.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;

namespace WindowsFormsApp1.GameLogic
{
	public class WorldData
	{
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

		public uint[,] World; // чтобы можно было узнать по координатам что там находится
		public Bot[] Bots;


		//Для поддержки эффективной перерисовки
		public uint[,] ChWorld;
		public ChangedCell[] ChangedCells;
		public uint NumberOfChangedCells;


		public uint CurrentBotsNumber;

		//public Point[] Grass;
		//public Point[] Organic;
		//public Point[] Minerals;
		//public Point[] Walls;
		//public Point[] Poison;

		//public uint[] ChangedGrass;
		//public uint[] ChangedOrganic;
		//public uint[] ChangedMinerals;
		//public uint[] ChangedWalls;
		//public uint[] ChangedPoison;

		//public uint NumberOfChangedGrass;
		//public uint NumberOfChangedOrganic;
		//public uint NumberOfChangedMinerals;
		//public uint NumberOfChangedWalls;
		//public uint NumberOfChangedPoison;


		public WorldData(GameOptions options)
		{
			StartBotsNumber = options.StartBotsNumber;
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

			// Создать все игровые массивы
			World = new uint[WorldWidth, WorldHeight];
			Bots = new Bot[MaxBotsNumber];
			ChWorld = new uint[WorldWidth, WorldHeight];
			ChangedCells = new ChangedCell[MaxBotsNumber];
		}
	}
}
