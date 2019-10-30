namespace BasicApi.Logic.Controllers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Framework.Errors;
    using BasicApi.Logic.Entities;
    using BasicApi.Logic.Repositories;

    /*
     * A controller for our company resources
     */
    [Route("api/companies")]
    public class CompanyController : Controller
    {
        private readonly CompanyRepository repository;

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
        public async Task<CompanyTransactions> GetCompanyTransactionsAsync(string id)
        {
            // Return a 400 if the id is not a number
            int idValue;
            if (!int.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out idValue) || idValue <= 0)
            {
                throw new ClientError(HttpStatusCode.BadRequest, "invalid_company_id", "The company id must be a positive numeric integer");
            }

            // Forward the numeric id to the repository
            return await this.repository.GetCompanyTransactionsAsync(idValue);
        }
    }
}
