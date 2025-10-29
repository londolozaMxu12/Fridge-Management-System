using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.Services;
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

        // Helper method to get customer details from order
        private (string name, string email, string phone) GetCustomerDetails(Order order)
        {
            if (order?.Customer?.User == null)
                return ("Unknown", "Unknown", "Unknown");

            return (
                order.Customer.User.FullName ?? "Unknown",
                order.Customer.User.Email ?? "Unknown",
                order.Customer.User.ContactNo ?? "Unknown"
            );
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            try
            {
                var query = _context.Orders
                    .Include(o => o.Customer)          // Include Customer
                        .ThenInclude(c => c.User)      // Then include User from Customer
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Fridge)
                        .ThenInclude(f => f.FridgeType)
                    .OrderByDescending(o => o.CreatedAt);

                var totalCount = await query.CountAsync();
                var pageSize = 5;
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Ensure pageIndex is within valid range
                pageIndex = Math.Max(1, Math.Min(pageIndex, totalPages > 0 ? totalPages : 1));

                var orders = await query
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.PageIndex = pageIndex;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalCount = totalCount;

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders for liaison");
                TempData["ErrorMessage"] = "An error occurred while loading orders.";
                return View(new List<Order>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)          // Include Customer
                        .ThenInclude(c => c.User)      // Then include User from Customer
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Fridge)
                        .ThenInclude(f => f.FridgeType)
                    .Include(o => o.Allocations)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if order has items (for debugging)
                if (order.Items == null || !order.Items.Any())
                {
                    _logger.LogWarning($"Order {id} has no items when viewing details");
                }
                else
                {
                    _logger.LogInformation($"Order {id} has {order.Items.Count} items");
                }

                // Get customer details
                var customerDetails = GetCustomerDetails(order);

                // Count orders for this customer
                var numOrders = await _context.Orders
                    .CountAsync(o => o.CustomerId == order.CustomerId);

                // Calculate order total from items
                var orderTotal = order.Items.Sum(item => item.UnitPrice * item.Quantity) + order.ShippingFee;

                ViewBag.NumOrders = numOrders;
                ViewBag.CustomerName = customerDetails.name;
                ViewBag.CustomerEmail = customerDetails.email;
                ViewBag.CustomerPhone = customerDetails.phone;
                ViewBag.OrderTotal = orderTotal;
                ViewBag.ItemCount = order.Items.Count;

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading order details for ID: {id}");
                TempData["ErrorMessage"] = "An error occurred while loading order details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? payment_status, string? order_status)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Fridge)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Store original values
                var originalPaymentStatus = order.PaymentStatus;
                var originalOrderStatus = order.OrderStatus;

                // Apply business rules - without logger since you commented it out
                var businessRules = new OrderBusinessRules(_context); 

                if (!string.IsNullOrEmpty(payment_status))
                {
                    var newPaymentStatus = payment_status.Trim();
                    await businessRules.ApplyBusinessRulesAsync(order, newPaymentStatus, originalOrderStatus);
                    order.PaymentStatus = newPaymentStatus;
                }

                if (!string.IsNullOrEmpty(order_status))
                {
                    var newOrderStatus = order_status.Trim();

                    if (!IsValidStatusTransition(order.OrderStatus, newOrderStatus))
                    {
                        TempData["ErrorMessage"] = $"Invalid status transition from {order.OrderStatus} to {newOrderStatus}.";
                        return RedirectToAction(nameof(Details), new { id });
                    }

                    await businessRules.ApplyBusinessRulesAsync(order, originalPaymentStatus, newOrderStatus);
                    order.OrderStatus = newOrderStatus;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Order updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", id);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }


        // Business Rule: Valid status transitions
        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            var validTransitions = new Dictionary<string, List<string>>
            {
                ["Received"] = new List<string> { "Accepted", "Cancelled" },
                ["Accepted"] = new List<string> { "Processing", "Cancelled" },
                ["Processing"] = new List<string> { "Shipped", "Cancelled" },
                ["Shipped"] = new List<string> { "Delivered", "Returned" },
                ["Delivered"] = new List<string> { "Returned" },
                ["Cancelled"] = new List<string> { }, // Once cancelled, cannot change
                ["Returned"] = new List<string> { }   // Once returned, cannot change
            };

            currentStatus = currentStatus ?? "Received";
            return validTransitions.ContainsKey(currentStatus) &&
                   validTransitions[currentStatus].Contains(newStatus);
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

                // No need to clear fridge reference from order since FridgeId is removed

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
                throw; // Re-throw to handle in calling method
            }
        }

        // Additional helper methods for customer data access
        public async Task<IActionResult> GetCustomerInfo(string customerId)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == customerId);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found" });
                }

                return Json(new
                {
                    success = true,
                    customerName = customer.User.FullName,
                    customerEmail = customer.User.Email,
                    customerPhone = customer.User.PhoneNumber,
                    businessName = customer.BusinessName,
                    customerType = customer.CustomerType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer info for {customerId}");
                return Json(new { success = false, message = "Error retrieving customer information" });
            }
        }

        // Method to get orders with customer details for export or reporting
        public async Task<IActionResult> OrdersWithCustomerDetails()
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Customer)
                        .ThenInclude(c => c.User)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Fridge)
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new
                    {
                        OrderId = o.Id,
                        CustomerName = o.Customer.User.FullName,
                        CustomerEmail = o.Customer.User.Email,
                        BusinessName = o.Customer.BusinessName,
                        OrderStatus = o.OrderStatus,
                        PaymentStatus = o.PaymentStatus,
                        TotalAmount = o.Items.Sum(i => i.UnitPrice * i.Quantity) + o.ShippingFee,
                        CreatedAt = o.CreatedAt,
                        ItemCount = o.Items.Count,
                        FridgeDetails = o.Items.Select(i => new
                        {
                            FridgeId = i.FridgeId,
                            FridgeName = i.Fridge.FridgeType.Name,
                            Brand = i.Fridge.FridgeType.Brand,
                            Model = i.Fridge.FridgeType.Model
                        })
                    })
                    .ToListAsync();

                return Json(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders with customer details");
                return Json(new { error = "An error occurred while loading orders" });
            }
        }

        // New method to get order items with fridge details
        public async Task<IActionResult> GetOrderItems(int orderId)
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            try
            {
                var orderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .Include(oi => oi.Fridge)
                        .ThenInclude(f => f.FridgeType)
                    .Select(oi => new
                    {
                        OrderItemId = oi.Id,
                        FridgeId = oi.FridgeId,
                        FridgeName = oi.Fridge.FridgeType.Name,
                        Brand = oi.Fridge.FridgeType.Brand,
                        Model = oi.Fridge.FridgeType.Model,
                        SerialNumber = oi.Fridge.SerialNumber,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice,
                        FridgeStatus = oi.Fridge.Status
                    })
                    .ToListAsync();

                return Json(new { success = true, items = orderItems });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading order items for order {orderId}");
                return Json(new { success = false, message = "Error loading order items" });
            }
        }

        // Helper method to generate invoice
        private async void GenerateInvoice(Order order)
        {
            try
            {
                // Check if invoice already exists
                var existingInvoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.OrderId == order.Id);

                if (existingInvoice == null)
                {
                    // Use your invoice service to generate invoice
                    var invoiceService = new InvoiceService(_context);
                    await invoiceService.GenerateInvoiceAsync(order.Id);
                    _logger.LogInformation("Invoice generated for order {OrderId}", order.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for order {OrderId}", order.Id);
                // Don't throw - invoice generation failure shouldn't block order update
            }
        }

    }
}