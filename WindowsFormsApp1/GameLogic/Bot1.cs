using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.Logger;
using WindowsFormsApp1.Static;

namespace WindowsFormsApp1.GameLogic
{
	// Бот с океана foo52
	public class Bot1
	{
		//private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
		//private static readonly object _busy = new();

		const int hueFrom = 200;

		private static readonly object _busyWorld1 = new();
		private static readonly object _busyWorld2 = new();
		private readonly object _busyBotEnergy = new();
		private readonly object _busyInsertedToReproductionList = new();
		private readonly object _busyReceptors = new();
		//private static readonly object _busyTotalEnergy = new();
		//private readonly object _busyBite = new();
		//private readonly object _busyInsertedToDeathList = new();

		// РЕЦЕПТОРЫ
		// меня укусили?
		// увидел рядом чтото?
		//private byte _recNum;
		private bool _recActive;
		private int _recContactDir;
		private byte _recWeight;
		public bool _recNew;
		public byte _recNewBranch;



		//private static long COUNTER1 = 0;
		//private static long COUNTER2 = 0;

		public Genom G;
		public Color Color;
		public Pointer PointerGeneral;
		public Pointer PointerReaction;
		private int _tm = 0;
		private int _tmR = 0;

		public CodeHistory hist;
		//public BotLog Log;

		public double Xd;
		public double Yd;
		public int Xi;
		public int Yi;


		public long Index;         // Индекс бота (может меняться)
		public int Direction;         // Направление бота
		public int Age;
		public int BiteMeCount;
		public int BiteImCount;
		public long Num;         // Номер бота (остается постоянным)

		private int _en;
		private int _size;

		public bool Alive;
		public bool InsertedToDeathList;  // чтобы не вставить бота два раза в этот лист, только для этого
		public bool InsertedToReproductionList;  // чтобы не вставить бота два раза в этот лист, только для этого
		private int _reproductionCycle;

		private int df = 0;

		private long _moved = 0;
		public bool Moved
		{
			get
			{
				return _moved > 10;
			}
		}
		public void ResetMoved()
		{
			_moved = 0;
		}
		public int Energy
		{
			get
			{
				return _en;
				//lock (_busyBotEnergy)
				//{
				//    return _en;
				//}
			}
			//set
			//{
			//    Log.AddLog($"Change Energy from {_en} to {value}");
			//    //if (_en == value)
			//    //{
			//    //}

			//    //df++;
			//    _en = value;
			//    //if (df > 1) throw new Exception();
			//    //df--;
			//    //lock (_busyBotEnergy)
			//    //{
			//    //}

			//    if (Data.BotColorMode == BotColorMode.Energy)
			//    {
			//        Color = GetGraduatedColor(Energy, 0, 6000);
			//        if (Data.DrawType == DrawType.OnlyChangedCells)
			//        {
			//            Func.FixChangeCell(Xi, Yi, Color);
			//        }
			//    }
			//}
		}

		public int Bite(int delta, int contactDir)
		{
			Interlocked.Increment(ref BiteMeCount);
			ActivateReceptor1(contactDir);
			return EnergyChange(delta);
		}

		#region Receptors
		private void ActivateReceptor(byte recWeight, byte bNew, int contactDir)
		{
			if (!_recActive || _recWeight > recWeight)
			{
				lock (_busyReceptors)
				{
					if (!_recActive || _recWeight > recWeight)
					{
						_recContactDir = contactDir;
						_recWeight = recWeight;
						_recNewBranch = bNew;
						_recNew = true;
						_recActive = true;  // надо делать в пследнюю очередь чтобы попало в GetCommand if (_recNum > 0) с полностью подготовленными данными
					}
				}
			}
		}


		// 1 - укус
		private void ActivateReceptor1(int contactDir)
		{
			ActivateReceptor(0, 0, contactDir);
		}


		// 2 - рядом бот
		private void ActivateReceptor2(int contactDir, bool rel, int dir) //, bool block, int massa, byte[] Shield, byte[] Attack, int dir, bool mov)
		{
			if (!_recActive || _recWeight > 3)
			{
				if (rel)
				{
					ActivateReceptor(3, 3, contactDir);
				}
				else
				{
					var needbigrotate = Dir.GetDirDiff(contactDir, dir) < Dir.NumberOfDirections / 4;
					if (needbigrotate)
					{
						ActivateReceptor(2, 2, contactDir);
					}
					else
					{
						ActivateReceptor(1, 1, contactDir);
					}
				}
			}
		}

		// 3 - рядом еда
		private void ActivateReceptor3(int contactDir)
		{
			ActivateReceptor(4, 4, contactDir);
		}

		// 4 - рядом минерал
		private void ActivateReceptor4(int contactDir)
		{
			ActivateReceptor(5, 5, contactDir);
		}

		// 5 - рядом край/стена
		private void ActivateReceptor5(int contactDir)
		{
			ActivateReceptor(6, 6, contactDir);
		}
		#endregion

		/// <summary>
		/// delta - энергия которая будет добавлена к энергии бота
		/// </summary>
		/// <param name="delta"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public int EnergyChange(int delta)
		{
			if (delta == 0) return 0;

			lock (_busyBotEnergy)
			{
				if (_en + delta < 0) delta = -_en;
				//Log.LogInfo($"Change Energy from {_en} to {_en + delta}");
				_en += delta;


				if (!Alive && _en > 0)
				{
					Alive = true;
					Interlocked.Decrement(ref Data.QtyFactBotDeath);
					//InsertedToDeathList = false;
				}

				if (_en == 0 && Alive)
				{
					Alive = false;
					Interlocked.Increment(ref Data.QtyFactBotDeath);
					if (!InsertedToDeathList)
					{
						InsertedToDeathList = true;
						Data.BotDeath[Interlocked.Increment(ref Data.QtyAllBotDeathMinusOne)] = this;
						//Log.LogInfo("bot inserted to DeathList");
					}
				}
			}

			if (_en < 0)
			{
				throw new Exception("fdgdfgdfg");
			}

			if (Data.BotColorMode == BotColorMode.Energy)
			{
				RefreshColor();
				if (Data.DrawType == DrawType.OnlyChangedCells)
				{
					Func.FixChangeCell(Xi, Yi, Color);
				}
			}

			return -delta;
		}

