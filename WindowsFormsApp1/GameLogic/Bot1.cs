using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using WindowsFormsApp1.Enums;

namespace WindowsFormsApp1.GameLogic
{
	// Бот с океана foo52
	public class Bot1 : Bot
	{
		private byte[] _code;
		private int _pointer;
		public Guid BotCodeHash;
		public Guid ParentCodeHash;
		public Guid GrandParentCodeHash;


		public Bot1(WorldData data, Point p, Direction dir, uint botNumber, int vx, int vy, byte[] code, int pointer, Guid hashCode)
			: base(data, p, dir, botNumber, vx, vy)
		{
			_code = code;
			_pointer = pointer;
			BotCodeHash = hashCode;
		}


		public override void Step()
		{
			Direction dir;
			int shift;
			bool stepComplete = false;

			// Получаем команду
			var cmdCode = GetCurrentCommand();

			// Выполняем команду
			switch (cmdCode)
			{
				//.....   прибавляем к указателю текущей команды значение команды   .....

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

				//Up = 0,
				//UpRight = 1,
				//Right = 2,
				//DownRight = 3,
				//Down = 4,
				//DownLeft = 5,
				//Left = 6,
				//UpLeft = 7


				case 23: //сменить направление относительно
					break;

				case 24: //сменить направление абсолютно
					break;

				case 25:
					ShiftCodePointer(1);
					break;

				case 26: //шаг в относительном напралении
					dir = (Direction)(((int)GetNextCode() + (int)_dir) % 8);
					shift = TryToMove(dir);
					ShiftCodePointer(shift);
					stepComplete = true;
					break;
				
				case 27: //шаг   в абсолютном направлении
					dir = (Direction)(((int)GetNextCode()) % 8);
					shift = TryToMove(dir);
					ShiftCodePointer(shift);
					stepComplete = true;
					break;



				default:
					throw new Exception("switch cmd");
					break;
			};

		}


		private int TryToMove(Direction dir)
		{
			// ДВИЖЕНИЕ
			// Алгоритм:

			// 1. Суммируем направление бота и движения
			// 2. По полученному суммарному направлению вычисляем дельта координаты клетки на которую предполагается передвинуться
			var (dX, dy) = _data.GetDelta(dir);
			var nX = _p.X + dX;
			var nY = _p.Y + dy;

			// Переход сквозь экран
			if (!_data.LeftRightEdge)
			{
				if (nX < 0) nX += _data.WorldWidth;
				if (nX >= _data.WorldWidth) nX -= _data.WorldWidth;
			}
			if (!_data.UpDownEdge)
			{
				if (nY < 0) nY += _data.WorldHeight;
				if (nY >= _data.WorldHeight) nY -= _data.WorldHeight;
			}


			// 3. Узнаем что находится на этой клетке
			var refContent = GetRefContent(nX, nY);

			// 4.1. Переход на клетку если там empty poison 
			// 4.2. Не переход на клетку если там  wall edge food mineral organic boot 
			if (refContent == RefContent.Empty || refContent == RefContent.Poison)
			{
				_data.World[_p.X, _p.Y] = (uint)RefContent.Empty;
				_data.World[nX, nY] = _num;

				_old.X = _p.X;
				_old.Y = _p.Y;
				_p.X = nX;
				_p.Y = nY;
			}

			return (int)refContent;
		}

		private RefContent GetRefContent(int x, int y)
		{
			//смещение условного перехода 2-пусто  3-стена  4-органика 5-бот 6-родня
			if (y < 0 || y >= _data.WorldHeight || x < 0 || x >= _data.WorldWidth) return RefContent.Edge;

			var cont = _data.World[x, y];

			return cont switch
			{
				0 => RefContent.Empty,
				65500 => RefContent.Grass,
				65501 => RefContent.Organic,
				65502 => RefContent.Mineral,
				65503 => RefContent.Wall,
				65504 => RefContent.Poison,
				_ => CodeShiftRelative(cont)
			};
		}

		private RefContent CodeShiftRelative(uint cont)
		{
			if (cont > 0 && cont <= _data.MaxBotsNumber)
			{
				// надо определить родственник ли бот
				if (IsRelative((Bot1)_data.Bots[cont]))
				{
					return RefContent.Relative;
				}
				else
				{
					return RefContent.Bot;
				}
			}
			else 
			{
				throw new Exception("if (cont > 0 && cont <= _data.MaxBotsNumber)");
			}
		}

		private bool IsRelative(Bot1 b2)
		{
			if (BotCodeHash == b2.BotCodeHash || BotCodeHash == b2.ParentCodeHash || BotCodeHash == b2.GrandParentCodeHash) return true;
			if (ParentCodeHash == b2.BotCodeHash || ParentCodeHash == b2.ParentCodeHash || ParentCodeHash == b2.GrandParentCodeHash) return true;
			if (GrandParentCodeHash == b2.BotCodeHash || GrandParentCodeHash == b2.ParentCodeHash || GrandParentCodeHash == b2.GrandParentCodeHash) return true;
			return false;
		}

		public byte GetCurrentCommand()
		{
			return _code[_pointer];
		}
		private byte GetNextCode()
		{
			return _code[_pointer + 1 >= _data.CodeLength ? 0 : _pointer + 1];
		}
		private void ShiftCodePointer(int shift)
		{
			_pointer = (_pointer + shift) % _data.CodeLength;
		}
	}
}

