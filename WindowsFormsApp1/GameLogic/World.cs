﻿using System;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.Static;
using static System.Windows.Forms.Design.AxImporter;

namespace WindowsFormsApp1.GameLogic
{
    public class World
    {
        private readonly Seeder _seeder;

        public World()
        {
            _seeder = new Seeder();
        }

        public void Initialize()
        {
            // Засевание объектов
            _seeder.SeedItems();

            // Засевание ботов
            _seeder.SeedBots();


            Data.SeedTotalEnergy = Data.TotalEnergy;
        }

        static long cnt51 = 0;
        static long cnt52 = 0;

        public void Step()
        {
            //Func.CheckWorld3();
            //Func.CheckWorld2();
            //var bc1 = Data.CurrentNumberOfBots;
            

            Data.QtyFactBotDeath = 0;
            Data.QtyAllBotDeathMinusOne = -1;
            Data.IndexOfLastBotReproduction = -1;

            if (Data.Checks) Func.CHECK1();

            Parallel.For(1, Data.CurrentNumberOfBots, (i, state) =>
            {
                Data.Bots[i].Step();
            });

            //for (var i =1; i<=Data.CurrentNumberOfBots; i++)
            //{
            //    Data.Bots[i].Step();
            //}



            Test.NextInterval(10, "BOTS ACTIONS CYCLE");

            //Func.CheckWorld3();
            //Func.CheckWorld2();

            if (Data.Checks) Func.CHECK2();


            //Data.Wlog.ClearLog();
            //var (totalEnergy, dctEnergy) = Func.GetAllBotsEnergy();
            // ============ REPRODUCTION ===================================================================

            //var forlog1 = Data.IndexOfLastBotReproduction + 1;      // В массиве Data.BotReproduction последний индекс бота который хочет размножиться
            //var forlog2 = Data.QtyAllBotDeathMinusOne + 1;          // Количество собирающихся и собиравшихся умереть ботов
            //var forlog3 = Data.QtyFactBotDeath;                     // Фактическое количество умирающих ботов
            //var forlog4 = Data.TotalQtyBotReproduction;
            //var forlog5 = Data.TotalQtyBotDeath;

            Data.IndexOfLastBotDeathArrayUsedForReproduction = -1;
            Data.QtyFactBotDeathUsedForReproduction = 0;
            Data.Check_QtyFailedReproduction = 0;                       // Количество неуспешных размножений (изза нехватки энергии или места)
            if (Data.IndexOfLastBotReproduction > -1)
            {
                //SearchDouble();
                Parallel.For(0, (int)Data.IndexOfLastBotReproduction + 1, Func.ReproductionBot);
                //SearchDouble();
            }
            Data.TotalQtyBotDeath += Data.QtyFactBotDeathUsedForReproduction;

            //Data.Wlog.LogInfo("");
            //Data.Wlog.LogInfo("REPRODUCTION");
            //Data.Wlog.LogInfo($"Data.ArraySizeOfBotReproduction: {forlog1} - Failed:{Data.Check_QtyFailedReproduction} = {forlog1 - Data.Check_QtyFailedReproduction}");
            //Data.Wlog.LogInfo($"Data.LastIndexOfBotDeathArrayUsedForReproduction: {Data.IndexOfLastBotDeathArrayUsedForReproduction}");
            //Data.Wlog.LogInfo($"Data.ReproductionCnt: {forlog4} > {Data.TotalQtyBotReproduction}   {Data.TotalQtyBotReproduction - forlog4}");

            //Data.Wlog.LogInfo("");
            //Data.Wlog.LogInfo("DEATH AT REPRODUCTION");
            //Data.Wlog.LogInfo($"Data.ArraySizeOfBotDeath: {forlog2}");
            //Data.Wlog.LogInfo($"Data.NumberOfBotDeathFactCnt: {forlog3}");
            //Data.Wlog.LogInfo($"Data.DeathCnt: {forlog5} > {Data.TotalQtyBotDeath}   {Data.TotalQtyBotDeath - forlog5}");

            Test.NextInterval(12, "reproduction");
            // =============================================================================================
            //Func.CheckBotsEnergy(dctEnergy, totalEnergy);

            if (Data.Checks) Func.CHECK3();


            // ============ DEATH ==========================================================================
            long cont;
            //Data.Wlog.LogInfo("");
            //Data.Wlog.LogInfo("DEATH");
            //Data.Wlog.LogInfo($"Data.LastIndexOfBotDeathArrayUsedForReproduction: {Data.IndexOfLastBotDeathArrayUsedForReproduction}");
            //Data.Wlog.LogInfo($"Data.QtyAllBotDeathMinusOne: {Data.QtyAllBotDeathMinusOne}");
            //Data.Wlog.LogInfo($"Data.QtyFactBotDeathUsedForReproduction: {Data.QtyFactBotDeathUsedForReproduction}");
            //Data.Wlog.LogInfo($"Data.QtyRemovedBotsOnStep: {Data.QtyRemovedBotsOnStep}");
            //Data.Wlog.LogInfo($"Data.QtyFactBotDeath: {Data.QtyFactBotDeath}");

            if (Data.IndexOfLastBotDeathArrayUsedForReproduction < Data.QtyAllBotDeathMinusOne) // еще есть в запасе умирающие боты
            {
                //Data.Wlog.LogInfo($"Data.CurrentNumberOfBots: {Data.CurrentNumberOfBots}");
                //Data.Wlog.LogInfo($"Data.LastIndexOfBotDeathArrayUsedForReproduction: {Data.IndexOfLastBotDeathArrayUsedForReproduction}");
                //Data.Wlog.LogInfo("Data.CurrentNumberOfBots - Data.NumberOfBotDeathFactCnt + Data.NumberOfBotDeathFactUsedForReproductionCnt: " +
                //                  $"{Data.CurrentNumberOfBots - Data.QtyFactBotDeath + Data.QtyFactBotDeathUsedForReproduction}");

                Data.IndexEnclusiveBeforeReplacesBots = Data.CurrentNumberOfBots - Data.QtyFactBotDeath + Data.QtyFactBotDeathUsedForReproduction;
                Data.QtyRemovedBotsOnStep = 0;
                Data.IndexOfLastBotPlusOne = Data.CurrentNumberOfBots + 1;
                // МОГУТ МЕНЯТЬСЯ ИНДЕКСЫ БОТОВ ЗДЕСЬ !!!!!!!!!!!!!!!!!
                Parallel.For((int)Data.IndexOfLastBotDeathArrayUsedForReproduction + 1, (int)Data.QtyAllBotDeathMinusOne + 1, Func.DeathBot);
                Data.CurrentNumberOfBots -= Data.QtyRemovedBotsOnStep; 
                Data.TotalQtyBotDeath += Data.QtyRemovedBotsOnStep;
            }
            Test.NextInterval(11, "death");
            // =============================================================================================

            if (Data.Checks) Func.CHECK4();

            //         int cnt3 = 0;
            //         for (long botNumber = 1; botNumber <= Data.CurrentNumberOfBots; botNumber++)
            //         {
            //             if (Data.Bots[botNumber].InsertedToDeathList)
            //             {
            //		for (var i = 1; i < Data.MaxBotsNumber; i++)
            //                 {
            //                     if (Data.Bots[i] == null)
            //                     {
            //                         goto frg;
            //                     }
            //                     cnt3 += Data.Bots[i].InsertedToDeathList ? 1 : 0;
            //                 }
            //		//var cnt2 = Data.Bots.Take(100).Count(b => b.InsertedToDeathList);
            //	}
            //}
            //frg:
            //Func.CheckWorld3();
            //int cnt5 = 0;
            //for (long botNumber = 1; botNumber <= Data.CurrentNumberOfBots; botNumber++)
            //{
            //    if (Data.Bots[botNumber].InsertedToReproductionList)
            //    {
            //        cnt5++;
            //    }
            //}
            //if (cnt5 != 0) throw new Exception("dfgdf");
            //Func.CheckWorld3();
            //Func.CheckWorld2();
            //for (long botNumber = 1; botNumber <= Data.CurrentNumberOfBots; botNumber++)
            //{
            //	Data.Bots[botNumber].Step();
            //	//Bots[botNumber].Live();
            //	//Bots[botNumber].Move();
            //}

            Data.CurrentStep++;
            while (Data.SeedFood && Data.TotalEnergy < Data.SeedTotalEnergy)
            {
                if (Func.TryGetRandomFreeCell(out var x, out var y))
                {
                    Data.World[x, y] = (long)CellContent.Grass;
                    Func.FixChangeCell(x, y, Color.Green);

                    Data.TotalEnergy += Data.FoodEnergy;
                }
                else
                {
                    break;
                }
            }
            //Func.CheckWorld3();

            var cnt1 = 0;
            var cnt2 = 0;
            cnt51 = cnt52;
            cnt52 = Data.CurrentNumberOfBots;
            var cnt3 = Data.QtyFactBotDeath;
            var cnt4 = Data.TotalQtyBotReproduction;
            var cnt5 = Data.Check_QtyFailedReproduction;
            for (var i = 1; i < Data.NumberOfChangedCells; i++)
            {
                var obj = Data.ChangedCells[i];

                if (obj.Color != null)
                {
                    var qwe = Data.World[obj.X, obj.Y];
                    if (qwe == 0)
                    {
                    }
                    cnt1++;
                }
                else
                {
                    cnt2++;
                }


                Data.ChWorld[obj.X, obj.Y] = 0;
            }

        }
    }
}


