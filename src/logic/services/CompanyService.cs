namespace SampleApi.Logic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Framework.Api.Base.Errors;
    using SampleApi.Logic.Entities;
    using SampleApi.Logic.Errors;

    /*
     * The entry point to service logic
     */
    public class CompanyService
    {
        private readonly CompanyRepository repository;

        public CompanyService(CompanyRepository repository)
        {
            this.repository = repository;
        }

        /*
         * Return the list of companies from a hard coded data file
         */
        public async Task<IEnumerable<Company>> GetCompanyListAsync(string[] regionsCovered)
        {
            // Use a micro services approach of getting all data
            var data = await this.repository.GetCompanyListAsync();

            // Filter on what the user is allowed to access
            return data.Where(c => this.IsUserAuthorizedForCompany(c, regionsCovered));
        }

        /*
         * Get transaction details for a company
         */
        public async Task<CompanyTransactions> GetCompanyTransactionsAsync(int id, string[] regionsCovered)
        {
            var data = await this.repository.GetCompanyTransactionsAsync(id);
            if (data == null || !this.IsUserAuthorizedForCompany(data.Company, regionsCovered))
            {
                throw this.UnauthorizedError(id);
            }

            return data;
        }

        /*
         * Apply claims that were read when the access token was first validated
         */
        private bool IsUserAuthorizedForCompany(Company company, string[] regionsCovered)
        {
            return regionsCovered.AsEnumerable().Any(ur => ur == company.Region);
        }

        /*
         * Return 404 for both not found items and also those that are not authorized
         */
        private ClientError UnauthorizedError(int companyId)
        {
            var message = $"Transactions for company {companyId} were not found for this user";

            // TODO
            // return new BusinessError("company_not_found", message);
            return new ClientError(HttpStatusCode.NotFound, "company_not_found", message);
        }
    }
}
