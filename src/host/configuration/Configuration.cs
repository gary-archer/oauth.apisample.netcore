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
            dynamic data = JsonConvert.DeserializeObject(text);

            return new Configuration
            {
                Api = data.api.ToObject<ApiConfiguration>(),
                Logging = data.logging.ToObject<LoggingConfiguration>(),
                OAuth = data.oauth.ToObject<OAuthConfiguration>()
            };
        }
    }
}