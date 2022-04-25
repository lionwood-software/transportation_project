using TransportApp.EntityModel;

namespace TransportApp.MainApi.Models.StopStations
{
    public class ViewStopStation
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public string NameUA { get; set; }
        public string NameEN { get; set; }
        public string NameRU { get; set; }
        public StopStationType Type { get; set; }
        public GeolocationForViewStopStation Geolocation { get; set; }
    }
}