/*
// Возможные ошибки: может ли бот одновременно быть вставлен в оба списка? 

// Включен лог



мутации 5>1 байт; мутации .1>10 процентов; food tru>false;  SeedBotEnergy 1000>50000; случайное направление; добавлен PhotosynthesisLayerHeight
не уменьшать энергию


 ИДЕИ
// 4.3. Непонятно переход на клетку если там  food mineral organic
Стоит ограничение на здоровье и бот не может иметь более 90 единиц здоровья 
если бот зайдет на клетку с ядом то он погибнет
Если вначале схватить яд то он преобразуется в еду
Боты отличают чужого от своего, если код-геном отличается более, чем на один байт. 
1. из 64 осталось 8 ботов. каждому добавляем 7 копий в любом месте, одна из которых мутант.
2. бот как набрал достаточно энергии создает свою копию с каким-то процентом мутации.
Когда отпочковывается новый бот, он встраивается в цепочку ботов перед предком.
боты переполнены энергией и они должны отпочкавать потомка, но свободного места нет и они погибают
В стандартном режиме цвет зависит от способа получения энергии. Любители фотосинтеза зеленеют, любители «минералов» синеют, а мясоеды краснеют.
У всеядных может быть промежуточный цвет.
В режиме отображения энергии, чем больше энергии, тем бот краснее, чем меньше энергии, тем бот желтее.
todo размножение делать не на каждом шаге а раз в 10 шагов, а то облепленные другими боты на каждом шаге не могут размножиться
todo если геном больше не используется (Bots=0) то удалять геном чтоб память не забивал
todo при поедании если релатив возвращать что просто бот как у foo52 ?
todo добавить обработку если наступил на яд
todo в FixChangeCell не перерисовывать если на первоначальном экране ячейка такого же цвета
todo в FixChangeCell перевести на ConcurrentDictionary AddOrUpdate
todo при удалении и создании ботов не пересоздавать объект а переиспользовать его
фотосинтез только в верхгних слоях
мина
default: shift = cmdCode; stepComplete = false; break; если cmdCode =0 то что будет? зависнет?
первых делать сразу с 5000 чтоб они наделились и потом были в равных правах и самые сильные остались
смерть бота: после смерти должна сохраниться общая масса все оргпнизмов или общая энергия
есть команды а есть события. событие это когда чтото происходит и начинает выполняться код этого события или 
мутировать может не один байт а несколько или даже все
кнопка посчитать суммарную энергию
фотосинтез должен давать мало энергии тогда потребуется много команд фотосинтеза в коде и тем
самым меньше будут появляться хищники с фотосинтезом
радиация, которая действует на ботов в самом верхнем слое мира ====
боты притягиваютс к большой массе
по запаху двигается
наступает только на ту клетку где вокруг нет хищников
удачно мутировавшие гены чтоб больше не мутировали или с меньшей вероятностью?

ЕСЛИ РЯДОМ НЕТ СВОБОДНОЙ ЯЧЕЙКИ
// - просто накапливать энергию дальше
// - передать энергию соседу
// - в случайном месте создать нового бота
// - в ближайшем пустом месте создать нового бота
// - в ближайшем пустом месте рядом с родственником (найти по цепочке родственников бота с пустыми соседними ячейками)
// - умереть
// - взорваться
// - создавать бота на Organic, Mineral, Poison(?), Grass
// - сделать чтобы если нет места для появления бота на каждом шаге не была очередная попытка воспроизводства
// - если готов поделиться но вокруг все клетки заняты, то сам умирает а на его месте появляется потомок


КОМАНДЫ ДЕЙСТВИЙ
сменить направление относительно   ....
сменить направление абсолютно   ....
фотосинтез ................
шаг  в относительном напралении  .................    
шаг   в абсолютном направлении     .................    
съесть в относительном напралении       ...............
съесть  в абсолютном направлении      ...............
посмотреть  в относительном напралении ...................................
посмотреть в абсолютном направлении ...................................
делится   в относительном напралении  - если у бота больше энергии или минералов, чем у соседа, то они распределяются поровну 
делится  в абсолютном направлении ........................
отдать - безвозмездно отдать часть энергии и минералов соседу в относительном напралении  ........................
отдать - безвозмездно отдать часть энергии и минералов соседу в абсолютном направлении  ........................
поделиться энергией
выравнится по горизонтали  ...............................
многоклеточность ( создание потомка, приклееного к боту )......
деление (создание свободноживущего потомка) ................
преобразовать минералы в энерию ...................
мутировать   ................................... 
спорная команда, во время её выполнения меняются случайным образом две случайные команды 
читал, что микроорганизмы могут усилить вероятность мутации своего генома в неблагоприятных условиях       
генная атака  ...................................
команда периодической смены направления при определенной вероятности направление менятеся случайно или нет
поменять направление на противоположное
бот может взорваться при определенных условиях
притворится мертвым или стеной
случайное направление , допустим цифра 8 (0-7 это обычные направления)
делиться - если у бота больше энергии или минералов, чем у соседа, то они распределяются поровну          
команда перехода на случайное количество шагов в программе
притянуть бота
притянуться к боту
дальнее зрение

УСЛОВНЫЕ КОМАНДЫ
сколько энергии?
условие от возраста
условие от кол-ва окружающих
условие от высоты
какой мой уровень?
какое моё здоровье
сколько  минералов
окружен ли бот    ................
приход энергии есть? ........................
минералы прибавляются? ............................
многоклеточный ли я ? ........................ 
много ли осталось пищи таокго типа - может пока не есть пусть размножится
далеко ли я от центра племени (для появления роев)

КОМАНДЫ РЕАКЦИИ
объединяться при нападении вокруг альфы
возмодны события укус с различной стороны, также таймер или достижение какого то возраста или ...




----------------------------


//Координаты бота могут быть от 0 до _data.WorldWidth-1 включительно и по y также
//допустим WorldWidth=500 и cellwidth =2 тогда крайнее левопе положение бота 0,1 , а правое положение бота 998,999
//Координаты LensX могут быть от 0 до _data.WorldWidth - _data.LensWidth включительно и по y также
//Допустим размер линзы 20  (cellwidth =2) и она прижата в левый верхний угол
//тогда левая и верхняя  граница будут идти по 0(реальный)
//а правая и нижняя по 39 



// ДВИЖЕНИЕ
// Алгоритм:
// 1. Суммируем направление бота и движения
// 2. По полученному суммарному направлению вычисляем дельта координаты клетки на которую предполагается передвинуться
// 3. Узнаем что находится на этой клетке
// 4.1. Переход на клетку если там free poison
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
