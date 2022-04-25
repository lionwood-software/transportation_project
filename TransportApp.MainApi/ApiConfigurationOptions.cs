using Configuration.Core;

namespace TransportApp.MainApi
{
    public class ApiConfigurationOptions : ConfigurationOptions
    {
        public string FeedbackEmail { get; set; }
        public int AverageSecondsOnStop { get; set; }
    }
}
