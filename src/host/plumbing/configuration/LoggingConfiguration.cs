namespace SampleApi.Host.Plumbing.Configuration
{
    using Newtonsoft.Json.Linq;

    /*
     * Logging configuration settings
     */
    public sealed class LoggingConfiguration
    {
        // The name of the API
        public string ApiName { get; set; }

        // The production configuration
        public JObject Production { get; set; }

        // The development configuration
        public JObject Development { get; set; }
    }
}
