namespace SampleApi.Logic.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using SampleApi.Logic.Claims;
    using SampleApi.Logic.Entities;
    using SampleApi.Logic.Errors;
    using SampleApi.Logic.Repositories;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Errors;

    /*
     * The entry point to service logic
     */
    public class CompanyService
    {
        private readonly CompanyRepository repository;
        private readonly CustomClaimsPrincipal claims;

        public CompanyService(CompanyRepository repository, CustomClaimsPrincipal claims)
        {
            this.repository = repository;
            this.claims = claims;
        }

        /*
         * Return the list of companies from a hard coded data file
         */
        public async Task<IEnumerable<Company>> GetCompanyListAsync()
        {
            // This example starts by loading all data
            var data = await this.repository.GetCompanyListAsync();

            // Filter on what the user is allowed to access
            return data.Where(c => this.IsUserAuthorizedForCompany(c));
        }

        /*
         * Get transaction details for a company
         */
        public async Task<CompanyTransactions> GetCompanyTransactionsAsync(int id)
        {
            var data = await this.repository.GetCompanyTransactionsAsync(id);
            if (data == null || !this.IsUserAuthorizedForCompany(data.Company))
            {
                throw this.UnauthorizedError(id);
            }

            return data;
        }

        /*
         * A simple example of applying domain specific claims
         */
        private bool IsUserAuthorizedForCompany(Company company)
        {
            // The admin role is granted access to all resources
            var role = this.claims.JwtClaims.GetStringClaim(CustomClaimNames.Role).ToLower();
            if (role == "admin")
            {
                return true;
            }

            // Unknown roles are granted no access to resources
            if (role != "user")
            {
                return false;
            }

            // For the user role, authorize based on a business rule that links the user to regional data
            var extraClaims = this.claims.ExtraClaims as SampleExtraClaims;
            return extraClaims.Regions.Any(ur => ur == company.Region);
        }

        /*
         * Return 404 for both not found items and also those that are not authorized
         */
        private ClientError UnauthorizedError(int companyId)
        {
            var message = $"Transactions for company {companyId} were not found for this user";
            return ErrorFactory.CreateClientError(
                HttpStatusCode.NotFound,
                SampleErrorCodes.CompanyNotFound,
                message);
        }
    }
}
