namespace TransportApp.MainApi.Models.Route
{
    public class GeolocationForViewRoute
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class GeolocationForStopStationForViewRoute
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