		public void EnergySet(int en)
		{
			if (en <= 0) throw new Exception("sdfsdf");
			if (_en != 0) throw new Exception("fglkeru84");

			lock (_busyBotEnergy)
			{
				_en = en;
			}
		}

		public void RecNumClear()
		{
			_recActive = false;
		}

		public Bot1(int x, int y, int dir, long botNumber, long botIndex, int en, Genom genom)
		{
			PointerGeneral = new Pointer();
			PointerReaction = new Pointer();

			this.G = genom;
			hist = new CodeHistory();
			//Log = new BotLog();

			Direction = dir;
			Num = botNumber;
			Index = botIndex;
			_en = en;
			Age = 0;
			BiteMeCount = 0;
			BiteImCount = 0;

			Xd = x;
			Yd = y;
			Xi = x;
			Yi = y;

			InsertedToDeathList = false;
			InsertedToReproductionList = false;
			Alive = true;
			_reproductionCycle = 0;
			//Log.LogInfo($"bot was born. index:{Index}");
		}

		public void RefreshColor()
		{
			Color = Data.BotColorMode switch
			{
				BotColorMode.GenomColor => G.Color,
				BotColorMode.PraGenomColor => G.PraColor,
				BotColorMode.PlantPredator => G.Plant ? Color.Green : Color.Red,
				BotColorMode.Energy => GetGraduatedColor(Energy, 0, 6000),
				BotColorMode.Age => GetGraduatedColor(500 - Age, 0, 500),
				BotColorMode.GenomAge => GetGraduatedColor(6000 - (int)(Data.CurrentStep - G.BeginStep), 0, 6000),
				_ => throw new Exception("Color = Data.BotColorMode switch")
			};
		}


		private Color GetGraduatedColor(int grad, int min, int max)
		{
			// 0 -		FFFF00
			// 1000 -	FFFFFF
			//0 ->	H = 120 S = 100 V =100
			//20 -> H = 60	S = 100 V =100
			//40 -> H = 0	S = 100 V =100

			var hue = hueFrom - hueFrom * grad / (max - min);
			if (hue < 0) hue = 0;
			if (hue > hueFrom) hue = hueFrom;

			return Data.GrColors[hue];
		}


		public void Step()
		{
			var t = Stopwatch.GetTimestamp();

			// Некий алгоритм активности бота в рамках одного игорового шага.
			// В результате которого он на основании данных от рецепторов или без них
			// изменит свое состояние (координаты, геном, энергия, здоровье, возраст, направление, цвет, минералы, ...)
			// Также может размножиться

			//Func.CheckWorld2(Index, Num, Xi, Yi);
			//Func.CheckWorld2(Index, Num, Xi, Yi);


			t = Test2.Mark(1, t);

			CommandCycle();

			t = Test2.Mark(2, t);

			Age++;

			var realchange = EnergyChange(Data.DeltaEnergyOnStep);
			if (Data.DeltaEnergyOnStep != 0) Interlocked.Add(ref Data.TotalEnergy, -realchange);

			if (Data.BotColorMode == BotColorMode.GenomAge || Data.BotColorMode == BotColorMode.Age)
			{
				RefreshColor();
				if (Data.DrawType == DrawType.OnlyChangedCells)
				{
					Func.FixChangeCell(Xi, Yi, Color);
				}
			}


			//Func.CheckWorld2(Index, Num, Xi, Yi);
			//Func.CheckWorld2(Index, Num, Xi, Yi);

			if (CanReproduct())
			{
				ToReproductionList();
			}

			//Func.CheckWorld2(Index, Num, Xi, Yi);
			Test2.Mark(3, t);
		}

		private (bool Ev, Pointer Pointer, byte Cmd, byte Par) GetCommand()
		{
			if (_recActive)  // Есть сигнал от рецепторов. Цикл по командам конкретного event.
			{
				if (_recNew) // новая сработка рецептора. Обновление PointerReaction делается только здесь
				{
					lock (_busyReceptors)
					{
						if (_recNew)
						{
							PointerReaction.B = (byte)(_recNewBranch + Data.GenomGeneralBranchCnt);
							PointerReaction.CmdNum = 0;
							_recNew = false;
							_tmR = 0;
						}
					}
				}

				var (cmd, par) = G.GetReactionCommandAndSetAct(PointerReaction, true);

				return (true, PointerReaction, cmd, par);
			}
			else
			{
				var (cmd, par) = G.GetGeneralCommandAndSetAct(PointerGeneral, true);
				return (false, PointerGeneral, cmd, par);
			}
		}


