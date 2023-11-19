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
	public class CmdHistory
	{
		public byte b;
		public byte c;
		public bool ev;
		public byte recNum;
		public int tm;
	}

	public class StepHistory
	{

		public CmdHistory[] CmdStep;
		public uint Step;
		public byte cmdCnt;
		public int tm1;
		public int tm2;

		public StepHistory(int maxx)
		{
			CmdStep = new CmdHistory[maxx];
			for (var x = 0; x < maxx; x++)
			{
				CmdStep[x] = new CmdHistory();
			}
		}
	}



	public class CodeHistory
	{
		private const int maxx = 15;                    // максимальное количество команд в шаге
		private const int maxy = 10;                    // макимальное количетсов команд, которые могут быть записаны

		public StepHistory[] H = new StepHistory[maxx]; // массив команд
		public int historyPointerY = -1;                  // номер команды в которую сейчас записываются шаги


		public CodeHistory()
		{
			for (var y = 0; y < maxy; y++)
			{
				H[y] = new StepHistory(maxx);
			}
		}

		public void BeginNewStep(int tm1)
		{
			historyPointerY = historyPointerY == maxy ? historyPointerY = 0 : historyPointerY + 1;
			if (historyPointerY == maxy) historyPointerY = 0;

			H[historyPointerY].cmdCnt = 0;
			H[historyPointerY].Step = Data.CurrentStep;
			H[historyPointerY].tm1 = tm1;
			H[historyPointerY].tm2 = 0;
		}

		public void EndNewStep(int tm2)
		{
			H[historyPointerY].tm2 = tm2;
			//pointer.CopyTo(H[historyPointerY].pointer);
		}

		public void SaveCmdToHistory(Pointer p, bool ev, byte recnum, int tm)
		{
			if (historyPointerY == -1) return;
			var cmdCnt = H[historyPointerY].cmdCnt;

			if (cmdCnt == maxx) throw new Exception("PutPtr(byte ptr) ");


			H[historyPointerY].CmdStep[cmdCnt].b = p.B;
			H[historyPointerY].CmdStep[cmdCnt].c = p.CmdNum;
			H[historyPointerY].CmdStep[cmdCnt].ev = ev;
			H[historyPointerY].CmdStep[cmdCnt].recNum = recnum;
			H[historyPointerY].CmdStep[cmdCnt].tm = tm;

			H[historyPointerY].cmdCnt++;
		}

		//====Для отображения=======================================================
		public (CmdHistory[], int, int, int, uint) GetLastStepPtrs(int delta)
		{
			if (historyPointerY < 0)
			{
				return (Array.Empty<CmdHistory>(), 0, 0, 0, 0);
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

			return (H[ptr].CmdStep, H[ptr].cmdCnt, H[ptr].tm1, H[ptr].tm2, H[ptr].Step);
		}

		public void Clear()
		{
			Array.Clear(H, 0, H.Length);
			historyPointerY = -1;
		}
	}
}