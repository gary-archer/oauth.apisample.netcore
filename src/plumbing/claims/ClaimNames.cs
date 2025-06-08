namespace FinalApi.Plumbing.Claims
{
    /*
     * Represents claims that the API uses from access tokens
     */
    public static class ClaimNames
    {
        // Standard claim names
        public static readonly string Issuer = "iss";
        public static readonly string Audience = "aud";
        public static readonly string Scope = "scope";
        public static readonly string Exp = "exp";
        public static readonly string Subject = "sub";

        // Custom claim names
        public static readonly string ManagerId = "manager_id";
        public static readonly string Role = "role";
    }
}
