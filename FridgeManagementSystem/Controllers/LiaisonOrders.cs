using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
   // [Authorize(Roles = "CustomerLiaison,Admin")]
    public class LiaisonOrdersController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly IOrderNotificationRepository _notification;

        public LiaisonOrdersController(FridgeManagementSystemContext context, IOrderNotificationRepository notification)
        {
            _context = context;
            _notification = notification;
        }

        public async Task<IActionResult> Index(int pageIndex = 1)
        {
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

            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ThenInclude(i => i.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Include(o => o.Fridge)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var numOrders = await _context.Orders
                .CountAsync(o => o.CustomerId == order.CustomerId);

            ViewBag.NumOrders = numOrders;

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, string payment_status, string order_status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(payment_status))
            {
                order.PaymentStatus = payment_status;
            }

            if (!string.IsNullOrEmpty(order_status))
            {
                order.OrderStatus = order_status;

                if (order_status == "accepted" && order.FridgeId == null)
                {
                    await AllocateFridgeToOrder(order);
                }
            }

            await _context.SaveChangesAsync();
            await _notification.NotifyCustomerAboutOrderUpdate(order);

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task AllocateFridgeToOrder(Order order)
        {
            var orderedFridgeId = order.Items.First().FridgeId;

            var availableFridge = await _context.Fridges
                .Include(f => f.FridgeType)
                .FirstOrDefaultAsync(f =>
                    f.Status == "Available" &&
                    f.FridgeId == orderedFridgeId);

            if (availableFridge != null)
            {
                availableFridge.Status = "Allocated";
                availableFridge.AllocationDate = DateTime.UtcNow;

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.UserId == order.CustomerId);
                if (customer != null)
                {
                    availableFridge.CustomerId = customer.Id;
                }

                order.FridgeId = availableFridge.FridgeId;

                var allocation = new Allocation
                {
                    CustomerId = availableFridge.CustomerId.Value,
                    AllocatedById = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    FridgeId = availableFridge.FridgeId,
                    AllocationDate = DateTime.UtcNow
                };

                _context.Allocations.Add(allocation);
                await _notification.NotifyAboutFridgeAllocation(allocation, User.Identity.Name);
            }
        }
    }
}