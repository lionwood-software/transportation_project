using Configuration.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Transport.Worker.DailyReports.Services;

namespace Transport.Worker.DailyReports
{
    class Program
    {
        private static int[] hours = new int[] { 8, 10, 12, 16, 21, 23 };

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json")
                   .AddEnvironmentVariables();

            var OS = Environment.OSVersion.ToString() + " " + (Environment.Is64BitOperatingSystem ? "x64" : "x86");
            var ip = Dns.GetHostAddresses(Dns.GetHostName());

            IServiceCollection services = new ServiceCollection();
            Startup startup = new Startup(builder.Build());
            startup.ConfigureServices(services);

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var configRabbitMq = serviceProvider.GetService<RabbitMQClientConfiguration>();
            var settingConfiguration = serviceProvider.GetService<SettingConfiguration>();

            Log.Logger = new LoggerConfiguration()
               .WriteTo.File(Path.Combine(Environment.CurrentDirectory, "logs", "log.txt"), rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
               .WriteTo.RabbitMQ((clientConfiguration, sinkConfiguration) =>
               {
                   clientConfiguration.From(configRabbitMq);
                   sinkConfiguration.TextFormatter = new Serilog.Formatting.Json.JsonFormatter();
                   sinkConfiguration.RestrictedToMinimumLevel = Serilog.Events.LogEventLevel.Error;
                   //sinkConfiguration.BatchPostingLimit = 10;
                   //sinkConfiguration.Period = TimeSpan.FromMinutes(1);

               }).Enrich.WithProperty("APP", settingConfiguration.NameAPP)
               .Enrich.WithProperty("OS", OS)
               .Enrich.WithProperty("IP", string.Join(",", ip.Select(x => x.ToString()).ToList()))
            .CreateLogger();

            var worker = serviceProvider.GetService<DailyReportSeederService>();

            ILogger _logger = Log.Logger;

            try
            {
                if (hours.Contains(Helpers.DateTimeHelper.GetUkraineTime(DateTime.Now).Value.Hour))
                {
                    worker.Process().Wait();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                // added sleep to send error in RabbitMq
                Thread.Sleep(5000);
            }
        }
    }
}
