namespace SampleApi.Plumbing.Claims
{
    /*
     * Claim names from the Authorization Server
     */
    public static class StandardClaimNames
    {
        public static readonly string Issuer = "iss";
        public static readonly string Audience = "aud";
        public static readonly string Subject = "sub";
        public static readonly string Scope = "scope";
        public static readonly string Exp = "exp";
        public static readonly string GivenName = "given_name";
        public static readonly string FamilyName = "family_name";
        public static readonly string Email = "email";
    }
}
