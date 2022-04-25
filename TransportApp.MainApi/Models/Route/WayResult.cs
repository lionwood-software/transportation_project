using System.Collections.Generic;
using StopStation = TransportApp.EntityModel.StopStation;
using Geolocation = TransportApp.EntityModel.Geolocation;

namespace TransportApp.MainApi.Models.Route
{
    public enum Type
    {
        Tram,
        Trolleybus,
        Bus,
        RouteTaxi,
        Start,
        Transfer,
        End
    }

    public class WayResult
    {
        public List<Way> Ways { get; set; } = new List<Way>();
        public double TotalPrice { get; set; }
        public double TotalDistance { get; set; }
        public double TotalOnFootDistance { get; set; }
        public double TotalOnRouteDistance { get; set; }
        public double TotalTime { get; set; }
        public double Interval { get; set; }
        public string RouteIdFirst { get; set; }
        public string RouteIdSecond { get; set; }
    }

    public abstract class Way
    {
        public double Distance { get; set; }
        public int Time { get; set; }
        public Type Type { get; set; }
    }

    public class FootWay : Way
    {
        public Geolocation StartGeolocation { get; set; }
        public Geolocation EndGeolocation { get; set; }
    }

    public class RouteWay : Way
    {
        public string RouteId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public bool Handicapped { get; set; }
        public List<Geolocation> Geolocations { get; set; } = new List<Geolocation>();
        public List<StopStation> StopStations { get; set; } = new List<StopStation>();
    }
}