using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransportApp.MainApi.Exceptions;
using TransportApp.MainApi.Services;

namespace TransportApp.MainApi.Controllers
{
    [ApiController]
    [Route("api/v1/{cityCode}/favourites")]
    public class FavouritesController : ControllerBase
    {
        private readonly FavouritesService _favoritesService;
        private readonly ILogger _logger;
        public FavouritesController(FavouritesService favoritesService)
        {
            throw new Exception("Error");
            _favoritesService = favoritesService;
            _logger = Log.Logger;
        }

        #region routes
        [HttpGet("{deviceId}/routes")]
        public async Task<IActionResult> GetFavoriteRoutes(string deviceID)
        {
            try
            {
                var favoriteRoute = await _favoritesService.GetFavouritesRoutesAsync(deviceID);
                return Ok(favoriteRoute);
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
        public async Task<IActionResult> AddFavoriteRoute(string deviceID, string routeID)
        {
            try
            {
                var routeId = await _favoritesService.AddFavouriteRouteAsync(deviceID, routeID);
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
        public async Task<IActionResult> DeleteFavoriteRoute(string deviceID, string routeID)
        {
            try
            {
                await _favoritesService.DeleteFavouriteRouteAsync(deviceID, routeID);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region stops
        [HttpGet("{deviceId}/stops")]
        public async Task<IActionResult> GetFavoriteStops(string deviceID)
        {
            try
            {
                var favoriteStop = await _favoritesService.GetFavouriteStopsAsync(deviceID);
                return Ok(favoriteStop);
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

        [HttpPost("{deviceId}/stops")]
        public async Task<IActionResult> AddFavoriteStop(string deviceID, string stopID)
        {
            try
            {
                var stopid = await _favoritesService.AddFavouriteStopAsync(deviceID, stopID);
                return Ok(stopid);
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

        [HttpDelete("{deviceId}/stops/{stopId}")]
        public async Task<IActionResult> DeleteFavoriteStop(string deviceID, string stopID)
        {
            try
            {
                await _favoritesService.DeleteFavouriteStopAsync(deviceID, stopID);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}