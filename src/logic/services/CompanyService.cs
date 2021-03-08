namespace SampleApi.Logic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using SampleApi.Logic.Entities;
    using SampleApi.Logic.Errors;
    using SampleApi.Plumbing.Errors;

    /*
     * The entry point to service logic
     */
    public class CompanyService
    {
        private readonly CompanyRepository repository;
        private readonly SampleCustomClaims claims;

        public CompanyService(CompanyRepository repository, SampleCustomClaims claims)
        {
            this.repository = repository;
            this.claims = claims;
        }

        /*
         * Return the list of companies from a hard coded data file
         */
        public async Task<IEnumerable<Company>> GetCompanyListAsync()
        {
            // Use a micro services approach of getting all data
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
            if (this.claims.IsAdmin)
            {
                return true;
            }

            return this.claims.RegionsCovered.AsEnumerable().Any(ur => ur == company.Region);
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
