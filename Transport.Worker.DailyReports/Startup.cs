using Configuration.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Core;
using Repository.MongoDb;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using System;
using System.Linq;
using System.Net.Http;
using Transport.Worker.DailyReports.Services;
using TransportApp.ExternalDataClient;
using TransportApp.Storage;

namespace Transport.Worker.DailyReports
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            var conf = new ConfigurationOptions();
            Configuration.Bind(conf);

            services.AddTransient(LoadRepositoryConfig);
            services.AddTransient(LoadSettingConfiguration);
            services.AddTransient(LoadSerilogMQConfig);
            services.AddTransient<IRepository, MongoDbRepository>();
            services.AddTransient<DailyReportSeederService>();

            services.AddHttpClient();
            services.AddTransient(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

                return new ExternalDataApiClient(httpClientFactory, conf.ExternalURLs?.FirstOrDefault(x => x.Name == "urlplan")?.URL);
            });
            services.AddTransient<IStorageService>(option =>
            {
                return new MinioStorageService(conf.MinioConfiguration);
            });
        }

        private RabbitMQClientConfiguration LoadSerilogMQConfig(IServiceProvider arg)
        {
            var configRabbitMq = new RabbitMQClientConfiguration();
            Configuration.Bind("SerilogRabbitMQClientConfiguration", configRabbitMq);

            return configRabbitMq;
        }

        private static SettingConfiguration LoadSettingConfiguration(IServiceProvider arg)
        {
            var conf = new ConfigurationOptions();
            Configuration.Bind(conf);

            var result = new SettingConfiguration
            {
                NameAPP = conf.SettingConfiguration.NameAPP,
            };
            return result;
        }

        private IRepositoryConfiguration LoadRepositoryConfig(IServiceProvider provider)
        {
            var conf = new ConfigurationOptions();
            Configuration.Bind(conf);

            var config = conf.DB.First();
            var result = new DbConfiguration
            {
                ConnectionString = config.ConnectionString,
                Database = config.Database,
                Name = config.Name,
                Type = config.Type
            };

            return result;
        }

        private class DbConfiguration : DbConfig, IRepositoryConfiguration
        {
        }
    }
}
