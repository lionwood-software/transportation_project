using System;

namespace TransportApp.MainApi.Models.Route
{
    public class MessageForViewRoute
    {
        public string Id { get; set; }
        public DateTime StartDate { get; set; }
        public string MessageUA { get; set; }
        public string MessageEN { get; set; }
        public string MessageRU { get; set; }
    }
}
