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
	public static class Test2
	{
		private const int numberOfIntervals = 30;

		private static readonly long _frequency = Stopwatch.Frequency;
		private static readonly long[] Total = new long[numberOfIntervals];
		private static readonly long[] Max = new long[numberOfIntervals];
		private static readonly long[] Cnt = new long[numberOfIntervals];

		static Test2()
		{
			Array.Clear(Total, 0, Total.Length);
			Array.Clear(Cnt, 0, Cnt.Length);
		}

		public static long Mark(int num, long tm)
		{
			var t = Stopwatch.GetTimestamp();
			var diff = t - tm;

			Interlocked.Add(ref Total[num], diff);
			Interlocked.Increment(ref Cnt[num]);
			if (Cnt[num] > 1000000 && diff > Max[num])
			{
				Max[num] = diff;
			}
			return t;
		}

		public static string GetText()
		{
			float avg;
			var sb = new StringBuilder();

			for (var i = 0; i < numberOfIntervals; i++)
			{
				if (Cnt[i] > 0)
				{
					//sb.AppendLine($"{i}. {(int)(1000000L * Total[i] / Cnt[i] / _frequency)} mcs");
					//sb.AppendLine($"{i}. {Total[i] / Cnt[i]}");
					//sb.AppendLine($"{i}. {Total[i]}/{Cnt[i]}");
					avg = Total[i] / (float)Cnt[i];
					sb.AppendLine($"{i}. {avg:0.000}   Mx:{Max[i]/avg :0}");
					Total[i] = 0;
					Cnt[i] = 0;
					Max[i] = 0;
				}
			}

			return sb.ToString();
		}
	}
}
