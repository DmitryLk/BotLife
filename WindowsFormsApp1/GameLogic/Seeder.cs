using System;
using System.Drawing;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
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
            for (uint botIndex = 1; botIndex <= Data.StartNumberOfBots; botIndex++)
            {

                // Координаты бота
                var (x, y) = Func.GetRandomFreeCell();

                // Создание кода бота
                var genom = Genom.CreateGenom();

                Func.CreateNewBot(x, y, botIndex, Data.SeedBotEnergy, genom);
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
                    var (x, y) = Func.GetRandomFreeCell();
                    Data.TotalEnergy += Data.FoodEnergy;

                    Data.World[x, y] = (uint)CellContent.Grass;
                    Func.ChangeCell(x, y, Color.Green);
                }
            }

            // Заполнение Organic
            if (Data.SeedOrganic)
            {
                for (var i = 0; i < Data.SeedOrganicNumber; i++)
                {
                    var (x, y) = Func.GetRandomFreeCell();

                    Data.World[x, y] = (uint)CellContent.Organic;
                    Func.ChangeCell(x, y, Color.Black);
                }
            }

            // Заполнение Minerals
            if (Data.SeedMinerals)
            {
                for (var i = 0; i < Data.SeedMineralsNumber; i++)
                {
                    var (x, y) = Func.GetRandomFreeCell();

                    Data.World[x, y] = (uint)CellContent.Mineral;
                    Func.ChangeCell(x, y, Color.Black);
                }
            }

            // Заполнение Walls
            if (Data.SeedWalls)
            {
                for (var i = 0; i < Data.SeedWallsNumber; i++)
                {
                    var (x, y) = Func.GetRandomFreeCell();

                    Data.World[x, y] = (uint)CellContent.Wall;
                    Func.ChangeCell(x, y, Color.Black);
                }
            }

            // Заполнение Poison
            if (Data.SeedPoison)
            {
                for (var i = 0; i < Data.SeedPoisonNumber; i++)
                {
                    var (x, y) = Func.GetRandomFreeCell();

                    Data.World[x, y] = (uint)CellContent.Poison;
                    Func.ChangeCell(x, y, Color.Black);
                }
            }
        }
    }
}
