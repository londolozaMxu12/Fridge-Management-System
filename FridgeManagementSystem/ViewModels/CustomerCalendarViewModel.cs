namespace FridgeManagementSystem.ViewModels
{
    public class CustomerCalendarViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public List<RepairSchedule> UpcomingSchedules { get; set; } = new List<RepairSchedule>();
    }
}
