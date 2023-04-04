using System;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using WindowsFormsApp1.Enums;

namespace WindowsFormsApp1.GameLogic
{
    public class World
    {
        private const int MinBotIndex = 1;
        private const int MaxBotIndex = 65499;

        private uint[,] _world; // чтобы можно было узнать по координатам что там находится
        private int _worldWidth;
        private int _worldHeight;

        private RandomService _randomService;
        private GameOptions _options;

        public Bot[] Bots;
        public Point[] Foods;
        public Point[] Organic;
        public Point[] Minerals;
        public Point[] Walls;
        public Point[] Poison;

        public int CurrentBotsNumber;

        public World(GameOptions options)
        {
            _worldHeight = options.WorldHeight;
            _worldWidth = options.WorldWidth;
            _world = new uint[_worldWidth, _worldHeight];

            _randomService = new RandomService(options);
            _options = options;

            GenerateWorld();
        }

        private void GenerateWorld()
        {
            // Засевание объектов
            SeedItems();

            // Засевание ботов
            SeedBots();
        }

        private void SeedBots()
        {
            Bots = new Bot[_options.MaxBotsNumber];
            for (var botNumber = 0; botNumber < _options.StartBotsNumber; botNumber++)
            {
                var bot = Bots[botNumber] = new Bot(_options.WorldWidth, _options.WorldHeight);

                // Координаты бота
                bot.P = GetRandomEmptyPoint();
                bot.Old = new Point(bot.P.X, bot.P.Y);

                // Направление бота
                bot.Dir = _randomService.GetRandomDirection();

                // Код бота
                bot.Code = new byte[_options.CodeLength];
                for (var i = 0; i < _options.CodeLength; i++)
                {
                    bot.Code[i] = _randomService.GetRandomBotCode();
                }
                bot.Pointer = 0;

                // Скорость бота (?)
                (bot.Vx, bot.Vy) = _randomService.GetRandomSpeed();
            }
            CurrentBotsNumber = _options.StartBotsNumber;
        }


        private void SeedItems()
        {
            // Заполнение Food
            if (_options.SeedFood)
            {
                Foods = new Point[_options.SeedFoodNumber];
                for (var i = 0; i < _options.SeedFoodNumber; i++)
                {
                    Foods[i] = GetRandomEmptyPoint();
                }
            }

            // Заполнение Organic
            if (_options.SeedOrganic)
            {
                Organic = new Point[_options.SeedOrganicNumber];
                for (var i = 0; i < _options.SeedOrganicNumber; i++)
                {
                    Organic[i] = GetRandomEmptyPoint();
                }
            }

            // Заполнение Minerals
            if (_options.SeedMinerals)
            {
                Minerals = new Point[_options.SeedMineralsNumber];
                for (var i = 0; i < _options.SeedMineralsNumber; i++)
                {
                    Minerals[i] = GetRandomEmptyPoint();
                }
            }

            // Заполнение Walls
            if (_options.SeedWalls)
            {
                Walls = new Point[_options.SeedWallsNumber];
                for (var i = 0; i < _options.SeedWallsNumber; i++)
                {
                    Walls[i] = GetRandomEmptyPoint();
                }
            }

            // Заполнение Poison
            if (_options.SeedPoison)
            {
                Poison = new Point[_options.SeedPoisonNumber];
                for (var i = 0; i < _options.SeedPoisonNumber; i++)
                {
                    Poison[i] = GetRandomEmptyPoint();
                }
            }
        }

        public void Step()
        {
            //Parallel.For(0, currentBotsNumber, i => Bots[i].Move());

            for (var botNumber = 0; botNumber < CurrentBotsNumber; botNumber++)
            {
                BotAction(Bots[botNumber]);
                //Bots[botNumber].Live();
                //Bots[botNumber].Move();
            }
        }




        private void TryToMove(Bot b, Direction dir)
        {
            var (dX, dy) = GetDeltaDirection(b.Dir, dir);
            var nX = b.P.X + dX;
            var nY = b.P.Y + dy;

            if (nX < 0) nX += _worldWidth;
            if (nX >= _worldWidth) nX -= _worldWidth;

            var cont = GetCellContent(nX, nY);
        }

        //===========================================
        private bool IsCellEmpty(int x, int y)
        {
            return _world[x, y] == 0;
        }

        private GetContent GetCellContent(int x, int y)
        {
            if (y < 0) return GetContent.Edge;
            if (y >= _worldHeight) return GetContent.Edge;

            var c = _world[x, y];

            return с switch
            {
                0 => GetContent.Empty,
                > 1 && <= MaxBotIndex => GetContent.B
            };


            // c >= 65500 зарезервированы
            // 0 - пусто
            // 1-65499 - bots
            // 65500 - food
            // 65501 - organic
            // 65502 - mineral
            // 65503 - wall
            // 65504 - poison

            return GetContent.Wall;
        }



