using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.Static;

namespace WindowsFormsApp1.GameLogic
{
	// Бот с океана foo52
	public class Bot1
	{
		public long Index;         // Индекс бота (может меняться)
		public long Num;         // Номер бота (остается постоянным)
		public bool Alive;
		public bool InsertedToDeathList;  // чтобы не вставить бота два раза в этот лист, только для этого
		public bool InsertedToReproductionList;  // чтобы не вставить бота два раза в этот лист, только для этого
		public int Direction;         // Направление бота
		public int Age;
		public int BiteMeCount;
		public int BiteImCount;
		public int DividedCount;
		public Genom G;
		public Color Color;
		public Pointer PointerGeneral = new Pointer();
		public Pointer PointerReaction = new Pointer();
		public CodeHistory hist = new CodeHistory();
		public double Xd;
		public double Yd;
		public int Xi;
		public int Yi;
		//public BotLog Log = new BotLog();

		private int _en;
		private int _size;
		private int _reproductionCycle;
		private long _moved = 0;
		private int _tm = 0;
		private int _tmR = 0;

		public bool ConnectedTo;
		public Bot1 ConnectedToBot;
		public long ConnectedToBotNum;
		private bool _forced;
		private int _forcedDir;

		public void Init(int x, int y, long ind, int en, Genom genom)
		{
			var dir = Func.GetRandomDirection();
			var botNumber = Interlocked.Increment(ref Data.MaxBotNumber);
			genom.IncBot();

			G = genom;
			Direction = dir;
			Num = botNumber;
			Index = ind;
			Xd = x;
			Yd = y;
			Xi = x;
			Yi = y;

			Age = 0;
			BiteMeCount = 0;
			BiteImCount = 0;
			DividedCount = 0;
			InsertedToDeathList = false;
			InsertedToReproductionList = false;
			Alive = true;
			_recActive = false;
			_moved = 0;
			_reproductionCycle = 0;

			ConnectedTo = false;
			ConnectedToBot = null;
			ConnectedToBotNum = 0;
			_tm = 0;
			_tmR = 0;

			PointerGeneral.Clear();
			PointerReaction.Clear();
			RefreshColor();

			if (en <= 0) throw new Exception("sdfsdf");
			if (_en != 0) throw new Exception("fglkeru84");

			lock (_busyBotEnergy)
			{
				_en = en;
			}
			//Log.LogInfo($"bot was initialized. index:{Index}");
		}

		//--------------------------------------------------------------

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
		private byte _recWeight;
		public bool _recNew;
		public byte _recNewBranch;
		private int _recDirToContact;

		// свойства контакта
		private double _recContactX;
		private double _recContactY;
		private int _recContactDirection;
		private Bot1 _recContactBot;
		private long _recContactBotNum;


		//private static long COUNTER1 = 0;
		//private static long COUNTER2 = 0;


