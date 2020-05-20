namespace SampleApi.Logic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using SampleApi.Host.Plumbing.Logging;
    using SampleApi.Logic.Entities;
    using SampleApi.Logic.Utilities;

    /*
     * A repository class for serving data
     */
    public class CompanyRepository
    {
        private readonly ILogEntry logEntry;
        private readonly JsonReader jsonReader;

        public CompanyRepository(JsonReader jsonReader, ILogEntry logEntry)
        {
            this.jsonReader = jsonReader;
            this.logEntry = logEntry;
        }

        /*
         * Return the list of companies from a hard coded data file
         */
        public async Task<IEnumerable<Company>> GetCompanyListAsync()
        {
            using (this.logEntry.CreatePerformanceBreakdown("getCompanyList"))
            {
                return await this.jsonReader.ReadDataAsync<IEnumerable<Company>>(@"./data/companyList.json");
            }
        }

        /*
         * Get transaction details for a company
         */
        public async Task<CompanyTransactions> GetCompanyTransactionsAsync(int id)
        {
            using (this.logEntry.CreatePerformanceBreakdown("getCompanyTransactions"))
            {
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
                        // Return the data once found
                        foundTransactions.Company = foundCompany;
                        return foundTransactions;
                    }
                }

                return null;
            }
        }
    }
}
