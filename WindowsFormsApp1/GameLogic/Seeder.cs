using System;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Xml.Linq;
using WindowsFormsApp1.Enums;

namespace WindowsFormsApp1.GameLogic
{
    public class Seeder
	{
        private uint[,] _world; // чтобы можно было узнать по координатам что там находится

        private RandomService _randomService;
        private WorldData _data;

        public Seeder(WorldData data, RandomService randomService)
        {
			randomService = _randomService;
			_data = data;
		}

		public void SeedBots()
        {
			_data.Bots = new Bot[_data.MaxBotsNumber];
            for (uint botNumber = 1; botNumber <= _data.StartBotsNumber; botNumber++)
            {

				// Координаты бота
				var p = _randomService.GetRandomEmptyPoint();

                // Направление бота
                var dir = _randomService.GetRandomDirection();

                // Скорость бота (?)
                var (vx, vy) = _randomService.GetRandomSpeed();

				// Создание кода бота
				var code = new byte[_data.CodeLength];
				for (var i = 0; i < _data.CodeLength; i++)
				{
					code[i] = _randomService.GetRandomBotCode();
				}
				var pointer = 0;
				var codeHash = Guid.NewGuid();

				var bot = new Bot1(_data, p, dir, botNumber, vx, vy, code, pointer, codeHash);
				_data.Bots[botNumber] = bot;
				_data.World[p.X, p.Y] = botNumber;

			}
			_data.CurrentBotsNumber = _data.StartBotsNumber;
        }


		public void SeedItems()
        {
            // Заполнение Food
            if (_data.SeedFood)
            {
				_data.Grass = new Point[_data.SeedFoodNumber];
                for (var i = 0; i < _data.SeedFoodNumber; i++)
                {
					_data.Grass[i] = _randomService.GetRandomEmptyPoint();
                }
            }

            // Заполнение Organic
            if (_data.SeedOrganic)
            {
				_data.Organic = new Point[_data.SeedOrganicNumber];
                for (var i = 0; i < _data.SeedOrganicNumber; i++)
                {
					_data.Organic[i] = _randomService.GetRandomEmptyPoint();
                }
            }

            // Заполнение Minerals
            if (_data.SeedMinerals)
            {
				_data.Minerals = new Point[_data.SeedMineralsNumber];
                for (var i = 0; i < _data.SeedMineralsNumber; i++)
                {
					_data.Minerals[i] = _randomService.GetRandomEmptyPoint();
                }
            }

            // Заполнение Walls
            if (_data.SeedWalls)
            {
				_data.Walls = new Point[_data.SeedWallsNumber];
                for (var i = 0; i < _data.SeedWallsNumber; i++)
                {
					_data.Walls[i] = _randomService.GetRandomEmptyPoint();
                }
            }

            // Заполнение Poison
            if (_data.SeedPoison)
            {
				_data.Poison = new Point[_data.SeedPoisonNumber];
                for (var i = 0; i < _data.SeedPoisonNumber; i++)
                {
					_data.Poison[i] = _randomService.GetRandomEmptyPoint();
                }
            }
        }
    }
}
