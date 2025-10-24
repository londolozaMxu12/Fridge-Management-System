using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;

namespace FridgeManagementSystem.Controllers
{
    [Authorize]
    public class CustomerOrdersController : Controller
    {
        private readonly FridgeManagementSystemContext _context;

        public CustomerOrdersController(FridgeManagementSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.CreatedAt);

            var totalCount = await query.CountAsync();
            var pageSize = 5;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var orders = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Include(o => o.Fridge)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}