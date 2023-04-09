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
		public Guid ParentGenomHash;
		public Guid GrandParentGenomHash;
		public Color Color;
		public int Bots; 

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
			return Code[pointer + 1 >= _data.CodeLength ? 0 : pointer + 1];
		}

		public bool IsRelative(Genom genom2)
		{
			if (GenomHash == genom2.GenomHash || GenomHash == genom2.ParentGenomHash || GenomHash == genom2.GrandParentGenomHash) return true;
			if (ParentGenomHash == genom2.GenomHash || ParentGenomHash == genom2.ParentGenomHash || ParentGenomHash == genom2.GrandParentGenomHash) return true;
			if (GrandParentGenomHash == genom2.GenomHash || GrandParentGenomHash == genom2.ParentGenomHash || GrandParentGenomHash == genom2.GrandParentGenomHash) return true;
			return false;
		}


		// Создание нового генома, абсолютно нового или мутированную копию предка
		private void CreateGenom(Genom parent)
		{
			Code = new byte[_data.CodeLength];
			GenomHash = Guid.NewGuid();
			Color = _func.GetRandomColor();

			if (parent == null)
			{
				for (var i = 0; i < _data.CodeLength; i++)
				{
					Code[i] = _func.GetRandomBotCode();
				}
				ParentGenomHash = Guid.Empty;
				GrandParentGenomHash = Guid.Empty;
			}
			else
			{
				for (var i = 0; i < _data.CodeLength; i++)
				{
					Code[i] = parent.Code[i];
				}

				// один байт в геноме подменяем
				Code[_func.GetRandomBotCodeIndex()] = _func.GetRandomBotCode();

				ParentGenomHash = parent.GenomHash;
				GrandParentGenomHash = parent.ParentGenomHash;
				_data.MutationCnt++;
			}
		}
	}
}

