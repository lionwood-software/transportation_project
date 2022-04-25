using System;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TransportApp.MainApi.Models.Contact;
using TransportApp.MainApi.Services;

namespace TransportApp.MainApi.Controllers
{
    [Route("api/v1/{cityCode}/contact")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly FeedbackService _feedbackService;
        private readonly ILogger _logger;

        public ContactController(FeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
            _logger = Log.Logger;
        }

        [HttpPost]
        public IActionResult SendEmail(ContactForm model)
        {
            try
            {
                _feedbackService.SendEmail(model.Email, model.Message);

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