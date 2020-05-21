namespace SampleApi.Host.Configuration
{
    using System.IO;
    using Newtonsoft.Json;
    using SampleApi.Plumbing.Configuration;

    /*
     * A class to manage our JSON configuration as an object
     */
    public class Configuration
    {
        // The API's specific configuration
        public ApiConfiguration Api { get; private set; }

        // API logging configuration
        public LoggingConfiguration Logging { get; private set; }

        // OAuth configuration
        public OAuthConfiguration OAuth { get; private set; }

        /*
         * A utility method to load the file and deal with casing
         */
        public static Configuration Load(string filePath)
        {
            string text = File.ReadAllText("./api.config.json");
            dynamic data = JsonConvert.DeserializeObject(text);

            return new Configuration()
            {
                Api = data.api.ToObject<ApiConfiguration>(),
                Logging = data.logging.ToObject<LoggingConfiguration>(),
                OAuth = data.oauth.ToObject<OAuthConfiguration>(),
            };
        }
    }
}