		private void CommandCycle()
		{
			byte cmd, par;
			bool ev, lastcmd = false;
			int cntJmp = 0;
			long t, t2;
			int tm;
			Pointer p_H;

			if (Data.HistoryOn) hist.BeginNewStep(_tm);
			do
			{
				t = Stopwatch.GetTimestamp();

				(ev, p_H, cmd, par) = GetCommand();

				t = Test2.Mark(4, t);

				cntJmp++;
				tm = 0;

				/*
								есть ли препятствие для движения?
								впереди есть боты?
								увиден бот? минерал? стена? еда?

								таймер
								какое моё здоровье
								много ли осталось родни? 
								=сколько энергии?
								условие от возраста
								=условие от высоты
								где ближайшая еда?
								есть ли у меня защита 1го типа
								есть ли у меня защита 2го типа
								есть ли у меня защита 3го типа
								окружен ли бот
								условие от кол-ва окружающих
								был ли предыдущий ход движением?
								приход энергии есть?
								=далеко ли я от центра племени (для появления роев)
								далеко ли от матки?
				
								+фотосинтез
								команда периодической смены направления при определенной вероятности направление менятеся случайно или нет. вероятность смены направления следующим байтом
								выравнится по горизонтали
								случайное направление , допустим цифра 8 (0-7 это обычные направления)
								команда перехода на случайное количество шагов в программе
								=поворот параллельно движению раздражителя
								посмотреть в направлении раздражителя
								сцепиться с родней
								=повернуться к контакту
								превратить в своего
								обездвижить бота
								снизить уровни атаки бота
								отключить защиту бота
								перебросить бота за себя
								стать невидимым для ботов
								притянуть бота
								притянуться к боту
								=оттолкнуть/ударить бота
								высосать энергию из окружающих ботов в каком-то радиусе
								оттолкнуть всех окружающих ботов
								кусать только тех у кого мало энергии
								повернуть бота от себя
								повернуть всех ботов от себя
								звать на помощь родню
								двигаться к ближайшей родне
								отталкиваться от опасных врагов
								активировать защиту от определенной атаки
								дальнее зрение
								бот может взорваться при определенных условиях
								притворится мертвым или стеной
								телепортация
								рои
								возможность собирнаться маленькими кучками
								тащить еду или чтото кудато
								мина
								При столкновении с другим ботов взрывается
								разные виды ппщи, разные виды минералов
								камуфляж
								муравьиная дорожка
								боты-зомби
								удачно мутировавшие гены чтоб больше не мутировали или с меньшей вероятностью?
								усики как у муравья/таракана
								по запаху двигается
								при каждой мутации масса может меняться вверх или вниз процентов на 10
								более массивные расталкивают менее массивных
								боты притягиваютс к большой массе
								Как-то надо сделать борьбу ботов неравной. Сейчас пока думаю у ботов должны быть различные уровни защиты. 
								можно защита на разных сторонах разная
								Более высокая атака-защита может также зависеть от массы, энергии, возраста, количетсва успешных боев
								общая сумма равна 3 защиты или 3 атаки или 2 защиты и 1 атака. так же здесь может быть фотосинтез
								суммировать количество типов атаки-защиты где превышение а не насколько превышение ???
								минусы: по идее со временем возможности должны нарастать а так они на одном уровне всегда будут
								при необходимости один тип защиты преобразовать в другой
								возможность периодического изменения направления движения 

								минералы:
								Возможно неплохая идея с минералами - от количества накопленных минералов зависит жесткость защиты
								Бот поел минералов у него повысился уровнеь защиты . поел 100 минералов при укусе тратится накопленные минералы а не энергия
								Минералы могут быть различными. Бот может перерабатывать энергию в минералы.
								допустим съел минерал 1го типа - появилась защита от 1го типа атаки, которая расходуется при каждой атаке 
								допустим тогда появятся такие боты которые не атакуют пока не наедятся минералов
								минусы: надо съесть минерал чтобы появилась защита


								избегать вражеских ботов стремиться к еде
								наступает только на ту клетку где вокруг нет хищников
								объединяться при нападении вокруг альфы

							=================================================
								если рядом бот большей массы то удирать
								сколько  минералов
								какой мой уровень?
								минералы прибавляются?
								многоклеточный ли я ?
								много ли осталось пищи таокго типа - может пока не есть пусть размножится
								какое сейчас время суток - в зависимости от времени суток меняеьтся поведения бота - может утром он стремится вверх после обеда вниз
								какое сейчас время года - в зависимости от времени года меняеьтся поведения бота - может зимой он стремится вверх летом вниз
								многоклеточность ( создание потомка, приклееного к боту )
								деление (создание свободноживущего потомка)
								преобразовать минералы в энерию
								мутировать (спорная команда, во время её выполнения меняются случайным образом две случайные команды читал, что микроорганизмы могут усилить вероятность мутации своего генома в неблагоприятных условиях)       
								генная атака
								чем сложнее тем больше выживаемость. вселенная устроена так что есть стремление к усложнению
									напрммер может собрать минералы трех видов будет полная защита
									напрммер собрал минералы 1 3 7 типа - появилось дальнее зрение
							=================================================
								укус с различной стороны
								рядом есть боты?

								шаг в относительном направлении
								шаг в абсолютном направлении
								укусить в относительном направлении
								укусить в абсолютном направлении
								посмотреть в относительном направлении
								посмотреть в абсолютном направлении

								+поворот абсолютно
								+поворот относительно
								+шаг вперед
								+укусить впереди
								+посмотреть вперед
								посмотреть вокруг
								?поворот относительно
								поворот относительно раздражителя
								поворот обратно относительно раздражителя
								?поворот на 180
								?поворт влево
								?поворот вправо
								?шаг относительно
								шаг относительно раздражителя
								шаг назад относительно раздражителя
								?шаг влево
								?шаг вправо
								?шаг назад
								???посмотреть вокруг
								укусить впереди
								?укусить в направлении раздражителя
								шаг в абсолютном направлении
								укусить в направлении раздражителя
								укусить в относительном направлении
								укусить в абсолютном направлении
								посмотреть в относительном напралении 
								посмотреть в абсолютном направлении
								делиться - если у бота больше энергии или минералов, чем у соседа, то они распределяются поровну          
								делится   в относительном напралении  - если у бота больше энергии или минералов, чем у соседа, то они распределяются поровну 
								делится  в абсолютном направлении
								отдать - безвозмездно отдать часть энергии и минералов соседу в относительном напралении
								отдать - безвозмездно отдать часть энергии и минералов соседу в абсолютном направлении
								поделиться энергией
							=================================================
				 */

				switch (cmd)
				{
					//// Rotate
					case Cmd.RotateAbsolute: tm = RotateAbsolute(par); break;
					case Cmd.RotateRelative: tm = RotateRelative(par); Test2.Mark(17, t); break;
					case Cmd.RotateRelativeContact: tm = RotateRelativeContact(par); Test2.Mark(10, t); break;
					case Cmd.RotateBackward: tm = RotateBackward(); Test2.Mark(11, t); break;
					case Cmd.RotateBackwardContact: tm = RotateBackwardContact(); Test2.Mark(12, t); break;
					case Cmd.RotateRandom: tm = RotateRandom(); break;
					case Cmd.AlignHorizontaly: tm = AlignHorizontaly(); break;

					//// Step
					case Cmd.StepRelative: tm = StepRelative(par); break;
					case Cmd.StepForward: tm = StepForward(); Test2.Mark(18, t); break;
					case Cmd.StepRelativeContact: tm = StepRelativeContact(par); Test2.Mark(13, t); break;
					case Cmd.StepBackward: tm = StepBackward(); Test2.Mark(14, t); break;
					case Cmd.StepBackwardContact: tm = StepBackwardContact(); Test2.Mark(15, t); break;

					//// Eat
					case Cmd.EatForward: tm = EatForward(); Test2.Mark(16, t); break;
					case Cmd.EatContact: tm = EatContact(); break;

					//// Look
					case Cmd.LookAround: tm = LookAround(); Test2.Mark(22, t); break;
					case Cmd.LookForward: tm = LookForward(); Test2.Mark(20, t); break;


					//// Other
					case Cmd.Photosynthesis: tm = Photosynthesis(); Test2.Mark(21, t); break;
					default: throw new Exception();
				};

				if (cmd == Cmd.StepForward)
				{
					if (_moved < 50) _moved += 5;
				}
				else
				{
					if (_moved > 0) _moved--;
				}

				t2 = Stopwatch.GetTimestamp();
				if (Data.HistoryOn) hist.SaveCmdToHistory(p_H, ev, tm);
				Test2.Mark(7, t2);

				if (ev)
				{
					PointerReaction.CmdNum++; if (PointerReaction.CmdNum >= Data.MaxCmdInStep) lastcmd = true;

					_tmR += tm;
					if (_tmR >= 100 || lastcmd)
					{
						_recActive = false; // завершаем команду реакции, переходим на обычные команды
					}
				}
				else
				{
					PointerGeneral.CmdNum++; if (PointerGeneral.CmdNum >= Data.MaxCmdInStep) lastcmd = true;
				}

				t = Test2.Mark(5, t);

				_tm += tm;
			}
			while (_tm < 100 && !lastcmd && cntJmp < 10);

			if (Data.HistoryOn) hist.EndNewStep(_tm);

			_tm -= 100;
			ShiftPointerGeneralToNextBranch();

			Test2.Mark(6, t);
		}

