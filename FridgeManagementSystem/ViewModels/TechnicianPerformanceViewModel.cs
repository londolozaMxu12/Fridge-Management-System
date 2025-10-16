namespace FridgeManagementSystem.ViewModels
{
    public class TechnicianPerformanceViewModel
    {
        public Employee Technician { get; set; }
        public int TotalFaultsAssigned { get; set; }
        public int CompletedFaults { get; set; }
        public int InProgressFaults { get; set; }
        public int OverdueFaults { get; set; }
        public double CompletionRate { get; set; }
        public double AverageCompletionTimeHours { get; set; }
        public int CustomerSatisfactionScore { get; set; } // 1-5 scale

        // Monthly statistics
        public List<MonthlyPerformance> MonthlyPerformance { get; set; } = new();

        // Priority breakdown
        public int CriticalFaultsCompleted { get; set; }
        public int HighFaultsCompleted { get; set; }
        public int MediumFaultsCompleted { get; set; }
        public int LowFaultsCompleted { get; set; }

        // Recent activity
        public List<Fault> RecentCompletedFaults { get; set; } = new();
    }
}
