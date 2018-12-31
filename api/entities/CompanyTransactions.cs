namespace api.Entities
{
    using System.Collections.Generic;

    /*
     * A composite response object
     */
    public class CompanyTransactions
    {
        public int? Id {get; set;}

        public Company Company {get; set;}

        public IEnumerable<Transaction> Transactions {get; set;}
    }
}