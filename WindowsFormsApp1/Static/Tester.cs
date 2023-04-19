﻿using System;
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
    public static class Test
    {
        private const int numberOfIntervals = 10;

        private static Interval[] _intervals = new Interval[numberOfIntervals];
        private static long _frequency = Stopwatch.Frequency;


        public static void InitInterval(int i, string name)
        {
            if (i > numberOfIntervals)
            {
                throw new Exception("if (i > numberOfIntervals)");
            }

            if (_intervals[i] == null)
            {
                _intervals[i] = new Interval();
            }

            _intervals[i].Active = true;
            _intervals[i].Num = i;
            _intervals[i].Name = name;
            _intervals[i].TotalMeasurement = 0;
            _intervals[i].NumberOfMeasurement = 0;
        }

        public static void BeginInterval(int i)
        {
            _intervals[i].LastMeasurementTimestamp = Stopwatch.GetTimestamp();
        }

        public static void EndInterval(int i)
        {
            _intervals[i].LastMeasurement = Stopwatch.GetTimestamp() - _intervals[i].LastMeasurementTimestamp;
            _intervals[i].TotalMeasurement += _intervals[i].LastMeasurement;
            _intervals[i].NumberOfMeasurement++;
        }

        public static void EndBeginInterval(int end, int begin)
        {
            _intervals[begin].LastMeasurementTimestamp = Stopwatch.GetTimestamp();
            _intervals[end].LastMeasurement = _intervals[begin].LastMeasurementTimestamp - _intervals[end].LastMeasurementTimestamp;
            _intervals[end].TotalMeasurement += _intervals[end].LastMeasurement;
            _intervals[end].NumberOfMeasurement++;
        }

        public static string GetText()
        {
            var sb = new StringBuilder();
            var sumElapsedMs = 0;

            foreach (var i in _intervals)
            {
                if (i != null && i.Active && i.NumberOfMeasurement != 0)
                {
                    //i.ElapsedMs = (int)(1000000L * i.TotalMeasurement / i.NumberOfMeasurement / _frequency);
                    i.ElapsedMs = (int)(1000000L * i.LastMeasurement / _frequency);
                    sumElapsedMs += i.ElapsedMs;
                }
            }

            foreach (var i in _intervals)
            {
                if (i != null && i.Active)
                {
                    sb.AppendLine($"{i.Num + 1}. {i.Name} {i.ElapsedMs} mcs ({100 * i.ElapsedMs / sumElapsedMs}%)");
                }
            }

            return sb.ToString();
        }
    }

    public class Interval
    {
        public bool Active;
        public int Num;
        public string Name;
        public long LastMeasurement;
        public long LastMeasurementTimestamp;
        public long TotalMeasurement;
        public int NumberOfMeasurement;
        public int ElapsedMs;
    }
}
