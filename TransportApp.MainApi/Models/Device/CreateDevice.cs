using System.ComponentModel.DataAnnotations;

namespace TransportApp.MainApi.Models.Device
{
    public class CreateDevice
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string Language { get; set; }
    }
}
