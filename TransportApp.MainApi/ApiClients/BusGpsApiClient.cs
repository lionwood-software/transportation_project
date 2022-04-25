using System.Net.Http;
using TransportApp.GpsClient;

namespace TransportApp.MainApi.ApiClients
{
    public class BusGpsApiClient : GpsApiClient
    {
        public BusGpsApiClient(IHttpClientFactory httpClientFactory, string baseUrl) : base(httpClientFactory, baseUrl)
        {
        }
    }
}
