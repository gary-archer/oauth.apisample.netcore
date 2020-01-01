namespace SampleApi.Logic.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SampleApi.Logic.Entities;

    /*
     * The entry point to service logic
     */
    public class CompanyService
    {
        // Claims for this API request
        private readonly SampleApiClaims claims;

        // A repository class
        private readonly CompanyRepository repository;

        /*
         * Receive dependencies when constructed
         */
        public CompanyService(SampleApiClaims claims, CompanyRepository repository)
        {
            this.claims = claims;
            this.repository = repository;
        }

        /*
         * Return the list of companies from a hard coded data file
         */
        public async Task<IEnumerable<Company>> GetCompanyListAsync()
        {
            return await this.repository.GetCompanyListAsync();
        }

        /*
         * Get transaction details for a company
         */
        public async Task<CompanyTransactions> GetCompanyTransactionsAsync(int id)
        {
            return await this.repository.GetCompanyTransactionsAsync(id);
        }
    }
}
