namespace SampleApi.Plumbing.Claims
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Security.Claims;
    using Newtonsoft.Json.Linq;

    /*
     * The claims of interest from the JWT access token
     */
    public class JwtClaims
    {
        public string Iss { get; set; }

        public object Aud { private get; set; }

        public string Scope { get; set; }

        public string Sub { get; set; }

        public int Exp { get; set; }

        /*
         * Return all claims as a list
         */
        public IEnumerable<Claim> ToList()
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(OAuthClaimNames.Issuer, this.Iss));

            var audiences = this.GetAudiences();
            foreach (var audience in audiences)
            {
                claims.Add(new Claim(OAuthClaimNames.Audience, audience));
            }

            claims.Add(new Claim(OAuthClaimNames.Scope, this.Scope));
            claims.Add(new Claim(OAuthClaimNames.Subject, this.Sub));
            claims.Add(new Claim(OAuthClaimNames.Exp, Convert.ToString(this.Exp, CultureInfo.InvariantCulture)));
            return claims;
        }

        /*
         * Look up an optional claim in the JWT
         */
        public string GetOptionalClaim(string name)
        {
            return null;
        }

        /*
         * Get audiences as an array
         */
        public IEnumerable<string> GetAudiences()
        {
            var results = new List<string>();

            if (this.Aud is string)
            {
                results.Add(this.Aud as string);
            }

            if (this.Aud is JArray)
            {
                var audiences = this.Aud as JArray;
                foreach (var audience in audiences)
                {
                    results.Add(audience.Value<string>());
                }
            }

            return results;
        }
    }
}
