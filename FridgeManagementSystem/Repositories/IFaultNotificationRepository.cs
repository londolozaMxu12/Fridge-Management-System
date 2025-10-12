namespace FridgeManagementSystem.Repositories
{
    public interface IFaultNotificationRepository
    {
        Task NotifyFaultReportedAsync(Fault fault);
        Task NotifyFaultAttendedAsync(Fault fault, Employee attendingTechnician);
        Task NotifyFaultStatusUpdateAsync(Fault fault, string oldStatus);
        Task NotifyRepairScheduledAsync(RepairSchedule schedule);
    }
}
