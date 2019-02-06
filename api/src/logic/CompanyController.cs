namespace BasicApi.Logic
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using BasicApi.Entities;

    /*
     * A controller for our company resources
     */
    [Route("api/companies")]
    public class CompanyController : Controller
    {
        private readonly CompanyRepository repository;

        /*
         * Receive dependencies
         */
        public CompanyController(CompanyRepository repository)
        {
            this.repository = repository;
        }

        /*
         * Get a list of summary information about companies
         */
        [HttpGet("")]
        public async Task<IEnumerable<Company>> GetCompanyListAsync()
        {
            return await this.repository.GetCompanyListAsync();
        }

        /*
         * Get transaction details for a company
         */
        [HttpGet("{id}/transactions")]
        public async Task<CompanyTransactions> GetCompanyTransactionsAsync(int id)
        {
            return await this.repository.GetCompanyTransactionsAsync(id);
        }
    }
}
