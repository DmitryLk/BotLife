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


		public System.Windows.Forms.Timer timer;
		private int[,] world;
		private Bot[] bots;
		private int currentBotsNumber;

		public Painter _painter;
		public Tester _test;


		Random _rnd = new Random(Guid.NewGuid().GetHashCode());

		public Life(Painter painter, Tester test)
		{
			_painter = painter;
			_painter.Configure(worldWidth, worldHeight, botWidth, botHeight);

			//timer = new System.Windows.Forms.Timer();
			//timer.Tick += new System.EventHandler(timer_Tick);
			//timer.Interval = 1;
			//timer.Enabled = false;

			_test = test;
			_test.InitInterval(1, "BotsAction();");
			_test.InitInterval(2, "RedrawWorld();");
			_test.InitInterval(3, "DrawBotOnFrame(bots[botNumber]);");
			_test.InitInterval(4, "PaintFrame();");

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

		public void Start()
		{
			//timer.Enabled = true;
			for (; ; )
			{
				Step();
			}
		}

		//private void timer_Tick(object sender, EventArgs e)
		//{
		//	Step();
		//}


		private void Step()
		{
			_test.BeginInterval(1);
			BotsAction();
			_test.EndBeginInterval(1, 2);
			RedrawWorld();
		}

		private void BotsAction()
		{
			//Parallel.For(0, currentBotsNumber, i => bots[i].Move());


			for (var botNumber = 0; botNumber < startBotsNumber; botNumber++)
			{
				bots[botNumber].Move();
			}
		}

		private void RedrawWorld()
		{
			_painter.StartNewFrame();
			_test.EndBeginInterval(2, 3);

			for (var botNumber = 0; botNumber < startBotsNumber; botNumber++)
			{
				_painter.DrawBotOnFrame(bots[botNumber]);
				//if (bots[botNumber].Moved || !bots[botNumber].OnceDrawed)
				//{
				//	_painter.DrawBotOnFrame(bots[botNumber]);
				//	bots[botNumber].OnceDrawed = true;
				//}
			}
			_test.EndBeginInterval(3, 4);

			_painter.PaintFrame();
			//await Task.Delay(1);
			_test.EndInterval(4);
		}
	}
}
