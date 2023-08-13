namespace SampleApi.Plumbing.Claims
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Claims;
    using Newtonsoft.Json.Linq;

    /*
     * The claims of interest from the JWT access token
     */
    public class JwtClaims
    {
        private readonly JObject payload;

        /*
         * Wrap the raw payload
         */
        public JwtClaims(string claimsJson)
        {
            this.payload = JObject.Parse(claimsJson);
        }

        /*
         * Return individual claim values
         */
        public string Iss
        {
            get
            {
                return ClaimsReader.GetStringClaim(this.payload, OAuthClaimNames.Issuer);
            }
        }

        public string Scope
        {
            get
            {
                return ClaimsReader.GetStringClaim(this.payload, OAuthClaimNames.Scope);
            }
        }

        public string Sub
        {
            get
            {
                return ClaimsReader.GetStringClaim(this.payload, OAuthClaimNames.Subject);
            }
        }

        public int Exp
        {
            get
            {
                var exp = ClaimsReader.GetStringClaim(this.payload, OAuthClaimNames.Exp);
                return Convert.ToInt32(exp, CultureInfo.InvariantCulture);
            }
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

            var exp = ClaimsReader.GetStringClaim(this.payload, OAuthClaimNames.Exp);
            identity.AddClaim(new Claim(OAuthClaimNames.Exp, exp));
        }

        /*
         * Set the name claim type to the subject claim
         */
        public string GetNameClaimType()
        {
            return OAuthClaimNames.Subject;
        }

        /*
         * Look up an optional claim in the JWT
         */
        public string GetOptionalStringClaim(string name)
        {
            var claim = ClaimsReader.GetOptionalStringClaim(this.payload, name);
            return claim == null ? null : claim.Value<string>();
        }

        /*
         * Get audiences as an array
         */
        public IEnumerable<string> GetAudiences()
        {
            var results = new List<string>();

            var aud = ClaimsReader.GetClaim(this.payload, OAuthClaimNames.Audience);
            if (aud is JArray)
            {
                var audiences = aud as JArray;
                foreach (var audience in audiences)
                {
                    results.Add(audience.Value<string>());
                }
            }
            else
            {
                results.Add(aud.Value<string>());
            }

            return results;
        }
    }
}