        //==============================================
        private void BotAction(Bot b)
        {
            // Получаем команду
            var cmdCode = b.Code[b.Pointer];

            // Выполняем команду
            switch (cmdCode)
            {
                //Up = 0,
                //UpRight = 1,
                //Right = 2,
                //DownRight = 3,
                //Down = 4,
                //DownLeft = 5,
                //Left = 6,
                //UpLeft = 7


                case 0: //Движение вперед
					TryToMove(b, Direction.Up);
                    break;

                case 1: //Движение вперед-вправо
					TryToMove(b, Direction.UpRight);
                    break;



                case 25:
                    b.Pointer++;
                    break;

                default:
                    throw new Exception("switch cmd");
                    break;
            };

        }

        private (int dX, int dY) GetDeltaDirection(Direction dir1, Direction dir2)
        {
            return (((int)dir1 + (int)dir2) % 8) switch
            {
                0 => (0, -1),
                1 => (1, -1),
                2 => (1, 0),
                3 => (1, 1),
                4 => (0, 1),
                5 => (-1, 1),
                6 => (-1, 0),
                7 => (-1, -1),
                _ => throw new Exception("return (((int)dir1 + (int)dir2) % 8) switch"),
            };
        }

        private Point GetRandomEmptyPoint()
        {
            var p = new Point();
            var i = 0;
            do
            {
				p.X = _randomService.GetRandomWorldX();
                p.Y = _randomService.GetRandomWorldY();
            }
            while (!IsCellEmpty(p.X, p.Y) && ++i < 100);

            return p;
        }

    }
}


/*
ДВИЖЕНИЕ
Алгоритм:
1. Суммируем направление бота и движения
2. По полученному суммарному направлению вычисляем дельта координаты клетки на которую предполагается передвинуться
3. Узнаем что находится на этой клетке

ВОЗРАСТ = ЗДОРОВЬЕ = ЭНЕРГИЯ
У бота есть здоровье и каждый ход оно уменьшается на единицу.
Если дойдет до 0 то бот умирает.
Стоит ограничение на здоровье и бот не может иметь более 90 единиц здоровья 

ПИТАНИЕ
Боту нужно найти еду которая добавляет 10 к здоровью.
Еду можно съесть зайдя на клетку с едой или схватив еду из соседней клетки

СТЕНА
бот упирается в стену

ЯД
если бот зайдет на клетку с ядом то он погибнет
Если вначале схватить яд то он преобразуется в еду

БОТ
типа стена
Боты отличают чужого от своего, если код-геном отличается более, чем на один байт. 

РАЗМНОЖЕНИЕ
1. из 64 осталось 8 ботов. каждому добавляем 7 копий в любом месте, одна из которых мутант.
2. бот как набрал достаточно энергии создает свою копию с каким-то процентом мутации.
Когда отпочковывается новый бот, он встраивается в цепочку ботов перед предком.
боты переполнены энергией и они должны отпочкавать потомка, но свободного места нет и они погибают

В стандартном режиме цвет зависит от способа получения энергии. Любители фотосинтеза зеленеют, любители «минералов» синеют, а мясоеды краснеют.
У всеядных может быть промежуточный цвет.

В режиме отображения энергии, чем больше энергии, тем бот краснее, чем меньше энергии, тем бот желтее.


		// КОМАНДЫ 0-63									Завершающая	Указатель
		// 0-7		сделать шаг							З			*	Направление зависит от числа и куда повернут бот в текущий момент.
		// 8-15		схватить еду или нейтрализовать яд	З			*
		// 16-23	посмотреть							Н			*	Бот остается на месте
		// 24-31	поворот								Н			1
		// 32-63	безусловный переход					Н

		// 25 - Фотосинтез		1						З

		поделиться энергией
		сколько энергии?

		// Незавершающая команда может выполняться до 10 раз после чего управлдение будет передано другому боту
		// Неизвестная команда является безусловным переходом



		// (*)Указатель команды перемещается в зависимости от того что было в этом направлении
		// яд - 1
		// стена - 2
		// бот - 3
		// еда - 4
		// пусто - 5

		// пусто - 2
		// стена - 3
		// органика - 4
		// бот - 5
		// свой - 6
		// чужой - 7


		// Поворот бота
		// 24 - 0
		// 25 - 45
		// 26 - 90
		// 27 - 135
		// 28 - 180
		// 29 - 225
		// 30 - 270
		// 31 - 315


 
 
 */
