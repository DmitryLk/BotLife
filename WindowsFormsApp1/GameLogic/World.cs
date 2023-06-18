using System;
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

            if (Data.Parallel)
            {
                Parallel.For(1, Data.CurrentNumberOfBots, i => { Data.Bots[i].Step(); });
            }
            else
            {
                for (var i = 1; i < Data.CurrentNumberOfBots; i++)
                {
                    Data.Bots[i].Step();
                }
            }


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

                if (Data.Parallel)
                {
                    Parallel.For(0, (int)Data.IndexOfLastBotReproduction + 1, Func.ReproductionBot);
                }
                else
                {
                    for (var i = 0; i < (int)Data.IndexOfLastBotReproduction + 1; i++)
                    {
                        Func.ReproductionBot(i);
                    }
                }

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
                if (Data.Parallel)
                {
                    Parallel.For((int)Data.IndexOfLastBotDeathArrayUsedForReproduction + 1, (int)Data.QtyAllBotDeathMinusOne + 1, Func.DeathBot);
                }
                else
                {
                    for (var i = (int)Data.IndexOfLastBotDeathArrayUsedForReproduction + 1; i < (int)Data.QtyAllBotDeathMinusOne + 1; i++)
                    {
                        Func.DeathBot(i);
                    }
                }

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

            //while (Data.SeedFood && Data.TotalEnergy < Data.SeedTotalEnergy)
            //{
            //    if (Func.TryGetRandomFreeCell(out var x, out var y))
            //    {
            //        Data.World[x, y] = (long)CellContent.Grass;
            //        Func.FixChangeCell(x, y,Color.Green);

            //        Data.TotalEnergy += Data.FoodEnergy;
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}


            //Func.CheckWorld3();

            //var cnt1 = 0;
            //var cnt2 = 0;
            //cnt51 = cnt52;
            //cnt52 = Data.CurrentNumberOfBots;
            //var cnt3 = Data.QtyFactBotDeath;
            //var cnt4 = Data.TotalQtyBotReproduction;
            //var cnt5 = Data.Check_QtyFailedReproduction;
            //for (var i = 1; i < Data.NumberOfChangedCells; i++)
            //{
            //    var obj = Data.ChangedCells[i];

            //    if (obj.Color != null)
            //    {
            //        var qwe = Data.World[obj.X, obj.Y];
            //        if (qwe == 0)
            //        {
            //        }
            //        cnt1++;
            //    }
            //    else
            //    {
            //        cnt2++;
            //    }
            //}
        }
    }
}
