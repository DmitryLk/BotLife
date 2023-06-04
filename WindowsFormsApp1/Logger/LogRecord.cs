using System;

namespace WindowsFormsApp1.Logger
{
    public class LogRecord
    {
        public string Text { get; init; }
        public uint Step { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
