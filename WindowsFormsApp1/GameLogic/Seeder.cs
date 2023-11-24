using System;
using System.Drawing;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Xml.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.Static;

namespace WindowsFormsApp1.GameLogic
{
    public class Seeder
    {
        public Seeder()
        {
        }

        public void SeedBots()
        {
            for (long botIndex = 1; botIndex <= Data.StartNumberOfBots; botIndex++)
            {
				// Координаты бота
				if (Func.TryGetRandomFreeCell(out var x, out var y))
				{
					// Создание кода бота
					var genom = Genom.CreateNewGenom();

					Data.TotalEnergy += Data.SeedBotEnergy;
					Func.CreateNewBot(x, y, botIndex, Data.SeedBotEnergy, genom);
                    Data.CurrentNumberOfBots++;
				}
            }
        }

        public void SeedItems()
        {
			// Заполнение Walls
			if (Data.SeedWalls)
			{
				for (var x = 0; x < Data.WorldWidth; x += 100)
				{
					for (var y = 0; y < Data.WorldHeight; y++)
					{

						Data.World[x, y] = (long)CellContent.Wall;
						Func.FixChangeCell(x, y, Color.Black);
					}
				}

				for (var y = 0; y < Data.WorldWidth; y += 100)
				{
					for (var x = 0; x < Data.WorldHeight; x++)
					{

						Data.World[x, y] = (long)CellContent.Wall;
						Func.FixChangeCell(x, y, Color.Black);
					}
				}

				Data.GridWalls = true;
				//for (var i = 0; i < Data.SeedWallsNumber; i++)
				//{
				//	if (Func.TryGetRandomFreeCell(out var x, out var y))
				//	{
				//		Data.World[x, y] = (long)CellContent.Wall;
				//		Func.FixChangeCell(x, y, Color.Black);
				//	}
				//}
			}

			// Заполнение Grass
			if (Data.SeedFood)
            {
                for (var i = 0; i < Data.SeedFoodNumber; i++)
                {
                    if (Func.TryGetRandomFreeCell(out var x, out var y))
                    {
						Interlocked.Increment(ref Data.CurrentNumberOfFood);

						Data.World[x, y] = (long)CellContent.Grass;
						Func.FixChangeCell(x, y, Color.Green);
					}
                }
            }

            // Заполнение Organic
            if (Data.SeedOrganic)
            {
                for (var i = 0; i < Data.SeedOrganicNumber; i++)
                {
                    if (Func.TryGetRandomFreeCell(out var x, out var y))
                    {
						Data.World[x, y] = (long)CellContent.Organic;
						Func.FixChangeCell(x, y, Color.Black);
					}
                }
            }

            // Заполнение Minerals
            if (Data.SeedMinerals)
            {
                for (var i = 0; i < Data.SeedMineralsNumber; i++)
                {
					if (Func.TryGetRandomFreeCell(out var x, out var y))
					{
						Data.World[x, y] = (long)CellContent.Mineral;
						Func.FixChangeCell(x, y, Color.Black);
					}
                }
            }

            // Заполнение Poison
            if (Data.SeedPoison)
            {
                for (var i = 0; i < Data.SeedPoisonNumber; i++)
                {
					if (Func.TryGetRandomFreeCell(out var x, out var y))
					{
						Data.World[x, y] = (long)CellContent.Poison;
						Func.FixChangeCell(x, y, Color.Black);
					}
                }
            }
        }
    }
}
