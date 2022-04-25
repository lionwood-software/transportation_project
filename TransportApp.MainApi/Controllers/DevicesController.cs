using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransportApp.MainApi.Models.Device;
using TransportApp.MainApi.Services;

namespace TransportApp.MainApi.Controllers
{
    [ApiController]
    [Route("api/v1/{cityCode}/devices")]
    public class DevicesController : ControllerBase
    {
        private readonly DeviceService _deviceService;
        private readonly ILogger _logger;

        public DevicesController(DeviceService deviceService)
        {
            _deviceService = deviceService;
            _logger = Log.Logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUpdateDeviceAsync(CreateDevice device)
        {
            try
            {
                await _deviceService.CreateUpdateDeviceAsync(device);

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