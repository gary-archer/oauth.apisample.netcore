namespace SampleApi.Host.Controllers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using SampleApi.Logic.Entities;
    using SampleApi.Logic.Errors;
    using SampleApi.Logic.Repositories;
    using SampleApi.Plumbing.Claims;
    using SampleApi.Plumbing.Errors;
    using SampleApi.Plumbing.OAuth;

    /*
     * A controller for our company resources
     */
    [Route("api/companies")]
    public class CompanyController : Controller
    {
        private readonly CompanyService service;

        public CompanyController(CompanyService service)
        {
            this.service = service;
        }

        /*
         * Get a list of summary information about companies
         */
        [HttpGet("")]
        public async Task<IEnumerable<Company>> GetCompanyListAsync()
        {
            // First check we have access to this level of data
            ScopeVerifier.Enforce(this.User.GetScopes(), "transactions_read");

            // Then return the list of companies
            return await this.service.GetCompanyListAsync();
        }

        /*
         * Get transaction details for a company
         */
        [HttpGet("{id}/transactions")]
        public async Task<CompanyTransactions> GetCompanyTransactionsAsync(string id)
        {
            // First check we have access to this level of data
            ScopeVerifier.Enforce(this.User.GetScopes(), "transactions_read");

            // Return a 400 if the id is not a number
            int idValue;
            if (!int.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out idValue) || idValue <= 0)
            {
                throw ErrorFactory.CreateClientError(
                    HttpStatusCode.BadRequest,
                    SampleErrorCodes.InvalidCompanyId,
                    "The company id must be a positive numeric integer");
            }

            // Forward the numeric id to the service
            return await this.service.GetCompanyTransactionsAsync(idValue);
        }
    }
}
