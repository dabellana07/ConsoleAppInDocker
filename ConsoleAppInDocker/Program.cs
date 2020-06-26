using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleAppInDocker.Contracts;
using ConsoleAppInDocker.Extensions;
using ConsoleAppInDocker.Services;
using ConsoleAppInDocker.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;

namespace ConsoleAppInDocker
{
    class Program
    {
        static ServiceProvider _serviceProvider;
        static ILogger<Program> _logger;
        static CancellationTokenSource _cancellationTokenSource;
        static IConfigurationRoot _configuration;

        static async Task Main(string[] args)
        {
            _configuration = GetAppConfiguration();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider(true);
            _logger = _serviceProvider.GetService<ILogger<Program>>();
            _cancellationTokenSource = new CancellationTokenSource();

            Init();
            await DoTask();
        }

        private static async Task DoTask()
        {
            var scope = _serviceProvider.CreateScope();
            var worker = scope.ServiceProvider.GetRequiredService<IWorker>();
            await worker.ExecuteAsync(_cancellationTokenSource.Token);
        }

        private static IConfigurationRoot GetAppConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            return builder.Build();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging(options =>
                {
                    options.AddConsole();
                })
                .Configure<LoggerFilterOptions>(options =>
                {
                    options.MinLevel = Microsoft.Extensions.Logging.LogLevel.Debug;
                });
            services.AddElasticSearch(_configuration);
            services.AddSingleton<IExternalLogReader, JsonLogFileReader>();
            services.AddSingleton<ILogDumperService, LogElasticService>();
            services.AddSingleton<IWorker, Worker>();
        }

        private static void Init()
        {
            _logger.LogDebug("Initializing app");
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogError("An error occurred: ", e.ExceptionObject);
        }

        private static void Dispose()
        {
            if (_serviceProvider == null)
            {
                return;
            }

            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }

            _cancellationTokenSource.Dispose();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            _logger.LogDebug("Shutting down app");

            _cancellationTokenSource.Cancel();

            _logger.LogDebug("Waiting for requests to finalize");

            _logger.LogDebug("Cleaning up resources");

            Dispose();
        }
    }
}
