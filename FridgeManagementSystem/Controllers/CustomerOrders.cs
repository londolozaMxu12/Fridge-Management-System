using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;

namespace FridgeManagementSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerOrdersController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly ILogger<CustomerOrdersController> _logger;

        public CustomerOrdersController(FridgeManagementSystemContext context, ILogger<CustomerOrdersController> logger = null)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Challenge();
                }

                var query = _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Fridge)
                    .ThenInclude(f => f.FridgeType)
                    .Where(o => o.CustomerId == userId) 
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
                _logger?.LogError(ex, "Error loading orders for customer");
                TempData["Error"] = "An error occurred while loading your orders.";
                return View(new List<Order>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Challenge();
                }

                var order = await _context.Orders
                    .Include(o => o.Items)
                    .ThenInclude(i => i.Fridge)
                    .ThenInclude(f => f.FridgeType)
                    .Include(o => o.Fridge)
                    .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId); 

                if (order == null)
                {
                    TempData["Error"] = "Order not found or you don't have permission to view it.";
                    return RedirectToAction(nameof(Index));
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading order details. OrderId: {OrderId}", id);
                TempData["Error"] = "An error occurred while loading order details.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}