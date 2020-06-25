using System.Collections.Generic;
using ConsoleAppInDocker.Models;

namespace ConsoleAppInDocker.Contracts
{
    public interface IExternalLogReader
    {
        List<LogEvent> ReadLogs();
    }
}
