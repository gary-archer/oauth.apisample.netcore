namespace FinalApi.Plumbing.Claims
{
    using System.Collections.Generic;
    using System.Text.Json.Nodes;

    /*
     * The claims from the JWT access token
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
                return ClaimsReader.GetStringClaim(this.payload, ClaimNames.Issuer);
            }
        }

        public string Scope
        {
            get
            {
                return ClaimsReader.GetStringClaim(this.payload, ClaimNames.Scope);
            }
        }

        public string Sub
        {
            get
            {
                return ClaimsReader.GetStringClaim(this.payload, ClaimNames.Subject);
            }
        }

        public int Exp
        {
            get
            {
                return ClaimsReader.GetIntegerClaim(this.payload, ClaimNames.Exp);
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

            var audienceNode = this.payload[ClaimNames.Audience];
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
    }
}
