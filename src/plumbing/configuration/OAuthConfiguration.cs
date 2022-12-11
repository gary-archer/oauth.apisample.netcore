namespace SampleApi.Plumbing.Configuration
{
    /*
     * Configuration settings to enable standard security and extensible use of claims
     */
    public sealed class OAuthConfiguration
    {
        // The expected issuer in JWT access tokens received
        public string Issuer { get; set; }

        // The expected audience in JWT access tokens received
        public string Audience { get; set; }

        // The endpoint from which to download the token signing public key
        public string JwksEndpoint { get; set; }

        // The strategy for domain specific claims, either 'jwt' or 'apiLookup'
        public string ClaimsStrategy { get; set; }

        // Optional claims caching configuration
        public ClaimsCacheConfiguration ClaimsCache { get; set; }
    }
}
