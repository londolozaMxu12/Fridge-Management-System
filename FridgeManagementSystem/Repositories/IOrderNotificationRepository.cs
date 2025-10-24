namespace FridgeManagementSystem.Repositories
{
    public interface IOrderNotificationRepository
    {
        Task NotifyCustomerLiaisonsAboutNewOrder(Order order);
        Task NotifyCustomerAboutOrderUpdate(Order order);
        Task NotifyAboutFridgeAllocation(Allocation allocation, string allocatedByName);
    }
}
