namespace FridgeManagementSystem.ViewModels
{
    public class TechnicianPerformanceViewModel
    {
        public Employee Technician { get; set; }
        public DateTime? ReportDateFrom { get; set; }
        public DateTime? ReportDateTo { get; set; }

        // Core Metrics
        public int TotalFaultsAssigned { get; set; }
        public int CompletedFaults { get; set; }
        public int InProgressFaults { get; set; }
        public int OverdueFaults { get; set; }
        public double CompletionRate { get; set; }

        // Graphical Data
        public List<MonthlyPerformance> MonthlyPerformance { get; set; } = new();
        public List<PerformanceTrend> PerformanceTrends { get; set; } = new();
        public List<PriorityMetrics> PriorityMetrics { get; set; } = new();

        // Export Data
        public string ReportTitle => $"Technician Performance Report - {Technician?.User?.FullName}";
        public string GeneratedOn => DateTime.Now.ToString("dd-MM-yyyy HH:mm");
        public string ReportPeriod => $"{ReportDateFrom?.ToString("dd-MM-yyyy")} to {ReportDateTo?.ToString("dd-MM-yyyy")}";

    }
}
