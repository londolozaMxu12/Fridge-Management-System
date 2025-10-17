namespace FridgeManagementSystem.ViewModels
{
    public class FaultTrend
    {
        public string Period { get; set; }
        public int ReportedFaults { get; set; }
        public int ResolvedFaults { get; set; }
        public int CriticalFaults { get; set; }
        public double ResolutionRate { get; set; }
        
        public double TrendDirection { get; set; } // -1 decreasing, 0 stable, 1 increasing
    }
}