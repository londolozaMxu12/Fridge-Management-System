using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Areas.Identity.Data
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string title, string message, string link = null);
        Task NotifyFridgeAllocationAsync(Allocation allocation, string allocatedByUser);
    }

    public class NotificationService : INotificationService
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(FridgeManagementSystemContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task CreateNotificationAsync(string userId, string title, string message, string link = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Link = link,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task NotifyFridgeAllocationAsync(Allocation allocation, string allocatedByUser)
        {
            // Get allocation details with related data
            var allocationDetails = await _context.Allocations
                .Include(a => a.Customer)
                .ThenInclude(c => c.User)
                .Include(a => a.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Include(a => a.AllocatedBy)
                .FirstOrDefaultAsync(a => a.AllocationId == allocation.AllocationId);

            if (allocationDetails == null) return;

            var customer = allocationDetails.Customer;
            var fridge = allocationDetails.Fridge;
            var allocatedBy = allocationDetails.AllocatedBy;

            // Notification for Customer
            var customerNotification = new Notification
            {
                UserId = customer.UserId,
                Title = "Fridge Allocation Confirmed",
                Message = $"Your fridge ({fridge.FridgeType.Brand} {fridge.FridgeType.Name}) with serial number {fridge.SerialNumber} has been allocated to your business. It will be delivered soon.",
                Link = $"/Customer/MyFridges",
                CreatedAt = DateTime.UtcNow
            };

            // Notification for Admin users
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in adminUsers)
            {
                var adminNotification = new Notification
                {
                    UserId = admin.Id,
                    Title = "Fridge Allocated to Customer",
                    Message = $"Fridge {fridge.SerialNumber} ({fridge.FridgeType.Brand} {fridge.FridgeType.Name}) has been allocated to {customer.User.FullName} ({customer.BusinessName}) by {allocatedBy.FullName}.",
                    Link = $"/Allocation/Details/{allocation.AllocationId}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(adminNotification);
            }

            _context.Notifications.Add(customerNotification);
            await _context.SaveChangesAsync();
        }
    }
}
