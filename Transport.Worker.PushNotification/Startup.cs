using Configuration.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository.Core;
using Repository.MongoDb;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using System;
using System.Linq;
using Transport.Worker.PushNotification.Services;

namespace Transport.Worker.PushNotification
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
            var conf = new WorkerConfigurationOptions();
            Configuration.Bind(conf);

            services.AddSingleton(_ => conf);
            services.AddTransient(LoadSettingConfiguration);
            services.AddTransient(LoadSerilogMQConfig);
            services.AddTransient<IRepository, MongoDbRepository>();
            services.AddTransient<PushNotificationService>();
            services.AddTransient(LoadRepositoryConfig);
        }

        private class DbConfiguration : DbConfig, IRepositoryConfiguration
        {
        }

        private IRepositoryConfiguration LoadRepositoryConfig(IServiceProvider provider)
        {
            var conf = new WorkerConfigurationOptions();
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

        private RabbitMQClientConfiguration LoadSerilogMQConfig(IServiceProvider arg)
        {
            var configRabbitMq = new RabbitMQClientConfiguration();
            Configuration.Bind("SerilogRabbitMQClientConfiguration", configRabbitMq);

            return configRabbitMq;
        }

        private static SettingConfiguration LoadSettingConfiguration(IServiceProvider arg)
        {
            var conf = new WorkerConfigurationOptions();
            Configuration.Bind(conf);

            var result = new SettingConfiguration
            {
                NameAPP = conf.SettingConfiguration.NameAPP
            };
            return result;
        }
    }
}
