namespace FinalApi.Logic.Claims
{
    /*
     * Custom claims used in the API's authorization, or for user identification
     */
    public static class CustomClaimNames
    {
        // Custom claims issued to access tokens
        public static readonly string ManagerId = "manager_id";
        public static readonly string Role = "role";

        // Custom claims looked up from the API's own data
        public static readonly string Title = "title";
        public static readonly string Regions = "regions";
    }
}
