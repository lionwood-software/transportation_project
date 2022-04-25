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
using Transport.Worker.PushNotification.Services;

namespace Transport.Worker.PushNotification
{
    class Program
    {
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

               }).Enrich.WithProperty("APP", settingConfiguration.NameAPP)
               .Enrich.WithProperty("OS", OS)
               .Enrich.WithProperty("IP", string.Join(",", ip.Select(x => x.ToString()).ToList()))
            .CreateLogger();
            
            var worker = serviceProvider.GetService<PushNotificationService>();
            ILogger _logger = Log.Logger;

            try
            {
                worker.Send().GetAwaiter().GetResult();
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
