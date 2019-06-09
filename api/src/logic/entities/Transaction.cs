namespace BasicApi.Logic.Entities
{
    /// <summary>
    /// A single transaction 
    /// </summary>
    public class Transaction
    {
        public string Id {get; set;}

        public string InvestorId {get; set;}

        public double AmountUsd {get; set;}
    }
}