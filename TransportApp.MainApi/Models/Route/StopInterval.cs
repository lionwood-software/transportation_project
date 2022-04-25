using System.Collections.Generic;

namespace TransportApp.MainApi.Models.Route
{
    public class StopInterval
    {
        public List<WeekInterval> Forward { get; set; }
        public List<WeekInterval> Backward { get; set; }
    }
}
