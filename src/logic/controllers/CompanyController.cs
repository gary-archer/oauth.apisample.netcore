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

    /// <summary>
    /// A controller for our company resources
    /// </summary>
    [Route("api/companies")]
    public class CompanyController : Controller
    {
        private readonly CompanyRepository repository;

        /// <summary>
        /// Receive dependencies
        /// </summary>
        /// <param name="repository">The repository used by the controller</param>
        public CompanyController(CompanyRepository repository)
        {
            this.repository = repository;
        }

        /// <summary>
        /// Get a list of summary information about companies
        /// </summary>
        /// <returns>A collection of companies</returns>
        [HttpGet("")]
        public async Task<IEnumerable<Company>> GetCompanyListAsync()
        {
            return await this.repository.GetCompanyListAsync();
        }

        /// <summary>
        /// Get transaction details for a company
        /// </summary>
        /// <param name="id">A numeric company id</param>
        /// <returns>A company and its transactions</returns>
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
