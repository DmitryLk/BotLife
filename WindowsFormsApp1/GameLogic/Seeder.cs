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
		private Func _func;
		private GameData _data;

		public Seeder(GameData data, Func func)
		{
			_func = func;
			_data = data;
		}

		public void SeedBots()
		{
			for (uint botIndex = 1; botIndex <= _data.StartNumberOfBots; botIndex++)
			{

				// Координаты бота
				var p = _func.GetRandomFreeCell();

				// Создание кода бота
				var code = new byte[_data.CodeLength];
				for (var i = 0; i < _data.CodeLength; i++)
				{
					code[i] = _func.GetRandomBotCode();
				}
				var codeHash = Guid.NewGuid();
				var color = _func.GetRandomColor();

				_func.CreateNewBot(p, botIndex, code, codeHash, Guid.Empty, Guid.Empty, color);
			}
			_data.CurrentNumberOfBots = _data.StartNumberOfBots;
			//Bots: 0[пусто] 1[бот _ind=1] 2[бот _ind=2]; StartBotsNumber=2 CurrentBotsNumber=2
		}

		public void SeedItems()
		{
			// Заполнение Grass
			if (_data.SeedFood)
			{
				for (var i = 0; i < _data.SeedFoodNumber; i++)
				{
					var p = _func.GetRandomFreeCell();

					_data.World[p.X, p.Y] = (uint)CellContent.Grass;
					_func.ChangeCell(p.X, p.Y, Color.Green);
				}
			}

			// Заполнение Organic
			if (_data.SeedOrganic)
			{
				for (var i = 0; i < _data.SeedOrganicNumber; i++)
				{
					var p = _func.GetRandomFreeCell();

					_data.World[p.X, p.Y] = (uint)CellContent.Organic;
					_func.ChangeCell(p.X, p.Y, Color.Black);
				}
			}

			// Заполнение Minerals
			if (_data.SeedMinerals)
			{
				for (var i = 0; i < _data.SeedMineralsNumber; i++)
				{
					var p = _func.GetRandomFreeCell();

					_data.World[p.X, p.Y] = (uint)CellContent.Mineral;
					_func.ChangeCell(p.X, p.Y, Color.Black);
				}
			}

			// Заполнение Walls
			if (_data.SeedWalls)
			{
				for (var i = 0; i < _data.SeedWallsNumber; i++)
				{
					var p = _func.GetRandomFreeCell();

					_data.World[p.X, p.Y] = (uint)CellContent.Wall;
					_func.ChangeCell(p.X, p.Y, Color.Black);
				}
			}

			// Заполнение Poison
			if (_data.SeedPoison)
			{
				for (var i = 0; i < _data.SeedPoisonNumber; i++)
				{
					var p = _func.GetRandomFreeCell();

					_data.World[p.X, p.Y] = (uint)CellContent.Poison;
					_func.ChangeCell(p.X, p.Y, Color.Black);
				}
			}
		}
	}
}
