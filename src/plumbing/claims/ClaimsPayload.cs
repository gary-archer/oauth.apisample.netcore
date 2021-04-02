namespace SampleApi.Plumbing.Claims
{
    /*
     * A simple wrapper for the claims in a decoded JWT or introspection / user info response
     */
    public class ClaimsPayload
    {
        private readonly object claims;

        public ClaimsPayload(object claims)
        {
            this.claims = claims;
        }
    }
}
