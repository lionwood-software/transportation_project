using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Repository.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransportApp.Cache;
using TransportApp.EntityModel;
using TransportApp.MainApi.Factory;
using TransportApp.MainApi.Models.Version;
using RouteEntity = TransportApp.EntityModel.Route;

namespace TransportApp.MainApi.Services
{
    public class VersionService
    {
        private readonly ICacheProvider _storage;
        private readonly IRepository _repository;

        public VersionService(ICacheProvider storage, IHttpContextAccessor httpContextAccessor, RepositoryFactory factory)
        {
            _storage = storage;
            _repository = GetRepository(httpContextAccessor, factory);
        }

        public async Task<CurrentVersion> GetCurrentVersionAsync()
        {
            CurrentVersion currentVersion = _storage.Get<CurrentVersion>($"current_routes_version");

            if (currentVersion == null)
            {
                var countTram = await GetCountRoutesByTypeAsync(VehicleType.Tram);
                var countTrol = await GetCountRoutesByTypeAsync(VehicleType.Trolleybus);
                var countBus = await GetCountRoutesByTypeAsync(VehicleType.Bus);
                var countMetro = await GetCountRoutesByTypeAsync(VehicleType.Metro);

                var tram = await GetLastUpdatedRouteByTypeAsync(VehicleType.Tram);
                var trol = await GetLastUpdatedRouteByTypeAsync(VehicleType.Trolleybus);
                var bus = await GetLastUpdatedRouteByTypeAsync(VehicleType.Bus);
                var metro = await GetLastUpdatedRouteByTypeAsync(VehicleType.Metro);

                currentVersion = new CurrentVersion
                {
                    Tram = new VersionVehicleType { Count = countTram, LastUpdated = tram?.UpdatedAt ?? DateTime.Now },
                    Trol = new VersionVehicleType { Count = countTrol, LastUpdated = trol?.UpdatedAt ?? DateTime.Now },
                    Bus = new VersionVehicleType { Count = countBus, LastUpdated = bus?.UpdatedAt ?? DateTime.Now },
                    Metro = new VersionVehicleType { Count = countMetro, LastUpdated = metro?.UpdatedAt ?? DateTime.Now }
                };

                _storage.Set($"current_routes_version", currentVersion);
            }

            return currentVersion;
        }

        private IRepository GetRepository(IHttpContextAccessor httpContextAccessor, RepositoryFactory factory)
        {
            try
            {
                var codeResult = httpContextAccessor.HttpContext.Request.RouteValues.TryGetValue("cityCode", out object cityCode);

                if (!codeResult)
                {
                    throw new Exception("City not found!");
                }

                return factory.GetRepository(cityCode.ToString());
            }
            catch
            {
                throw new Exception("City not found!");
            }
        }

        private Task<long> GetCountRoutesByTypeAsync(VehicleType type)
        {
            var filters = GetFilters(type);

            return _repository.GetCollection<RouteEntity>()
                .Find(Builders<RouteEntity>.Filter.And(filters))
                .CountDocumentsAsync();
        }

        private Task<RouteEntity> GetLastUpdatedRouteByTypeAsync(VehicleType type)
        {
            var filters = GetFilters(type);

            return _repository.GetCollection<RouteEntity>()
                .Find(Builders<RouteEntity>.Filter.And(filters))
                .SortByDescending(x => x.UpdatedAt)
                .FirstOrDefaultAsync();
        }

        private List<FilterDefinition<RouteEntity>> GetFilters(VehicleType type)
        {
            var filters = new List<FilterDefinition<RouteEntity>> { Builders<RouteEntity>.Filter.Eq(x => x.Active, true) };

            filters.Add(Builders<RouteEntity>.Filter.Or(
                Builders<RouteEntity>.Filter.Eq(x => x.TempRoute, false),
                Builders<RouteEntity>.Filter.And(
                    Builders<RouteEntity>.Filter.Eq(x => x.TempRoute, true),
                    Builders<RouteEntity>.Filter.Lte(x => x.TempStart, DateTime.Now),
                    Builders<RouteEntity>.Filter.Gt(x => x.TempEnd, DateTime.Now)
            )));

            filters.Add(Builders<RouteEntity>.Filter.Eq(x => x.Type, type));

            return filters;
        }
    }
}
