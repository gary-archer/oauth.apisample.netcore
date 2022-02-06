namespace SampleApi.Plumbing.Configuration
{
    /*
     * Configuration settings to enable standard security and extensible use of claims
     */
    public sealed class OAuthConfiguration
    {
        // Certain behaviour may be triggered by a provider's capabilities
        public string Provider { get; set; }

        // The expected issuer in JWT access tokens received
        public string Issuer { get; set; }

        // The expected audience in JWT access tokens received
        public string Audience { get; set; }

        // The endpoint from which to download the token signing public key
        public string JwksEndpoint { get; set; }

        // The URL to the Authorization Server's user info endpoint, if needed
        public string UserInfoEndpoint { get; set; }

        // The maximum number of minutes for which to cache claims, when applicable
        public int ClaimsCacheTimeToLiveMinutes { get; set; }
    }
}
