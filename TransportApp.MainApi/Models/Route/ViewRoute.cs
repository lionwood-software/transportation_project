using System;
using TransportApp.EntityModel;

namespace TransportApp.MainApi.Models.Route
{
    public class ViewRoute
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public VehicleType Type { get; set; }
        public BusType? BusType { get; set; }
        public double CostMoving { get; set; }
        public MessageForViewRoute Message { get; set; }
        public bool TempRoute { get; set; }
        public DateTime StartHour { get; set; }
        public DateTime EndHour { get; set; }
        public bool GPSStatus { get; set; }
        public RouteStopStationForViewRoute Forward { get; set; }
        public RouteStopStationForViewRoute Backward { get; set; }
        public CarrierForViewRoute Carrier { get; set; }
        public double Length { get; set; }
    }
}
