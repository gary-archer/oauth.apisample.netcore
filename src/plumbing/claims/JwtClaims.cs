namespace FinalApi.Plumbing.Claims
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Text.Json.Nodes;

    /*
     * The claims of interest from the JWT access token
     */
    public class JwtClaims
    {
        private readonly JsonNode payload;

        /*
         * Wrap the raw payload
         */
        public JwtClaims(string claimsJson)
        {
            this.payload = JsonNode.Parse(claimsJson);
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
                return ClaimsReader.GetIntegerClaim(this.payload, OAuthClaimNames.Exp);
            }
        }

        public string GetStringClaim(string name)
        {
            return ClaimsReader.GetStringClaim(this.payload, name);
        }

        /*
         * Get audiences as an array
         */
        public IEnumerable<string> GetAudiences()
        {
            var results = new List<string>();

            var audienceNode = this.payload[OAuthClaimNames.Audience];
            if (audienceNode != null)
            {
                var audiences = audienceNode as JsonArray;
                if (audiences != null)
                {
                    foreach (var audience in audiences)
                    {
                        results.Add(audience.GetValue<string>());
                    }
                }
                else
                {
                    results.Add(audienceNode.GetValue<string>());
                }
            }

            return results;
        }

        /*
         * Add claims to the identity in order for .NET authorization to work as expected
         */
        public void AddToClaimsIdentity(ClaimsIdentity identity)
        {
            identity.AddClaim(new Claim(OAuthClaimNames.Issuer, this.Iss));

            var audiences = this.GetAudiences();
            foreach (var audience in audiences)
            {
                identity.AddClaim(new Claim(OAuthClaimNames.Audience, audience));
            }

            identity.AddClaim(new Claim(OAuthClaimNames.Scope, this.Scope));
            identity.AddClaim(new Claim(OAuthClaimNames.Subject, this.Sub));

            var exp = ClaimsReader.GetIntegerClaim(this.payload, OAuthClaimNames.Exp);
            identity.AddClaim(new Claim(OAuthClaimNames.Exp, exp.ToString()));
        }
    }
}
