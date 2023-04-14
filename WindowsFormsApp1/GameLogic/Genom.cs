using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;

namespace WindowsFormsApp1.GameLogic
{
	public class Genom
	{

		public byte[] Code;
		public Guid GenomHash;
		public Guid ParentHash;
        public Guid GrandHash;
        public Guid PraHash;
		public Color Color;
        public int Bots;
        public int Level;

		private GameData _data;
		private Func _func;
		private Genom _parent;

		public Genom(GameData data, Func func, Genom parent = null)
		{
			_data = data;
			_func = func;
			_parent = parent;
			Bots = 0;
			CreateGenom(parent);
		}

		public byte GetCurrentCommand(int pointer)
		{
			return Code[pointer];
		}
		public byte GetNextCommand(int pointer)
		{
			return Code[pointer + 1 >= _data.GenomLength ? 0 : pointer + 1];
		}

		public bool IsRelative(Genom genom2)
		{
			if (GenomHash == genom2.GenomHash || GenomHash == genom2.ParentHash || GenomHash == genom2.GrandHash) return true;
			if (ParentHash == genom2.GenomHash || ParentHash == genom2.ParentHash || ParentHash == genom2.GrandHash) return true;
			if (GrandHash == genom2.GenomHash || GrandHash == genom2.ParentHash || GrandHash == genom2.GrandHash) return true;
			return false;
		}


		// Создание нового генома, абсолютно нового или мутированную копию предка
		private void CreateGenom(Genom parent)
		{
			Code = new byte[_data.GenomLength];
			GenomHash = Guid.NewGuid();
			Color = _func.GetRandomColor();

			if (parent == null)
			{
				for (var i = 0; i < _data.GenomLength; i++)
				{
					Code[i] = _func.GetRandomBotCode();
				}
				ParentHash = Guid.Empty;
				GrandHash = Guid.Empty;
                PraHash = GenomHash;
                Level = 1;
            }
			else
			{
				for (var i = 0; i < _data.GenomLength; i++)
				{
					Code[i] = parent.Code[i];
				}

				// один байт в геноме подменяем
				Code[_func.GetRandomBotCodeIndex()] = _func.GetRandomBotCode();

				ParentHash = parent.GenomHash;
				GrandHash = parent.ParentHash;
				PraHash = parent.PraHash;
				_data.MutationCnt++;
                Level = parent.Level + 1;
            }
		}
	}
}

