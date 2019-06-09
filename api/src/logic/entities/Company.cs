namespace BasicApi.Logic.Entities
{
    /// <summary>
    /// Details for a company
    /// </summary>
    public class Company
    {
        public int Id {get; set;}

        public string Name {get; set;}

        public double TargetUsd {get; set;}

        public double InvestmentUsd {get; set;}

        public int NoInvestors {get; set;}
    }
}