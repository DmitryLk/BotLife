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
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
	public class Game
	{
		public Painter _painter;
		public World _world;
		public Tester _test;

		public System.Windows.Forms.Timer timer;
		private GameOptions _cfg;



		public Game(Painter painter, Tester test)
		{
			_cfg = LoadConfig();

			var bitmapWidth = _cfg.WorldWidth * _cfg.BotWidth;
			var bitmapHeight = _cfg.WorldHeight * _cfg.BotHeight;
			_painter = painter;
			_painter.Configure(bitmapWidth, bitmapHeight, _cfg.BotWidth, _cfg.BotHeight, _cfg.ReportFrequency);

			//timer = new System.Windows.Forms.Timer();
			//timer.Tick += new System.EventHandler(timer_Tick);
			//timer.Interval = 1;
			//timer.Enabled = false;

			var worldOptions = new WorldOptions
			{
				StartBotsNumber = _cfg.StartBotsNumber,
				MaxBotsNumber = _cfg.MaxBotsNumber,
				WorldHeight = _cfg.WorldHeight,
				WorldWidth = _cfg.WorldWidth
			};


			_world = new World(worldOptions);

			_test = test;
			_test.InitInterval(1, "BotsAction();");
			_test.InitInterval(2, "RedrawWorld();");
			_test.InitInterval(3, "DrawBotOnFrame(bots[botNumber]);");
			_test.InitInterval(4, "PaintFrame();");
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

		public async Task Start()
		{
			//timer.Enabled = true;
			_test.BeginInterval(1);

			while (true)
			{
				await Step();
			}
		}

		//private void timer_Tick(object sender, EventArgs e)
		//{
		//	Step();
		//}


		private async Task Step()
		{
			await Task.Run(() => _world.Step());
			//await Task.Factory.StartNew(() => WorldStep(), TaskCreationOptions.LongRunning);
			//_world.Step();

			_test.EndBeginInterval(1, 2);

			RedrawWorld();
		}


		private void RedrawWorld()
		{
			_painter.StartNewFrame();
			_test.EndBeginInterval(2, 3);

			for (var botNumber = 0; botNumber < _world.CurrentBotsNumber; botNumber++)
			{
				var bot = _world.Bots[botNumber];

				//_painter.DrawBotOnFrame(bots[botNumber]);
				if (bot.Moved || bot.NoDrawed)
				{
					_painter.DrawBotOnFrame(bot);
					bot.NoDrawed = false;
				}
			}
			_test.EndBeginInterval(3, 4);

			_painter.PaintFrame();
			//await Task.Delay(1);
			_test.EndBeginInterval(4, 1);
		}
	}
}
