using Configuration.Core;

namespace Transport.Worker.PushNotification
{
    public class WorkerConfigurationOptions : ConfigurationOptions
    {
        public string TitleMessage { get; set; }
        public string JsonConfig { get; set; }
    }
}
