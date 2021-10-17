namespace SampleApi.Plumbing.Claims
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using IdentityModel;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Errors;

    /*
     * Claims received in the access token
     */
    public class BaseClaims
    {
        /*
         * Receive a claims principal after processing the access token
         */
        public BaseClaims(ClaimsPrincipal principal)
        {
            // Store claims
            this.Subject = principal.GetClaim(JwtClaimTypes.Subject);
            this.Scopes = principal.GetClaim(JwtClaimTypes.Scope).Split(' ');
            this.Expiry = Convert.ToInt32(principal.GetClaim(JwtClaimTypes.Expiration), CultureInfo.InvariantCulture);

            // Also store the principal, which in some cases is the JWT validation result
            this.Principal = principal;
        }

        /*
         * Receive individual claims when getting claims from the cache
         */
        public BaseClaims(string subject, string[] scopes, int expiry)
        {
            // Store claims
            this.Subject = subject;
            this.Scopes = scopes;
            this.Expiry = expiry;

            // Create a claims principal manually
            var claimsList = new List<Claim>();
            claimsList.Add(new Claim(JwtClaimTypes.Subject, this.Subject));
            var identity = new ClaimsIdentity(claimsList, "Bearer", JwtClaimTypes.Subject, string.Empty);
            this.Principal = new ClaimsPrincipal(identity);
        }

        public ClaimsPrincipal Principal { get; private set; }

        public string Subject { get; private set; }

        public string[] Scopes { get; private set; }

        public int Expiry { get; private set; }

        /*
         * Called when claims are deserialized during claims caching
         */
        public static BaseClaims ImportData(JObject data)
        {
            var subject = data.GetValue(JwtClaimTypes.Subject).Value<string>();
            var scope = data.GetValue(JwtClaimTypes.Scope).Value<string>();
            var expiry = data.GetValue(JwtClaimTypes.Expiration).Value<int>();
            return new BaseClaims(subject, scope.Split(" "), expiry);
        }

        /*
         * Called when claims are serialized during claims caching
         */
        public JObject ExportData()
        {
            dynamic data = new JObject();
            data.sub = this.Subject;
            data.scope = string.Join(" ", this.Scopes);
            data.exp = this.Expiry;
            return data;
        }

        /*
        * Make sure the token has the correct scope for an area of data
        */
        public void VerifyScope(string scope)
        {
            if (!this.Scopes.ToList().Exists((s) => s.Contains(scope)))
            {
                throw ErrorFactory.CreateClientError(
                    HttpStatusCode.Forbidden,
                    ErrorCodes.InsufficientScope,
                    "Access token does not have a valid scope for this API");
            }
        }
    }
}
