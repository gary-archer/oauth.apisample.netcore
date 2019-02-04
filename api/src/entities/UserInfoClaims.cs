namespace BasicApi.Entities
{
    /*
     * User Info claims used by the API
     */
    public class UserInfoClaims
    {
        public string GivenName {get; private set;}

        public string FamilyName {get; private set;}

        public string Email {get; private set;}

        /*
         * Construct from input
         */
        public UserInfoClaims(string givenName, string familyName, string email)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.Email = email;
        }
    }
}
