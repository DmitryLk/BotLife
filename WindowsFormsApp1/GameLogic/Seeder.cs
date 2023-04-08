using System;
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
		private RandomService _randomService;
		private WorldData _data;

		public Seeder(WorldData data, RandomService randomService)
		{
			randomService = _randomService;
			_data = data;
		}

		public void SeedBots()
		{
			for (uint botNumber = 1; botNumber <= _data.StartBotsNumber; botNumber++)
			{

				// Координаты бота
				var p = _randomService.GetRandomEmptyPoint();

				// Направление бота
				var dir = _randomService.GetRandomDirection();

				// Энергия бота
				var en = _data.InitialBotEnergy;

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

				var bot = new Bot1(_data, p, dir, botNumber, en, vx, vy, code, pointer, codeHash);
				_data.Bots[botNumber] = bot;
				_data.World[p.X, p.Y] = botNumber;

				_data.ChangeCell(p.X, p.Y, RefContent.Bot);
			}
			_data.CurrentBotsNumber = _data.StartBotsNumber;
		}

		public void SeedItems()
		{
			// Заполнение Grass
			if (_data.SeedFood)
			{
				for (var i = 0; i < _data.SeedFoodNumber; i++)
				{
					var p = _randomService.GetRandomEmptyPoint();
					_data.World[p.X, p.Y] = (uint)CellContent.Grass;

					_data.ChangeCell(p.X, p.Y, RefContent.Grass);
				}
			}

			// Заполнение Organic
			if (_data.SeedOrganic)
			{
				for (var i = 0; i < _data.SeedOrganicNumber; i++)
				{
					var p = _randomService.GetRandomEmptyPoint();
					_data.World[p.X, p.Y] = (uint)CellContent.Organic;

					_data.ChangeCell(p.X, p.Y, RefContent.Organic);
				}
			}

			// Заполнение Minerals
			if (_data.SeedMinerals)
			{
				for (var i = 0; i < _data.SeedMineralsNumber; i++)
				{
					var p = _randomService.GetRandomEmptyPoint();
					_data.World[p.X, p.Y] = (uint)CellContent.Mineral;

					_data.ChangeCell(p.X, p.Y, RefContent.Mineral);
				}
			}

			// Заполнение Walls
			if (_data.SeedWalls)
			{
				for (var i = 0; i < _data.SeedWallsNumber; i++)
				{
					var p = _randomService.GetRandomEmptyPoint();
					_data.World[p.X, p.Y] = (uint)CellContent.Wall;

					_data.ChangeCell(p.X, p.Y, RefContent.Wall);
				}
			}

			// Заполнение Poison
			if (_data.SeedPoison)
			{
				for (var i = 0; i < _data.SeedPoisonNumber; i++)
				{
					var p = _randomService.GetRandomEmptyPoint();
					_data.World[p.X, p.Y] = (uint)CellContent.Poison;

					_data.ChangeCell(p.X, p.Y, RefContent.Poison);
				}
			}
		}
	}
}
