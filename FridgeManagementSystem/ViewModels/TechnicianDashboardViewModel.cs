namespace FridgeManagementSystem.ViewModels
{
    public class TechnicianDashboardViewModel
    {
        // Statistics
        public int TotalFaults { get; set; }
        public int UnattendedFaults { get; set; }
        public int UrgentFaults { get; set; }
        public int MyActiveFaults { get; set; }
        public int MyCompletedFaults { get; set; }
        public double CompletionRate { get; set; }

        // Lists
        public List<RepairSchedule> UpcomingSchedules { get; set; } = new();
        public List<Fault> RecentUnattendedFaults { get; set; } = new();
        public List<Fault> MyRecentFaults { get; set; } = new();

        // Technician info
        public bool IsFaultTechnician { get; set; }
        public int? CurrentTechnicianId { get; set; }
        public string? TechnicianName { get; set; }

        // Existing properties
        public List<Fault> Faults { get; set; } = new();
        public List<RepairSchedule> Schedules { get; set; } = new();
    }
}
