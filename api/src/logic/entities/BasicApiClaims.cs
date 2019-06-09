namespace BasicApi.Logic.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security.Claims;
    using Framework.OAuth;
    using Framework.Utilities;

    /// <summary>
    /// Our API overrides the core claims to support additional custom claims
    /// </summary>
    public class BasicApiClaims : CoreApiClaims
    {
        // Our custom claim
        private const string CustomClaimAccountCovered = "accountCovered";

        // Product Specific data used for authorization
        [DataMember]
        public int[] AccountsCovered { get; set; }

        /// <summary>
        /// Read our custom claims from the claims principal
        /// </summary>
        /// <param name="principal">The claims principal to update ourself from</param>
        public override void ReadFromPrincipal(ClaimsPrincipal principal)
        {
            // Base processing
            base.ReadFromPrincipal(principal);

            // Our custom processing
            this.AccountsCovered = principal.GetStringClaimSet(CustomClaimAccountCovered)
                                            .Select(claim => Convert.ToInt32(claim, CultureInfo.InvariantCulture))
                                            .ToArray();
        }

        /// <summary>
        /// Add claims to be included in the claims principal
        /// </summary>
        /// <param name="claimsList">A list of claims to add to</param>
        public override void WriteToPrincipal(IList<Claim> claimsList)
        {
            // Base processing
            base.WriteToPrincipal(claimsList);

            // Our custom processing
            foreach (var accountCovered in this.AccountsCovered)
            {
                var stringValue = Convert.ToString(accountCovered, CultureInfo.InvariantCulture);
                claimsList.Add(new Claim(CustomClaimAccountCovered, stringValue));
            }
        }
    }
}
