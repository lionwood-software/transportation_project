using System;
using System.Collections.Generic;
using TransportApp.EntityModel;

namespace TransportApp.MainApi.Models.StopStations
{
    public class ViewStopWithRoutes
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public string NameUA { get; set; }
        public string NameEN { get; set; }
        public string NameRU { get; set; }
        public StopStationType Type { get; set; }
        public List<RouteForViewStopWithRoutes> Routes { get; set; } = new List<RouteForViewStopWithRoutes>();
    }

    public class RouteForViewStopWithRoutes
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public VehicleType VehicleType { get; set; }
        public BusType? BusType { get; set; }
        public DateTime? NextETA { get; set; }
        public DateTime? SecondETA { get; set; }
        public ViewStopStation LastStopStation { get; set; }
    }
}
