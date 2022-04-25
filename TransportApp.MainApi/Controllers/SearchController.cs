using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransportApp.MainApi.Services;

namespace TransportApp.MainApi.Controllers
{
    [Route("api/v1/{cityCode}/search")]
    [ApiController]
    public class SearchController : Controller
    {
        private readonly SearchService _service;
        private readonly ILogger _logger;

        public SearchController(SearchService service)
        {
            _service = service;
            _logger = Log.Logger;
        }

        [HttpGet("stops")]
        public async Task<IActionResult> FindStops(string query, string lang = null)
        {
            try
            {
                var stops = await _service.FindStopsAsync(query, lang);

                return Ok(stops);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("address/{lat},{lon}")]
        public async Task<IActionResult> FindAddressByLocation(double lat, double lon, string lang = null)
        {
            try
            {
                var stopStations = await _service.FindAddressByLocationAsync(lat, lon, lang);

                return Ok(stopStations);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}