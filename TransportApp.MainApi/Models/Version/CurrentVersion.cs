using System;

namespace TransportApp.MainApi.Models.Version
{
    public class CurrentVersion
    {
        public VersionVehicleType Tram { get; set; }
        public VersionVehicleType Trol { get; set; }
        public VersionVehicleType Bus { get; set; }
        public VersionVehicleType Metro { get; set; }
    }

    public class VersionVehicleType
    {
        public long Count { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
