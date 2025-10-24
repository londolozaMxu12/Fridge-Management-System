using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FridgeManagementSystem.Areas.Identity.Data;

namespace FridgeManagementSystem.ViewComponents
{
    public class CustomerCartViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerCartViewComponent(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
                return View(isCustomer);
            }

            return View(false);
        }
    }
}