using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ConsoleAppInDocker
{
    public class Worker : IWorker
    {
        private readonly ILogger<IWorker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting App");
            var counter = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug($"Counter: {++counter}");
                await Task.Delay(1000);
            }
        }
    }
}
