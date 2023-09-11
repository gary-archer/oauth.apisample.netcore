namespace SampleApi.Logic.Repositories
{
    using SampleApi.Logic.Claims;

    public class UserRepository
    {
        /*
        * Receive the manager ID in the access token, as a useful API identity, then look up extra claims
        * This is the preferred model, since it locks down the access token and provides the most useful API user identity
        */
        public SampleExtraClaims GetClaimsForManagerId(string managerId)
        {
            if (managerId == "20116")
            {
                // These claims are used for the guestadmin@mycompany.com user account
                return new SampleExtraClaims("Global Manager", new string[] { "Europe", "USA", "Asia" });
            }
            else if (managerId == "10345")
            {
                // These claims are used for the guestuser@mycompany.com user account
                return new SampleExtraClaims("Regional Manager", new string[] { "USA" });
            }
            else
            {
                // Use empty claims for unrecognized users
                return new SampleExtraClaims(string.Empty, new string[] { });
            }
        }

        /*
        * Receive the subject claim from the access token and look up all other claims
        * This is less optimal, since the token is less locked down and the API must map the subject to other values
        */
        public SampleExtraClaims GetClaimsForSubject(string subject)
        {
            if (subject == "77a97e5b-b748-45e5-bb6f-658e85b2df91")
            {
                // These claims are used for the guestadmin@mycompany.com user account
                var claims = new SampleExtraClaims("Global Manager", new string[] { "Europe", "USA", "Asia" });
                claims.AddCoreClaims("20116", "admin");
                return claims;
            }
            else if (subject == "a6b404b1-98af-41a2-8e7f-e4061dc0bf86")
            {
                // These claims are used for the guestuser@mycompany.com user account
                var claims = new SampleExtraClaims("Regional Manager", new string[] { "USA" });
                claims.AddCoreClaims("10345", "user");
                return claims;
            }
            else
            {
                // Use empty claims for unrecognized users
                var claims = new SampleExtraClaims(string.Empty, new string[] { });
                claims.AddCoreClaims(string.Empty, string.Empty);
                return claims;
            }
        }
    }
}
