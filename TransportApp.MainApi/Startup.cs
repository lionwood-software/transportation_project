using AutoMapper;
using Configuration.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using MQ.Rabbit;
using System;
using System.Linq;
using TransportApp.MainApi.Extensions;
using TransportApp.MainApi.Filters;
using MQ.ClientRealization;

namespace TransportApp.MainApi
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(env.ContentRootPath)
               .AddJsonFile("appsettings.json")
               .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var conf = new ApiConfigurationOptions();
            Configuration.Bind(conf);

            services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problems = new CustomBadRequest(context);
                    return new BadRequestObjectResult(problems);
                };
            })
            .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            services.AddHttpContextAccessor();

            services.ResolveServices();
            services.AddTransient(LoadMQConfig);
            services.ResolveFactories();
            services.ResolveApiClients(conf);
            services.ResolveCacheProvider(conf);

            services.AddTransient(LoadSettingsURL);
            services.AddTransient(LoadIdentityClientsConfigs);

            services.Configure<ApiConfigurationOptions>(Configuration);
            services.Configure<SettingConfiguration>((configureOptions) =>
            {
                configureOptions.NameAPP = conf.SettingConfiguration.NameAPP;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Transport API", Version = "v1" });
            });

            services.AddAutoMapper(typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowAll");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Main API V1");
            });
        }

        private List<SettingsURL> LoadSettingsURL(IServiceProvider arg)
        {
            var conf = new ApiConfigurationOptions();
            Configuration.Bind(conf);

            return conf.ExternalURLs;
        }

        private ISendEmailMQConfiguration LoadMQConfig(IServiceProvider arg)
        {
            var conf = new ApiConfigurationOptions();
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

        private List<IdentityConfig> LoadIdentityClientsConfigs(IServiceProvider arg)
        {
            var conf = new ConfigurationOptions();
            Configuration.Bind(conf);

            return conf.IdentityClients;
        }
    }
}
