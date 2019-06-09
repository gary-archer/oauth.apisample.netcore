namespace BasicApi.Logic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Framework.Errors;
    using BasicApi.Logic.Entities;
    using BasicApi.Utilities;

    /// <summary>
    /// A repository class for serving data
    /// </summary>
    public class CompanyRepository
    {
        // Claims for this API request
        private readonly BasicApiClaims claims;

        // This injected class deals with text file infrastructure
        private readonly JsonReader jsonReader;

        /// <summary>
        /// Receive dependencies when constructed
        /// </summary>
        /// <param name="claims">The claims for the user in the token</param>
        /// <param name="jsonReader">An infrastructure class</param>
        public CompanyRepository(BasicApiClaims claims, JsonReader jsonReader)
        {
            this.claims = claims;
            this.jsonReader = jsonReader;
        }

        /// <summary>
        /// Return the list of companies from a hard coded data file
        /// </summary>
        /// <returns>A collection of companies</returns>
        public async Task<IEnumerable<Company>> GetCompanyListAsync()
        {
            // Read company data
            var companies = await this.jsonReader.ReadDataAsync<IEnumerable<Company>>(@"./data/companyList.json");

            // We will then filter on only authorized companies
            return companies.Where(c => this.IsUserAuthorizedForCompany(c.Id));
        }

        /// <summary>
        /// Get transaction details for a company
        /// </summary>
        /// <param name="id">The company id</param>
        /// <returns>A company and its transactions</returns>
        public async Task<CompanyTransactions> GetCompanyTransactionsAsync(int id)
        {
            // If the user is unauthorized we do not return any data
            if (!this.IsUserAuthorizedForCompany(id)) {
                throw this.UnauthorizedError(id);
            }

            // Read company data and find the requested value by id
            var allCompanies = await this.jsonReader.ReadDataAsync<IEnumerable<Company>>(@"./data/companyList.json");
            var foundCompany = allCompanies.FirstOrDefault(c => c.Id == id);
            if (foundCompany != null)
            {
                // Read transactions data and find the requested value by id
                var allTransactions = await this.jsonReader.ReadDataAsync<IEnumerable<CompanyTransactions>>(@"./data/companyTransactions.json");
                var foundTransactions = allTransactions.FirstOrDefault(t => t.Id == id);
                if (foundTransactions != null)
                {
                    foundTransactions.Company = foundCompany;
                    return foundTransactions;
                }
            }

            throw this.UnauthorizedError(id);
        }

        /// <summary>
        /// Apply claims that were read when the access token was first validated
        /// </summary>
        /// <param name="companyId">The company id</param>
        /// <returns>True if the user covers this account</returns>
        private bool IsUserAuthorizedForCompany(int companyId) {
            return this.claims.AccountsCovered.Any(id => id == companyId);
        }

        /// <summary>
        /// Return a 404 error if the user is not authorized
        /// Requests for both unauthorized and non existent data are treated the same
        /// </summary>
        /// <param name="companyId">The company id</param>
        /// <returns>A 404 error</returns>
        private ClientError UnauthorizedError(int companyId) {
            return new ClientError(
                HttpStatusCode.NotFound,
                "company_not_found",
                $"Company {companyId} was not found for this user");
        }
    }
}
