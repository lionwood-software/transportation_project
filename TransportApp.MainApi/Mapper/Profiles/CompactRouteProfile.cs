using AutoMapper;
using System.Linq;
using TransportApp.EntityModel;
using TransportApp.MainApi.Models.CompactRoute;

namespace TransportApp.MainApi.Mapper.Profiles
{
    public class CompactRouteProfile : Profile
    {
        public CompactRouteProfile()
        {
            CreateMap<Route, CompactRoute>()
                .AfterMap((src, dest) =>
                {
                    if (src.Forward.Stops.Any())
                    {
                        var startStop = src.Forward.Stops.First();
                        dest.StartStop = new StopForCompactRoute
                        {
                            Number = startStop.Number,
                            NameEN = startStop.NameEN,
                            NameRU = startStop.NameRU,
                            NameUA = startStop.NameUA
                        };

                        var endStop = src.Forward.Stops.Last();
                        dest.EndStop = new StopForCompactRoute
                        {
                            Number = endStop.Number,
                            NameEN = endStop.NameEN,
                            NameRU = endStop.NameRU,
                            NameUA = endStop.NameUA
                        };
                    }
                });
            CreateMap<Message, MessageForCompactRoute>();
        }
    }
}
