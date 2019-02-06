namespace BasicApi.Plumbing.OAuth
{
    using System.Runtime.Serialization;

    /*
     * API claims used for authorization
     */
    [DataContract]
    public class ApiClaims
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
        public string FamilyName {get; private set;}
        public string Email {get; private set;}

        // Product Specific data used for authorization
        [DataMember]
        public int[] AccountsCovered {get; set;}

        /*
         * Construct from input
         */
        public ApiClaims(string userId, string callingApplicationId, string[] scopes)
        {
            this.UserId = userId;
            this.ClientId = callingApplicationId;
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
    }
}
