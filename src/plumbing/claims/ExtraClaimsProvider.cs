﻿namespace FinalApi.Plumbing.Claims
{
    using System;
    using System.Security.Claims;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;

    /*
     * Add extra claims that you cannot, or do not want to, manage in the authorization server
     */
    public class ExtraClaimsProvider
    {
        /*
         * Get additional claims from the API's own database
         */
        #pragma warning disable 1998
        public virtual async Task<ExtraClaims> LookupExtraClaimsAsync(JwtClaims jwtClaims, IServiceProvider serviceProvider)
        {
            return new ExtraClaims();
        }
        #pragma warning restore 1998

        /*
         * Create a claims principal that manages lookups across both token claims and extra claims
         */
        public virtual CustomClaimsPrincipal CreateClaimsPrincipal(JwtClaims jwtClaims, ExtraClaims extraClaims)
        {
            var identity = new ClaimsIdentity("Bearer", OAuthClaimNames.Subject, null);
            jwtClaims.AddToClaimsIdentity(identity);
            extraClaims.AddToClaimsIdentity(identity);
            return new CustomClaimsPrincipal(jwtClaims, extraClaims, identity);
        }

        /*
         * Deserialize extra claims after they have been read from the cache
         */
        public virtual ExtraClaims DeserializeFromCache(JsonNode data)
        {
            return ExtraClaims.ImportData(data);
        }
    }
}
