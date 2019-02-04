namespace BasicApi.Entities
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

        // The calling application's client id can potentially be used for authorization
        [DataMember]
        public string CallingApplicationId {get; private set;}

        // OAuth scopes can represent high level areas of the business
        [DataMember]
        public string[] Scopes {get; private set;}

        // Details from the Central User Data for given name, family name and email
        [DataMember]
        public UserInfoClaims UserInfo {get; private set;}

        // Product Specific data used for authorization
        [DataMember]
        public int[] UserCompanyIds {get; private set;}

        /*
         * Construct from input
         */
        public ApiClaims(string userId, string callingApplicationId, string[] scopes)
        {
            this.UserId = userId;
            this.CallingApplicationId = callingApplicationId;
            this.Scopes = scopes;
        }

        /*
         * Set fields after receiving OAuth user info data
         */
        public void SetCentralUserData(string givenName, string familyName, string email)
        {
            this.UserInfo = new UserInfoClaims(givenName, familyName, email);
        }

        /*
         * Set a custom business rule
         */
        public void setProductSpecificUserRights(int[] userCompanyIds)
        {
            this.UserCompanyIds = userCompanyIds;
        }
    }
}
