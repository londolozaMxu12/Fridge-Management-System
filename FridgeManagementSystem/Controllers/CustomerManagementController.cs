using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class CustomerManagementController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public CustomerManagementController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;

        }
        public async Task<IActionResult> ListCustomerManagement()
        {

            //var user = userManager.Users.ToList();
            var customerManager = await userManager.GetUsersInRoleAsync("Customer Management");

            return View(customerManager);
        }
        public IActionResult Index()
        {
            return View();

        }
    }
}
