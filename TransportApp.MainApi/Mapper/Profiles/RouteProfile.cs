using AutoMapper;
using TransportApp.EntityModel;
using TransportApp.MainApi.Models.Route;
using TransportApp.EntityModel.CacheEntity;

namespace TransportApp.MainApi.Mapper.Profiles
{
    public class RouteProfile : Profile
    {
        public RouteProfile()
        {
            CreateMap<Route, ViewRoute>();
            CreateMap<Message, MessageForViewRoute>();
            CreateMap<RouteStopStation, RouteStopStationForViewRoute>();
            CreateMap<StopStation, StopStationForViewRoute>();
            CreateMap<Geolocation, GeolocationForStopStationForViewRoute>();
            CreateMap<Geolocation, GeolocationForViewRoute>();
            CreateMap<Carrier, CarrierForViewRoute>();

            CreateMap<Models.Route.StopInterval, EntityModel.StopInterval>().ReverseMap();
            CreateMap<Models.Route.WeekInterval, EntityModel.WeekInterval>().ReverseMap();

            // cached entities
            CreateMap<RouteInfo, RouteInfoVM>();
            CreateMap<VehicleInfo, VehicleInfoForRouteInfoVM>()
                .ForMember(dest => dest.Azimuth, src => src.MapFrom(i => i.AzimuthFirst))
                .ForMember(dest => dest.Timestamp, src => src.MapFrom(i => i.GpsTimestampFirst));
        }
    }
}