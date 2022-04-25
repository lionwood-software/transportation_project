using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransportApp.MainApi.Exceptions;
using TransportApp.MainApi.Models.StopStations;
using TransportApp.MainApi.Services;

namespace TransportApp.MainApi.Controllers
{
    [Route("api/v1/{cityCode}/stopstations")]
    [ApiController]
    public class StopStationsController : ControllerBase
    {
        private readonly StopStationService _stopStationService;
        private readonly ILogger _logger;

        public StopStationsController(StopStationService stopStationService)
        {
            _stopStationService = stopStationService;
            _logger = Log.Logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            try
            {
                var stopStation = await _stopStationService.GetByIdAsync<ViewStopStation>(id);

                if (stopStation == null)
                {
                    return NotFound("StopStation not found.");
                }

                return Ok(stopStation);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("{id}/routes")]
        public async Task<IActionResult> GetRoutesByIdStopAsync(string id)
        {
            try
            {
                var stop = await _stopStationService.GetRoutesByIdStopAsync(id);
                return Ok(stop);
            }
            catch (NotFoundException ex)
            {
                _logger.Debug(ex, ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var stopStations = _stopStationService.GetAll<ViewStopStation>();

                return Ok(stopStations);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("many")]
        public async Task<IActionResult> GetStopsIds([FromQuery]string[] ids)
        {
            try
            {
                var stops = await _stopStationService.GetAllStopsByIdsAsync(ids);

                return Ok(stops);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}