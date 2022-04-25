using TransportApp.EntityModel;

namespace TransportApp.MainApi.Models.StopStations
{
    public class TransferStations
    {
        public StopStation StartStopStation { get; set; }
        public StopStation EndStopStation { get; set; }
        public double Distance { get; set; }
    }
}
