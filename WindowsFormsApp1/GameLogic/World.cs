﻿using System;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using WindowsFormsApp1.Enums;
using static System.Windows.Forms.Design.AxImporter;

namespace WindowsFormsApp1.GameLogic
{
	public class World
	{
		private RandomService _randomService;
		private Seeder _seeder;
		private static WorldData _data;

		public World(WorldData data)
		{
			_data = data;
			_randomService = new RandomService(_data);
			_seeder = new Seeder(_data, _randomService);
		}

		public void Initialize()
		{
			// Засевание объектов
			_seeder.SeedItems();

			// Засевание ботов
			_seeder.SeedBots();
		}


		public void Step()
		{
			//Parallel.For(0, currentBotsNumber, i => Bots[i].Move());
			for (uint botNumber = 1; botNumber < _data.CurrentBotsNumber; botNumber++)
			{
				_data.Bots[botNumber].Step();
				//Bots[botNumber].Live();
				//Bots[botNumber].Move();
			}
		}
	}
}


/*
// ДВИЖЕНИЕ
// Алгоритм:
// 1. Суммируем направление бота и движения
// 2. По полученному суммарному направлению вычисляем дельта координаты клетки на которую предполагается передвинуться
// 3. Узнаем что находится на этой клетке
// 4.1. Переход на клетку если там empty poison
// 4.2. Не переход на клетку если там  wall edge
// 4.3. Непонятно переход на клетку если там  food mineral organic


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
 _healthPointChange = -PoisonDamage;

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

        // c >= 65500 зарезервированы
        // 0 - пусто
        // 1-65499 - bots
        // 65500 - food
        // 65501 - organic
        // 65502 - mineral
        // 65503 - wall
        // 65504 - poison


		// Поворот бота
		// 24 - 0
		// 25 - 45
		// 26 - 90
		// 27 - 135
		// 28 - 180
		// 29 - 225
		// 30 - 270
		// 31 - 315


//поменять направление на противоположное
//мина
//притворится мертвым
//=== радиация, которая действует на ботов в самом верхнем слое мира ====
//............  время года  .....................................
if $cyc < 15  // разрешенно не более 15 команд


//...............  сменить направление относительно   ....
			if ($command == 23)
//...............  сменить направление абсолютно   ....
			if ($command == 24)
//...............  фотосинтез ................
			if ($command == 25)
//...............  шаг  в относительном напралении  .................    
			if ($command == 26)
//...............  шаг   в абсолютном направлении     .................    
			if ($command == 27)
//..............   съесть в относительном напралении       ...............
			if ($command == 28)
//..............   съесть  в абсолютном направлении      ...............
			if ($command == 29)
//.............   посмотреть  в относительном напралении ...................................
			if ($command == 30) 
//.............   посмотреть в абсолютном направлении ...................................
			if ($command == 31)// пусто - 2 стена - 3 органик - 4 бот -5 родня -  6
// делиться - если у бота больше энергии или минералов, чем у соседа, то они распределяются поровну          
//.............   делится   в относительном напралении  ........................
			if (($command == 32) || ($command == 42))    // здесь я увеличил шансы появления этой команды                   
 //.............   делится  в абсолютном направлении ........................
			if (($command == 33) || ($command == 51))     // здесь я увеличил шансы появления этой команды                    
// отдать - безвозмездно отдать часть энергии и минералов соседу			
//.............   отдать   в относительном напралении  ........................
			if (($command == 34) || ($command == 50) )     // здесь я увеличил шансы появления этой команды                    
//.............   отдать  в абсолютном направлении  ........................
			if (($command == 35) || ($command == 52) )       // здесь я увеличил шансы появления этой команды                    
//...................   выравнится по горизонтали  ...............................
			if ($command == 36)
//...................  какой мой уровень (на какой высоте бот)  .........
			if ($command == 37)
//...................  какое моё здоровье  ...............................
			if ($command == 38)
//...................сколько  минералов ...............................
			if ($command == 39)
//...........  многоклеточность ( создание потомка, приклееного к боту )......
			if ($command == 40)  
//...............  деление (создание свободноживущего потомка) ................
			if ($command == 41)      
//...............  окружен ли бот    ................
			if ($command == 43)   
//.............. приход энергии есть? ........................
			if ($command == 44)  
//............... минералы прибавляются? ............................
			if ($command == 45)  
//.............. многоклеточный ли я ? ........................ 
			if ($command == 46)  
//.................. преобразовать минералы в энерию ...................
			if ($command == 47) 
//................      мутировать   ................................... 
// спорная команда, во время её выполнения меняются случайным образом две случайные команды 
// читал, что микроорганизмы могут усилить вероятность мутации своего генома в неблагоприятных условиях       
			if ($command == 48)
//................   генная атака  ...................................
			if ($command == 49)
//................    если ни с одной команд не совпало ................. 
//................    значит безусловный переход        .................
//.....   прибавляем к указателю текущей команды значение команды   .....
 */
