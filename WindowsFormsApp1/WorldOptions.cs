using System;

namespace WindowsFormsApp1
{
	public class WorldOptions
	{
		public int StartBotsNumber;
		public int MaxBotsNumber;
		public int WorldWidth;
		public int WorldHeight;

        public class SettleObjectsSettings
        {
            public bool SettleFood;
            public float SettleFoodСhance;

            public bool SettleWalls;
            public float SettleWallsСhance;

            public bool SettleMinerals;
            public float SettleMIneralsСhance;

            public bool SettleOrganic;
            public float SettleOrganicСhance;
        }
    }
}
