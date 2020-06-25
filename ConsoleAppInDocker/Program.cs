using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleAppInDocker.Contracts;
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

        static async Task Main(string[] args)
        {
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
            services.AddSingleton<IExternalLogReader, JsonLogFileReader>();
            services.AddSingleton<ILogDumperService, LogElasticService>();
            services.AddSingleton<IWorker, Worker>();
        }

        private static void AddElasticSearch(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var uri = new Uri(configuration["ElasticSearchConfig:Url"]);
            var username = configuration["ElasticSearchConfig:Username"];
            var password = configuration["ElasticSearchConfig:Password"];

            var settings = new ConnectionSettings(uri);
            settings.BasicAuthentication(username, password);
            settings.DisableDirectStreaming();
            settings.OnRequestCompleted(call =>
            {
                Debug.WriteLine("Endpoint Called: " + call.Uri);

                if (call.RequestBodyInBytes != null)
                {
                    Debug.WriteLine("Request Body: " + Encoding.UTF8.GetString(call.RequestBodyInBytes));
                }

                if (call.ResponseBodyInBytes != null)
                {
                    Debug.WriteLine("Response Body: " + Encoding.UTF8.GetString(call.ResponseBodyInBytes));
                }

                if (call.ResponseMimeType != null)
                {
                    Debug.WriteLine("Response Mime: " + call.ResponseMimeType);
                }
            });

            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client);
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
