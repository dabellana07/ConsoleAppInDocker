using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConsoleAppInDocker.Contracts;
using ConsoleAppInDocker.Models;
using Nest;

namespace ConsoleAppInDocker.Services
{
    public class LogElasticService : ILogDumperService
    {
        private const string IndexName = "logs";

        private readonly IElasticClient _client;

        public LogElasticService(IElasticClient client)
        {
            _client = client;
        }

        public async Task AddLog(LogEvent logEvent)
        {
            await _client.IndexAsync(logEvent, i => i
                .Index(IndexName)
                .Id(logEvent.Id));
        }

        public void BulkAdd(List<LogEvent> logEvents)
        {
            var bulkAllObservable = _client.BulkAll(logEvents, l => l
                    .Index(IndexName)
                    .BackOffTime("30s")
                    .BackOffRetries(2)
                    .RefreshOnCompleted()
                    .MaxDegreeOfParallelism(Environment.ProcessorCount)
                    .Size(1000))
                .Wait(TimeSpan.FromMinutes(15), next =>
                {
                });
        }
    }
}
