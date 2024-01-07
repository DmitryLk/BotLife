using Newtonsoft.Json.Linq;
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
		public byte[] val = new byte[4];

		public unsafe (byte B0, byte B1, bool Ev, bool Force, byte Tm) GetHistoryData()
		{
			fixed (byte* ptr = val)
			{
				var b0 = ((byte*)ptr)[0];
				var b1 = ((byte*)ptr)[1];
				var b2 = ((byte*)ptr)[2];
				var b3 = ((byte*)ptr)[3];
				return (((byte*)ptr)[0], ((byte*)ptr)[1], (((byte*)ptr)[2] & 1) == 0 ? false : true, (((byte*)ptr)[2] & 2) == 0 ? false : true, ((byte*)ptr)[3]);
			}
		}

		public unsafe void SaveHistoryData(byte B0, byte B1, bool Ev, bool Force, byte B3)
		{
			fixed (byte* ptr = val)
			{
				var bptr = (byte*)ptr;
				byte B2 = 0;

				bptr[0] = B0;
				bptr[1] = B1;
				if (Ev) B2 |= 1;
				if (Force) B2 |= 2;
				bptr[2] = B2;
				bptr[3] = B3;
			}
		}
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

		public void SaveCmdToHistory(Pointer p, bool ev, int tm)
		{
			if (historyPointerY == -1) return;
			var cmdCnt = H[historyPointerY].cmdCnt;

			if (cmdCnt == maxx) throw new Exception("PutPtr(byte ptr) ");

			H[historyPointerY].CmdStep[cmdCnt].SaveHistoryData(p.B, p.CmdNum, ev, false, (byte)tm);

			H[historyPointerY].cmdCnt++;
		}

		public void SaveForcedCmdToHistory(byte cmd, byte par, int tm)
		{
			if (historyPointerY == -1) return;
			var cmdCnt = H[historyPointerY].cmdCnt;

			if (cmdCnt == maxx) throw new Exception("PutPtr(byte ptr) ");

			H[historyPointerY].CmdStep[cmdCnt].SaveHistoryData(cmd, par, false, true, (byte)tm);

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