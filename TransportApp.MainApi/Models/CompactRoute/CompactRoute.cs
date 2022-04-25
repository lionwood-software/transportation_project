using TransportApp.EntityModel;

namespace TransportApp.MainApi.Models.CompactRoute
{
    public class CompactRoute
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public VehicleType Type { get; set; }
        public BusType? BusType { get; set; }
        public MessageForCompactRoute Message { get; set; }
        public StopForCompactRoute StartStop { get; set; }
        public StopForCompactRoute EndStop { get; set; }
    }
}
