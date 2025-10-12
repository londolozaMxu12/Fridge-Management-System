namespace FridgeManagementSystem.ViewModels
{
    public class FaultDetailsViewModel
    {
        public int FaultId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public FaultPriority Priority { get; set; }
        public FaultStatus Status { get; set; }
        public DateTime ReportedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string ResolutionNotes { get; set; }
        public string FaultTechnician { get; set; }
        public string ReportedBy { get; set; }
        public string BusinessName { get; set; }
        public string FridgeInfo { get; set; }
        public List<RepairScheduleViewModel> RepairSchedules { get; set; } = new();
    }
}
