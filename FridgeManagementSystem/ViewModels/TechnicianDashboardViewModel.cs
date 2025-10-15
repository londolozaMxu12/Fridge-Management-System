namespace FridgeManagementSystem.ViewModels
{
    public class TechnicianDashboardViewModel
    {
        public List<RepairSchedule> Schedules { get; set; }
        public List<Fault> Faults { get; set; } = new List<Fault>();
        public bool IsFaultTechnician { get; set; }
        public int? CurrentTechnicianId { get; set; }
    }
}
