namespace BasicApi.Logic.Entities
{
    using System.Collections.Generic;

    /// <summary>
    /// A company and its transactions
    /// </summary>
    public class CompanyTransactions
    {
        public int? Id {get; set;}

        public Company Company {get; set;}

        public IEnumerable<Transaction> Transactions {get; set;}
    }
}