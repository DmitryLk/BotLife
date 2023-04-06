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
		public Painter _painter;
		public World _world;
		public WorldData _data;
		public Tester _test;
		public System.Windows.Forms.Timer timer;

		public Game(Painter painter, Tester test)
		{
			var options = LoadConfig();

			var bitmapWidth = options.WorldWidth * options.CellWidth;
			var bitmapHeight = options.WorldHeight * options.CellHeight;
			_painter = painter;
			_painter.Configure(bitmapWidth, bitmapHeight, options.CellWidth, options.CellHeight, options.ReportFrequency);

			//timer = new System.Windows.Forms.Timer();
			//timer.Tick += new System.EventHandler(timer_Tick);
			//timer.Interval = 1;
			//timer.Enabled = false;

			_data = new WorldData(options);
			_world = new World(_data);

			_test = test;
			_test.InitInterval(0, "BotsAction();");
			_test.InitInterval(1, "RedrawWorld();");
			_test.InitInterval(2, "DrawBotOnFrame(bots[botNumber]);");
			_test.InitInterval(3, "PaintFrame();");
		}

		public async Task Start()
		{
			//timer.Enabled = true;
			_world.Initialize();
			_test.BeginInterval(0);

			while (true)
			{
				await Step();
			}
		}

		private async Task Step()
		{
			await Task.Run(() => _world.Step());
			//await Task.Factory.StartNew(() => WorldStep(), TaskCreationOptions.LongRunning);
			//_world.Step();
			_test.EndBeginInterval(0, 1);
			RedrawWorld();
		}

		private void RedrawWorld()
		{
			_painter.StartNewFrame();
			_test.EndBeginInterval(1, 2);

			for (var i = 0; i < _data.NumberOfChangedBots; i++)
			{
				var bot = _data.Bots[_data.ChangedBots[i]];

				_painter.DrawBotOnFrame(bot);
			}
			_data.NumberOfChangedBots = 0;

			for (var i = 0; i < _data.NumberOfChangedItems; i++)
			{
				var item = _data.ChangedItems[i];

				_painter.DrawItemOnFrame(item);
			}
			_data.NumberOfChangedItems = 0;

			_test.EndBeginInterval(2, 3);

			_painter.PaintFrame();
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
