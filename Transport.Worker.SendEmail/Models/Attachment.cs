namespace Transport.Worker.SendEmail.Models
{
    public class Attachment
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string ContentBase64String { get; set; }
    }
}
