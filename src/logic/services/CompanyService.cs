namespace FinalApi.Logic.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using FinalApi.Logic.Entities;
    using FinalApi.Logic.Errors;
    using FinalApi.Logic.Repositories;
    using FinalApi.Plumbing.Claims;
    using FinalApi.Plumbing.Errors;

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
            var role = this.claims.Jwt.GetStringClaim(ClaimNames.Role).ToLowerInvariant();
            if (role == "admin")
            {
                return true;
            }

            // Unknown roles are granted no access to resources
            if (role != "user")
            {
                return false;
            }

            // Apply a business rule that links the user's regions to the region of a company resource
            return this.claims.Extra.Regions.Any(ur => ur == company.Region);
        }

        /*
         * Return 404 for both not found items and also those that are not authorized
         */
        private ClientError UnauthorizedError(int companyId)
        {
            var message = $"Transactions for company {companyId} were not found for this user";
            return ErrorFactory.CreateClientError(
                HttpStatusCode.NotFound,
                ErrorCodes.CompanyNotFound,
                message);
        }
    }
}
