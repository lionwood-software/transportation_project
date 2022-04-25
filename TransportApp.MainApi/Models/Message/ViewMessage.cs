using System;

namespace TransportApp.MainApi.Models.Message
{
    public class ViewMessage
    {
        public string Id { get; set; }
        public DateTime StartDate { get; set; }
        public string MessageUA { get; set; }
        public string MessageEN { get; set; }
        public string MessageRU { get; set; }
        public bool IsReaden { get; set; }
    }
}