		//===================================================================================================
		//// Rotate
		//1 C
		private int RotateAbsolute(int dir)
		{
			var tm = Dir.GetDirDiff(Direction, dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(dir);

			return tm;
		}

		//2 CE
		private int RotateRelative(int dir)
		{
			var tm = Dir.GetDirDiff(0, dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(Direction + dir);

			return tm;
		}

		//3 E
		private int RotateRelativeContact(int dir)
		{
			var tm = Dir.GetDirDiff(Direction, _recContactDir + dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(_recContactDir + dir);

			return tm;
		}

		//4 E
		private int RotateBackward()
		{
			var tm = Dir.NumberOfDirections / 2 * CmdType.CmdTime(CmdType.Rotate);

			Rotate(Dir.GetOppositeDirection(Direction));

			return tm;
		}

		//5 E
		private int RotateBackwardContact()
		{
			var dir = Dir.GetOppositeDirection(_recContactDir);

			var tm = Dir.GetDirDiff(Direction, dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(dir);

			return tm;
		}

		//6 C - может быть лучше для E ?
		private int RotateRandom()
		{
			var dir = Func.GetRandomDirection();

			var tm = Dir.GetDirDiff(Direction, dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(dir);

			return tm;
		}

		//7 C
		private int AlignHorizontaly()
		{
			var dir = 16;

			var tm = Dir.GetDirDiff(Direction, dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(dir);

			return tm;
		}

		//// Step
		//10 11 C
		private int StepForward()
		{
			var move = Step(GetDirForward());

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		//12 E
		private int StepRelative(int dir)
		{
			var move = Step((Direction + dir) % Dir.NumberOfDirections);

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		//13 E
		private int StepRelativeContact(int dir)
		{
			var move = Step((_recContactDir + dir) % Dir.NumberOfDirections);

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		//14 E
		private int StepBackward()
		{
			var move = Step(Dir.GetOppositeDirection(Direction));

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		//15 E
		private int StepBackwardContact()
		{
			var move = Step(Dir.GetOppositeDirection(_recContactDir));

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		//// Eat
		//20 21 CE
		private int EatForward()
		{
			var eat = Eat(GetDirForward());

			return eat ? CmdType.CmdTime(CmdType.EatSuccessful) : CmdType.CmdTime(CmdType.EatNotSuccessful);
		}

		//22 E
		private int EatContact()
		{
			var eat = Eat(_recContactDir);

			return eat ? CmdType.CmdTime(CmdType.EatSuccessful) : CmdType.CmdTime(CmdType.EatNotSuccessful);
		}

		//// Look
		//30 31 C
		private int LookForward()
		{
			Look(GetDirForward());

			return CmdType.CmdTime(CmdType.Look);
		}

		//32 CE
		private int LookAround()
		{
			LookAroundForEnemyAndRelatives();

			return CmdType.CmdTime(CmdType.LookAround);
		}

		//// Other
		//40 C
		private int Photosynthesis()
		{
			if (Yi < Data.PhotosynthesisLayerHeight)
			{
				EnergyChange(Data.PhotosynthesisEnergy);
				Interlocked.Add(ref Data.TotalEnergy, Data.PhotosynthesisEnergy);
				G.Plant = true;
				//genom.Color = Color.Green;

				return CmdType.CmdTime(CmdType.PhotosynthesisSuccessful);
			}
			else
			{
				return CmdType.CmdTime(CmdType.PhotosynthesisNotSuccessful);
			}
		}


		//===================================================================================================

		/*
								########    ###    ######## 
								##         ## ##      ##    
								##        ##   ##     ##    
								######   ##     ##    ##    
								##       #########    ##    
								##       ##     ##    ##    
								######## ##     ##    ##    
		 */
		private bool Eat(int dir)
		{
			// Алгоритм:
			// 1. Узнаем координаты клетки на которую надо посмотреть
			var (nXi, nYi) = GetCoordinatesByDirectionOnlyDifferent(dir);


			// 2. Узнаем что находится на этой клетке
			if (IsItEdge(nXi, nYi))
			{
				ActivateReceptor5(dir);
				return false;
			}

			if (Data.World[nXi, nYi] == 65503)
			{
				ActivateReceptor5(dir);
				return false;
			}


			// Grass
			bool grass = false;
			if (Data.World[nXi, nYi] == 65500)
			{
				lock (_busyWorld1)
				{
					if (Data.World[nXi, nYi] == 65500)
					{
						Data.World[nXi, nYi] = 0;

						grass = true;
					}
				}
			}

			if (grass)
			{
				EnergyChange(Data.FoodEnergy);
				Interlocked.Add(ref Data.TotalEnergy, Data.FoodEnergy);
				Interlocked.Decrement(ref Data.CurrentNumberOfFood);
				if (Data.DrawType == DrawType.OnlyChangedCells) Func.FixChangeCell(nXi, nYi, null);
				return true;
			}


			var cont = Data.World[nXi, nYi];

			// Bot || Relative
			if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
			{
				return BiteBot(cont, dir);
			}
			else
			{
				return false;
			}
		}


		private bool BiteBot(long cont, int dir)
		{
			var eatedBot = Data.Bots[cont];

			// Растение не может есть животное
			if (G.Plant && !eatedBot.G.Plant)
			{
				return false;
			}

			// Животное может есть растение, но ни тогда когда его осталось мало
			if (!G.Plant && eatedBot.G.Plant)
			{
				if (eatedBot.G.CurBots < 2)
				{
					return false;
				}
			}

			// Не может есть родственника
			if (G.GenomHash == eatedBot.G.GenomHash)
			{
				return false;
			}

			// Не может есть нового
			if (Data.DelayForNewbie && Data.CurrentStep - eatedBot.G.BeginStep < 10)
			{
				return false;
			}

			//var olden = Energy;
			var atc = 0;
			for (var i = 0; i < G.AttackTypesCnt; i++)
			{
				//1
				if (G.AttackTypes[i].Level > eatedBot.G.Shield[G.AttackTypes[i].Type])
				{
					atc += G.AttackTypes[i].Level - eatedBot.G.Shield[G.AttackTypes[i].Type];
				}

				//2
				//if (G.AttackTypes[i].Level > eatedBot.G.Shield[G.AttackTypes[i].Type]*2)
				//{
				//	atc += G.AttackTypes[i].Level - eatedBot.G.Shield[G.AttackTypes[i].Type]*2;
				//}

				//3
				//if (G.AttackTypes[i].Level > eatedBot.G.Shield[G.AttackTypes[i].Type])
				//{
				//	atc++;
				//}

				//4
				//if (G.AttackTypes[i].Level > eatedBot.G.Shield[G.AttackTypes[i].Type] && eatedBot.G.Shield[G.AttackTypes[i].Type] == 0)
				//{
				//	atc += G.AttackTypes[i].Level - eatedBot.G.Shield[G.AttackTypes[i].Type];
				//}
			}

			if (atc > 0)
			{
				// Data.BiteEnergy / 2 * atc - отрицательное число. возвращается положительное число.

				var k = 20;
				if (!Moved && eatedBot.Moved) k = Data.MovedBiteStrength;

				var requestedEnergy = Data.BiteEnergy * 10 / k * atc;

				Interlocked.Increment(ref BiteImCount);
				var gotEnergyByEating = eatedBot.Bite(requestedEnergy, Dir.GetOppositeDirection(dir));
				EnergyChange(gotEnergyByEating);

				//var gotEnergyByEating = eatedBot.EnergyChange(Data.BiteEnergy);
				if (gotEnergyByEating < 0) throw new Exception("dfgdfg");
				return true;
			}
			else
			{
				return false;
			}


			//eatedBot.Log.LogInfo($"bot was bited. energy:{eatedBot.Energy}");
			//Log.LogInfo($"bot{Index} bite bot{cont} and got {gotEnergyByEating} energy. From {olden} to {Energy}.");
		}

		private void ShareEnergy()
		{
			// Передать энергию окружающим ботам
			var n = ThreadSafeRandom.Next(8);
			int nXi, nYi;
			long cont;
			int i = 0;
			var ent = (Energy - Data.ReproductionBotEnergy) / 2;

			if (ent > 0)
			{
				do
				{
					(nXi, nYi) = Func.GetCoordinatesByDelta(Xi, Yi, n);

					if (nYi >= 0 && nYi < Data.WorldHeight && nXi >= 0 && nXi < Data.WorldWidth)
					{
						cont = Data.World[nXi, nYi];

						if (cont >= 1 && cont <= Data.CurrentNumberOfBots && G.IsRelative(Data.Bots[cont].G))
						{
							var targetBot = Data.Bots[cont];

							if (Energy > targetBot.Energy /*&& targetBot.Energy > 0 && !targetBot.InsertedToDeathList*/)
							{
								var transferedEnergy = EnergyChange(-ent);
								targetBot.EnergyChange(transferedEnergy);
								if (transferedEnergy < 0) throw new Exception("dfgdfg");
							}
						}
					}
					if (++n >= 8) n -= 8;
					i++;
				}
				while (CanReproduct() && i <= 8);
			}
		}


		/*
							##        #######   #######  ##    ## 
							##       ##     ## ##     ## ##   ##  
							##       ##     ## ##     ## ##  ##   
							##       ##     ## ##     ## #####    
							##       ##     ## ##     ## ##  ##   
							##       ##     ## ##     ## ##   ##  
							########  #######   #######  ##    ## 
		 */
		private void Look(int dir)
		{
			// Алгоритм:
			// 1. Узнаем координаты клетки на которую надо посмотреть
			var (nXi, nYi) = GetCoordinatesByDirectionOnlyDifferent(dir);

			// 2. Узнаем что находится на этой клетке

			//смещение условного перехода 2-пусто  3-стена  4-органика 5-бот 6-родня

			// Если координаты попадают за экран то вернуть RefContent.Edge
			if (IsItEdge(nXi, nYi))
			{
				ActivateReceptor5(dir);
				return;
			}

			var cont = Data.World[nXi, nYi];

			if (cont == 65503)
			{
				ActivateReceptor5(dir);
				return;
			}

			if (cont == 65500)
			{
				ActivateReceptor3(dir);
				return;
			}

			if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
			{
				ActivateReceptor2(dir, G.IsRelative(Data.Bots[cont].G), Data.Bots[cont].Direction);
			}

			return;
		}

		private void LookAroundForEnemyAndRelatives()
		{
			int nXi, nYi, dir;
			Bot1 b;
			bool edge = false;
			int xEdge = 0, yEdge = 0;
			bool grass = false;
			int xGrass = 0, yGrass = 0;

			Bot1 rel = null;
			for (var n = 0; n < 8; n++)
			{
				(nXi, nYi) = Func.GetCoordinatesByDelta(Xi, Yi, n);

				if (!IsItEdge(nXi, nYi))
				{
					var cont = Data.World[nXi, nYi];

					if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
					{
						if (!G.IsRelative(Data.Bots[cont].G))
						{
							b = Data.Bots[cont];
							dir = Dir.Round(Math.Atan2(Xd - b.Xd, b.Yd - Yd) * Dir.NumberOfDirections / 2 / Math.PI + Dir.NumberOfDirections / 2);
							if (dir == 64) dir = 0;
							//var dir1 = Dir.NearbyCellsDirection[n];
							ActivateReceptor2(dir, false, b.Direction);
							return;
						}
						else
						{
							rel = Data.Bots[cont];
						}
					}

					if (cont == 65503)
					{
						edge = true;
						xEdge = nXi;
						yEdge = nYi;
					}

					if (cont == 65500)
					{
						grass = true;
						xGrass = nXi;
						yGrass = nYi;
					}
				}
				else
				{
					edge = true;
					xEdge = nXi;
					yEdge = nYi;
				}
			}

			if (rel != null)
			{
				dir = Dir.Round(Math.Atan2(Xd - rel.Xd, rel.Yd - Yd) * Dir.NumberOfDirections / 2 / Math.PI + Dir.NumberOfDirections / 2);
				if (dir == 64) dir = 0;
				//var dir1 = Dir.NearbyCellsDirection[n];
				ActivateReceptor2(dir, true, rel.Direction);
				return;
			}

			if (edge)
			{
				dir = Dir.Round(Math.Atan2(Xd - xEdge, yEdge - Yd) * Dir.NumberOfDirections / 2 / Math.PI + Dir.NumberOfDirections / 2);
				if (dir == 64) dir = 0;
				ActivateReceptor5(dir);
				return;
			}

			if (grass)
			{
				dir = Dir.Round(Math.Atan2(Xd - xGrass, yGrass - Yd) * Dir.NumberOfDirections / 2 / Math.PI + Dir.NumberOfDirections / 2);
				if (dir == 64) dir = 0;
				ActivateReceptor3(dir);
				return;
			}
		}


		/*
							########   #######  ########    ###    ######## ######## 
							##     ## ##     ##    ##      ## ##      ##    ##       
							##     ## ##     ##    ##     ##   ##     ##    ##       
							########  ##     ##    ##    ##     ##    ##    ######   
							##   ##   ##     ##    ##    #########    ##    ##       
							##    ##  ##     ##    ##    ##     ##    ##    ##       
							##     ##  #######     ##    ##     ##    ##    ######## 
		 */
		private void Rotate(int dir)
		{
			while (dir >= Dir.NumberOfDirections) dir -= Dir.NumberOfDirections;
			while (dir < 0) dir += Dir.NumberOfDirections;

			Direction = dir;
		}


		//https://www.messletters.com/ru/big-text/ banner3

		/*
							##     ##  #######  ##     ## ######## 
							###   ### ##     ## ##     ## ##       
							#### #### ##     ## ##     ## ##       
							## ### ## ##     ## ##     ## ######   
							##     ## ##     ##  ##   ##  ##       
							##     ## ##     ##   ## ##   ##       
							##     ##  #######     ###    ######## 
		 */
		#region Move

		private bool Step(int dir)
		{
			//Func.CheckWorld2(Index, Num, Xi, Yi);

			// Алгоритм:
			// 1. Узнаем координаты предполагаемого перемещения
			var (nXd, nYd, nXi, nYi, iEqual) = GetCoordinatesByDirectionForMove(dir);

			if (iEqual)
			{
				// ПЕРЕМЕЩЕНИЕ
				Xd = nXd;
				Yd = nYd;
				return true;
			}


			// Если координаты попадают за экран то вернуть RefContent.Edge
			if (IsItEdge(nXi, nYi))
			{
				ActivateReceptor5(dir);
				return false;
			}



			bool move = false;

			if (Data.World[nXi, nYi] == 0) //Free
			{
				lock (_busyWorld2)
				{
					if (Data.World[nXi, nYi] == 0)
					{
						// ПЕРЕМЕЩЕНИЕ A
						Data.World[Xi, Yi] = 0;
						//Thread.MemoryBarrier();
						Data.World[nXi, nYi] = Index;
						move = true;
					}
				}
			}

			if (move)
			{
				// ПЕРЕМЕЩЕНИЕ B

				if (Data.DrawType == DrawType.OnlyChangedCells)
				{
					Func.FixChangeCell(Xi, Yi, null);
					Func.FixChangeCell(nXi, nYi, Color);
				}

				//Log.LogInfo($"bot was moved from {Xi}/{Yi} to {nXi}/{nYi}.");

				Xd = nXd;
				Yd = nYd;
				Xi = nXi;
				Yi = nYi;

				//Func.CheckWorld2(Index, Num, Xi, Yi);


				return true;
			}
			else
			{
				//Func.CheckWorld2(Index, Num, Xi, Yi);

				var cont = Data.World[nXi, nYi];

				if (cont == 65500)
				{
					ActivateReceptor3(dir);
				}

				if (cont == 65503)
				{
					ActivateReceptor5(dir);
				}

				if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
				{
					ActivateReceptor2(dir, G.IsRelative(Data.Bots[cont].G), Data.Bots[cont].Direction);
				}

				//Func.CheckWorld2(Index, Num, Xi, Yi);
				return false;
			}
		}

		#endregion

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public bool CanReproduct()
		{
			return Energy >= Data.ReproductionBotEnergy;
		}

		public void HoldReproduction()
		{
			_reproductionCycle = Data.HoldReproductionTime;
		}

		private void ToReproductionList()
		{
			if (_reproductionCycle == Data.HoldReproductionTime)
			{
				ShareEnergy();
			}

			if (_reproductionCycle-- > 0) return;


			if (!InsertedToReproductionList)
			{
				lock (_busyInsertedToReproductionList)
				{
					if (!InsertedToReproductionList)
					{

						InsertedToReproductionList = true;
						Data.BotReproduction[Interlocked.Increment(ref Data.IndexOfLastBotReproduction)] = this;
					}
				}
			}
		}


		private bool IsItEdge(int x, int y)
		{
			return
				((Data.UpDownEdge && (y < 0 || y >= Data.WorldHeight)) ||
			(Data.LeftRightEdge && (x < 0 || x >= Data.WorldWidth)));
		}


		private void ShiftPointerGeneralToNextBranch()
		{
			if (PointerGeneral.B == G.ActiveGeneralBranchCnt - 1)
			{
				PointerGeneral.B = 0;
			}
			else
			{
				PointerGeneral.B++;
			}

			PointerGeneral.CmdNum = 0;
		}

		private (double newXdouble, double newYdouble, int newXint, int newYint, bool iEqual) GetCoordinatesByDirectionForMove(int dir)
		{
			var (deltaXdouble, deltaYdouble) = Dir.Directions1[dir];

			var newXdouble = Xd + deltaXdouble;
			var newYdouble = Yd + deltaYdouble;

			var newXint = Dir.Round(newXdouble);
			var newYint = Dir.Round(newYdouble);

			var iEqual = newXint == Xi && newYint == Yi;


			// Проверка перехода сквозь экран
			if (!Data.LeftRightEdge)
			{
				if (newXint < 0)
				{
					newXint += Data.WorldWidth;
					newXdouble += Data.WorldWidth;
				}

				if (newXint >= Data.WorldWidth)
				{
					newXint -= Data.WorldWidth;
					newXdouble -= Data.WorldWidth;
				}
			}

			if (!Data.UpDownEdge)
			{
				if (newYint < 0)
				{
					newYint += Data.WorldHeight;
					newYdouble += Data.WorldHeight;
				}

				if (newYint >= Data.WorldHeight)
				{
					newYint -= Data.WorldHeight;
					newYdouble -= Data.WorldHeight;
				}
			}

			return (newXdouble, newYdouble, newXint, newYint, iEqual);
		}
		
		private (int newXint, int newYint) GetCoordinatesByDirectionOnlyDifferent(int dir)
		{
			var (deltaXdouble, deltaYdouble) = Dir.Directions1[dir];

			var newXint = Dir.Round(Xd + deltaXdouble);
			var newYint = Dir.Round(Yd + deltaYdouble);

			if (newXint == Xi && newYint == Yi)
			{
				// координаты не изменились. надо шагнуть дальше
				(deltaXdouble, deltaYdouble) = Dir.Directions2[dir];

				newXint = Dir.Round(Xd + deltaXdouble);
				newYint = Dir.Round(Yd + deltaYdouble);

				// проверка на изменение координат еще раз. такого не может быть
				if (newXint == Xi && newYint == Yi)
				{
					throw new Exception("if (newXint == _Xi && newYint == _Yi)");
				}

				if (newXint - Xi > 1 || newXint - Xi < -1 || newYint - Yi > 1 || newYint - Yi < -1)
				{
					throw new Exception("if (newXint - _Xi > 1 || newXint - _Xi < -1 || newYint - _Yi > 1 || newYint - _Yi < -1)");
				}
			}

			// Проверка перехода сквозь экран
			if (!Data.LeftRightEdge)
			{
				if (newXint < 0)
				{
					newXint += Data.WorldWidth;
				}

				if (newXint >= Data.WorldWidth)
				{
					newXint -= Data.WorldWidth;
				}
			}

			if (!Data.UpDownEdge)
			{
				if (newYint < 0)
				{
					newYint += Data.WorldHeight;
				}

				if (newYint >= Data.WorldHeight)
				{
					newYint -= Data.WorldHeight;
				}
			}

			return (newXint, newYint);
		}

		//private bool IdentifyRelativity(long cont)
		//{
		//	return Genom.IsRelative(Data.Bots[cont].Genom);
		//}

		#region Direction
		private int GetDirAbsolute(byte par)
		{
			return par % Dir.NumberOfDirections;
		}

		private int GetDirRelative(byte par)
		{
			return (Direction + par) % Dir.NumberOfDirections;
		}

		private int GetDirForward()
		{
			return Direction;
		}

		private int GetDirRelativeWithRandom(byte par)
		{
			var rand = ThreadSafeRandom.Next(100);

			var shift = rand switch
			{
				99 => ThreadSafeRandom.Next(256),
				98 => ThreadSafeRandom.Next(11) - 5,
				_ => 0
			};

			if (shift < 0) shift += Dir.NumberOfDirections;
			return (Direction + par + shift) % Dir.NumberOfDirections;
		}
		#endregion

		#region GetText
		public string GetText1()
		{
			var sb = new StringBuilder();

			sb.AppendLine($"Age: {Age}");
			sb.AppendLine($"Num: {Num}");
			sb.AppendLine($"Index: {Index}");

			sb.AppendLine($"Energy: {Energy}");
			sb.AppendLine($"_dir: {Direction}");


			sb.AppendLine("");
			sb.AppendLine($"Genom {G.PraNum} {(G.Num != 0 ? $"({G.Num})" : "")}Lev{G.Level}");
			sb.AppendLine($"ActGen: {G.Act.Count(g => g > 0)}");

			sb.AppendLine("");
			sb.AppendLine($"Color: R{Color.R} G{Color.G} B{Color.B}");
			sb.AppendLine($"Active branch: {G.ActiveGeneralBranchCnt}");

			sb.AppendLine("");
			sb.AppendLine($"Attack {string.Join(",", G.Attack)}");
			sb.AppendLine($"Shield {string.Join(",", G.Shield)}");
			//sb.AppendLine($"Pra: {G.PraHash.ToString().Substring(0, 8)}");
			//sb.AppendLine($"Hash: {G.GenomHash.ToString().Substring(0, 8)}");
			//sb.AppendLine($"Parent: {G.ParentHash.ToString().Substring(0, 8)}");
			//sb.AppendLine($"Grand: {G.GrandHash.ToString().Substring(0, 8)}");

			return sb.ToString();
		}

		public string GetText2(int delta)
		{
			byte cmd, par;
			var sb = new StringBuilder();

			//sb.AppendLine($"23r,24a - rotate; 26r,27a - step");
			//sb.AppendLine($"28r,29a - eat; 30r,31a - look");

			sb.AppendLine($"Current OldPointer: {PointerGeneral.BOld}.{PointerGeneral.CmdNumOld}");
			sb.AppendLine($"Current Pointer: {PointerGeneral.B}.{PointerGeneral.CmdNum}");
			sb.AppendLine("");

			if (hist.historyPointerY >= 0)
			{
				var (hist, histPtrCnt, tm1, tm2, step) = this.hist.GetLastStepPtrs(delta);
				//sb.AppendLine($"OldPointer: {pointer.BOld}.{pointer.CmdNumOld}");
				//sb.AppendLine($"Pointer: {pointer.B}.{pointer.CmdNum}");
				sb.AppendLine($"Step: {step}");
				sb.AppendLine($"tm1:{tm1}  tm2:{tm2}");

				sb.AppendLine($"cmds cnt: {histPtrCnt - 1}");
				//sb.AppendLine($"cmds: {string.Join(", ", hist.Take(histPtrCnt))}");
				sb.AppendLine("");

				for (var i = 0; i < histPtrCnt; i++)
				{
					string cmdTxt;

					cmd = G.Code[hist[i].b, hist[i].c, 0];
					par = G.Code[hist[i].b, hist[i].c, 1];

					cmdTxt = $"{Cmd.CmdName(cmd)} {cmd}({hist[i].b}.{hist[i].c})";

					string dirStr;
					if (Data.CommandsWithParameter[cmd])
					{
						dirStr = Dir.GetDirectionStringFromCode(par);
					}
					else
					{
						dirStr = string.Empty;
					}

					string ev;
					if (hist[i].ev)
					{
						ev = $"EV{hist[i].b}";
					}
					else
					{
						ev = "";
					}


					if (cmdTxt != "")
					{

						sb.AppendLine($"{cmdTxt} {dirStr} {(hist[i].ev ? ev : "")}   ==tm:{hist[i].tm}");
					}
				}
			}

			sb.AppendLine("");

			return sb.ToString();
		}
		#endregion



		//private async Task<bool> SetBusy()
		//{
		//	try
		//	{
		//		await _semaphore.WaitAsync();
		//	}
		//	catch
		//	{
		//		ReleaseBusy();
		//		return false;
		//	}
		//	return true;
		//}

		//private void ReleaseBusy()
		//{
		//	try
		//	{
		//		_semaphore.Release();
		//	}
		//	catch { }
		//}

		//private bool TrySetBusyMode()
		//{
		//	lock (_syncBusyMode)
		//	{
		//		if (_busyState) return false;
		//		_busyState = true;
		//		return true;
		//	}
		//}
		//private void ReleaseBusyMode()
		//{
		//	lock (_syncBusyMode)
		//	{
		//		_busyState = false;
		//	}
		//}

		//	private async Task<(SocketAcceptionResult, ISocket)> GetSocket(ISocketManager socketManager, string tokenId)
		//	{
		//		try
		//		{
		//			var suссessBusy = await SetBusy();
		//			if (!suссessBusy)
		//			{
		//				return (SocketAcceptionResult.UnknownError, null);
		//			}
		//		}
		//		// ReleaseBusy() в finally, чтобы перед каждым return его не писать
		//		finally
		//		{
		//			// Метод не возвращает исключений.
		//			ReleaseBusy();
		//		}
		//	}
		//}
	}
}
