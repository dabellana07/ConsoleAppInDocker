using System;
using System.Threading;
using System.Threading.Tasks;
using ConsoleAppInDocker.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleAppInDocker
{
    public class Worker : IWorker
    {
        private readonly IExternalLogReader _externalLogReader;
        private readonly ILogDumperService _logDumperService;
        private readonly ILogger<Worker> _logger;
        private readonly int _intervalMilliseconds;

        public Worker(
            ILogger<Worker> logger,
            IConfiguration configuration,
            IExternalLogReader logReader,
            ILogDumperService logDumperService
        )
        {
            _intervalMilliseconds = int.Parse(configuration["Interval"]);
            _externalLogReader = logReader;
            _logDumperService = logDumperService;
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting App");
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    ReadAndWriteLogs();
                }
                catch (Exception e)
                {
                    _logger.LogError("AN ERROR OCURRED: " + e.ToString());
                }

                await Task.Delay(_intervalMilliseconds, cancellationToken);
            }
        }

        private void ReadAndWriteLogs()
        {
            var logs = _externalLogReader.ReadLogs();

            _logger.LogInformation("Number of logs: " + logs.Count);

            _logDumperService.BulkAdd(logs);
        }
    }
}
