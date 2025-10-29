using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
    [Authorize]
    public class AllocationController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderNotificationRepository _notification;
        private readonly ILogger<AllocationController> _logger;

        public AllocationController(
            FridgeManagementSystemContext context,
            UserManager<ApplicationUser> userManager,
            IOrderNotificationRepository notification,
            ILogger<AllocationController> logger)
        {
            _context = context;
            _userManager = userManager;
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

        // GET: List all orders that need allocation (Accepted orders with reserved fridges)
        public async Task<IActionResult> Index()
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            var ordersNeedingAllocation = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Fridge)
                    .ThenInclude(f => f.FridgeType)
                .Where(o => o.OrderStatus == "Accepted" || o.OrderStatus == "Processing" || o.OrderStatus == "Shipped" &&
                           o.Items.Any(i => i.Fridge.Status == "Reserved"))
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            return View(ordersNeedingAllocation);
        }

        // GET: Show allocation details for a specific order
        public async Task<IActionResult> Details(int orderId)
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            var order = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(c => c.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Fridge)
                    .ThenInclude(f => f.FridgeType)
                .Include(o => o.Allocations)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Get reserved fridges for this order from OrderItems
            var reservedFridges = order.Items
                .Where(i => i.Fridge != null && i.Fridge.Status == "Reserved")
                .Select(i => i.Fridge)
                .ToList();

            ViewBag.ReservedFridges = reservedFridges;
            ViewBag.OrderTotal = order.Items.Sum(i => i.Quantity * i.UnitPrice) + order.ShippingFee;
            ViewBag.TotalUnits = order.Items.Sum(i => i.Quantity);
            ViewBag.ReservedCount = reservedFridges.Count;

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Allocate(int orderId, int fridgeId)
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Fridge)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                var fridge = await _context.Fridges
                    .Include(f => f.FridgeType)
                    .FirstOrDefaultAsync(f => f.FridgeId == fridgeId);

                if (order == null || fridge == null)
                {
                    TempData["ErrorMessage"] = "Order or fridge not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if this fridge is part of the order's reserved items
                var orderItem = order.Items.FirstOrDefault(i => i.FridgeId == fridgeId);
                if (orderItem == null)
                {
                    TempData["ErrorMessage"] = $"Fridge {fridge.SerialNumber} is not part of order {orderId}.";
                    return RedirectToAction(nameof(Details), new { orderId });
                }

                // Check if fridge is Reserved
                if (fridge.Status != "Reserved")
                {
                    TempData["ErrorMessage"] = $"Fridge {fridge.SerialNumber} is not reserved for allocation. Current status: {fridge.Status}";
                    return RedirectToAction(nameof(Details), new { orderId });
                }

                // Update fridge status from Reserved to Allocated
                fridge.Status = "Allocated";
                fridge.AllocationDate = DateTime.UtcNow;
                fridge.CustomerId = order.CustomerId; // Associate fridge with customer

                var customerEntity = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == order.CustomerId);

                if (customerEntity == null)
                {
                    TempData["ErrorMessage"] = "Customer record not found.";
                    return RedirectToAction(nameof(Details), new { orderId });
                }

                // Create allocation record
                var allocation = new Allocation
                {
                    CustomerId = customerEntity.Id,
                    AllocatedById = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    FridgeId = fridgeId,
                    OrderId = orderId,
                    AllocationDate = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Allocations.Add(allocation);

                await _context.SaveChangesAsync();

                // Check if all fridges in the order are now allocated
                var remainingReservedFridges = order.Items
                    .Where(i => i.Fridge.Status == "Reserved")
                    .ToList();

                if (!remainingReservedFridges.Any())
                {
                    // All fridges allocated, update order status to Processing
                    order.OrderStatus = "Processing";
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"All fridges allocated for order {orderId}. Order status updated to Processing.");
                }

                // Notify customer and liaisons
                await _notification.NotifyAboutFridgeAllocation(order, User.Identity.Name, order.CustomerId);

                TempData["SuccessMessage"] = $"Fridge {fridge.SerialNumber} allocated successfully to order {orderId}";
                return RedirectToAction(nameof(Details), new { orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error allocating fridge {fridgeId} to order {orderId}");
                TempData["ErrorMessage"] = "An error occurred during allocation.";
                return RedirectToAction(nameof(Details), new { orderId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AllocateAll(int orderId)
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Fridge)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                var reservedFridges = order.Items
                    .Where(i => i.Fridge != null && i.Fridge.Status == "Reserved")
                    .Select(i => i.Fridge)
                    .ToList();

                if (!reservedFridges.Any())
                {
                    TempData["ErrorMessage"] = "No reserved fridges found to allocate.";
                    return RedirectToAction(nameof(Details), new { orderId });
                }

                var allocatedFridgeIds = new List<int>();
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                foreach (var fridge in reservedFridges)
                {
                    // Update fridge status from Reserved to Allocated
                    fridge.Status = "Allocated";
                    fridge.AllocationDate = DateTime.UtcNow;
                    fridge.CustomerId = order.CustomerId;

                    // Create allocation record for each fridge
                    var allocation = new Allocation
                    {
                        CustomerId = order.CustomerId,
                        AllocatedById = userId,
                        FridgeId = fridge.FridgeId,
                        OrderId = orderId,
                        AllocationDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.Allocations.Add(allocation);
                    allocatedFridgeIds.Add(fridge.FridgeId);
                }

                // Update order status to Processing since all fridges are allocated
                order.OrderStatus = "Processing";

                await _context.SaveChangesAsync();

                // Notify customer and liaisons
                await _notification.NotifyAboutFridgeAllocation(order, User.Identity.Name, order.CustomerId);

                TempData["SuccessMessage"] = $"All {allocatedFridgeIds.Count} fridges allocated successfully to order {orderId}";
                return RedirectToAction(nameof(Details), new { orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error allocating all fridges for order {orderId}");
                TempData["ErrorMessage"] = "An error occurred during bulk allocation.";
                return RedirectToAction(nameof(Details), new { orderId });
            }
        }

        // GET: Allocation history
        public async Task<IActionResult> History()
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            try
            {
                var allocations = await _context.Allocations
                    .Include(a => a.Order)
                    .Include(a => a.Fridge)
                        .ThenInclude(f => f.FridgeType)
                    .Include(a => a.Customer)
                        .ThenInclude(c => c.User)
                    .Include(a => a.AllocatedBy)
                    .Where(a => a.IsActive)
                    .OrderByDescending(a => a.AllocationDate)
                    .ToListAsync();

                return View(allocations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading allocation history");
                return View(new List<Allocation>());
            }
        }

        // GET: Get allocation statistics
        public async Task<IActionResult> Statistics()
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            try
            {
                var totalAllocations = await _context.Allocations.CountAsync(a => a.IsActive);
                var todayAllocations = await _context.Allocations
                    .CountAsync(a => a.IsActive && a.AllocationDate.Date == DateTime.UtcNow.Date);

                var pendingOrders = await _context.Orders
                    .CountAsync(o => o.OrderStatus == "Accepted" &&
                                   o.Items.Any(i => i.Fridge.Status == "Reserved"));

                var stats = new
                {
                    TotalAllocations = totalAllocations,
                    TodayAllocations = todayAllocations,
                    PendingOrders = pendingOrders
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading allocation statistics");
                return Json(new { error = "Error loading statistics" });
            }
        }
    }
}