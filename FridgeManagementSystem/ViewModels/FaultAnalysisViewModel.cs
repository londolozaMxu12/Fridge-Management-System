namespace FridgeManagementSystem.ViewModels
{
    public class FaultAnalysisViewModel
    {
        // Overall statistics
        public int TotalFaults { get; set; }
        public int ResolvedFaults { get; set; }
        public double ResolutionRate { get; set; }

        // Status distribution
        public Dictionary<FaultStatus, int> FaultsByStatus { get; set; } = new();

        // Priority distribution
        public Dictionary<FaultPriority, int> FaultsByPriority { get; set; } = new();

        // Monthly trends
        public List<MonthlyFaultStats> MonthlyTrends { get; set; } = new();

        // Common fault types
        public List<FaultTypeStats> CommonFaultTypes { get; set; } = new();

        // Technician performance comparison
        public List<TechnicianStats> TechnicianStats { get; set; } = new();

        // Fridge type analysis
        public List<FridgeTypeStats> FridgeTypeStats { get; set; } = new();
    }
}
