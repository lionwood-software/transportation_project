using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransportApp.MainApi.Exceptions;
using TransportApp.MainApi.Services;

namespace TransportApp.MainApi.Controllers
{
    [ApiController]
    [Route("api/v1/{cityCode}/saved")]
    public class SavedController : ControllerBase
    {
        private readonly SavedService _savedService;
        private readonly ILogger _logger;
        public SavedController(SavedService savedService)
        {
            throw new Exception("Error");
            _savedService = savedService;
            _logger = Log.Logger;
        }

        [HttpGet("{deviceId}/routes")]
        public async Task<IActionResult> GetSavedRoutes(string deviceID)
        {
            try
            {
                var saveRoute = await _savedService.GetSavedRoutesAsync(deviceID);
                return Ok(saveRoute);
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

        [HttpPost("{deviceId}/routes")]
        public async Task<IActionResult> AddSavedRoute(string deviceID, string routeID)
        {
            try
            {
                var routeId = await _savedService.AddSavedRouteAsync(deviceID, routeID);
                return Ok(routeId);
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

        [HttpDelete("{deviceId}/routes/{routeId}")]
        public async Task<IActionResult> DeleteSavedRoute(string deviceID, string routeID)
        {
            try
            {
                await _savedService.DeleteSavedRouteAsync(deviceID, routeID);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}