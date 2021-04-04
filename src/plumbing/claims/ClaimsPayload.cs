namespace SampleApi.Plumbing.Claims
{
    using System;
    using SampleApi.Plumbing.Errors;

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

        public Func<string, string> StringClaimCallback { get; set; }

        public Func<string, string[]> StringArrayClaimCallback { get; set; }

        public string GetStringClaim(string name)
        {
            if (this.StringClaimCallback == null)
            {
                throw new InvalidOperationException("StringClaimCallback is null in the ClaimsPayload class");
            }

            var value = this.StringClaimCallback(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw ErrorUtils.FromMissingClaim(name);
            }

            return value;
        }

        public string[] GetStringArrayClaim(string name)
        {
            if (this.StringArrayClaimCallback == null)
            {
                throw new InvalidOperationException("StringArrayClaimCallback is null in the ClaimsPayload class");
            }

            return this.StringArrayClaimCallback(name);
        }
    }
}
