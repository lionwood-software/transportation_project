using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using TransportApp.IdentityClient;
using TransportApp.Cache;
using TransportApp.MainApi.Factory;
using TransportApp.MainApi.Services;
using TransportApp.NominatimClient;
using MQ.ClientRealization;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using TransportApp.MainApi.ApiClients;
using System.Net.Http;

namespace TransportApp.MainApi.Extensions
{
    public static class DependencyInjectionResolver
    {
        public static IServiceCollection ResolveServices(this IServiceCollection services)
        {
            services.AddTransient<StopStationService>();
            services.AddTransient<RouteService>();
            services.AddTransient<MessageService>();
            services.AddTransient<DeviceService>();
            services.AddTransient<FeedbackService>();
            services.AddSingleton<ISendEmailMQ, SendEmailMQ>();
            services.AddTransient<SearchService>();
            services.AddTransient<RouteProcessingService>();
            services.AddTransient<FavouritesService>();
            services.AddTransient<SavedService>();
            services.AddTransient<VersionService>();
            services.AddTransient<SettingsService>();

            return services;
        }

        public static IServiceCollection ResolveFactories(this IServiceCollection services)
        {
            services.AddSingleton<RepositoryFactory>();

            return services;
        }

        public static IServiceCollection ResolveApiClients(this IServiceCollection services, ApiConfigurationOptions conf)
        {
            services.AddHttpClient();

            services.AddTransient(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

                return new IdentityApiClient(httpClientFactory, conf.Identity?.Authority);
            });

            services.AddTransient(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

                return new NominatimApiClient(httpClientFactory, conf.ExternalURLs.FirstOrDefault(x => x.Name == "nominatim")?.URL);
            });

            services.AddTransient(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

                return new TramGpsApiClient(httpClientFactory, conf.ExternalURLs.FirstOrDefault(x => x.Name == "gps-tram")?.URL);
            });

            services.AddTransient(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

                return new BusGpsApiClient(httpClientFactory, conf.ExternalURLs.FirstOrDefault(x => x.Name == "gps-bus")?.URL);
            });

            return services;
        }

        public static IServiceCollection ResolveCacheProvider(this IServiceCollection services, ApiConfigurationOptions conf)
        {
            _ = conf.Cache ?? throw new ArgumentNullException(nameof(conf.Cache));

            services.AddSingleton(_ =>
            {
                var configuration = new RedisConfiguration
                {
                    AbortOnConnectFail = false,
                    KeyPrefix = "",
                    Hosts = new RedisHost[]
                    {
                        new RedisHost(){Host = conf.Cache.Host, Port = conf.Cache.Port}
                    },
                    AllowAdmin = true,
                    ConnectTimeout = 3000,
                    Database = conf.Cache.Db,
                    Ssl = false,
                    Password = conf.Cache.Password,
                    ServerEnumerationStrategy = new ServerEnumerationStrategy()
                    {
                        Mode = ServerEnumerationStrategy.ModeOptions.All,
                        TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                        UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
                    },
                    SyncTimeout = 3000
                };
                configuration.ConfigurationOptions.ResolveDns = bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var _);

                return configuration;
            });
            services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
            services.AddSingleton<ISerializer, NewtonsoftSerializer>();
            services.AddTransient<ICacheProvider, RedisCacheProvider>();

            return services;
        }
    }
}
