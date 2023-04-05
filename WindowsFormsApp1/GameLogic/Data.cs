using System;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Xml.Linq;
using WindowsFormsApp1.Enums;

namespace WindowsFormsApp1.GameLogic
{
    public class WorldData
    {
        public uint[,] World; // чтобы можно было узнать по координатам что там находится

        public int WorldWidth;
        public int WorldHeight;
        public uint StartBotsNumber;
        public uint MaxBotsNumber;
        public int CodeLength;
        public int MaxCode;
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


        public Bot[] Bots;
        public Point[] Grass;
        public Point[] Organic;
        public Point[] Minerals;
        public Point[] Walls;
        public Point[] Poison;

        public uint CurrentBotsNumber;

        public WorldData(GameOptions options)
        {
            WorldWidth = options.WorldWidth;
            WorldHeight = options.WorldHeight;
            StartBotsNumber = options.StartBotsNumber;
            MaxBotsNumber = options.MaxBotsNumber;
            CodeLength = options.CodeLength;
            MaxCode = options.MaxCode;
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

            World = new uint[WorldWidth, WorldHeight];
        }

		public (int dX, int dY) GetDeltaDirection(Direction dir1, Direction dir2)
		{
			return (((int)dir1 + (int)dir2) % 8) switch
			{
				0 => (0, -1),
				1 => (1, -1),
				2 => (1, 0),
				3 => (1, 1),
				4 => (0, 1),
				5 => (-1, 1),
				6 => (-1, 0),
				7 => (-1, -1),
				_ => throw new Exception("return (((int)dir1 + (int)dir2) % 8) switch"),
			};
		}

		public (int dX, int dY) GetDelta(Direction dir)
		{
			return dir switch
			{
				Direction.Up => (0, -1),
				Direction.UpRight => (1, -1),
				Direction.Right => (1, 0),
				Direction.DownRight => (1, 1),
				Direction.Down => (0, 1),
				Direction.DownLeft => (-1, 1),
				Direction.Left => (-1, 0),
				Direction.UpLeft => (-1, -1),
				_ => throw new Exception("return (((int)dir1 + (int)dir2) % 8) switch"),
			};
		}
	}
}
