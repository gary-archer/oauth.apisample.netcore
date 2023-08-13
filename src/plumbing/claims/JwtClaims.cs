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
         * Set the name claim type to the subject claim
         */
        public string GetNameClaimType()
        {
            return OAuthClaimNames.Subject;
        }

        /*
         * Add claims to the identity in order for .NET authorization to work as expected
         */
        public void AddClaims(ClaimsIdentity identity)
        {
            identity.AddClaim(new Claim(OAuthClaimNames.Issuer, this.Iss));

            var audiences = this.GetAudiences();
            foreach (var audience in audiences)
            {
                identity.AddClaim(new Claim(OAuthClaimNames.Audience, audience));
            }

            identity.AddClaim(new Claim(OAuthClaimNames.Scope, this.Scope));
            identity.AddClaim(new Claim(OAuthClaimNames.Subject, this.Sub));
            identity.AddClaim(new Claim(OAuthClaimNames.Exp, Convert.ToString(this.Exp, CultureInfo.InvariantCulture)));
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
