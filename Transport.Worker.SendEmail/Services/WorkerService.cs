using MQ.ClientRealization;
using MQ.Core;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Threading;
using Transport.Worker.SendEmail.Models;
using TransportAPP.General.Interfaces;

namespace Transport.Worker.SendEmail.Services
{
    public class WorkerService : IRunService
    {
        public ISendEmailMQ MQClient { get; }
        public AutoResetEvent exitEvent = new AutoResetEvent(false);
        private readonly ISendService<SendEmailMessage> _sendEmailService;
        private readonly ILogger _logger;

        public WorkerService(ISendEmailMQ sendEmailMQ, ISendService<SendEmailMessage> sendEmailService)
        {
            _sendEmailService = sendEmailService;
            MQClient = sendEmailMQ;
            _logger = Log.Logger;
        }

        public void Run()
        {
            MQClient.Message += MQClient_MessageReceivedAsync;
            MQClient.Consume();
            Console.WriteLine("Worker.Send email has started");
            exitEvent.WaitOne();
        }

        private void MQClient_MessageReceivedAsync(object sender, MQMessage mQMessage)
        {
            try
            {
                var msg = JsonConvert.DeserializeObject<SendEmailMessage>(mQMessage.Message);
                _sendEmailService.Send(msg);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);
                throw ex;
            }
        }
    }
}
