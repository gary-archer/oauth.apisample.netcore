namespace FinalApi.Logic.Entities
{
    /*
     * Details for a company
     */
    public class Company
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Region { get; set; }

        public double TargetUsd { get; set; }

        public double InvestmentUsd { get; set; }

        public int NoInvestors { get; set; }
    }
}