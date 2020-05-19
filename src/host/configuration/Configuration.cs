namespace SampleApi.Host.Configuration
{
    using System.IO;
    using Framework.Api.Base.Configuration;
    using Framework.Api.OAuth.Configuration;
    using Newtonsoft.Json;

    /*
     * A class to manage our JSON configuration as an object
     */
    public class Configuration
    {
        // The API's own configuration
        public ApiConfiguration Api { get; private set; }

        // The API base framework configuration
        public LoggingConfiguration Logging { get; private set; }

        // The API OAuth framework configuration
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