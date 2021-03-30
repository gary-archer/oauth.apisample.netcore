namespace SampleApi.Plumbing.Claims
{
    using System.Linq;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using SampleApi.Plumbing.Errors;

    /*
     * Claims received in the access token
     */
    public class BaseClaims
    {
        public BaseClaims(string subject, string[] scopes, int expiry)
        {
            this.Subject = subject;
            this.Scopes = scopes;
            this.Expiry = expiry;
        }

        public string Subject { get; set; }

        public string[] Scopes { get; set; }

        public int Expiry { get; set; }

        public static BaseClaims ImportData(JObject data)
        {
            var subject = data.GetValue("subject").Value<string>();
            var scope = data.GetValue("scopes").Value<string>();
            var expiry = data.GetValue("expiry").Value<int>();
            return new BaseClaims(subject, scope.Split(" "), expiry);
        }

        public JObject ExportData()
        {
            dynamic data = new JObject();
            data.subject = this.Subject;
            data.scopes = string.Join(" ", this.Scopes);
            data.expiry = this.Expiry;
            return data;
        }

        /*
        * Make sure the token has the correct scope for an area of data
        */
        public void VerifyScope(string scope)
        {
            if (!this.Scopes.ToList().Exists((s) => s.Contains(scope)))
            {
                throw ErrorFactory.CreateClientError(
                    HttpStatusCode.Forbidden,
                    ErrorCodes.InsufficientScope,
                    "Access token does not have a valid scope for this API");
            }
        }
    }
}
