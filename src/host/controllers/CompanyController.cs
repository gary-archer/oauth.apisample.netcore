namespace SampleApi.Host.Controllers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using Framework.Api.Base.Errors;
    using Microsoft.AspNetCore.Mvc;
    using SampleApi.Host.Claims;
    using SampleApi.Logic.Entities;
    using SampleApi.Logic.Errors;
    using SampleApi.Logic.Repositories;

    /*
     * A controller for our company resources
     */
    [Route("api/companies")]
    public class CompanyController : Controller
    {
        private readonly CompanyService service;
        private readonly SampleApiClaims claims;

        public CompanyController(CompanyService service, SampleApiClaims claims)
        {
            this.service = service;
            this.claims = claims;
        }

        /*
         * Get a list of summary information about companies
         */
        [HttpGet("")]
        public async Task<IEnumerable<Company>> GetCompanyListAsync()
        {
            return await this.service.GetCompanyListAsync(this.claims.RegionsCovered);
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
                    ErrorCodes.InvalidCompanyId,
                    "The company id must be a positive numeric integer");
            }

            // Forward the numeric id to the service
            return await this.service.GetCompanyTransactionsAsync(idValue, this.claims.RegionsCovered);
        }
    }
}
