using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WindowsFormsApp1.Enums
{
	//смещение условного перехода 2-пусто  3-стена  4-органика 5-бот 6-родня

	public enum RefContent
	{
		Free = 2,
		Wall = 3,
		Organic = 4,
		Bot = 5,
		Relative = 6,
		Mineral = 7,
		Edge = 8,
		Poison = 9,
		Grass = 10
	}
}
