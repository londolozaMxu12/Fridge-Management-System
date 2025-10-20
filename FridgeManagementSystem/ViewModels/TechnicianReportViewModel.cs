using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class TechnicianReportViewModel
    {
        [Display(Name = "Technician")]
        public int? TechnicianId { get; set; }

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-3);

        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; } = DateTime.Now;

        [Display(Name = "Report Type")]
        public string ReportType { get; set; } = "performance";

        public bool IncludeCharts { get; set; } = true;

        public string TechnicianName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? JoinDate { get; set; }

        public TechnicianPerformanceMetrics Performance { get; set; } = new();
        public List<TechnicianFaultDetail> FaultDetails { get; set; } = new();
        public List<MonthlyPerformance> MonthlyTrends { get; set; } = new();

        public Dictionary<string, int> FaultsByStatus { get; set; } = new();
        public Dictionary<string, int> FaultsByPriority { get; set; } = new();
        public Dictionary<string, decimal> MonthlyCompletionRate { get; set; } = new();

        public bool HasChartData => FaultsByStatus?.Any() == true ||
                               FaultsByPriority?.Any() == true ||
                               MonthlyCompletionRate?.Any() == true;

        public bool HasStatusData => FaultsByStatus?.Any() == true;
        public bool HasPriorityData => FaultsByPriority?.Any() == true;
        public bool HasTrendData => MonthlyCompletionRate?.Any() == true;

        public Dictionary<string, int> AllStatusesWithCounts { get; set; } = new();
        public Dictionary<string, int> AllPrioritiesWithCounts { get; set; } = new();

        // Helper properties to get all possible values
        public static List<string> AllStatuses => Enum.GetNames(typeof(FaultStatus)).ToList();
        public static List<string> AllPriorities => Enum.GetNames(typeof(FaultPriority)).ToList();
    }
}

