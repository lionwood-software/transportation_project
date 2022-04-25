using AutoMapper;
using Configuration.Core;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransportApp.BaseClient;
using TransportApp.EntityModel;
using TransportApp.IdentityClient;
using TransportApp.MainApi.Factory;
using TransportApp.MainApi.Models.Device;

namespace TransportApp.MainApi.Services
{
    public class DeviceService : BaseService<Device>
    {
        private readonly IdentityApiClient _apiClient;
        private readonly IdentityConfig _config;
        private const string ApiClient = "identity";

        public DeviceService(IMapper mapper, IHttpContextAccessor httpContextAccessor, RepositoryFactory factory, IdentityApiClient apiClient, List<IdentityConfig> config)
            : base(mapper, httpContextAccessor, factory)
        {
            _apiClient = apiClient;
            _config = config.FirstOrDefault(x => x.Name == ApiClient) ?? throw new ArgumentNullException($"Authorization configuration for \"{ApiClient}\" api client not found!");
        }

        public override async Task<Y> GetByIdAsync<Y>(string token)
        {
            var user = await _repository.GetCollection<Device>().Find(x => x.DeviceToken == token).SingleOrDefaultAsync();

            return user as Y;
        }

        public async Task CreateUpdateDeviceAsync(CreateDevice device)
        {
            var user = await GetByIdAsync<Device>(device.Token);

            if (user == null)
            {
                await _repository.GetCollection<Device>().InsertOneAsync(new Device(device.Token, device.Type, device.Language));

                string token = await AuthorityToken.GetAccessTokenAsync(ApiClient, _config.Authority, _config.ClientId, _config.ClientSecret, string.Join(" ", _config.Scope));
                await _apiClient.PostAsync<string>("api/v1/devices", device, token);
            }
            else
            {
                FilterDefinition<Device> filter = Builders<Device>.Filter.Where(x => x.DeviceToken == device.Token);
                UpdateDefinition<Device> update = Builders<Device>.Update.Set(x => x.Language, device.Language)
                    .Set(x => x.UpdatedAt, DateTime.Now);
                await _repository.GetCollection<Device>().UpdateOneAsync(filter, update);
            }
        }
    }
}