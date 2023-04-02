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
	public class Main
	{
		public System.Windows.Forms.Timer timer;
		private int[,] world;
		private Bot[] bots;
		private int currentBotsNumber;
		private WorldOptions _cfg;

		public Painter _painter;
		public World _world;
		public Tester _test;


		Random _rnd = new Random(Guid.NewGuid().GetHashCode());

		public Main(Painter painter, Tester test)
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

			_world = new World();


			_test = test;
			_test.InitInterval(1, "BotsAction();");
			_test.InitInterval(2, "RedrawWorld();");
			_test.InitInterval(3, "DrawBotOnFrame(bots[botNumber]);");
			_test.InitInterval(4, "PaintFrame();");

			// CREATE WORLD
			world = new int[_cfg.WorldWidth, _cfg.WorldHeight];
			bots = new Bot[_cfg.MaxBotsNumber];

			// Создание ботов
			for (var botNumber = 0; botNumber < _cfg.StartBotsNumber; botNumber++)
			{
				bots[botNumber] = new Bot(_rnd, _cfg.WorldWidth, _cfg.WorldHeight);
			}
			currentBotsNumber = _cfg.StartBotsNumber;
		}

		public void CreateWorld()
		{
		}

		private WorldOptions LoadConfig() 
		{
			using (StreamReader r = new StreamReader("config.json"))
			{
				string json = r.ReadToEnd();
				//dynamic array = JsonConvert.DeserializeObject(json);
				WorldOptions config = JsonConvert.DeserializeObject<WorldOptions>(json);
				return config;
			}
		}

		public async Task Start()
		{
			//timer.Enabled = true;
			_test.BeginInterval(1);
			
			while(true)
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
			//await Task.Factory.StartNew(() => WorldStep(), TaskCreationOptions.LongRunning);
			await Task.Run(() => WorldStep());
			//WorldStep();
			_test.EndBeginInterval(1, 2);
			RedrawWorld();
		}

		private void WorldStep()
		{
			//Parallel.For(0, currentBotsNumber, i => bots[i].Move());


			for (var botNumber = 0; botNumber < currentBotsNumber; botNumber++)
			{
				bots[botNumber].Live();
				//bots[botNumber].Move();
			}
		}

		private void RedrawWorld()
		{
			_painter.StartNewFrame();
			_test.EndBeginInterval(2, 3);

			for (var botNumber = 0; botNumber < currentBotsNumber; botNumber++)
			{
				//_painter.DrawBotOnFrame(bots[botNumber]);
				if (bots[botNumber].Moved || bots[botNumber].NoDrawed)
				{
					_painter.DrawBotOnFrame(bots[botNumber]);
					bots[botNumber].NoDrawed = false;
				}
			}
			_test.EndBeginInterval(3, 4);

			_painter.PaintFrame();
			//await Task.Delay(1);
			_test.EndBeginInterval(4, 1);
		}
	}
}
