using System.Net.Http;
using TransportApp.GpsClient;

namespace TransportApp.MainApi.ApiClients
{
    public class TramGpsApiClient : GpsApiClient
    {
        public TramGpsApiClient(IHttpClientFactory httpClientFactory, string baseUrl) : base(httpClientFactory, baseUrl)
        {
        }
    }
}
