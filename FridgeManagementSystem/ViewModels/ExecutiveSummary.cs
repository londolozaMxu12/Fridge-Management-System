namespace FridgeManagementSystem.ViewModels
{
    public class ExecutiveSummary
    {
        public int TotalFaults { get; set; }
        public int ResolvedFaults { get; set; }
        public double ResolutionRate { get; set; }
        public double AvgResolutionTime { get; set; }
        
        public string? TopPerformingTechnician { get; set; }
        public string? MostCommonFaultType { get; set; }
        public string? KeyInsight { get; set; }
    }
}