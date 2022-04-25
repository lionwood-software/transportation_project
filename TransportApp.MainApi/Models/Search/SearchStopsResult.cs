using System.Collections.Generic;

namespace TransportApp.MainApi.Models.Search
{
    public class SearchStopsResult
    {
        public List<Stop> Places { get; set; }
        public List<Stop> Stops { get; set; }
    }

    public class Stop
    {
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}
