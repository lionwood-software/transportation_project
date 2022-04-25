using TransportApp.EntityModel;

namespace TransportApp.MainApi.Models.Route
{
    public class StopStationForViewRoute
    {
        public int Index { get; set; }
        public string Id { get; set; }
        public int Number { get; set; }
        public string NameUA { get; set; }
        public string NameEN { get; set; }
        public string NameRU { get; set; }
        public StopStationType Type { get; set; }
        public GeolocationForStopStationForViewRoute Geolocation { get; set; }
    }
}
