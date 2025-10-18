namespace FridgeManagementSystem.ViewModels
{
    public class TechnicianPerformanceViewModel
    {
        // Basic properties
       
        public int TotalFaultsAssigned { get; set; }
        public int CompletedFaults { get; set; }
        public int InProgressFaults { get; set; }
        public int OverdueFaults { get; set; }
        public double CompletionRate { get; set; }
        public double EfficiencyScore { get; set; }
        public double AverageCompletionTimeHours { get; set; }

        // Collections
        public List<MonthlyPerformance> MonthlyPerformance { get; set; }
        public List<PriorityMetrics> PriorityMetrics { get; set; }

        // Additional properties from your controller
        public Employee Technician { get; set; }
        public DateTime? ReportDateFrom { get; set; }
        public DateTime? ReportDateTo { get; set; }
        public List<PerformanceTrend> PerformanceTrends { get; set; }
        public List<TechnicianComparison> TechnicianComparisons { get; set; }

        // Export Properties
        public string ReportTitle => "Comprehensive Fault Analysis Report";
        public string GeneratedOn => DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        public string ReportPeriod => $"{ReportDateFrom?.ToString("yyyy-MM-dd")} to {ReportDateTo?.ToString("yyyy-MM-dd")}";

    }
}
