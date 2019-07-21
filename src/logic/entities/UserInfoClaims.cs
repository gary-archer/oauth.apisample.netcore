namespace BasicApi.Logic.Entities
{
    /// <summary>
    /// The API's user claims
    /// </summary>
    public class UserInfoClaims
    {
        public string GivenName {get; private set;}

        public string FamilyName {get; private set;}

        public string Email {get; private set;}

        /// <summary>
        /// Construct from input
        /// </summary>
        /// <param name="givenName">The given name</param>
        /// <param name="familyName">The family name</param>
        /// <param name="email">The email</param>
        public UserInfoClaims(string givenName, string familyName, string email)
        {
            this.GivenName = givenName;
            this.FamilyName = familyName;
            this.Email = email;
        }
    }
}
