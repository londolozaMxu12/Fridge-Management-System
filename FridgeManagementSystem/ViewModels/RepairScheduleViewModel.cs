namespace FridgeManagementSystem.ViewModels
{
    public class RepairScheduleViewModel
    {
        public int RepairScheduleId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Technician { get; set; }
        public ScheduleStatus Status { get; set; }
        public string Notes { get; set; }
        public decimal EstimatedHours { get; set; }
    }
}