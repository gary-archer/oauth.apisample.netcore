namespace BasicApi.Plumbing.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using IdentityModel;
    using Microsoft.AspNetCore.Http;
    using BasicApi.Plumbing.OAuth;

    /*
     * A helper class to map between a .Net claims principal and our plain claims object
     */
    public static class ClaimsMapper
    {
        /*
         * A product specific claim for which there is no standard constant
         */
        public const string CustomClaimAccountCovered = "accountCovered";

        /*
         * The Identity Model introspection already sets token claims in the claims principal
         * So we only need to add claims for central user data or product user data
         */
        public static ClaimsPrincipal SerializeToClaimsPrincipal(ApiClaims claims)
        {
            var claimsList = new List<Claim>();

            // Add token claims
            claimsList.Add(new Claim(JwtClaimTypes.Subject, claims.UserId));
            claimsList.Add(new Claim(JwtClaimTypes.ClientId, claims.ClientId));
            foreach (var scope in claims.Scopes)
            {
                claimsList.Add(new Claim(JwtClaimTypes.Scope, scope));
            }

            // Add user info claims
            claimsList.Add(new Claim(JwtClaimTypes.GivenName, claims.GivenName));
            claimsList.Add(new Claim(JwtClaimTypes.FamilyName, claims.FamilyName));
            claimsList.Add(new Claim(JwtClaimTypes.Email, claims.Email));
            
            // Add product user claims
            foreach (var accountCovered in claims.AccountsCovered)
            {
                var stringValue = Convert.ToString(accountCovered, CultureInfo.InvariantCulture);
                claimsList.Add(new Claim(CustomClaimAccountCovered, stringValue));
            }

            // Create the 
            var identity = new ClaimsIdentity(claimsList, "Bearer", JwtClaimTypes.Subject, string.Empty);
            return new ClaimsPrincipal(identity);
        }

        /*
         * Read the claims we are interested in from the claims principal into a type safe object
         */
        public static ApiClaims DeserializeApiClaims(this ClaimsPrincipal principal)
        {
            // Read token values
            string userId = principal.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject)?.Value;
            string clientId = principal.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.ClientId)?.Value;
            var scopes = principal.Claims.Where(c => c.Type == JwtClaimTypes.Scope).Select(c => c.Value).ToList();
            
            // Read user info values
            string givenName = principal.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.GivenName)?.Value;
            string familyName = principal.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.FamilyName)?.Value;
            string email = principal.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Email)?.Value;
            
            // Read accounts covered
            var accountsCovered = principal.Claims.Where(c => c.Type == CustomClaimAccountCovered).Select(
                c => Convert.ToInt32(c.Value, CultureInfo.InvariantCulture)).ToList();

            // Verify that expected claims are present
            CheckStringClaim(JwtClaimTypes.Subject, userId);
            CheckStringClaim(JwtClaimTypes.ClientId, clientId);
            CheckCollectionClaim<string>(JwtClaimTypes.Scope, scopes);
            CheckStringClaim(JwtClaimTypes.GivenName, givenName);
            CheckStringClaim(JwtClaimTypes.FamilyName, familyName);
            CheckStringClaim(JwtClaimTypes.Email, email);
            CheckCollectionClaim<int>(CustomClaimAccountCovered, accountsCovered);

            // Create and return our custom object
            var claims = new ApiClaims();
            claims.SetTokenInfo(userId, clientId, scopes.ToArray());
            claims.SetCentralUserInfo(givenName, familyName, email);
            claims.AccountsCovered = accountsCovered.ToArray();
            return claims;
        }

        private static void CheckStringClaim(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Unable to read claim ${name} from the claim identity");
            }

        }

        private static void CheckCollectionClaim<T>(string name, IList<T> values)
        {
            if (values.Count == 0)
            {
                throw new InvalidOperationException($"Unable to read collection claim ${name} from the claim identity");
            }
        }

        /*
         * Look up the expiry time from the token, whose claim is added by Identity Model introspection
         */
        /*public static int GetAccessTokenExpirationClaim(this ClaimsPrincipal principal)
        {
            var expiryClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Expiration);
            if (expiryClaim == null)
            {
                throw new InvalidOperationException("Unable to find expiry claim with which to cache claims");
            }

            return Convert.ToInt32(expiryClaim.Value, CultureInfo.InvariantCulture);
        }*/
    }
}