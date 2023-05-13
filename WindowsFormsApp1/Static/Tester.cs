using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1.Static
{
    public class Interval
    {
        public string Name;
        public long TotalMeasurement;
        public long LastMeasurement;
        public int NumberOfMeasurement;
    }

    public static class Test
    {
        //private const int numberOfIntervals = 10;

        private static readonly Dictionary<int, Interval> Ins = new Dictionary<int, Interval>();
        private static readonly long _frequency = Stopwatch.Frequency;

        public static long CurrentMeasurementTimestamp;
        public static long LastMeasurementTimestamp = 0;

        public static void NextInterval(int num, string name)
        {
            CurrentMeasurementTimestamp = Stopwatch.GetTimestamp();
            if (!Ins.ContainsKey(num))
            {
                Ins.Add(num, new Interval
                {
                    Name = name,
                    NumberOfMeasurement = 0,
                    TotalMeasurement = 0
                });
            }

            if (LastMeasurementTimestamp != 0)
            {
                var timestep = CurrentMeasurementTimestamp - LastMeasurementTimestamp;
                Ins[num].LastMeasurement = timestep;
                Ins[num].TotalMeasurement += timestep;
                Ins[num].NumberOfMeasurement++;
            }

            LastMeasurementTimestamp = CurrentMeasurementTimestamp;
        }


        public static string GetText()
        {
            var sb = new StringBuilder();
            var sumElapsedMs = 0;

            foreach (var i in Ins.OrderBy(i=>i.Key))
            {
                sumElapsedMs += (int)i.Value.LastMeasurement;
            }

            foreach (var i in Ins.OrderBy(i => i.Key))
            {
                sb.AppendLine($"{i.Key}. {i.Value.Name} " +
                              $"{(int)(1000000L * i.Value.TotalMeasurement / i.Value.NumberOfMeasurement / _frequency)} " +
                              $"mcs ({100 * i.Value.LastMeasurement / sumElapsedMs}%)");
            }

            return sb.ToString();
        }
    }
}
