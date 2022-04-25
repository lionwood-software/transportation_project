using TransportApp.EntityModel;

namespace TransportApp.MainApi.Models.StopStations
{
    public class StopStationWithDistance
    {
        public StopStation ProcessStopStation { get; set; }
        public double Distance { get; set; }
    }
}
