using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace ConsoleAppInDocker.Extensions
{
    public static class ElasticSearchExtensions
    {
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
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
    }
}
