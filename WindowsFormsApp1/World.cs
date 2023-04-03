using System;
using System.Net.Mail;

namespace WindowsFormsApp1
{
    public class World
    {
        private uint[,] _world; // чтобы можно было узнать по координатам что там находится
        private int _worldWidth;
        private int _worldHeight;



        public Bot[] Bots;
        public int CurrentBotsNumber;
        Random _rnd = new Random(Guid.NewGuid().GetHashCode());

        public World(WorldOptions options)
        {
            // CREATE WORLD
            _worldHeight = options.WorldHeight;
            _worldWidth = options.WorldWidth;
            _world = new uint[_worldWidth, _worldHeight];
            GenerateWorldObjects();

            // Создание ботов
            Bots = new Bot[options.MaxBotsNumber];
            for (var botNumber = 0; botNumber < options.StartBotsNumber; botNumber++)
            {
                Bots[botNumber] = new Bot(_rnd, options.WorldWidth, options.WorldHeight);
            }
            CurrentBotsNumber = options.StartBotsNumber;

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

        public enum Direction
        {
            Up = 0,
            UpRight = 1,
            Right = 2,
            DownRight = 3,
            Down = 4,
            DownLeft = 5,
            Left = 6,
            UpLeft = 7
        }



        private void TryMove(Bot b, Direction dir)
        {
            var (dX, dy) = GetDeltaDirection(b.Dir, dir);
            var nX = b.X + dX;
            var nY = b.Y + dy;
            
            if (nX < 0) nX += _worldWidth;
            if (nX >= _worldWidth) nX -= _worldWidth;

            var cont = DefineCellContent(nX, nY);
        }

        //===========================================
        private GetContent DefineCellContent(int x, int y)
        {
            if (y < 0) return GetContent.Edge;
            if (y >= _worldHeight) return GetContent.Edge;

            var c = _world[x,y];

            // c >= 65500 зарезервированы
            // 0 - пусто
            // 1-65499 - bots
            // 65500 - food
            // 65501 - wall
            // 65502 - organic
            // 65503 - mineral
            // 65504 - poison

            return GetContent.Wall;
        }

        public enum ContentObject
        {
            Empty = 0,
            Food = 65500,
            Organic = 65501,
            Mineral = 65503,
            Wall = 65504
        }

        public enum GetContent
        {
            Empty = 0,
            Food = 1,
            Organic = 2,
            Mineral = 3,
            Wall = 4,
            Relative = 5,
            Alien = 6,
            Poison = 7,
            Edge = 8
        }

        private void GenerateWorldObjects()
        {
            // накидать food wall organic? poison
            // Food
            for (var i =) {
            }
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
                    TryMove(b, Direction.Up);
                    break;

                case 1: //Движение вперед-вправо
                    TryMove(b, Direction.UpRight);
                    break;



                case 25:
                    _pointer++;
                    break;

                default:
                    new Exception("switch cmd");
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
    }
}
