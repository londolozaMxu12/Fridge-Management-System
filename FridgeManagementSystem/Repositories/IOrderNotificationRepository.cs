namespace FridgeManagementSystem.Repositories
{
    public interface IOrderNotificationRepository
    {
        Task NotifyCustomerAboutOrderUpdate(Order order, string previousStatus);
        Task NotifyAboutFridgeAllocation(Order order, string allocatedBy, string customerId);
        Task NotifyAboutStockShortage(int fridgeId, int orderId);
        Task NotifyAboutPaymentStatusUpdate(Order order, string previousPaymentStatus);
        Task NotifyLiaisonsAboutNewOrder(Order order);
        Task NotifyAboutFreedFridges(Order order, List<int> freedFridgeIds);
        Task NotifyLiaisonsAboutOrderReadyForAllocation(Order order);
    }
}
