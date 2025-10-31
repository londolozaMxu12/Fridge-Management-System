using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using System.Security.Claims;

namespace FridgeManagementSystem.ViewComponents
{
    public class CustomerCartViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FridgeManagementSystemContext _context;

        public CustomerCartViewComponent(
            UserManager<ApplicationUser> userManager,
            FridgeManagementSystemContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
                if (isCustomer)
                {
                    // Get the actual cart count from database
                    var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cart = await _context.ShoppingCart
                        .Include(c => c.Items)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    var cartCount = cart?.Items.Sum(i => i.Quantity) ?? 0;

                    // Pass both the count and a flag to show the cart
                    var model = new CartViewModel
                    {
                        ShowCart = true,
                        Count = cartCount
                    };
                    return View(model);
                }
            }

            return View(new CartViewModel { ShowCart = false, Count = 0 });
        }
    }

    public class CartViewModel
    {
        public bool ShowCart { get; set; }
        public int Count { get; set; }
    }
}