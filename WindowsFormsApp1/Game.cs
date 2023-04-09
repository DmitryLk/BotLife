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
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
	public class Game
	{
		public Presenter _presenter;
		public World _world;
		public GameData _data;
		public Tester TEST;
		public System.Windows.Forms.Timer timer;

		public bool Started;
		public bool PausedMode;
		public bool Drawed;

		public Game(Presenter presenter, Tester test)
		{
			var options = LoadConfig();
			_data = new GameData(options);

			_presenter = presenter;
			_presenter.Configure(_data);

			//timer = new System.Windows.Forms.Timer();
			//timer.Tick += new System.EventHandler(timer_Tick);
			//timer.Interval = 1;
			//timer.Enabled = false;

			_world = new World(_data);

			TEST = test;
			TEST.InitInterval(0, "BotsAction();");
			TEST.InitInterval(1, "RedrawWorld();");
			TEST.InitInterval(2, "DrawBotOnFrame(bots[botNumber]);");
			TEST.InitInterval(3, "PaintFrame();");
			TEST.InitInterval(4, "PrintInfo();");

			Started = false;
			PausedMode = true;
			Drawed = true;
			_data.ReportFrequencyCurrent = _data.ReportFrequencyDrawed;
		}

		public async Task Start()
		{
			Started = true;
			//timer.Enabled = true;
			_world.Initialize();
		}

		public async Task Work()
		{
			do
			{
				await Step();
			}
			while (!PausedMode);
		}


		public void MutationToggle()
		{
			_data.Mutation = !_data.Mutation;
		}

		public void DrawedToggle()
		{
			Drawed = !Drawed;
			_data.ReportFrequencyCurrent = Drawed ? _data.ReportFrequencyDrawed : _data.ReportFrequencyNoDrawed;
		}

		private async Task Step()
		{
			TEST.BeginInterval(0);
			await Task.Run(() => _world.Step());
			TEST.EndBeginInterval(0, 1);
			//await Task.Factory.StartNew(() => WorldStep(), TaskCreationOptions.LongRunning);
			//_world.Step();
			if (Drawed)
			{
				RedrawWorld();
			}
			else
			{
				TEST.EndBeginInterval(1, 2);
				TEST.EndBeginInterval(2, 3);
				TEST.EndBeginInterval(3, 4);
			}
			_presenter.PrintInfo();
			TEST.EndBeginInterval(4, 0);
			//await Task.Delay(5000);
		}

		private void RedrawWorld()
		{
			_presenter.StartNewFrame();
			TEST.EndBeginInterval(1, 2);

			// Рисование изменений на битмапе экрана (сразу не отображаются)
			for (var i = 0; i < _data.NumberOfChangedCells; i++)
			{
				var obj = _data.ChangedCells[i];

				_presenter.DrawCellOnFrame(obj.X, obj.Y, obj.Color);
				_data.ChWorld[obj.X, obj.Y] = 0;
			}
			_data.NumberOfChangedCells = 0;
			//======================================

			TEST.EndBeginInterval(2, 3);

			_presenter.PaintFrame();
			//await Task.Delay(1);
			TEST.EndBeginInterval(3, 4);
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
