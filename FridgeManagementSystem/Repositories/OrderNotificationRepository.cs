using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Services
{
    public class OrderNotificationRepository : IOrderNotificationRepository
    {
        private readonly FridgeManagementSystemContext _context;

        public OrderNotificationRepository(FridgeManagementSystemContext context)
        {
            _context = context;
        }

        public async Task NotifyCustomerLiaisonsAboutNewOrder(Order order)
        {
            
            var customerLiaisons = await _context.Users
                .Where(u => _context.Employees
                    .Any(e => e.UserId == u.Id &&
                             e.EmployeeType.Name == "CustomerLiaison" &&
                             e.IsActive))
                .ToListAsync();

            foreach (var liaison in customerLiaisons)
            {
                var notification = new Notification
                {
                    UserId = liaison.Id,
                    Title = "New Order Received",
                    Message = $"New order #{order.Id} has been placed, and requires processing.",
                    Link = $"/LiaisonOrders/Details/{order.Id}"
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();
        }

        public async Task NotifyCustomerAboutOrderUpdate(Order order)
        {
            var notification = new Notification
            {
                UserId = order.CustomerId,
                Title = "Order Status Updated",
                Message = $"Your order #{order.Id} status has been updated to {order.OrderStatus}.",
                Link = $"/CustomerOrders/Details/{order.Id}"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task NotifyAboutFridgeAllocation(Allocation allocation, string allocatedByName)
        {
            
            var otherLiaisons = await _context.Users
                .Where(u => u.Id != allocation.AllocatedById &&
                           _context.Employees
                               .Any(e => e.UserId == u.Id &&
                                        e.EmployeeType.Name == "CustomerLiaison" &&
                                        e.IsActive))
                .ToListAsync();

            foreach (var liaison in otherLiaisons)
            {
                var notification = new Notification
                {
                    UserId = liaison.Id,
                    Title = "Fridge Allocated",
                    Message = $"Customer {allocation.Customer.User.FullName} has been allocated a fridge by {allocatedByName}.",
                    Link = $"LiaisonOrders/Details/{allocation.AllocationId}"
                };

                _context.Notifications.Add(notification);
            }

            var customerUserId = await _context.Customers
                .Where(c => c.Id == allocation.CustomerId)
                .Select(c => c.UserId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(customerUserId))
            {
                var customerNotification = new Notification
                {
                    UserId = customerUserId,
                    Title = "Fridge Allocated",
                    Message = $"A fridge has been allocated to your business. Serial: {allocation.Fridge.SerialNumber}",
                    Link = $"/CustomerOrders/Details/{allocation.FridgeId}"
                };

                _context.Notifications.Add(customerNotification);
            }

            await _context.SaveChangesAsync();
        }
    }
}