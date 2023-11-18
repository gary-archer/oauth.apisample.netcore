namespace SampleApi.Host.Configuration
{
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using SampleApi.Plumbing.Configuration;

    /*
     * A class to manage our JSON configuration as an object
     */
    public class Configuration
    {
        // The API's specific configuration
        public ApiConfiguration Api { get; private set; }

        // Common logging
        public LoggingConfiguration Logging { get; private set; }

        // Common OAuth processing
        public OAuthConfiguration OAuth { get; private set; }

        /*
         * A utility method to load the file and deal with casing
         */
        public static Configuration Load(string filePath)
        {
            string text = File.ReadAllText(filePath);
            var data = JsonNode.Parse(text);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            return new Configuration
            {
                Api = data["api"].Deserialize<ApiConfiguration>(options),
                Logging = data["logging"].Deserialize<LoggingConfiguration>(options),
                OAuth = data["oauth"].Deserialize<OAuthConfiguration>(options),
            };
        }
    }
}
