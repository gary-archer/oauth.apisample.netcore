namespace FinalApi.Host.Controllers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using FinalApi.Logic.Entities;
    using FinalApi.Logic.Errors;
    using FinalApi.Logic.Services;
    using FinalApi.Plumbing.Errors;
    using Microsoft.AspNetCore.Mvc;

    /*
     * A controller for our company resources
     */
    [Route("investments/companies")]
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
            return await this.service.GetCompanyListAsync();
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
