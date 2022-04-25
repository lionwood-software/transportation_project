using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Linq;
using System.Threading.Tasks;
using TransportApp.EntityModel;
using TransportApp.MainApi.Exceptions;
using TransportApp.MainApi.Factory;
using TransportApp.MainApi.Models.SaveFavorite;

namespace TransportApp.MainApi.Services
{
    public class FavouritesService : BaseService<Device>
    {
        public FavouritesService(IMapper mapper, IHttpContextAccessor httpContextAccessor, RepositoryFactory factory) : base(mapper, httpContextAccessor, factory)
        {
        }

        public async Task<FavoriteStop> GetFavouriteStopsAsync(string deviceId)
        {
            var saveFavorite = await _repository.GetCollection<Device>().Find(x => x.DeviceToken == deviceId).SingleOrDefaultAsync();
            var favoriteRoutes = new FavoriteStop()
            {
                DeviceID = saveFavorite.DeviceToken,
                FavoriteStops = saveFavorite.FavouriteStops
            };
            return favoriteRoutes;
        }

        public async Task<FavoriteRoute> GetFavouritesRoutesAsync(string deviceId)
        {
            var saveFavorite = await _repository.GetCollection<Device>().Find(x => x.DeviceToken == deviceId).SingleOrDefaultAsync();
            var favoriteRoutes = new FavoriteRoute()
            {
                DeviceID = saveFavorite.DeviceToken,
                FavoriteRoutes = saveFavorite.FavouriteRoutes
            };
            return favoriteRoutes;
        }

        public async Task<string> AddFavouriteStopAsync(string deviceId, string stopId)
        {
            var device = await _repository.GetCollection<Device>().Find(x => x.DeviceToken == deviceId).SingleOrDefaultAsync();
            if (device == null)
            {
                throw new NotFoundException("Device not found!");
            }

            if (!device.FavouriteStops.Any(x => x == stopId))
            {
                device.FavouriteStops.Add(stopId);
                _repository.GetCollection<Device>().ReplaceOne(x => x.Id == device.Id, device);
            }

            return device.Id;
        }

        public async Task<string> AddFavouriteRouteAsync(string deviceId, string routeId)
        {
            var device = await _repository.GetCollection<Device>().Find(x => x.DeviceToken == deviceId).SingleOrDefaultAsync();
            if (device == null)
            {
                throw new NotFoundException("Device not found!");
            }

            if (!device.FavouriteRoutes.Any(x => x == routeId))
            {
                device.FavouriteRoutes.Add(routeId);
                _repository.GetCollection<Device>().ReplaceOne(x => x.Id == device.Id, device);
            }

            return device.Id;
        }

        public async Task DeleteFavouriteRouteAsync(string deviceId, string routeId)
        {
            var device = await _repository.GetCollection<Device>().Find(x => x.DeviceToken == deviceId).SingleOrDefaultAsync();
            if (device == null)
            {
                throw new NotFoundException("Device not found");
            }
            else
            {
                device.FavouriteRoutes.Remove(routeId);
                _repository.GetCollection<Device>().ReplaceOne(x => x.Id == device.Id, device);
            }
        }

        public async Task DeleteFavouriteStopAsync(string deviceId, string stopId)
        {
            var device = await _repository.GetCollection<Device>().Find(x => x.DeviceToken == deviceId).SingleOrDefaultAsync();
            if (device == null)
            {
                throw new NotFoundException("Device not found");
            }

            device.FavouriteStops.Remove(stopId);
            _repository.GetCollection<Device>().ReplaceOne(x => x.Id == device.Id, device);
        }
    }
}
