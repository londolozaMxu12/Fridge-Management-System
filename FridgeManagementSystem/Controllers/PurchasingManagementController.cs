using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class PurchasingManagementController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public PurchasingManagementController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;

        }
        public async Task<IActionResult> ListPurchasingManagement()
        {

            //var user = userManager.Users.ToList();
            var purchasingManager = await userManager.GetUsersInRoleAsync(" Purchasing Management");

            return View(purchasingManager);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
