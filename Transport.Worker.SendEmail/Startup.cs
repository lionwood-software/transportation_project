using Configuration.Core;
using Configuration.Core.SendEmail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQ.ClientRealization;
using MQ.Rabbit;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using System;
using System.Linq;
using Transport.Worker.SendEmail.Models;
using Transport.Worker.SendEmail.Services;
using TransportAPP.General.Interfaces;

namespace Transport.Worker.SendEmail
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
            services.AddTransient(LoadMQConfig);
            services.AddTransient(LoadEmailCredential);
            services.AddTransient(LoadSettingConfiguration);
            services.AddTransient(LoadSerilogMQConfig);
            services.AddTransient<ISendEmailMQ, SendEmailMQ>();
            services.AddTransient<IRunService, WorkerService>();
            services.AddTransient<ISendService<SendEmailMessage>, SendEmailService>();
        }

        private ISendEmailMQConfiguration LoadMQConfig(IServiceProvider arg)
        {
            var conf = new ConfigurationOptions();
            Configuration.Bind(conf);

            var result = conf.MQ.Where(x => x.Name == "SendEmail")
               .Select(item => new MessageQueue
               {
                   Name = item.Name,
                   Type = item.Type,
                   Host = item.Host,
                   Username = item.Username,
                   Password = item.Password,
                   VirtualHost = item.VirtualHost,
                   Port = item.Port,
                   QueueName = item.QueueName,
                   Exchange = item.Exchange,
                   ExchangeType = item.ExchangeType
               })
               .FirstOrDefault();

            return new SendEmailMQConfiguration(result);
        }

        private RabbitMQClientConfiguration LoadSerilogMQConfig(IServiceProvider arg)
        {
            var configRabbitMq = new RabbitMQClientConfiguration();
            Configuration.Bind("SerilogRabbitMQClientConfiguration", configRabbitMq);

            return configRabbitMq;
        }

        private static EmailCredential LoadEmailCredential(IServiceProvider arg)
        {
            var conf = new ConfigurationOptions();
            Configuration.Bind(conf);

            var result = new EmailCredential
            {
                Address = conf.EmailCredential.Address,
                Host = conf.EmailCredential.Host,
                Password = conf.EmailCredential.Password,
                Port = conf.EmailCredential.Port,
            };
            return result;
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

    }
}
