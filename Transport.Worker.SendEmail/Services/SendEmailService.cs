using Configuration.Core.SendEmail;
using System;
using System.IO;
using System.Linq;
using System.Net;
using Transport.Worker.SendEmail.Models;
using TransportAPP.General.Interfaces;
using MailAttachment = System.Net.Mail.Attachment;

namespace Transport.Worker.SendEmail.Services
{
    public class SendEmailService : ISendService<SendEmailMessage>
    {
        private readonly EmailCredential _emailCredentialConfigs;

        public SendEmailService(EmailCredential option)
        {
            _emailCredentialConfigs = option;
        }

        public void Send(SendEmailMessage msg)
        {
            var streamList = new System.Collections.Generic.List<Stream>();
            try
            {
                using var smtpClient = new System.Net.Mail.SmtpClient(_emailCredentialConfigs.Host, _emailCredentialConfigs.Port)
                {
                    Credentials = new NetworkCredential(_emailCredentialConfigs.Address, _emailCredentialConfigs.Password),
                    EnableSsl = true
                };
                var message = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(_emailCredentialConfigs.Address),
                    Subject = msg.Subject,
                    Body = msg.Message,
                    IsBodyHtml = msg.IsBodyHtml
                };

                foreach (var item in msg.EmailReceivers)
                {
                    message.To.Add(new System.Net.Mail.MailAddress(item));
                }

                if (msg.Attachments != null && msg.Attachments.Any())
                {
                    foreach (var attachment in msg.Attachments)
                    {
                        byte[] file = Convert.FromBase64String(attachment.ContentBase64String);
                        var memoryStream = new MemoryStream(file);
                        memoryStream.Position = 0;
                        streamList.Add(memoryStream);
                        
                        message.Attachments.Add(new MailAttachment(memoryStream, attachment.FileName, attachment.ContentType));
                    }
                }

                smtpClient.Send(message);
            }
            finally
            {
                // dispose stream for each attachment
                foreach (var stream in streamList)
                {
                    stream.Dispose();
                }
            }
        }
    }
}
