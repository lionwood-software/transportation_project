using System.Collections.Generic;

namespace TransportApp.MainApi.Models.Route
{
    public class RouteStopStationForViewRoute
    {
        public string StopsId { get; set; }
        public string PathId { get; set; }
        public List<StopStationForViewRoute> Stops { get; set; }
        public List<GeolocationForViewRoute> Geolocation { get; set; }
    }
}
