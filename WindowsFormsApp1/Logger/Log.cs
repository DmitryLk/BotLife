using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WindowsFormsApp1.Static;

namespace WindowsFormsApp1.Logger
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
