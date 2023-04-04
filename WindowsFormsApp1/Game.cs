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
using WindowsFormsApp1.GameLogic;
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
		private GameOptions _options;



		public Game(Painter painter, Tester test)
		{
			_options = LoadConfig();

			var bitmapWidth = _options.WorldWidth * _options.BotWidth;
			var bitmapHeight = _options.WorldHeight * _options.BotHeight;
			_painter = painter;
			_painter.Configure(bitmapWidth, bitmapHeight, _options.BotWidth, _options.BotHeight, _options.ReportFrequency);

			//timer = new System.Windows.Forms.Timer();
			//timer.Tick += new System.EventHandler(timer_Tick);
			//timer.Interval = 1;
			//timer.Enabled = false;

			_world = new World(_options);

			_test = test;
			_test.InitInterval(0, "BotsAction();");
			_test.InitInterval(1, "RedrawWorld();");
			_test.InitInterval(2, "DrawBotOnFrame(bots[botNumber]);");
			_test.InitInterval(3, "PaintFrame();");
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
			_test.BeginInterval(0);

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

			_test.EndBeginInterval(0, 1);

			RedrawWorld();
		}


		private void RedrawWorld()
		{
			_painter.StartNewFrame();
			_test.EndBeginInterval(1, 2);

			for (var botNumber = 0; botNumber < _world.CurrentBotsNumber; botNumber++)
			{
				var bot = _world.Bots[botNumber];

				if (bot.Moved || bot.P.NoDrawed)
				{
					_painter.DrawBotOnFrame(bot);
					bot.P.NoDrawed = false;
				}
			}
			_test.EndBeginInterval(2, 3);

			_painter.PaintFrame();
			//await Task.Delay(1);
			_test.EndBeginInterval(3, 0);
		}
	}
}
