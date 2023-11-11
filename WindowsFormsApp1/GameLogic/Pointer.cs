using System.Xml.Linq;

namespace WindowsFormsApp1.GameLogic
{
	public class Pointer
	{
		private byte _b;
		private byte _cmdNum;
		private byte _bOld;
		private byte _cmdNumOld;

		public byte B
		{
			get => _b;
			set
			{
				_bOld = _b;
				_b = value;
			}
		}

		public byte CmdNum
		{
			get => _cmdNum;
			set
			{
				_cmdNumOld = _cmdNum;
				_cmdNum = value;
			}
		}

		public byte BOld
		{
			get => _bOld;
		}

		public byte CmdNumOld
		{
			get => _cmdNumOld;
		}

		public Pointer()
		{
			_b = 0;
			_cmdNum = 0;
			_bOld = 0;
			_cmdNumOld = 0;
		}

		public void Clear()
		{
			_b = 0;
			_cmdNum = 0;
			_bOld = 0;
			_cmdNumOld = 0;
		}

		public void CopyTo(Pointer p)
		{ 
			p._b = _b;
			p._cmdNum = _cmdNum;
			p._bOld = _bOld;
			p._cmdNumOld = _cmdNumOld;
		}
	}
}