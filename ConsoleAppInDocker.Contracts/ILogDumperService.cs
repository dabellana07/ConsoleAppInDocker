using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleAppInDocker.Models;

namespace ConsoleAppInDocker.Contracts
{
    public interface ILogDumperService
    {
        Task AddLog(LogEvent logEvent);
        void BulkAdd(List<LogEvent> logEvents);
    }
}
