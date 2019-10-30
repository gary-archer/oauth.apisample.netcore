namespace BasicApi.Logic.Entities
{
    using System.Collections.Generic;

    /*
     * A company and its transactions
     */
    public class CompanyTransactions
    {
        public int? Id {get; set;}

        public Company Company {get; set;}

        public IEnumerable<Transaction> Transactions {get; set;}
    }
}