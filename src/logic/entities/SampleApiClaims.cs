namespace SampleApi.Logic.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security.Claims;
    using Framework.Api.Base.Utilities;
    using Framework.Api.OAuth.Claims;

    /*
     * Our API overrides the core claims to support additional custom claims
     */
    public class SampleApiClaims : CoreApiClaims
    {
        // Our custom claim
        private const string CustomClaimRegionsCovered = "regionsCovered";

        // Product Specific data used for authorization
        [DataMember]
        public string[] RegionsCovered { get; set; }

        /*
         * Read our custom claims from the claims principal
         */
        public override void Load(ClaimsPrincipal principal)
        {
            // Base processing
            base.Load(principal);

            // Our custom processing
            this.RegionsCovered = principal.GetStringClaimSet(CustomClaimRegionsCovered).ToArray();
        }

        /*
         * Add claims to be included in the claims principal
         */
        public override void Output(IList<Claim> claimsList)
        {
            // Base processing
            base.Output(claimsList);

            // Our custom processing
            foreach (var accountCovered in this.RegionsCovered)
            {
                var stringValue = Convert.ToString(accountCovered, CultureInfo.InvariantCulture);
                claimsList.Add(new Claim(CustomClaimRegionsCovered, stringValue));
            }
        }
    }
}
