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

        public CompanyService(CompanyRepository repository)
        {
            this.repository = repository;
        }

        /*
         * Return the list of companies from a hard coded data file
         */
        public async Task<IEnumerable<Company>> GetCompanyListAsync(bool isAdmin, string[] regionsCovered)
        {
            // Use a micro services approach of getting all data
            var data = await this.repository.GetCompanyListAsync();

            // Filter on what the user is allowed to access
            return data.Where(c => this.IsUserAuthorizedForCompany(c, isAdmin, regionsCovered));
        }

        /*
         * Get transaction details for a company
         */
        public async Task<CompanyTransactions> GetCompanyTransactionsAsync(int id, bool isAdmin, string[] regionsCovered)
        {
            var data = await this.repository.GetCompanyTransactionsAsync(id);
            if (data == null || !this.IsUserAuthorizedForCompany(data.Company, isAdmin, regionsCovered))
            {
                throw this.UnauthorizedError(id);
            }

            return data;
        }

        /*
         * A simple example of applying domain specific claims
         */
        private bool IsUserAuthorizedForCompany(Company company, bool isAdmin, string[] regionsCovered)
        {
            if (isAdmin)
            {
                return true;
            }

            return regionsCovered.AsEnumerable().Any(ur => ur == company.Region);
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
