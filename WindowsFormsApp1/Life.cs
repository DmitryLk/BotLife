using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
	public class Life
	{
		// Settings
		private const int startBotsNumber = 40000;
		private const int worldWidth = 500;
		private const int worldHeight = 500;
		private const int botWidth = 2;
		private const int botHeight = 2;
		private const int maxBotsNumber = worldWidth * worldHeight;
		private const int reportFrequency = 100;


		public System.Windows.Forms.Timer timer;
		private int[,] world;
		private Bot[] bots;
		private int currentBotsNumber;

		public Painter _painter;
		public Tester ___test;


		Random _rnd = new Random(Guid.NewGuid().GetHashCode());

		public Life(Painter painter, Tester test)
		{
			_painter = painter;
			_painter.Configure(worldWidth, worldHeight, botWidth, botHeight, reportFrequency);

			//timer = new System.Windows.Forms.Timer();
			//timer.Tick += new System.EventHandler(timer_Tick);
			//timer.Interval = 1;
			//timer.Enabled = false;

			___test = test;
			___test.InitInterval(1, "BotsAction();");
			___test.InitInterval(2, "RedrawWorld();");
			___test.InitInterval(3, "DrawBotOnFrame(bots[botNumber]);");
			___test.InitInterval(4, "PaintFrame();");

			InitWorld();
		}

		public void InitWorld()
		{
			world = new int[worldWidth, worldHeight];
			bots = new Bot[maxBotsNumber];

			// Создание ботов
			for (var botNumber = 0; botNumber < startBotsNumber; botNumber++)
			{
				bots[botNumber] = new Bot(_rnd, worldWidth, worldHeight);
			}
			currentBotsNumber = startBotsNumber;
		}

		public async Task Start()
		{
			//timer.Enabled = true;
			___test.BeginInterval(1);
			for (; ; )
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
			//await Task.Run(() => WorldStep());
			WorldStep();


			___test.EndBeginInterval(1, 2);
			RedrawWorld();
		}

		private void WorldStep()
		{
			//Parallel.For(0, currentBotsNumber, i => bots[i].Move());


			for (var botNumber = 0; botNumber < startBotsNumber; botNumber++)
			{
				//bots[botNumber].Step();
				bots[botNumber].Move();
			}
		}

		private void RedrawWorld()
		{
			_painter.StartNewFrame();
			___test.EndBeginInterval(2, 3);

			for (var botNumber = 0; botNumber < startBotsNumber; botNumber++)
			{
				//_painter.DrawBotOnFrame(bots[botNumber]);
				if (bots[botNumber].Moved || bots[botNumber].NoDrawed)
				{
					_painter.DrawBotOnFrame(bots[botNumber]);
					bots[botNumber].NoDrawed = false;
				}
			}
			___test.EndBeginInterval(3, 4);

			_painter.PaintFrame();
			//await Task.Delay(1);
			___test.EndBeginInterval(4, 1);
		}
	}
}
