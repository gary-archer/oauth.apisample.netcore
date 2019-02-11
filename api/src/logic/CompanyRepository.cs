namespace BasicApi.Logic
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using BasicApi.Entities;
    using BasicApi.Plumbing.Errors;
    using BasicApi.Plumbing.OAuth;
    using BasicApi.Plumbing.Utilities;

    /*
     * A repository class for serving data
     */
    public class CompanyRepository
    {
        /*
         * Claims for this API request
         */
        private readonly ApiClaims claims;

        /*
         * This injected class deals with text file infrastructure
         */
        private readonly JsonReader jsonReader;
    
        /*
         * Receive claims when constructed
         */
        public CompanyRepository(ApiClaims claims, JsonReader jsonReader)
        {
            this.claims = claims;
            this.jsonReader = jsonReader;
        }

        /*
         * Return the list of companies from a hard coded data file
         */
        public async Task<IEnumerable<Company>> GetCompanyListAsync()
        {
            // Read company data
            var companies = await this.jsonReader.ReadDataAsync<IEnumerable<Company>>(@"./data/companyList.json");

            // We will then filter on only authorized companies
            return companies.Where(c => this.IsUserAuthorizedForCompany(c.Id));
        }

        /*
         * Get transaction details for a company
         */
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

        /*
         * Apply claims that were read when the access token was first validated
         */
        private bool IsUserAuthorizedForCompany(int companyId) {
            return this.claims.AccountsCovered.Any(id => id == companyId);
        }

        /*
        * Return a 404 error if the user is not authorized
        * Requests for both unauthorized and non existent data are treated the same
        */
        private ClientError UnauthorizedError(int companyId) {
            return new ClientError(
                404,
                "company_not_found",
                $"Company {companyId} was not found for this user");
        }
    }
}
