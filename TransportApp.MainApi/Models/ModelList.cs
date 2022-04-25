using System.Collections.Generic;

namespace TransportApp.MainApi.Models
{
    public class ModelList<T>
    {
        public int PageCount { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public List<T> Data { get; set; }
    }
}
