using System;
using System.Drawing;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Xml.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;

namespace WindowsFormsApp1.GameLogic
{
	public class Seeder
	{
		public Seeder()
		{
		}

		public void SeedBots()
		{
			for (uint botIndex = 1; botIndex <= Data.StartNumberOfBots; botIndex++)
			{

				// Координаты бота
				var p = Func.GetRandomFreeCell();

				// Создание кода бота
				var genom = Genom.CreateNewGenom();

				Func.CreateNewBot(p, botIndex, genom);
			}
			Data.CurrentNumberOfBots = Data.StartNumberOfBots;
			//Bots: 0[пусто] 1[бот _ind=1] 2[бот _ind=2]; StartBotsNumber=2 CurrentBotsNumber=2
		}

		public void SeedItems()
		{
			// Заполнение Grass
			if (Data.SeedFood)
			{
				for (var i = 0; i < Data.SeedFoodNumber; i++)
				{
					var p = Func.GetRandomFreeCell();
					Data.TotalEnergy += Data.FoodEnergy;

					Data.World[p.X, p.Y] = (uint)CellContent.Grass;
					Func.ChangeCell(p.X, p.Y, Color.Green);
				}
			}

			// Заполнение Organic
			if (Data.SeedOrganic)
			{
				for (var i = 0; i < Data.SeedOrganicNumber; i++)
				{
					var p = Func.GetRandomFreeCell();

					Data.World[p.X, p.Y] = (uint)CellContent.Organic;
					Func.ChangeCell(p.X, p.Y, Color.Black);
				}
			}

			// Заполнение Minerals
			if (Data.SeedMinerals)
			{
				for (var i = 0; i < Data.SeedMineralsNumber; i++)
				{
					var p = Func.GetRandomFreeCell();

					Data.World[p.X, p.Y] = (uint)CellContent.Mineral;
					Func.ChangeCell(p.X, p.Y, Color.Black);
				}
			}

			// Заполнение Walls
			if (Data.SeedWalls)
			{
				for (var i = 0; i < Data.SeedWallsNumber; i++)
				{
					var p = Func.GetRandomFreeCell();

					Data.World[p.X, p.Y] = (uint)CellContent.Wall;
					Func.ChangeCell(p.X, p.Y, Color.Black);
				}
			}

			// Заполнение Poison
			if (Data.SeedPoison)
			{
				for (var i = 0; i < Data.SeedPoisonNumber; i++)
				{
					var p = Func.GetRandomFreeCell();

					Data.World[p.X, p.Y] = (uint)CellContent.Poison;
					Func.ChangeCell(p.X, p.Y, Color.Black);
				}
			}
		}
	}
}
