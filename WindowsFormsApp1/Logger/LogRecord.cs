using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using System.Xml.Linq;
using WindowsFormsApp1.Dto;
using WindowsFormsApp1.Enums;
using WindowsFormsApp1.Static;

namespace WindowsFormsApp1.Log
{
    public class LogRecord
    {
        public string Text { get; init; }
        public uint Step { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
