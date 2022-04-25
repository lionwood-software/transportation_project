using TransportApp.EntityModel;

namespace TransportApp.MainApi.Models.Route
{
    public class ViewRouteNumbersWithBusType
    {
        public string Number { get; set; }
        public BusType? Type { get; set; }
    }
}
