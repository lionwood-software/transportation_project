using System.Collections.Generic;
using TransportApp.EntityModel;

namespace TransportApp.MainApi.Models.Route
{
    public class RouteInfoVM
    {
        public RouteInfoVM()
        {
            Vehicles = new List<VehicleInfoForRouteInfoVM>();
        }

        public long Timestamp { get; set; }
        public string RouteId { get; set; }
        public string Number { get; set; }
        public VehicleType VehicleType { get; set; }
        public bool Handicapped { get; set; }
        public List<VehicleInfoForRouteInfoVM> Vehicles { get; set; }
    }

    public class VehicleInfoForRouteInfoVM
    {
        public VehicleInfoForRouteInfoVM()
        {
            Normal = new List<Geolocation>();
        }

        public long Timestamp { get; set; }
        public int Speed { get; set; }
        public int Azimuth { get; set; }
        public DirectionType Direction { get; set; }
        public string Bort_number { get; set; }
        public bool Handicapped { get; set; }
        public bool IsSleep { get; set; }
        public bool Jump { get; set; }
        public List<Geolocation> Normal { get; set; }
    }
}
