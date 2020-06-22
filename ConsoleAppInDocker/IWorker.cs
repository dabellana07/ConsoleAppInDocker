using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppInDocker
{
    public interface IWorker
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
