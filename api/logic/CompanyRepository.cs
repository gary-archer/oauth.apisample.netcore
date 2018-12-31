namespace api.Logic
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using api.Entities;
    using api.Plumbing;

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
        public async Task<IEnumerable<Company>> GetListAsync()
        {
            // Read company data
            var companies = await this.jsonReader.ReadDataAsync<IEnumerable<Company>>(@"./data/companyList.json");

            // We will then filter on only authorized companies
            var authorizedCompanies = companies.Where(c => this.IsUserAuthorizedForCompany(c.Id));
            return authorizedCompanies;
        }

        /*
         * Get transaction details for a company
         */
        public async Task<CompanyTransactions> GetTransactionsAsync(int id)
        {
            // If the user is unauthorized we do not return any data
            if (!this.IsUserAuthorizedForCompany(id)) {
                throw new ClientError(404, "DataAccess", $"Transactions for company {id} were not found for this user");
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

            throw new ClientError(404, "DataAccess", $"Transactions for company {id} were not found for this user");
        }

        /*
         * Apply claims that were read when the access token was first validated
         */
        private bool IsUserAuthorizedForCompany(int companyId) {
            return this.claims.UserCompanyIds.Any(id => id == companyId);
        }
    }
}
