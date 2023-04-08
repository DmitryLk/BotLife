using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.GameLogic;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
	public class Game
	{
		public Presenter _presenter;
		public World _world;
		public WorldData _data;
		public Tester _test;
		public System.Windows.Forms.Timer timer;

		public bool Started;
		public bool PausedMode;
		public Game(Presenter presenter, Tester test)
		{
			var options = LoadConfig();
			_data = new WorldData(options);

			var bitmapWidth = options.WorldWidth * options.CellWidth;
			var bitmapHeight = options.WorldHeight * options.CellHeight;
			_presenter = presenter;
			_presenter.Configure(_data, bitmapWidth, bitmapHeight, options.CellWidth, options.CellHeight, options.ReportFrequency);

			//timer = new System.Windows.Forms.Timer();
			//timer.Tick += new System.EventHandler(timer_Tick);
			//timer.Interval = 1;
			//timer.Enabled = false;

			_world = new World(_data);

			_test = test;
			_test.InitInterval(0, "BotsAction();");
			_test.InitInterval(1, "RedrawWorld();");
			_test.InitInterval(2, "DrawBotOnFrame(bots[botNumber]);");
			_test.InitInterval(3, "PaintFrame();");

			Started = false;
			PausedMode = true;
		}

		public async Task Start()
		{
			Started = true;
			//timer.Enabled = true;
			_world.Initialize();
			_test.BeginInterval(0);

			while (!PausedMode)
			{
				await Step();
			}
		}

		public async Task Work()
		{
			do
			{
				await Step();
			}
			while (!PausedMode);
		}

		public async Task Step()
		{
			 await Task.Run(() => _world.Step());
			//await Task.Factory.StartNew(() => WorldStep(), TaskCreationOptions.LongRunning);
			//_world.Step();
			_test.EndBeginInterval(0, 1);
			RedrawWorld();
			//await Task.Delay(5000);
		}

		private void RedrawWorld()
		{
			_presenter.StartNewFrame();
			_test.EndBeginInterval(1, 2);

			// Рисование изменений на битмапе экрана (сразу не отображаются)
			for (var i = 0; i < _data.NumberOfChangedCells; i++)
			{
				var obj = _data.ChangedCells[i];

				_presenter.DrawCellOnFrame(obj.X, obj.Y, obj.RefContent switch 
				{ 
					RefContent.Free => null,
					RefContent.Grass => Color.Green,
					RefContent.Bot => Color.Red,
					RefContent.Relative => Color.Red,
					_ => Color.Black
				});
				_data.ChWorld[obj.X, obj.Y] = 0;
			}
			_data.NumberOfChangedCells = 0;
			//======================================

			_test.EndBeginInterval(2, 3);

			_presenter.PaintFrame();
			//await Task.Delay(1);
			_test.EndBeginInterval(3, 0);
		}

		private GameOptions LoadConfig()
		{
			using (StreamReader r = new StreamReader("config.json"))
			{
				string json = r.ReadToEnd();
				//dynamic array = JsonConvert.DeserializeObject(json);
				GameOptions config = JsonConvert.DeserializeObject<GameOptions>(json);
				return config;
			}
		}

		//private void timer_Tick(object sender, EventArgs e)
		//{
		//	Step();
		//}
	}
}
