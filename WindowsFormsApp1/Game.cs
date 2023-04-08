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
		public GameData _data;
		public Tester _test;
		public System.Windows.Forms.Timer timer;

		public Game(Presenter presenter, Tester test, GameData data)
		{
			_data = data;

			_presenter = presenter;

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
			_test.InitInterval(4, "PrintInfo();");

			_data.Started = false;
			_data.PausedMode = true;
			_data.Drawed = true;
			_data.ReportFrequencyCurrent = _data.ReportFrequencyDrawed;
		}

		public async Task Start()
		{
			_data.Started = true;
			//timer.Enabled = true;
			_world.Initialize();
		}

		public async Task Work()
		{
			do
			{
				await Step();
			}
			while (!_data.PausedMode);
		}

		public async Task Step()
		{
			_test.BeginInterval(0);
			await Task.Run(() => _world.Step());
			_test.EndBeginInterval(0, 1);
			//await Task.Factory.StartNew(() => WorldStep(), TaskCreationOptions.LongRunning);
			//_world.Step();
			if (_data.Drawed)
			{
				RedrawWorld();
			}
			else 
			{
				_test.EndBeginInterval(1, 2);
				_test.EndBeginInterval(2, 3);
				_test.EndBeginInterval(3, 4);
			}
			_presenter.PrintInfo();
			_test.EndBeginInterval(4, 0);
			//await Task.Delay(5000);
		}

		public void MutationToggle()
		{
			_data.Mutation = !_data.Mutation;
		}

		private void RedrawWorld()
		{
			_presenter.StartNewFrame();
			_test.EndBeginInterval(1, 2);

			// Рисование изменений на битмапе экрана (сразу не отображаются)
			for (var i = 0; i < _data.NumberOfChangedCells; i++)
			{
				var obj = _data.ChangedCells[i];

				_presenter.DrawCellOnFrame(obj.X, obj.Y, obj.Color);
				_data.ChWorld[obj.X, obj.Y] = 0;
			}
			_data.NumberOfChangedCells = 0;
			//======================================

			_test.EndBeginInterval(2, 3);

			_presenter.PaintFrame();
			//await Task.Delay(1);
			_test.EndBeginInterval(3, 4);
		}


		//private void timer_Tick(object sender, EventArgs e)
		//{
		//	Step();
		//}
	}
}
