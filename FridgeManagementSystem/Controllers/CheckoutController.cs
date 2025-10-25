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
    public class CheckoutController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly IOrderNotificationRepository _notification;

        public CheckoutController(FridgeManagementSystemContext context, IOrderNotificationRepository notification)
        {
            _context = context;
            _notification = notification;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);

            var cart = await _context.ShoppingCart
                .Include(c => c.Items)
                .ThenInclude(i => i.Fridge)
                .ThenInclude(f => f.FridgeType)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            ViewBag.Total = cart.Items.Sum(i => i.Quantity * i.Fridge.Price);
            ViewBag.DeliveryAddress = user?.Address ??", " + user?.Suburb ?? ", " + user?.City ?? "No address provided";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);

            var cart = await _context.ShoppingCart
                .Include(c => c.Items)
                .ThenInclude(i => i.Fridge)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return Json(new { success = false, message = "Cart is empty, no fridge added yet" });
            }

            var order = new Order
            {
                CustomerId = userId,
                DeliveryAddress = user?.Address?? ", "+ user?.Suburb?? ", " + user?.City ?? "",
                PaymentMethod = "CashOnDelivery",
                PaymentStatus = "pending",
                OrderStatus = "Received",
                ShippingFee = 0,
                CreatedAt = DateTime.Now
            };

            foreach (var item in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    FridgeId = item.FridgeId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Fridge.Price
                });

                var fridge = await _context.Fridges.FindAsync(item.FridgeId);
                if (fridge != null)
                {
                    fridge.Status = "Allocated";
                    fridge.AllocationDate = DateTime.UtcNow;

                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.UserId == userId);
                    if (customer != null)
                    {
                        fridge.CustomerId = customer.Id;
                    }
                }
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            await _notification.NotifyLiaisonsAboutNewOrder(order);

            return Json(new { success = true, orderId = order.Id });
        }

        public async Task<IActionResult> OrderConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Fridge)
                .ThenInclude(f => f.FridgeType)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}