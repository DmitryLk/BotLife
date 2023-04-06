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
		public int SeedBotEnergy;

		public int CodeLength;
		public int MaxCode;
		public int MaxUncompleteJump;

		public int FoodEnergy;


		public uint[,] World; // чтобы можно было узнать по координатам что там находится
		public Bot[] Bots;
		public uint[] ChangedBots;
		public uint NumberOfChangedBots;
		public ChangedItem[] ChangedItems;
		public uint NumberOfChangedItems;

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

		public uint CurrentBotsNumber;

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
			SeedBotEnergy = options.SeedBotEnergy;

			CodeLength = options.CodeLength;
			MaxCode = options.MaxCode;
			MaxUncompleteJump = options.MaxUncompleteJump;

			FoodEnergy = options.FoodEnergy;

			World = new uint[WorldWidth, WorldHeight];
		}
	}
}
