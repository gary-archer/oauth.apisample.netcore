namespace SampleApi.Plumbing.Configuration
{
    /*
     * Configuration settings to enable standard security and extensible use of claims
     */
    public sealed class OAuthConfiguration
    {
        // The OAuth strategy to use, either 'standard' or 'claims-caching'
        public string Strategy { get; set; }

        // The expected issuer of access tokens
        public string Issuer { get; set; }

        // The expected audience of access tokens
        public string Audience { get; set; }

        // The strategy for validating access tokens, either 'jwt' or 'introspection'
        public string TokenValidationStrategy { get; set; }

        // The endpoint from which to download the token signing public key, when validating JWTs
        public string JwksEndpoint { get; set; }

        // The endpoint for token introspection
        public string IntrospectEndpoint { get; set; }

        // The client id with which to call the introspection endpoint
        public string IntrospectClientId { get; set; }

        // The client secret with which to call the introspection endpoint
        public string IntrospectClientSecret { get; set; }

        // The URL to the Authorization Server's user info endpoint, which could be an internal URL
        // This is used with the claims caching strategy, when we need to look up user info claims
        public string UserInfoEndpoint { get; set; }

        // The maximum number of minutes for which to cache claims, when using the claims caching strategy
        public int ClaimsCacheTimeToLiveMinutes { get; set; }
    }
}
