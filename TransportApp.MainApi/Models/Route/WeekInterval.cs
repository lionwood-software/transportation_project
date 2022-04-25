using System;
using System.Collections.Generic;

namespace TransportApp.MainApi.Models.Route
{
    public class WeekInterval
    {
        public string StopId { get; set; }
        public List<DateTime> Weekday { get; set; }
        public List<DateTime> Weekend { get; set; }
    }
}
