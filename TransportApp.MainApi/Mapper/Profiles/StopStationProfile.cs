using AutoMapper;
using TransportApp.EntityModel;
using TransportApp.MainApi.Models.StopStations;

namespace TransportApp.MainApi.Mapper.Profiles
{
    public class StopStationProfile : Profile
    {
        public StopStationProfile()
        {
            CreateMap<StopStation, ViewStopStation>();
            CreateMap<Geolocation, GeolocationForViewStopStation>();
        }
    }
}
