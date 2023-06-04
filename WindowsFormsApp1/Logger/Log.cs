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
    public class BotLog
    {
        List<LogRecord> _log;
        public BotLog()
        {
            _log = new List<LogRecord>();
        }


        public void LogInfo(string text)
        {
            _log.Add(new LogRecord
            {
                Text = text,
                Step = Data.CurrentStep,
                CreatedAt = DateTime.Now,
            });
        }

        public List<LogRecord> GetLog()
        {
            return _log.OrderBy(l => l.CreatedAt).ToList();
        }
        
        public string GetLogString()
        {
            var sb = new StringBuilder();
            foreach (var l in _log.OrderBy(l => l.CreatedAt).ToList())
            {
                sb.AppendLine(l.Text);
            }

            return sb.ToString();
        }

        public void ClearLog()
        {
            _log.Clear();
        }
    }
}
