namespace Framework.OAuth
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security.Claims;
    using IdentityModel;
    using Framework.Utilities;

    /*
     * API claims used for authorization
     */
    [DataContract]
    public class CoreApiClaims
    {
        // The immutable user id from the access token, which may exist in the API's database
        [DataMember]
        public string UserId {get; private set;}

        // The client id, which typically represents the calling application
        [DataMember]
        public string ClientId {get; private set;}

        // OAuth scopes can represent high level areas of the business
        [DataMember]
        public string[] Scopes {get; private set;}

        // Details from the Central User Data for given name, family name and email
        [DataMember]
        public string GivenName {get; private set;}

        [DataMember]
        public string FamilyName {get; private set;}

        [DataMember]
        public string Email {get; private set;}

        /*
         * Set token claims after introspection
         */
        public void SetTokenInfo(string userId, string clientId, string[] scopes)
        {
            this.UserId = userId;
            this.ClientId = clientId;
            this.Scopes = scopes;
        }

        /*
         * Set fields after receiving OAuth user info data
         */
        public void SetCentralUserInfo(string givenName, string familyName, string email)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.Email = email;
        }

        /*
         * Update this claims object from the claims principal
         */
        public virtual void ReadFromPrincipal(ClaimsPrincipal principal)
        {
            // Read token values
            this.UserId = principal.GetStringClaim(JwtClaimTypes.Subject);
            this.ClientId = principal.GetStringClaim(JwtClaimTypes.ClientId);
            this.Scopes = principal.GetStringClaimSet(JwtClaimTypes.Scope).ToArray();

            // Read user info values
            this.GivenName = principal.GetStringClaim(JwtClaimTypes.GivenName);
            this.FamilyName = principal.GetStringClaim(JwtClaimTypes.FamilyName);
            this.Email = principal.GetStringClaim(JwtClaimTypes.Email);
        }

        /*
         * Add claims to be included in the claims principal
         */
        public virtual void WriteToPrincipal(IList<Claim> claimsList)
        {
            // Add token claims
            claimsList.Add(new Claim(JwtClaimTypes.Subject, this.UserId));
            claimsList.Add(new Claim(JwtClaimTypes.ClientId, this.ClientId));
            foreach (var scope in this.Scopes)
            {
                claimsList.Add(new Claim(JwtClaimTypes.Scope, scope));
            }

            // Add user info claims
            claimsList.Add(new Claim(JwtClaimTypes.GivenName, this.GivenName));
            claimsList.Add(new Claim(JwtClaimTypes.FamilyName, this.FamilyName));
            claimsList.Add(new Claim(JwtClaimTypes.Email, this.Email));
        }
    }
}
