using System.Collections.Generic;

namespace Transport.Worker.SendEmail.Models
{
    public class SendEmailMessage
    {
        public string Subject { get; set; }
        public string Message{ get; set; }
        public List<Attachment> Attachments { get; set; }
        public List<string> EmailReceivers{ get; set; }
        public bool IsBodyHtml { get; set; }
    }
}
