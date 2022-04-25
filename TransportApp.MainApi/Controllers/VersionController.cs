using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransportApp.MainApi.Models.Version;
using TransportApp.MainApi.Services;

namespace TransportApp.MainApi.Controllers
{
    [Route("api/v1/{cityCode}/version")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        private readonly VersionService _versionService;
        private readonly ILogger _logger;

        public VersionController(VersionService versionService)
        {
            _versionService = versionService;
            _logger = Log.Logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetVersion()
        {
            try
            {
                CurrentVersion result = await _versionService.GetCurrentVersionAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}