namespace SampleApi.Logic.Entities
{
    /*
     * User claims returned to the UI by this API
     */
    public class UserInfoClaims
    {
        public string GivenName {get; private set;}

        public string FamilyName {get; private set;}

        public string Email {get; private set;}

        public UserInfoClaims(string givenName, string familyName, string email)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.Email = email;
        }
    }
}
