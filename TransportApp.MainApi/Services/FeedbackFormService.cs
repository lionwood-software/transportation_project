using Microsoft.Extensions.Options;
using MQ.ClientRealization;
using Serilog;
using System;
using System.Collections.Generic;

namespace TransportApp.MainApi.Services
{
    public class FeedbackService
    {
        private readonly ILogger _logger;

        private readonly ISendEmailMQ _sendEmailMQ;
        private string _emailForFeedback { get; set; }

        public FeedbackService(ISendEmailMQ sendEmailMQ, IOptions<ApiConfigurationOptions> option)
        {
            _sendEmailMQ = sendEmailMQ;
            _logger = Log.Logger;
            _emailForFeedback = option?.Value.FeedbackEmail ?? throw new ArgumentNullException("Email for contact form does not exist!");
        }

        public void SendEmail(string email, string message)
        {
            _sendEmailMQ.Publish(new SendMessageModel
            {
                EmailReceivers = new List<string>() { _emailForFeedback },
                Subject = "Feedback icity",
                Message = $"Email:{email} <br> {message}",
                IsBodyHtml = true
            });
        }
    }
}
