using System;

namespace TransportApp.MainApi.Models.CompactRoute
{
    public class MessageForCompactRoute
    {
        public DateTime StartDate { get; set; }
        public string MessageUA { get; set; }
        public string MessageEN { get; set; }
        public string MessageRU { get; set; }
    }
}
