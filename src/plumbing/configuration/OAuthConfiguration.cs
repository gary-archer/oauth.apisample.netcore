namespace SampleApi.Plumbing.Configuration
{
    /*
     * Configuration settings to enable standard security and extensible use of claims
     */
    public sealed class OAuthConfiguration
    {
        // The OAuth strategy to use, either 'standard' or 'claims-caching'
    public string Strategy;

    // The expected issuer of access tokens
    public string Issuer;

    // The expected audience of access tokens
    public string Audience;

    // The strategy for validating access tokens, either 'jwt' or 'introspection'
    public string TokenValidationStrategy;

    // The endpoint from which to download the token signing public key, when validating JWTs
    public string JwksEndpoint;

    // The endpoint for token introspection
    public string IntrospectEndpoint;

    // The client id with which to call the introspection endpoint
    public string IntrospectClientId;

    // The client secret with which to call the introspection endpoint
    public string IntrospectClientSecret;

    // The URL to the Authorization Server's user info endpoint, which could be an internal URL
    // This is used with the claims caching strategy, when we need to look up user info claims
    public string UserInfoEndpoint;

    // The maximum number of minutes for which to cache claims, when using the claims caching strategy
    public int ClaimsCacheTimeToLiveMinutes;
}
