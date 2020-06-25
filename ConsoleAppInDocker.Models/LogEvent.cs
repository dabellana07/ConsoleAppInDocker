using System;
namespace ConsoleAppInDocker.Models
{
    public class LogEvent
    {
        public long Id { get; set; }

        public string Level { get; set; }

        public string Description { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
