namespace Framework.Api.Base.Configuration
{
    using Newtonsoft.Json.Linq;

    /*
     * Framework configuration settings
     */
    public class FrameworkConfiguration
    {
        // The name of the API
        public string ApiName { get; set; }

        // The logging configuration
        public JObject Logging { get; set; }
    }
}
