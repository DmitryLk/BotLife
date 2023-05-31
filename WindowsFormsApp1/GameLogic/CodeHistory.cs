using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.Static;

namespace WindowsFormsApp1.GameLogic
{
	public class CodeHistory
	{
		private const int maxx = 15;
		private const int maxy = 10;

		public byte[][] codeHistory = new byte[maxy][];
		public byte[] ptrs = new byte[maxy];
		public int historyPointerY = -1;


		public CodeHistory()
		{
			for (var y = 0; y < maxy; y++)
			{
				codeHistory[y] = new byte[maxx];
			}
		}

		public void SavePtr(int ptr)
		{
			if (ptrs[historyPointerY] == maxx) throw new Exception("PutPtr(byte ptr) ");
			codeHistory[historyPointerY][ptrs[historyPointerY]] = (byte)ptr;
			ptrs[historyPointerY]++;
		}
		public void BeginNewStep()
		{
			historyPointerY++;
			if (historyPointerY == maxy) historyPointerY = 0;
			ptrs[historyPointerY] = 0;
		}

		public (byte[], int) GetLastStepPtrs(int delta)
		{
			if (historyPointerY < 0)
			{
				return (Array.Empty<byte>(), 0);
			}

			var ptr = historyPointerY + delta;

			while (ptr < 0)
			{
				ptr += maxy;
			}

			while (ptr >= maxy)
			{
				ptr -= maxy;
			}


			return (codeHistory[ptr], ptrs[ptr]);
		}

        public void Clear()
        {
            Array.Clear(codeHistory, 0, codeHistory.Length);
            Array.Clear(ptrs, 0, ptrs.Length);
            historyPointerY = -1;
        }
	}
}