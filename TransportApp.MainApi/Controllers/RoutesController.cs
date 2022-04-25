using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using CoordinateEntity = TransportApp.EntityModel.Coordinate;
using Type = TransportApp.EntityModel.VehicleType;
using TransportApp.MainApi.Models.Route;
using TransportApp.MainApi.Services;
using TransportApp.MainApi.Exceptions;
using TransportApp.EntityModel;
using System.Linq;
using System.Collections.Generic;
using TransportApp.MainApi.ApiClients;

namespace TransportApp.MainApi.Controllers
{
    [Route("api/v1/{cityCode}/routes")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly RouteService _routeService;
        private readonly RouteProcessingService _routeProcessingService;
        private readonly ILogger _logger;
        private readonly TramGpsApiClient _tramGpsApiClient;

        public RoutesController(RouteService routeService, RouteProcessingService routeProcessingService, TramGpsApiClient tramGpsApiClient)
        {
            _routeService = routeService;
            _routeProcessingService = routeProcessingService;
            _logger = Log.Logger;
            _tramGpsApiClient = tramGpsApiClient;
        }

        [HttpGet("gps")]
        public async Task<IActionResult> GetGpsAsync()
        {
            try
            {
                var data = await _tramGpsApiClient.GetAsync<GpsClient.Models.GpsData>("");

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            try
            {
                var route = await _routeService.GetByIdAsync<ViewRoute>(id);

                if (route == null)
                {
                    return NotFound("Route not found.");
                }

                return Ok(route);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/vehicles")]
        public async Task<IActionResult> GetDataByIdAsync(string id)
        {
            try
            {
                var route = await _routeService.GetRouteData(id);

                if (route == null)
                {
                    return NotFound("Route not found.");
                }

                return Ok(route);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{type}/{number}")]
        public async Task<IActionResult> GetByTypeAndNumberAsync(Type type, string number, BusType? busType = null)
        {
            try
            {
                var route = await _routeService.GetByTypeAndNumberAsync(type, number, busType);

                return Ok(route);
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

        [HttpGet("search")]
        public async Task<IActionResult> GetPosibleRoutes(double lat1, double lon1, double lat2, double lon2, bool includeSuburban = false)
        {
            try
            {
                var routes = await _routeProcessingService.SearchRoutesParallelAsync(new CoordinateEntity { Lat = lat1, Lon = lon1 }, new CoordinateEntity { Lat = lat2, Lon = lon2 }, includeSuburban);

                return Ok(routes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("details/{id}")]
        public IActionResult GetDetailsById(string id)
        {
            try
            {
                var route = _routeService.GetDetailsById(id);

                if (route == null)
                {
                    return NotFound("Route not found.");
                }

                return Ok(route);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("details/{id1},{id2}")]
        public IActionResult GetDetailsBy2Ids(string id1, string id2)
        {
            try
            {
                var routes = _routeService.GetDetailsByIds(new List<string>() { id1, id2 });

                if (!routes.Any())
                {
                    return NotFound("Routes not found.");
                }

                return Ok(routes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("details")]
        public IActionResult GetDetailsByIds([FromQuery] List<string> ids)
        {
            try
            {
                var routes = _routeService.GetDetailsByIds(ids);

                if (!routes.Any())
                {
                    return NotFound("Routes not found.");
                }

                return Ok(routes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
        {
            try
            {
                var routes = await _routeService.GetAllAsync(null, page, pageSize, null);

                return Ok(routes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetAllByType(Type type, int page = 1, int pageSize = 10, string routeNumber = null)
        {
            try
            {
                var routes = await _routeService.GetAllAsync(type, page, pageSize, routeNumber);

                return Ok(routes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("compact")]
        public async Task<IActionResult> GetAllCompact(int page = 1, int pageSize = 10)
        {
            try
            {
                var routes = await _routeService.GetCompactAsync(page, pageSize);

                return Ok(routes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{type}/compact")]
        public async Task<IActionResult> GetCompactByType(Type type, int page = 1, int pageSize = 10)
        {
            try
            {
                var routes = await _routeService.GetCompactAsync(page, pageSize, type);

                return Ok(routes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{type}/numbers")]
        public async Task<IActionResult> GetRouteNumbers(Type type)
        {
            try
            {
                var routeNumbers = await _routeService.GetRouteNumbersByTypeAsync(type);

                return Ok(routeNumbers);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{type}/{busType}/numbers")]
        public async Task<IActionResult> GetRouteNumbers(Type type, BusType busType)
        {
            try
            {
                var routeNumbers = await _routeService.GetRouteNumbersByTypeAndBusTypeAsync(type, busType);

                return Ok(routeNumbers);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("many")]
        public async Task<IActionResult> GetRoutesByIds([FromQuery] string[] ids)
        {
            try
            {
                var routes = await _routeService.GetAllRoutesByIdsAsync(ids);

                return Ok(routes);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}