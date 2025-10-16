namespace FridgeManagementSystem.ViewModels
{
    public class MonthlyFaultStats
    {
        public string Month { get; set; }
        public int ReportedFaults { get; set; }
        public int ResolvedFaults { get; set; }
        public double ResolutionRate { get; set; }
    }
}