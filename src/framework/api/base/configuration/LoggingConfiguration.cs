namespace Framework.Api.Base.Configuration
{
    using Newtonsoft.Json.Linq;

    /*
     * Framework configuration settings
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
