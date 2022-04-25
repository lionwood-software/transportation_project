using System;
using System.IO;
using System.Linq;
using System.Net;
using Configuration.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;

namespace TransportApp.MainApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = Log.Logger;

            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "MainAPI");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var OS = Environment.OSVersion.ToString() + " " + (Environment.Is64BitOperatingSystem ? "x64" : "x86");
                    var ip = Dns.GetHostAddresses(Dns.GetHostName());

                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, 5000);
                        options.Listen(IPAddress.Any, 5443, listenOptions =>
                        {
                            listenOptions.UseHttps("self-signed.pfx", "$M2Kpx)!uE]YX0o");
                        });
                    });
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSerilog((context, config) =>
                    {
                        var configRabbitMq = new RabbitMQClientConfiguration();
                        context.Configuration.Bind("SerilogRabbitMQClientConfiguration", configRabbitMq);
                        SettingConfiguration settingConfiguration = new SettingConfiguration();
                        context.Configuration.Bind("SettingConfiguration", settingConfiguration);

                        config
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
                        .Enrich.WithProperty("IP", string.Join(",", ip.Select(x => x.ToString()).ToList()));
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Trace);
                });
    }
}
