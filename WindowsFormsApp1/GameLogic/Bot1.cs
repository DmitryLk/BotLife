using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
        //private static readonly object _busy = new object();

        const int hueFrom = 200;

        private static readonly object _busyWorld1 = new object();
        private static readonly object _busyWorld2 = new object();
        private readonly object _busyBotEnergy = new object();
        private readonly object _busyInsertedToReproductionList = new object();
        //private static readonly object _busyTotalEnergy = new object();
        //private readonly object _busyBite = new object();
        //private readonly object _busyInsertedToDeathList = new object();



        //private static long COUNTER1 = 0;
        //private static long COUNTER2 = 0;

        public Genom Genom;
        public Color Color;
        public int Pointer;
        public int OldPointer;

        public CodeHistory Hist;
        //public BotLog Log;

        public double Xd;
        public double Yd;
        public int Xi;
        public int Yi;


        public long Index;         // Индекс бота (может меняться)
        public int Direction;         // Направление бота
        public int Age;
        public long Num;         // Номер бота (остается постоянным)

        private int _en;
        private int _size;

        public bool Alive;
        public bool InsertedToDeathList;  // чтобы не вставить бота два раза в этот лист, только для этого
        public bool InsertedToReproductionList;  // чтобы не вставить бота два раза в этот лист, только для этого
        private int _reproductionCycle;

        private int df = 0;

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

        public int EnergyChange(int delta)
        {
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
                Color = GetGraduatedColor(Energy, 0, 6000);
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


        public Bot1(int x, int y, int dir, long botNumber, long botIndex, int en, Genom genom, int pointer)
        {
            Pointer = pointer;
            OldPointer = pointer;
            this.Genom = genom;
            Hist = new CodeHistory();
            //Log = new BotLog();

            Direction = dir;
            Num = botNumber;
            Index = botIndex;
            _en = en;
            Age = 0;

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
                BotColorMode.GenomColor => Genom.Color,
                BotColorMode.PraGenomColor => Genom.PraColor,
                BotColorMode.PlantPredator => Genom.Plant ? Color.Green : Color.Red,
                BotColorMode.Energy => GetGraduatedColor(Energy, 0, 6000),
                BotColorMode.Age => GetGraduatedColor(Age, 0, 500),
                BotColorMode.GenomAge => GetGraduatedColor((int)(Data.CurrentStep - Genom.BeginStep), 0, 10000),
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
            // Некий алгоритм активности бота в рамках одного игорового шага.
            // В результате которого он на основании данных от рецепторов или без них
            // изменит свое состояние (координаты, геном, энергия, здоровье, возраст, направление, цвет, минералы, ...)
            // Также может размножиться

            //Direction dir;
            int shift = 0;
            bool stepComplete = false;
            int cntJump = 0;
            //Func.CheckWorld2(Index, Num, Xi, Yi);
            //Func.CheckWorld2(Index, Num, Xi, Yi);

            if (Data.Hist) Hist.BeginNewStep();

            do
            {
                // 1. Определяем команду которую будет делать бот
                var cmdCode = Genom.GetCurrentCommand(Pointer);
                if (Data.Hist) Hist.SavePtr(Pointer);

                // 2. Выполняем команду
                switch (cmdCode)
                {

                    case 23: (shift, stepComplete) = Rotate(GetDirRelative()); break;    // ПОВОРОТ относительно								2,               false
                    case 24: (shift, stepComplete) = Rotate(GetDirAbsolute()); break;    // ПОВОРОТ абсолютно								2,               false
                    case 25: (shift, stepComplete) = Photosynthesis(); break;               // ФОТОСИНТЕЗ                                       1,               true
                    case 26: (shift, stepComplete) = Step(GetDirRelative()); break;      // ДВИЖЕНИЕ шаг в относительном напралении			(int)refContent, true
                    case 27: (shift, stepComplete) = Step(GetDirAbsolute()); break;      // ДВИЖЕНИЕ шаг в абсолютном направлении			(int)refContent, true
                    case 28: (shift, stepComplete) = Eat(GetDirRelative()); break;       // СЪЕСТЬ в относительном напралении				(int)refContent, true
                    case 29: (shift, stepComplete) = Eat(GetDirAbsolute()); break;       // СЪЕСТЬ в абсолютном направлении					(int)refContent, true
                    case 30: (shift, stepComplete) = Look(GetDirRelative()); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                    case 31: (shift, stepComplete) = Look(GetDirAbsolute()); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 32: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 33: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 34: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 35: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 36: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 37: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 38: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 39: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 40: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 41: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 42: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 43: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 44: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 45: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 46: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 47: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 48: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 49: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 50: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 51: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 52: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 53: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 54: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 55: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                                                                                         //case 56: (shift, stepComplete) = LookAtRelativeDirection(); break;      // ПОСМОТРЕТЬ в относительном напралении			(int)refContent, false
                                                                                         //case 57: (shift, stepComplete) = LookAtAbsoluteDirection(); break;      // ПОСМОТРЕТЬ  в абсолютном напралении				(int)refContent, false
                    default: shift = cmdCode; stepComplete = false; break;
                };

                cntJump++;
                // Прибавляем к указателю текущей команды значение команды
                ShiftCodePointer(shift);
            }
            while (!stepComplete && cntJump < Data.MaxUncompleteJump);

            Age++;

            EnergyChange(Data.DeltaEnergyOnStep);

            Interlocked.Add(ref Data.TotalEnergy, Data.DeltaEnergyOnStep);


            //Func.CheckWorld2(Index, Num, Xi, Yi);
            //Func.CheckWorld2(Index, Num, Xi, Yi);

            //Reproduction
            if (CanReproduct())
            {
                ToReproductionList();
            }

            //Func.CheckWorld2(Index, Num, Xi, Yi);

            // 0-7		движение
            // 8-15		схватить еду или нейтрализовать яд
            // 16-23	посмотреть
            // 24-31	поворот
            // 32-63	безусловный переход
            // 25       фотосинтез

            //...............  сменить направление относительно   ....			if ($command == 23)
            //...............  сменить направление абсолютно   ....			if ($command == 24)
            //...............  фотосинтез ................			if ($command == 25)
            //...............  шаг  в относительном напралении  .................    			if ($command == 26)
            //...............  шаг   в абсолютном направлении     .................  			if ($command == 27)
            //..............   съесть в относительном напралении       ..............			if ($command == 28)
            //..............   съесть  в абсолютном направлении      ...............			if ($command == 29)
            //.............   посмотреть  в относительном напралении ...................................			if ($command == 30) 
            //.............   посмотреть в абсолютном направлении ...................................			if ($command == 31)// пусто - 2 стена - 3 органик - 4 бот -5 родня -  6
            ///////////////////////////////////////////
            // делиться - если у бота больше энергии или минералов, чем у соседа, то они распределяются поровну          //.............   делится   в относительном напралении  ........................			if (($command == 32) || ($command == 42))    // здесь я увеличил шансы появления этой команды                   
            //.............   делится  в абсолютном направлении ........................			if (($command == 33) || ($command == 51))     // здесь я увеличил шансы появления этой команды                    
            // отдать - безвозмездно отдать часть энергии и минералов соседу			//.............   отдать   в относительном напралении  ........................			if (($command == 34) || ($command == 50) )     // здесь я увеличил шансы появления этой команды                    
            //.............   отдать  в абсолютном направлении  ........................			if (($command == 35) || ($command == 52) )       // здесь я увеличил шансы появления этой команды                    
            //...................   выравнится по горизонтали  ...............................			if ($command == 36)
            //...................  какой мой уровень (на какой высоте бот)  .........			if ($command == 37)
            //...................  какое моё здоровье  ..............................			if ($command == 38)
            //...................сколько  минералов ...............................			if ($command == 39)
            //...........  многоклеточность ( создание потомка, приклееного к боту )......			if ($command == 40)  
            //...............  деление (создание свободноживущего потомка) ................			if ($command == 41)      
            //...............  окружен ли бот    ................			if ($command == 43)   
            //.............. приход энергии есть? ........................			if ($command == 44)  
            //............... минералы прибавляются? ............................			if ($command == 45)  
            //.............. многоклеточный ли я ? ........................ 			if ($command == 46)  
            //.................. преобразовать минералы в энерию ...................			if ($command == 47) 
            //................      мутировать   ................................... // спорная команда, во время её выполнения меняются случайным образом две случайные команды // читал, что микроорганизмы могут усилить вероятность мутации своего генома в неблагоприятных условиях       			if ($command == 48)
            //................   генная атака  ...................................			if ($command == 49)
        }


        public bool CanReproduct()
        {
            return Energy >= Data.ReproductionBotEnergy;
        }

        public void HoldReproduction() 
        {
            _reproductionCycle = 20;
        }

        private void ToReproductionList()
        {
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

        private (int shift, bool stepComplete) Photosynthesis()
        {
            if (Yi < Data.PhotosynthesisLayerHeight)
            {
                EnergyChange(Data.PhotosynthesisEnergy);
                Interlocked.Add(ref Data.TotalEnergy, Data.PhotosynthesisEnergy);
                Genom.Plant = true;
                //genom.Color = Color.Green;
                return (1, true);
            }
            else
            {
                return (1, false);
            }
        }

        private (int shift, bool stepComplete) Eat(int dir)
        {
            // Алгоритм:
            // 1. Узнаем координаты клетки на которую надо посмотреть
            var (nXi, nYi) = GetCoordinatesByDirectionOnlyDifferent(dir);

            // 2. Узнаем что находится на этой клетке
            if (nYi < 0 || nYi >= Data.WorldHeight || nXi < 0 || nXi >= Data.WorldWidth) return ((int)RefContent.Edge, true);

            long cont = -1;


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
                if (Data.DrawType == DrawType.OnlyChangedCells) Func.FixChangeCell(nXi, nYi, null);
                return ((int)RefContent.Grass, true);
            }


            cont = Data.World[nXi, nYi];

            // для расчета shift code
            var refContent = cont switch
            {
                0 => RefContent.Free,
                65500 => RefContent.Grass,
                65501 => RefContent.Organic,
                65502 => RefContent.Mineral,
                65503 => RefContent.Wall,
                65504 => RefContent.Poison,
                _ => cont >= 1 && cont <= Data.CurrentNumberOfBots
                    ? Genom.IsRelative(Data.Bots[cont].Genom)
                        ? RefContent.Relative
                        : RefContent.Bot
                    : throw new Exception("return cont switch")
            };

            // Bot || Relative
            if (refContent == RefContent.Bot || refContent == RefContent.Relative)
            {
                EatBot(cont);
            }

            return ((int)refContent, true);
        }


        private void EatBot(long cont)
        {
            var eatedBot = Data.Bots[cont];

            // Растение не может есть животное
            if (Genom.Plant && !eatedBot.Genom.Plant)
            {
                return;
            }

            // Животное может есть растение, но ни тогда когда его осталось мало
            if (!Genom.Plant && eatedBot.Genom.Plant)
            {
                if (eatedBot.Genom.CurBots < 2)
                {
                    return;
                }
            }

			// Не может есть родственника
			if (Genom.GenomHash == eatedBot.Genom.GenomHash)
			{
				return;
			}

			var gotEnergyByEating = eatedBot.EnergyChange(Data.BiteEnergy);
            //var olden = Energy;
            EnergyChange(gotEnergyByEating);

            if (gotEnergyByEating < 0) throw new Exception("dfgdfg");

            //eatedBot.Log.LogInfo($"bot was bited. energy:{eatedBot.Energy}");
            //Log.LogInfo($"bot{Index} bite bot{cont} and got {gotEnergyByEating} energy. From {olden} to {Energy}.");
        }


        private (int shift, bool stepComplete) Look(int dir)
        {
            // Алгоритм:
            // 1. Узнаем координаты клетки на которую надо посмотреть
            var (nXi, nYi) = GetCoordinatesByDirectionOnlyDifferent(dir);

            // 2. Узнаем что находится на этой клетке

            //смещение условного перехода 2-пусто  3-стена  4-органика 5-бот 6-родня

            // Если координаты попадают за экран то вернуть RefContent.Edge
            if (nYi < 0 || nYi >= Data.WorldHeight || nXi < 0 || nXi >= Data.WorldWidth) return ((int)RefContent.Edge, true);

            var cont = Data.World[nXi, nYi];
            var refContent = cont switch
            {
                0 => RefContent.Free,
                65500 => RefContent.Grass,
                65501 => RefContent.Organic,
                65502 => RefContent.Mineral,
                65503 => RefContent.Wall,
                65504 => RefContent.Poison,
                _ => cont >= 1 && cont <= Data.CurrentNumberOfBots
                    ? Genom.IsRelative(Data.Bots[cont].Genom)
                        ? RefContent.Relative
                        : RefContent.Bot
                    : throw new Exception("return cont switch")
            };

            return ((int)refContent, false);
        }


        private (int shift, bool stepComplete) Rotate(int dir)
        {
            Direction = dir;
            return (2, false);
        }

        //https://www.messletters.com/ru/big-text/ banner3
        //////////////////////////////////////////////////////////////////
        //			##     ##  #######  ##     ## ######## 
        //			###   ### ##     ## ##     ## ##       
        //			#### #### ##     ## ##     ## ##       
        //			## ### ## ##     ## ##     ## ######   
        //			##     ## ##     ##  ##   ##  ##       
        //			##     ## ##     ##   ## ##   ##       
        //			##     ##  #######     ###    ######## 
        //////////////////////////////////////////////////////////////////

        #region Move

        private (int shift, bool stepComplete) Step(int dir)
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
                return ((int)RefContent.Free, true);
            }


            // Если координаты попадают за экран то вернуть RefContent.Edge
            if (nYi < 0 || nYi >= Data.WorldHeight || nXi < 0 || nXi >= Data.WorldWidth) return ((int)RefContent.Edge, true);



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


                return ((int)RefContent.Free, true);
            }

            //Func.CheckWorld2(Index, Num, Xi, Yi);


            var cont = Data.World[nXi, nYi];
            var refContent = cont switch
            {
                0 => RefContent.Free,
                65500 => RefContent.Grass,
                65501 => RefContent.Organic,
                65502 => RefContent.Mineral,
                65503 => RefContent.Wall,
                65504 => RefContent.Poison,
                _ => cont >= 1 && cont <= Data.CurrentNumberOfBots
                    ? Genom.IsRelative(Data.Bots[cont].Genom)
                        ? RefContent.Relative
                        : RefContent.Bot
                    : throw new Exception("return cont switch")
            };

            //Func.CheckWorld2(Index, Num, Xi, Yi);
            return ((int)refContent, true);
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
        #endregion

        //////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////

        private void ShiftCodePointer(int shift)
        {
            OldPointer = Pointer;
            Pointer = (Pointer + shift) % Data.GenomLength;
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
        private int GetDirAbsolute()
        {
            return Dir.GetDirectionFromCodeAbsolute(Genom.GetNextCommand(Pointer));
        }
        private int GetDirRelative()
        {
            return Dir.GetDirectionFromCodeRelative(Direction, Genom.GetNextCommand(Pointer));
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
            sb.AppendLine($"Genom {Genom.PraNum} {(Genom.Num != 0 ? $"({Genom.Num})" : "")}Lev{Genom.Level}");
            sb.AppendLine($"Bots: {Genom.CurBots}");

            sb.AppendLine("");
            sb.AppendLine($"Color: R{Color.R} G{Color.G} B{Color.B}");
            sb.AppendLine($"Pra: {Genom.PraHash.ToString().Substring(0, 8)}");
            sb.AppendLine($"Hash: {Genom.GenomHash.ToString().Substring(0, 8)}");
            sb.AppendLine($"Parent: {Genom.ParentHash.ToString().Substring(0, 8)}");
            sb.AppendLine($"Grand: {Genom.GrandHash.ToString().Substring(0, 8)}");

            return sb.ToString();
        }

        public string GetText2(int delta)
        {
            var sb = new StringBuilder();

            //sb.AppendLine($"23r,24a - rotate; 26r,27a - step");
            //sb.AppendLine($"28r,29a - eat; 30r,31a - look");

            sb.AppendLine($"OldPointer: {OldPointer}");
            sb.AppendLine($"Pointer: {Pointer}");

            if (Hist.historyPointerY >= 0)
            {
                var (hist, histPtrCnt) = Hist.GetLastStepPtrs(delta);

                sb.AppendLine($"jumps cnt: {histPtrCnt - 1}");
                sb.AppendLine($"jumps: {string.Join(", ", hist.Take(histPtrCnt))}");

                for (var i = 0; i < histPtrCnt; i++)
                {
                    var cmdTxt = Genom.Code[hist[i]] switch
                    {
                        23 => "Поворот относительно",
                        24 => "Поворот абсолютно",
                        25 => "Фотосинтез",
                        26 => "Шаг относительно",  //шаг
                        27 => "Шаг абсолютно",
                        28 => "Есть относительно",    //съесть
                        29 => "Есть абсолютно",
                        30 => "Посмотерть относительно",  //посмотреть
                        31 => "Посмотерть абсолютно",
                        _ => ""
                    };

                    var dirStr = Dir.GetDirectionStringFromCode(Genom.GetNextCommand(hist[i]));
                    if (cmdTxt != "")
                    {
                        sb.AppendLine($"{cmdTxt} {dirStr}");
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
