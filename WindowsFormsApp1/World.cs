using System;

namespace WindowsFormsApp1
{
	public class World
	{
		private int[,] world;
		public Bot[] Bots;
		public int CurrentBotsNumber;
		Random _rnd = new Random(Guid.NewGuid().GetHashCode());

		public World(WorldOptions options)
		{
			// CREATE WORLD
			world = new int[options.WorldWidth, options.WorldHeight];
			Bots = new Bot[options.MaxBotsNumber];

			// Создание ботов
			for (var botNumber = 0; botNumber < options.StartBotsNumber; botNumber++)
			{
				Bots[botNumber] = new Bot(_rnd, options.WorldWidth, options.WorldHeight);
			}
			CurrentBotsNumber = options.StartBotsNumber;

		}

		public void Step()
		{
			//Parallel.For(0, currentBotsNumber, i => Bots[i].Move());

			for (var botNumber = 0; botNumber < CurrentBotsNumber; botNumber++)
			{
				Bots[botNumber].Live();
				//Bots[botNumber].Move();
			}
		}
	}
}
