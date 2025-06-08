namespace FinalApi.Plumbing.Configuration
{
    using System.Text.Json.Nodes;

    /*
     * Logging configuration settings
     */
    public sealed class LoggingConfiguration
    {
        // The name of the API
        public string ApiName { get; set; }

        // The production configuration
        public JsonNode Production { get; set; }

        // The development configuration
        public JsonNode Development { get; set; }
    }
}
