using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransportApp.MainApi.Services;

namespace TransportApp.MainApi.Controllers
{
    [ApiController]
    [Route("api/v1/{cityCode}/messages")]
    public class MessagesController : ControllerBase
    {
        private readonly MessageService _messageService;
        private readonly ILogger _logger;

        public MessagesController(MessageService messageService)
        {
            _messageService = messageService;
            _logger = Log.Logger;
        }

        [HttpGet("general/{deviceId}")]
        public async Task<IActionResult> GetAllGeneral(string deviceId)
        {
            try
            {
                var allMessage = await _messageService.GetAllGeneralAsync(deviceId);

                return Ok(allMessage);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("general/count/{deviceId}")]
        public async Task<IActionResult> GetCount(string deviceId)
        {
            try
            {
                var countMessage = await _messageService.GetCountAsync(deviceId);

                return Ok(countMessage);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
} 