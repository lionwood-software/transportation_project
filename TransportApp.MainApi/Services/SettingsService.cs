using AutoMapper;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Threading.Tasks;
using TransportApp.EntityModel;
using TransportApp.MainApi.Factory;

namespace TransportApp.MainApi.Services
{
    public class SettingsService : BaseService<Setting>
    {
        public SettingsService(IMapper mapper, IHttpContextAccessor httpContextAccessor, RepositoryFactory factory) : base(mapper, httpContextAccessor, factory)
        {
        }

        public Task<Setting> GetGlobalSettings()
        {
            return _repository.GetCollection<Setting>()
                .Find(x => x.Id != null)
                .FirstOrDefaultAsync();
        }
    }
}
