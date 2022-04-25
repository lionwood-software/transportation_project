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
    public class SavedService : BaseService<Device>
    {
        public SavedService(IMapper mapper, IHttpContextAccessor httpContextAccessor, RepositoryFactory factory) : base(mapper, httpContextAccessor, factory)
        {
        }

        public async Task<SaveRoute> GetSavedRoutesAsync(string deviceId)
        {
            var device = await _repository.GetCollection<Device>().Find(x => x.DeviceToken == deviceId).SingleOrDefaultAsync();
            var saveRoute = new SaveRoute()
            {
                DeviceID = device.DeviceToken,
                SaveRoutes = device.SavedRoutes
            };
            return saveRoute;
        }

        public async Task<string> AddSavedRouteAsync(string deviceId, string routeId)
        {
            var device = await _repository.GetCollection<Device>().Find(x => x.DeviceToken == deviceId).SingleOrDefaultAsync();
            if (device == null)
            {
                throw new NotFoundException("Device not found!");
            }

            if (!device.SavedRoutes.Any(x => x == routeId))
            {
                device.SavedRoutes.Add(routeId);
                _repository.GetCollection<Device>().ReplaceOne(x => x.Id == device.Id, device);
            }
            return device.Id;
        }

        public async Task DeleteSavedRouteAsync(string deviceId, string routeId)
        {
            var device = await _repository.GetCollection<Device>().Find(x => x.DeviceToken == deviceId).SingleOrDefaultAsync();
            if (device == null)
            {
                throw new NotFoundException("Device not found");
            }

            device.SavedRoutes.Remove(routeId);
            _repository.GetCollection<Device>().ReplaceOne(x => x.Id == device.Id, device);
        }
    }
}
