namespace BasicApi.Entities
{
    /*
     * A single transaction
     */
    public class Transaction
    {
        public string Id {get; set;}

        public string InvestorId {get; set;}

        public double AmountUsd {get; set;}
    }
}