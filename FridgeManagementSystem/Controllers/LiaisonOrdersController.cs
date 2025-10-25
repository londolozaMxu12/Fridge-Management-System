using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
    [Authorize]
    public class LiaisonOrdersController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly IOrderNotificationRepository _notification;
        private readonly ILogger<LiaisonOrdersController> _logger;

        public LiaisonOrdersController(FridgeManagementSystemContext context, IOrderNotificationRepository notification, ILogger<LiaisonOrdersController> logger)
        {
            _context = context;
            _notification = notification;
            _logger = logger;
        }
        private async Task<bool> IsCustomerLiaisonAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees
                .Include(e => e.EmployeeType)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.IsActive);

            return employee?.EmployeeType?.Name == "CustomerLiaison";
        }

        private bool ValidateOrderStatusChange(string newOrderStatus, string currentOrderStatus, string currentPaymentStatus, out string errorMessage)
        {
            errorMessage = null;

            // Define valid status transitions
            var validTransitions = new Dictionary<string, string[]>
            {
                ["Received"] = new[] { "Accepted", "Cancelled" },
                ["Accepted"] = new[] { "Processing", "Shipped", "Cancelled" },
                ["Processing"] = new[] { "Shipped", "Cancelled" },
                ["Shipped"] = new[] { "Delivered", "Returned" },
                ["Delivered"] = new[] { "Returned" },
                ["Cancelled"] = new string[] { },
                ["Returned"] = new string[] { }
            };

            // Check if payment is accepted when trying to accept order
            if (newOrderStatus?.ToLower() == "accepted" && currentPaymentStatus?.ToLower() != "accepted")
            {
                errorMessage = "Cannot accept order: Payment must be accepted first.";
                return false;
            }

            // Check if order is accepted before shipping/delivering
            if ((newOrderStatus?.ToLower() == "shipped" || newOrderStatus?.ToLower() == "delivered") &&
                currentOrderStatus?.ToLower() != "accepted" && currentOrderStatus?.ToLower() != "processing")
            {
                errorMessage = $"Cannot {newOrderStatus.ToLower()} order: Order must be accepted first.";
                return false;
            }

            // Check valid status transitions
            if (validTransitions.ContainsKey(currentOrderStatus))
            {
                var allowedNextStatuses = validTransitions[currentOrderStatus];
                if (!allowedNextStatuses.Contains(newOrderStatus, StringComparer.OrdinalIgnoreCase))
                {
                    errorMessage = $"Cannot change order status from {currentOrderStatus} to {newOrderStatus}. " +
                                  $"Valid next statuses: {string.Join(", ", allowedNextStatuses)}";
                    return false;
                }
            }

            return true;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ThenInclude(i => i.Fridge)
                .ThenInclude(f => f.FridgeType)
                .OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync();
            var pageSize = 5;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var orders = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Orders = orders;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View(orders);
        }
        public async Task<IActionResult> Details(int id)
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items) // Ensure this is included
                .ThenInclude(i => i.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Include(o => o.Fridge)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Check if order has items (for debugging)
            if (order.Items == null || !order.Items.Any())
            {
                _logger.LogWarning($"Order {id} has no items when viewing details");
            }

            var numOrders = await _context.Orders
                .CountAsync(o => o.CustomerId == order.CustomerId);

            ViewBag.NumOrders = numOrders;

            return View(order);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string payment_status, string order_status)
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            try
            {
                var order = await _context.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Fridge)
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound();
                }

                string previousOrderStatus = order.OrderStatus;
                string previousPaymentStatus = order.PaymentStatus;

                if (!string.IsNullOrEmpty(payment_status))
                {
                    order.PaymentStatus = payment_status;

                    // Notify about payment status change
                    if (payment_status != previousPaymentStatus)
                    {
                        await _notification.NotifyAboutPaymentStatusUpdate(order, previousPaymentStatus);
                    }
                }

                if (!string.IsNullOrEmpty(order_status))
                {
                    // Validate the status change
                    if (!ValidateOrderStatusChange(order_status, order.OrderStatus, order.PaymentStatus, out string validationError))
                    {
                        TempData["ErrorMessage"] = validationError;
                        return RedirectToAction(nameof(Details), new { id });
                    }

                    // When order is accepted, it becomes visible in Allocation queue
                    if (order_status.ToLower() == "accepted" && previousOrderStatus?.ToLower() != "accepted")
                    {
                        
                        order.OrderStatus = order_status;
                        await _notification.NotifyLiaisonsAboutOrderReadyForAllocation(order);
                    }
                    // Handle order cancellation - free up reserved fridges
                    else if (order_status.ToLower() == "cancelled" && previousOrderStatus?.ToLower() != "cancelled")
                    {
                        await FreeReservedFridges(order);
                        order.OrderStatus = order_status;
                    }
                    else
                    {
                        order.OrderStatus = order_status;
                    }

                    // Notify about status change
                    if (order_status != previousOrderStatus)
                    {
                        await _notification.NotifyCustomerAboutOrderUpdate(order, previousOrderStatus);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Order updated successfully";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order {id}");
                TempData["ErrorMessage"] = "An error occurred while updating the order";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private async Task FreeReservedFridges(Order order)
        {
            try
            {
                // Check if order has items
                if (order.Items == null || !order.Items.Any())
                {
                    _logger.LogWarning($"Order {order.Id} has no items, no fridges to free.");
                    return;
                }

                var freedFridgeIds = new List<int>();

                // Free all reserved fridges in this order
                foreach (var item in order.Items)
                {
                    var fridge = await _context.Fridges.FindAsync(item.FridgeId);
                    if (fridge != null && fridge.Status == "Reserved")
                    {
                        fridge.Status = "Available";

                        fridge.CustomerId = null;

                        freedFridgeIds.Add(fridge.FridgeId);

                        _logger.LogInformation($"Freed fridge {fridge.FridgeId} from cancelled order {order.Id}");
                    }
                }

                // Remove any allocations for this order
                var allocations = _context.Allocations.Where(a => a.OrderId == order.Id);
                _context.Allocations.RemoveRange(allocations);

                // Clear fridge reference from order
                order.FridgeId = null;

                // Notify about freed fridges
                if (freedFridgeIds.Any())
                {
                    await _notification.NotifyAboutFreedFridges(order, freedFridgeIds);
                }

                _logger.LogInformation($"Freed {freedFridgeIds.Count} reserved fridges from cancelled order {order.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error freeing reserved fridges for order {order.Id}");
            }
        }
        //private async Task<bool> AllocateFridgeToOrder(Order order)
        //{
        //    try
        //    {
        //        // Check if order has items
        //        if (order.Items == null || !order.Items.Any())
        //        {
        //            _logger.LogWarning($"Order {order.Id} has no items, cannot allocate fridge.");
        //            return false;
        //        }

        //        var allocatedFridgeIds = new List<int>();

        //        // Allocate all reserved fridges in this order
        //        foreach (var item in order.Items)
        //        {
        //            var fridge = await _context.Fridges.FindAsync(item.FridgeId);
        //            if (fridge != null && fridge.Status == "Reserved")
        //            {
        //                // Change from Reserved to Allocated
        //                fridge.Status = "Allocated";
        //                fridge.AllocationDate = DateTime.Now;

        //                allocatedFridgeIds.Add(fridge.FridgeId);

        //                _logger.LogInformation($"Allocated fridge {fridge.FridgeId} to order {order.Id}");
        //            }
        //        }

        //        // Create allocation records
        //        foreach (var fridgeId in allocatedFridgeIds)
        //        {
        //            var allocation = new Allocation
        //            {
        //                CustomerId = order.Customer.Id, // You might need to adjust this
        //                AllocatedById = User.FindFirstValue(ClaimTypes.NameIdentifier),
        //                FridgeId = fridgeId,
        //                OrderId = order.Id,
        //                AllocationDate = DateTime.UtcNow
        //            };

        //            _context.Allocations.Add(allocation);
        //        }

        //        // Notify about successful allocation
        //        if (allocatedFridgeIds.Any())
        //        {
        //            // You might need to adjust this notification call based on your Allocation model
        //            await _notification.NotifyAboutFridgeAllocation(order, User.Identity.Name, order.CustomerId);
        //        }

        //        _logger.LogInformation($"Allocated {allocatedFridgeIds.Count} fridges to order {order.Id}");
        //        return allocatedFridgeIds.Any();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error allocating fridges for order {order.Id}");
        //        return false;
        //    }
        //}

    }
}