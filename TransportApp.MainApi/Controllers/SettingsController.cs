using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransportApp.MainApi.Services;

namespace TransportApp.MainApi.Controllers
{
    [Route("api/v1/{cityCode}/settings")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly SettingsService _settingsService;
        private readonly ILogger _logger;

        public SettingsController(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _logger = Log.Logger;
        }

        [HttpGet("is-visible-suburban")]
        public async Task<IActionResult> GetIsVisibleSuburban()
        {
            try
            {
                var settings = await _settingsService.GetGlobalSettings();
                if (settings == null)
                {
                    return NotFound("Settings not found.");
                }

                return Ok(settings.IsVisibleSuburban);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}