		public bool Moved
		{
			get
			{
				return _moved > 10;
			}
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

		public void Force(int forceDir)
		{
			_forcedDir = forceDir;
			_forced = true;
		}

		public int Bite(int delta, int dirToContact, Bot1 recContactBot)
		{
			Interlocked.Increment(ref BiteMeCount);
			ActivateReceptor1(dirToContact, recContactBot);
			return EnergyChange(delta);
		}

		public void Touch(int dir, bool isRel, Bot1 recContactBot)
		{
			ActivateReceptor2(dir, isRel, recContactBot);
		}

		#region Receptors
		private void ActivateReceptor(byte recWeight, byte bNew, int dirToContact, double contactX, double contactY, Bot1 recContactBot)
		{
			if (!_recActive || _recWeight > recWeight)
			{
				lock (_busyReceptors)
				{
					if (!_recActive || _recWeight > recWeight)
					{
						// свойства контакта
						_recContactX = contactX;
						_recContactY = contactY;
						if (recContactBot != null)
						{
							_recContactDirection = recContactBot.Direction;
							_recContactBot = recContactBot;
							_recContactBotNum = recContactBot.Num;
						}
						else
						{
							_recContactDirection = 0;
							_recContactBot = null;
							_recContactBotNum = 0;
						}

						_recDirToContact = dirToContact;
						_recNewBranch = bNew;
						_recNew = true;
						_recActive = true;  // надо делать в пследнюю очередь чтобы попало в GetCommand if (_recNum > 0) с полностью подготовленными данными
					}
				}
			}
		}


		// 1 - укус
		private void ActivateReceptor1(int dirToContact, Bot1 recContactBot)
		{
			ActivateReceptor(0, Branches.React_Bite, dirToContact, recContactBot.Xd, recContactBot.Yd, recContactBot);
		}


		// 2 - рядом бот
		private void ActivateReceptor2(int dirToContact, bool isRel, Bot1 recContactBot) //, bool block, int massa, byte[] Shield, byte[] Attack, int dir, bool mov)
		{
			if (!_recActive || _recWeight > 3)
			{
				if (isRel)
				{
					ActivateReceptor(3, Branches.React_Bot_Relat, dirToContact, recContactBot.Xd, recContactBot.Yd, recContactBot);
					return;
				}

				var dig = recContactBot.G.Digestion;

				if (dig == G.Digestion)
				{
					ActivateReceptor(1, Branches.React_Bot_Enemy, dirToContact, recContactBot.Xd, recContactBot.Yd, recContactBot);

					var needbigrotate = Dir.GetDirDiff(dirToContact, recContactBot.Direction) < Dir.NumberOfDirections / 4;
					if (needbigrotate)
					{
						ActivateReceptor(2, Branches.React_Bot_Bigrot, dirToContact, recContactBot.Xd, recContactBot.Yd, recContactBot);
						return;
					}
					else
					{
						ActivateReceptor(1, Branches.React_Bot_NoBigrot, dirToContact, recContactBot.Xd, recContactBot.Yd, recContactBot);
						return;
					}
				}

				if (dig < G.Digestion)
				{
					ActivateReceptor(2, Branches.React_Bot_LessDigestion, dirToContact, recContactBot.Xd, recContactBot.Yd, recContactBot);
					return;
				}


				if (dig > G.Digestion)
				{
					ActivateReceptor(2, Branches.React_Bot_BiggerDigestion, dirToContact, recContactBot.Xd, recContactBot.Yd, recContactBot);
					return;
				}
			}
		}

		// 3 - рядом еда
		private void ActivateReceptor3(int dirToContact, double contactX, double contactY)
		{
			ActivateReceptor(4, Branches.React_Grass, dirToContact, contactX, contactY, null);
		}

		// 4 - рядом минерал
		private void ActivateReceptor4(int dirToContact, double contactX, double contactY)
		{
			ActivateReceptor(5, Branches.React_Mineral, dirToContact, contactX, contactY, null);
		}

		// 5 - рядом край/стена
		private void ActivateReceptor5(int dirToContact, double contactX, double contactY)
		{
			ActivateReceptor(6, Branches.React_Wall, dirToContact, contactX, contactY, null);
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

		public void RefreshColor()
		{
			Color = Data.BotColorMode switch
			{
				BotColorMode.GenomColor => G.Color,
				BotColorMode.PraGenomColor => G.PraColor,
				BotColorMode.PlantPredator => GetGraduatedColor(G.Digestion, 0, Data.DigestionTypeCount),
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
							PointerReaction.B = (byte)(_recNewBranch);
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

		private bool ProcessExternalForce()
		{
			if (ConnectedTo)
			{
				// Проверка что это тот же самый бот, а не обновленный (новый созданный на месте старого)
				if (ConnectedToBot.Alive && ConnectedToBot.Num == ConnectedToBotNum)
				{
					var de = Energy - ConnectedToBot.Energy;
					if (de > 20 /*&& targetBot.Energy > 0 && !targetBot.InsertedToDeathList*/)
					{
						var transferedEnergy = EnergyChange(-de / 2);
						ConnectedToBot.EnergyChange(transferedEnergy);
						if (transferedEnergy < 0) throw new Exception("dfgdfg");
					}

					//if (de < -20 /*&& targetBot.Energy > 0 && !targetBot.InsertedToDeathList*/)
					//{
					//	var transferedEnergy = ConnectedToBot.EnergyChange(de / 2);
					//	EnergyChange(transferedEnergy);
					//	if (transferedEnergy < 0) throw new Exception("dfgdfg");
					//}

					var dx = ConnectedToBot.Xi - Xi;
					var dy = ConnectedToBot.Yi - Yi;

					if (dx < -1 || dx > 1 || dy < -1 || dy > 1)
					{
						var dir = Dir.GetDirectionTo(Xd, Yd, ConnectedToBot.Xd, ConnectedToBot.Yd);
						_forced = false;
						Step(dir);
						if (Data.HistoryOn) hist.SaveForcedCmdToHistory(Cmd.StepAbsolute, (byte)dir, 0);
						return true;
					}
				}
				else
				{
					ConnectedTo = false;
				}
			}

			if (_forced)
			{
				_forced = false;
				Step(_forcedDir);
				if (Data.HistoryOn) hist.SaveForcedCmdToHistory(Cmd.StepAbsolute, (byte)_forcedDir, 0);
				return true;
			}

			return false;
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

			t = Stopwatch.GetTimestamp();

			var externalInfluence = ProcessExternalForce();

			while (_tm < 100 && !lastcmd && cntJmp < 10)
			{
				(ev, p_H, cmd, par) = GetCommand();

				if (externalInfluence && Data.CmdClass[cmd] == CmdClass.Step)
				{
					cmd = Cmd.Nothing;
				}


				t = Test2.Mark(4, t);

				cntJmp++;
				tm = 0;

				/*
					при каждой мутации масса может меняться вверх или вниз процентов на 10
					более массивные расталкивают менее массивных
					боты притягиваютс к большой массе
					если рядом бот большей массы то удирать
					бот может взорваться при определенных условиях
					При столкновении с другим ботов взрывается
					муравьиная дорожка
					боты-зомби
					удачно мутировавшие гены чтоб больше не мутировали или с меньшей вероятностью?
					усики как у муравья/таракана
					по запаху двигается
					можно защита на разных сторонах разная
					избегать вражеских ботов стремиться к еде
					многоклеточность ( создание потомка, приклееного к боту )
					преобразовать минералы в энерию
					кусать только тех у кого мало энергии

					УСЛОВИЯ
					далеко ли от матки?
					какое моё здоровье
					есть ли препятствие для движения?
					впереди есть боты?
					увиден бот? минерал? стена? еда?
					таймер закончился?
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
					сколько  минералов
					какой мой уровень?
					минералы прибавляются?
					многоклеточный ли я ?
					много ли осталось пищи таокго типа - может пока не есть пусть размножится
					какое сейчас время суток - в зависимости от времени суток меняеьтся поведения бота - может утром он стремится вверх после обеда вниз
					какое сейчас время года - в зависимости от времени года меняеьтся поведения бота - может зимой он стремится вверх летом вниз
				 */



				switch (cmd)
				{
					//звать на помощь родню
					//двигаться к ближайшей родне
					//рои
					//возможность собирнаться маленькими кучками
					//объединяться при нападении вокруг альфы

					//сломать стену перед собой
					//телепортация
					//отталкиваться от опасных врагов
					//наступает только на ту клетку где вокруг нет хищников


					//// CHANGE OWN PROPERTIES
					//стать невидимым для ботов
					//камуфляж
					//притворится мертвым или стеной
					//активировать защиту от определенной атаки
					//при необходимости один тип защиты преобразовать в другой

					//// INFLUENCE ON CONTACT
					//повернуть контакт
					//повернуть бота от себя
					//повернуть всех ботов от себя
					//повернуть всех ботов к себе
					//обездвижить бота
					//снизить уровни атаки бота
					//отключить защиту бота
					//перебросить бота за себя
					//перебросить бота в сторону0
					//поменяться местами

					//оттолкнуть / ударить бота
					//оттолкнуть всех окружающих ботов
					//оттолкнуть всех окружающих ботов с большого радиуса
					//притянуть бота
					//притянуть ботов с большого радиуса
					//превратить в своего
					//высосать энергию из окружающих ботов в каком - то радиусе

					//// ROTATE
					case Cmd.RotateAbsolute: tm = RotateAbsolute(par); break;
					case Cmd.RotateRelative: tm = RotateRelative(par); Test2.Mark(10, t); break;
					case Cmd.RotateBackward: tm = RotateBackward(); Test2.Mark(12, t); break;
					case Cmd.RotateRandom: tm = RotateRandom(); break;
					case Cmd.AlignHorizontaly: tm = AlignHorizontaly(); break;
					case Cmd.RotateRelativeContact: tm = RotateRelativeContact(par); Test2.Mark(11, t); break;
					case Cmd.RotateParallelContact: tm = RotateParallelContact(); Test2.Mark(11, t); break;
					case Cmd.RotateBackwardContact: tm = RotateBackwardContact(); Test2.Mark(13, t); break;
					case Cmd.RotateToContact: tm = RotateToContact(); Test2.Mark(11, t); break;
					//не поворачиваться на этом шаге

					//// STEP
					case Cmd.StepAbsolute: tm = StepAbsolute(par, externalInfluence); break;
					case Cmd.StepRelative: tm = StepRelative(par, externalInfluence); break;
					case Cmd.StepForward: tm = StepForward(externalInfluence); Test2.Mark(14, t); break;
					case Cmd.StepBackward: tm = StepBackward(externalInfluence); Test2.Mark(16, t); break;
					case Cmd.StepRelativeContact: tm = StepRelativeContact(par, externalInfluence); Test2.Mark(15, t); break;
					case Cmd.StepBackwardContact: tm = StepBackwardContact(externalInfluence); Test2.Mark(17, t); break;
					case Cmd.StepToContact: tm = StepToContact(externalInfluence); Test2.Mark(17, t); break;
					case Cmd.StepNearContact: tm = StepNearContact(par, externalInfluence); Test2.Mark(17, t); break;
					//переместится под бота
					//переместится к боту (притянуться к боту)
					//переместится над ботом

					//шаг влево относительно контакта
					//шаг вправо относительно контакта

					//переместится перед ботом
					//переместится сзади бота
					//перепрыгнуть бота
					//встать рядом с ботом
					//копировать движеия
					//не двигаться на этом шаге


					//// EAT
					case Cmd.EatForward: tm = EatForward(); Test2.Mark(18, t); break;
					case Cmd.EatContact: tm = EatContact(); break;

					//// LOOK
					case Cmd.LookAround1: tm = LookAround1(); Test2.Mark(19, t); break;
					case Cmd.LookAround2: tm = LookAround2(); Test2.Mark(20, t); break;
					case Cmd.LookForward: tm = LookForward(); Test2.Mark(21, t); break;

					//// OTHERS
					case Cmd.ClingToContact: tm = ClingToContact(); break;
					case Cmd.Nothing: tm = 0; break;
					case Cmd.PushContact: tm = PushContact(); break;
					//сцепиться с родней
					//#прицепиться к контакту
					//прицепить к себе контакта
					//держаться на определенном расстоянии от того к кому прицеплен, например не больше 10

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

				if (Data.HistoryOn && p_H != null) hist.SaveCmdToHistory(p_H, ev, tm);
				Test2.Mark(7, t2);

				if (ev)
				{
					PointerReaction.CmdNum++;
					//if (PointerReaction.CmdNum >= Data.MaxCmdInStep) lastcmd = true;

					_tmR += tm;

					//if (_tmR >= 100 || lastcmd)
					if (_tmR >= 100 || PointerReaction.CmdNum >= Data.MaxCmdInStep)
					{
						_recActive = false; // завершаем команду реакции, переходим на обычные команды
					}
				}
				else
				{
					PointerGeneral.CmdNum++;
					if (PointerGeneral.CmdNum >= Data.MaxCmdInStep) lastcmd = true;
				}

				t = Test2.Mark(5, t);

				_tm += tm;
			}

			if (Data.HistoryOn) hist.EndNewStep(_tm);

			_tm -= 100;
			if (_tm < 0) _tm = 0;
			//if (_tm < -10000)
			//{
			//	throw new Exception("67343");
			//}	

			ShiftPointerGeneralToNextBranch();

			Test2.Mark(6, t);
		}

		//===================================================================================================
		//// Rotate
		private int RotateAbsolute(int dir)
		{
			var tm = Dir.GetDirDiff(Direction, dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(dir);

			return tm;
		}

		private int RotateRelative(int dir)
		{
			var tm = Dir.GetDirDiff(0, dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(Direction + dir);

			return tm;
		}

		private int RotateRelativeContact(int dir)
		{
			var tm = Dir.GetDirDiff(Direction, _recDirToContact + dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(_recDirToContact + dir);

			return tm;
		}

		private int RotateToContact()
		{
			var tm = Dir.GetDirDiff(Direction, _recDirToContact) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(_recDirToContact);

			return tm;
		}

		private int RotateBackward()
		{
			var tm = Dir.NumberOfDirections / 2 * CmdType.CmdTime(CmdType.Rotate);

			Rotate(Dir.GetOppositeDirection(Direction));

			return tm;
		}

		private int RotateBackwardContact()
		{
			var dir = Dir.GetOppositeDirection(_recDirToContact);

			var tm = Dir.GetDirDiff(Direction, dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(dir);

			return tm;
		}

		private int RotateRandom()
		{
			var dir = Func.GetRandomDirection();

			var tm = Dir.GetDirDiff(Direction, dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(dir);

			return tm;
		}

		private int AlignHorizontaly()
		{
			var dir = 16;

			var tm = Dir.GetDirDiff(Direction, dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(dir);

			return tm;
		}

		private int RotateParallelContact()
		{
			var dir = _recContactDirection;

			var tm = Dir.GetDirDiff(Direction, dir) * CmdType.CmdTime(CmdType.Rotate);

			Rotate(dir);

			return tm;
		}

		//// Step
		private int StepAbsolute(int dir, bool externalInfluence)
		{
			if (externalInfluence) return CmdType.CmdTime(CmdType.StepNotSuccessful);

			var move = Step(dir);

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		private int StepForward(bool externalInfluence)
		{
			if (externalInfluence) return CmdType.CmdTime(CmdType.StepNotSuccessful);

			var move = Step(GetDirForward());

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		private int StepRelative(int dir, bool externalInfluence)
		{
			if (externalInfluence) return CmdType.CmdTime(CmdType.StepNotSuccessful);

			var move = Step((Direction + dir) % Dir.NumberOfDirections);

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		private int StepRelativeContact(int dir, bool externalInfluence)
		{
			if (externalInfluence) return CmdType.CmdTime(CmdType.StepNotSuccessful);

			var move = Step((_recDirToContact + dir) % Dir.NumberOfDirections);

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		private int StepBackward(bool externalInfluence)
		{
			if (externalInfluence) return CmdType.CmdTime(CmdType.StepNotSuccessful);

			var move = Step(Dir.GetOppositeDirection(Direction));

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		private int StepBackwardContact(bool externalInfluence)
		{
			if (externalInfluence) return CmdType.CmdTime(CmdType.StepNotSuccessful);

			var move = Step(Dir.GetOppositeDirection(_recDirToContact));

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		private int StepToContact(bool externalInfluence)
		{
			if (externalInfluence) return CmdType.CmdTime(CmdType.StepNotSuccessful);

			var move = Step(_recDirToContact);

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		private int StepNearContact(int dir, bool externalInfluence)
		{
			if (externalInfluence) return CmdType.CmdTime(CmdType.StepNotSuccessful);

			var (deltaXdouble, deltaYdouble) = Dir.Directions2[dir];

			var dir2 = Dir.GetDirectionTo(Xd, Yd, _recContactX + deltaXdouble, _recContactY + deltaYdouble);

			var move = Step(dir2);

			return move ? CmdType.CmdTime(CmdType.StepSuccessful) : CmdType.CmdTime(CmdType.StepNotSuccessful);
		}

		//// Eat
		private int EatForward()
		{
			var eat = Eat(GetDirForward());

			return eat ? CmdType.CmdTime(CmdType.EatSuccessful) : CmdType.CmdTime(CmdType.EatNotSuccessful);
		}

		private int EatContact()
		{
			var eat = Eat(_recDirToContact);

			return eat ? CmdType.CmdTime(CmdType.EatContactSuccessful) : CmdType.CmdTime(CmdType.EatContactNotSuccessful);
		}

		//// Look
		private int LookForward()
		{
			Look(GetDirForward());

			return CmdType.CmdTime(CmdType.Look);
		}

		private int LookAround1()
		{
			LookAround(1, 8);
			return CmdType.CmdTime(CmdType.LookAround1);
		}

		private int LookAround2()
		{
			LookAround(2, 24);
			return CmdType.CmdTime(CmdType.LookAround2);
		}

		private int ClingToContact()
		{
			if (!ConnectedTo && _recContactBot != null && _recContactBot.Alive && _recContactBot.Num == _recContactBotNum)
			{
				ConnectedTo = true;
				ConnectedToBot = _recContactBot;
				ConnectedToBotNum = _recContactBotNum;

				return CmdType.CmdTime(CmdType.ClingToSuccessful);
			}
			else
			{
				return CmdType.CmdTime(CmdType.ClingToNotSuccessful);
			}
		}

		private int PushContact()
		{
			if (_recContactBot != null && _recContactBot.Alive && _recContactBot.Num == _recContactBotNum)
			{
				_recContactBot.Force(_recDirToContact);
				return CmdType.CmdTime(CmdType.PushContactSuccessful);
			}
			else
			{
				return CmdType.CmdTime(CmdType.PushContactNotSuccessful);
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
		private bool Photosynthesis()
		{
			//if (Data.TotalEnergy < Data.KeptTotalEnergy && DividedCount == 0 && Yi < Data.PhotosynthesisLayerHeight)
			if (Data.TotalEnergy < Data.KeptTotalEnergy && Yi < Data.PhotosynthesisLayerHeight)
			{
				EnergyChange(Data.PhotosynthesisEnergy);
				Interlocked.Add(ref Data.TotalEnergy, Data.PhotosynthesisEnergy);
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool Eat(int dir)
		{
			if (G.Digestion == 0) // ТОЛЬКО ДЛЯ РАСТЕНИЙ
			{
				return Photosynthesis();
			}

			// Алгоритм:
			// 1. Узнаем координаты клетки на которую надо съесть
			var (nXi, nYi) = GetCoordinatesByDirectionOnlyDifferent(dir);


			// 2. Узнаем что находится на этой клетке
			if (IsItEdge(nXi, nYi))
			{
				ActivateReceptor5(dir, nXi, nYi);
				return false;
			}

			if (Data.World[nXi, nYi] == 65503)
			{
				ActivateReceptor5(dir, nXi, nYi);
				return false;
			}


			// Grass
			if (G.Digestion == 1)  // ТОЛЬКО ДЛЯ ТРАВОЯДНЫХ
			{
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
			}


			var cont = Data.World[nXi, nYi];

			// Bot или Relative
			if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
			{
				return EatBot(cont, dir);
			}
			else
			{
				return false;
			}
		}


		private bool EatBot(long cont, int dir)
		{
			var eatedBot = Data.Bots[cont];

			// Может есть своего уровня или на уровень меньше.
			if (eatedBot.G.Digestion != G.Digestion && eatedBot.G.Digestion + 1 != G.Digestion)
			{
				return false;
			}

			// Животное может есть растение, но ни тогда когда его осталось мало
			if (eatedBot.G.Digestion == 0 && eatedBot.DividedCount == 0)
			{
				return false;
				if (eatedBot.G.CurBots < 2)
				{
					return false;
				}
			}

			//if (eatedBot.DividedCount == 0)
			//{
			//	return false;
			//}

			//if (eatedBot.G.CurBots < 10)
			//{
			//	return false;
			//}

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

			// Не может распространяться более чем 40% от всех ботов
			if (G.CurBots >= Data.CurrentNumberOfBots * 0.4)
			{
				return false;
			}

			// Не может есть многоклетоочных
			//if (eatedBot.ConnectedTo && eatedBot.ConnectedToBot.Alive && eatedBot.ConnectedToBot.Num == eatedBot.ConnectedToBotNum)
			//{
			//	return false;
			//}

			//var olden = Energy;
			var atc = 0;
			//if (eatedBot.G.Digestion + 1 == G.Digestion)
			//{
			//	atc = 2;
			//}

			//if (eatedBot.G.Digestion == G.Digestion || eatedBot.G.Digestion + 1 == G.Digestion)
			if (eatedBot.G.Digestion + 1 == G.Digestion || eatedBot.G.Digestion == G.Digestion)
			{
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
			}

			if (atc > 0)
			{
				// Data.BiteEnergy / 2 * atc - отрицательное число. возвращается положительное число.

				var k = 20;
				if (!Moved && eatedBot.Moved) k = Data.MovedBiteStrength;

				var requestedEnergy = Data.BiteEnergy * 10 / k * atc;

				Interlocked.Increment(ref BiteImCount);
				var gotEnergyByEating = eatedBot.Bite(requestedEnergy, Dir.GetOppositeDirection(dir), this);
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
					(nXi, nYi) = Func.GetCoordinatesByDelta(Xi, Yi, n, 1);

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
				ActivateReceptor5(dir, nXi, nYi);
				return;
			}

			var cont = Data.World[nXi, nYi];

			if (cont == 65503)
			{
				ActivateReceptor5(dir, nXi, nYi);
				return;
			}

			if (cont == 65500)
			{
				ActivateReceptor3(dir, nXi, nYi);
				return;
			}

			if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
			{
				var b = Data.Bots[cont];
				ActivateReceptor2(dir, G.IsRelative(b.G), b);
			}

			return;
		}

		private void LookAround(int widht, int cnt)
		{
			int nXi, nYi, dirToContact;
			Bot1 b;

			Bot1 rel = null;
			bool edge = false; int xEdge = 0, yEdge = 0;
			bool grass = false; int xGrass = 0, yGrass = 0;

			var nrnd = ThreadSafeRandom.Next(cnt);


			for (var i = 0; i < cnt; i++)
			{
				nrnd++; if (nrnd >= cnt) nrnd = 0;

				(nXi, nYi) = Func.GetCoordinatesByDelta(Xi, Yi, nrnd, widht);

				if (!IsItEdge(nXi, nYi))
				{
					var cont = Data.World[nXi, nYi];

					if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
					{
						if (!G.IsRelative(Data.Bots[cont].G))
						{
							b = Data.Bots[cont];

							dirToContact = Dir.GetDirectionTo(Xd, Yd, b.Xd, b.Yd);
							//var dir1 = Dir.NearbyCellsDirection[n];
							ActivateReceptor2(dirToContact, false, b);
							return;
						}
						else
						{
							rel = Data.Bots[cont];
						}
					}

					if (cont == 65503)  // wall
					{
						edge = true;
						xEdge = nXi;
						yEdge = nYi;
					}

					if (cont == 65500)  // grass
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
				dirToContact = Dir.GetDirectionTo(Xd, Yd, rel.Xd, rel.Yd);
				//var dir1 = Dir.NearbyCellsDirection[n];
				ActivateReceptor2(dirToContact, true, rel);
				return;
			}

			if (edge)
			{
				dirToContact = Dir.GetDirectionTo(Xd, Yd, xEdge, yEdge);
				ActivateReceptor5(dirToContact, xEdge, yEdge);
				return;
			}

			if (grass)
			{
				dirToContact = Dir.GetDirectionTo(Xd, Yd, xGrass, yGrass);
				ActivateReceptor3(dirToContact, xGrass, yGrass);
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
			if (G.Digestion == 0) // ТОЛЬКО ДЛЯ РАСТЕНИЙ
			{
				return false;
			}
			//Func.CheckWorld2(Index, Num, Xi, Yi);

			// Алгоритм:
			// 1. Узнаем координаты предполагаемого перемещения
			var (nXd, nYd, nXi, nYi, deltaXInt, deltaYInt, iEqual) = GetCoordinatesByDirectionForMove(dir);

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
				ActivateReceptor5(dir, nXi, nYi);
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

				ProcessingTouchedCells(deltaXInt, deltaYInt);

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
					ActivateReceptor3(dir, nXi, nYi);
				}

				if (cont == 65503)
				{
					ActivateReceptor5(dir, nXi, nYi);
				}

				if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
				{
					var b = Data.Bots[cont];
					ActivateReceptor2(dir, G.IsRelative(b.G), b);
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
			if (PointerGeneral.CmdNum != 0)
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
		}

		private void ProcessingTouchedCells(int deltaXInt, int deltaYInt)
		{
			int x, y;
			var touched = Dir.GetTouchedCells(deltaXInt, deltaYInt);
			foreach (var t in touched)
			{

				x = Xi + t.X;
				y = Yi + t.Y;

				// Переход сквозь экран
				if (!Data.LeftRightEdge)
				{
					if (x < 0) x += Data.WorldWidth;
					if (x >= Data.WorldWidth) x -= Data.WorldWidth;
				}
				else
				{
					if (x < 0 || x >= Data.WorldWidth)
					{
						ActivateReceptor5(t.Dir, x, y);
						continue;
					}
				}

				if (!Data.UpDownEdge)
				{
					if (y < 0) y += Data.WorldHeight;
					if (y >= Data.WorldHeight) y -= Data.WorldHeight;
				}
				else
				{
					if (y < 0 || y >= Data.WorldHeight)
					{
						ActivateReceptor5(t.Dir, x, y);
						continue;
					}
				}

				var cont = Data.World[x, y];

				if (cont == 65503)
				{
					ActivateReceptor5(t.Dir, x, y);
					return;
				}

				if (cont == 65500)
				{
					ActivateReceptor3(t.Dir, x, y);
					return;
				}

				if (cont >= 1 && cont <= Data.CurrentNumberOfBots)
				{
					var b = Data.Bots[cont];
					var isRel = G.IsRelative(b.G);
					ActivateReceptor2(t.Dir, isRel, b);
					b.Touch(t.DirOp, isRel, this);
				}
			}
		}

		private (double newXdouble, double newYdouble, int newXint, int newYint, int deltaXInt, int deltaYInt, bool iEqual) GetCoordinatesByDirectionForMove(int dir)
		{

			var (deltaXdouble, deltaYdouble) = Dir.Directions1[dir];

			var newXdouble = Xd + deltaXdouble;
			var newYdouble = Yd + deltaYdouble;

			var newXint = Dir.Round(newXdouble);
			var newYint = Dir.Round(newYdouble);

			var iEqual = newXint == Xi && newYint == Yi;

			var deltaXInt = newXint - Xi;
			var deltaYInt = newYint - Yi;

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

			return (newXdouble, newYdouble, newXint, newYint, deltaXInt, deltaYInt, iEqual);
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

			// Переход сквозь экран
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

		//private (double newX, double newY) GetCoordinatesByDirectionOnlyDifferent2(int dir)
		//{
		//	var (deltaXdouble, deltaYdouble) = Dir.Directions1[dir];

		//	if (Dir.Round(_recContactX + deltaXdouble) == Dir.Round(_recContactX) && Dir.Round(_recContactY + deltaYdouble) == Dir.Round(_recContactY))
		//	{
		//		(deltaXdouble, deltaYdouble) = Dir.Directions2[dir];
		//	}

		//	var newX = _recContactX + deltaXdouble;
		//	var newY = _recContactY + deltaYdouble;

		//	// Переход сквозь экран
		//	if (!Data.LeftRightEdge)
		//	{
		//		if (newX < 0)
		//		{
		//			newX += Data.WorldWidth;
		//		}

		//		if (newX >= Data.WorldWidth)
		//		{
		//			newX -= Data.WorldWidth;
		//		}
		//	}

		//	if (!Data.UpDownEdge)
		//	{
		//		if (newY < 0)
		//		{
		//			newY += Data.WorldHeight;
		//		}

		//		if (newY >= Data.WorldHeight)
		//		{
		//			newY -= Data.WorldHeight;
		//		}
		//	}

		//	return (newX, newY);
		//}

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
			sb.AppendLine($"Digestion: {G.Digestion}");
			sb.AppendLine($"Connected to: {(ConnectedTo ? $"Yes {ConnectedToBotNum}" : "No")}");

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
					string sev = "";
					string bc = "";

					//var (b, c, ev, _, tm) = hist[i].GetAllHistoryData();
					var (b0, b1, ev, force, tm) = hist[i].GetHistoryData();

					if (!force)
					{
						cmd = G.Code[b0, b1, 0];
						par = G.Code[b0, b1, 1];

						bc = $"({b0}.{b1})";

						if (ev)
						{
							sev = $"EV{b0}";
						}
					}
					else
					{
						cmd = b0;
						par = b1;
					}


					cmdTxt = $"{cmd} {Data.CmdName[cmd]} {bc}";

					string dirStr;
					if (Data.CmdWithParameter[cmd])
					{
						dirStr = Dir.GetDirectionStringFromCode(par);
					}
					else
					{
						dirStr = string.Empty;
					}


					if (cmdTxt != "")
					{

						sb.AppendLine($"{cmdTxt} {dirStr} {(ev ? sev : "")}   ==tm:{tm}");
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
