namespace Framework.Configuration
{
    /// <summary>
    /// Framework specific OAuth settings
    /// </summary>
    public sealed class OAuthConfiguration
    {
        public string Authority {get; set;}

        public string ClientId {get; set;}

        public string ClientSecret {get; set;}

        public int DefaultTokenCacheMinutes {get; set;}
    }
}
