using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace NetworkSpeedTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ITelemetryInitializer>(new TelemetryInitializer("NetworkSpeedTest"));
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.AddHostedService<Worker>();
                });
    }
}