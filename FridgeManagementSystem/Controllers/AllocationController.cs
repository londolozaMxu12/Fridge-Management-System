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
        // GET: List all orders that need allocation (Accepted orders without allocated fridges)
        public async Task<IActionResult> Index()
        {
            if (!await IsCustomerLiaisonAsync())
            {
                return Forbid();
            }

            var ordersNeedingAllocation = await _context.Orders
        .Include(o => o.Customer)
        .Include(o => o.Items)
            .ThenInclude(i => i.Fridge)
            .ThenInclude(f => f.FridgeType)
        .Where(o => o.OrderStatus == "Accepted" &&
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
                .Include(o => o.Items)
                    .ThenInclude(i => i.Fridge)
                    .ThenInclude(f => f.FridgeType)
                .Include(o => o.Allocations)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Get reserved fridges for this order
            var reservedFridgeIds = order.Items
                .Where(i => i.Fridge != null && i.Fridge.Status == "Reserved")
                .Select(i => i.FridgeId)
                .ToList();

            var reservedFridges = await _context.Fridges
                .Include(f => f.FridgeType)
                .Where(f => reservedFridgeIds.Contains(f.FridgeId))
                .ToListAsync();

            ViewBag.ReservedFridges = reservedFridges;
            return View(order);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Allocate(int orderId, int fridgeId)
        {
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

                // Check if fridge is Reserved
                if (fridge.Status != "Reserved")
                {
                    TempData["ErrorMessage"] = $"Fridge {fridge.SerialNumber} is not reserved for allocation. Current status: {fridge.Status}";
                    return RedirectToAction(nameof(Details), new { orderId });
                }

                // Update fridge status from Reserved to Allocated
                fridge.Status = "Allocated";
                fridge.AllocationDate = DateTime.UtcNow;

                // Link fridge to order
                order.FridgeId = fridgeId;

                var customerEntity = await _context.Customers
                     .FirstOrDefaultAsync(c => c.UserId == order.CustomerId);

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
                    AllocationDate = DateTime.UtcNow
                };

                _context.Allocations.Add(allocation);

                // Update order status to Processing after allocation
                //order.OrderStatus = "Accepted";

                await _context.SaveChangesAsync();

                // Notify customer and liaisons
                await _notification.NotifyAboutFridgeAllocation(order, User.Identity.Name, order.CustomerId);

                TempData["SuccessMessage"] = $"Fridge {fridge.SerialNumber} allocated successfully to order {orderId}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error allocating fridge {fridgeId} to order {orderId}");
                TempData["ErrorMessage"] = "An error occurred during allocation.";
                return RedirectToAction(nameof(Details), new { orderId });
            }
        }

        // GET: Allocation history
        public async Task<IActionResult> History()
        {
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

                // Return an empty list, not a single Allocation
                return View(new List<Allocation>());
            }
        }
    }
}