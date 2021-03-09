namespace SampleApi.Plumbing.OAuth
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.Logging;
    using SampleApi.Plumbing.Security;

    /*
     * The technology neutral algorithm for validating access tokens and returning claims
     */
    internal sealed class OAuthAuthorizer : IAuthorizer
    {
        private readonly ClaimsCache cache;
        private readonly OAuthAuthenticator authenticator;
        private readonly CustomClaimsProvider customClaimsProvider;
        private readonly LogEntry logEntry;

        public OAuthAuthorizer(
            ClaimsCache cache,
            OAuthAuthenticator authenticator,
            CustomClaimsProvider customClaimsProvider,
            ILogEntry logEntry)
        {
            this.cache = cache;
            this.authenticator = authenticator;
            this.customClaimsProvider = customClaimsProvider;
            this.logEntry = (LogEntry)logEntry;
        }

        /*
         * The entry point to populate claims from an access token
         */
        public async Task<ApiClaims> ExecuteAsync(HttpRequest request)
        {
            // First handle missing tokens
            var accessToken = this.ReadAccessToken(request);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw ErrorFactory.CreateClient401Error("No access token was received in the bearer header");
            }

            // If cached results already exist for this token then return them immediately
            var accessTokenHash = this.Sha256(accessToken);
            var cachedClaims = await this.cache.GetClaimsForTokenAsync(accessTokenHash);
            if (cachedClaims != null)
            {
                return cachedClaims;
            }

            // Create a child log entry for authentication related work
            // This ensures that any errors and performances in this area are reported separately to business logic
            var authorizationLogEntry = this.logEntry.CreateChild("Authorizer");

            // Validate the token, read token claims, and do a user info lookup
            var token = await this.authenticator.ValidateTokenAsync(accessToken);

            // Get OAuth user info
            var userInfo = await this.authenticator.GetUserInfoAsync(accessToken);

            // Add custom claims from the API's own data if needed
            var custom = await this.customClaimsProvider.GetCustomClaimsAsync(token, userInfo);

            // Cache the claims against the token hash until the token's expiry time
            var claims = new ApiClaims(token, userInfo, custom);
            await this.cache.AddClaimsForTokenAsync(accessTokenHash, claims);

            // Finish logging here, and note that on exception our logging disposes the child
            authorizationLogEntry.Dispose();
            return claims;
        }

        /*
         * Try to read the bearer token from the authorization header
         */
        private string ReadAccessToken(HttpRequest request)
        {
            string authorization = request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var parts = authorization.Split(' ');
                if (parts.Length == 2 && parts[0] == "Bearer")
                {
                    return parts[1];
                }
            }

            return null;
        }

        /*
         * Get the hash of an input string
         */
        private string Sha256(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
