namespace SampleApi.Plumbing.Configuration
{
    /*
     * Configuration related to claims caching in the API
     */
    public sealed class ClaimsCacheConfiguration
    {
        // The URL to the Authorization Server's user info endpoint, for user info claims
        public string UserInfoEndpoint { get; set; }

        // The maximum number of minutes for which to cache claims
        public int TimeToLiveMinutes { get; set; }
    }
}
