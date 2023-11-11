namespace WindowsFormsApp1.GameLogic
{
	public class Pointer
	{
		public byte b;
		public byte cmd;
		public byte bOld;
		public byte cmdOld;

		public Pointer()
		{
			b = 0;
			cmd = 0;
			bOld = 0;
			cmdOld = 0;
		}

		public void Clear()
		{
			b = 0;
			cmd = 0;
			bOld = 0;
			cmdOld = 0;
		}

		public void CopyTo(Pointer p)
		{ 
			p.b = b;
			p.cmd = cmd;
			p.bOld = bOld;
			p.cmdOld = cmdOld;
		}
	}
}