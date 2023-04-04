using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Enums;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Label = System.Windows.Forms.Label;
using TextBox = System.Windows.Forms.TextBox;

namespace WindowsFormsApp1.GameLogic
{
    public class RandomService
    {
        private Random _rnd = new Random(Guid.NewGuid().GetHashCode());
        private GameOptions _options;

        public RandomService(GameOptions options)
        {
            _rnd = new Random(Guid.NewGuid().GetHashCode());
            _options = options;
        }

        public Direction GetRandomDirection()
        {
            return (Direction)_rnd.Next(0, 8);
        }

        public byte GetRandomBotCode()
        {
            return (byte)_rnd.Next(_options.MaxCode + 1);
        }

        public int GetRandomWorldX()
        {
            return _rnd.Next(0, _options.WorldWidth);
        }

        public int GetRandomWorldY()
        {
            return _rnd.Next(0, _options.WorldHeight);
        }

        public (int, int) GetRandomSpeed()
        {
            //do
            //{
            //	_vx = rnd.Next(-1, 2);
            //	_vy = rnd.Next(-1, 2);
            //}
            //while (_vx == 0 && _vy == 0);

            if (_rnd.Next(100) > 97)
            {
                return (_rnd.Next(-1, 2), _rnd.Next(-1, 2));
            }
            return (0, 0);
        }
    }
}
