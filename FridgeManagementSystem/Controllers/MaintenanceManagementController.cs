using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class MaintenanceManagementController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public MaintenanceManagementController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;

        }
        public async Task<IActionResult> ListMaintenanceManagement()
        {

            //var user = userManager.Users.ToList();
            var maintenanceManager = await userManager.GetUsersInRoleAsync(" Maintenance Management");

            return View(maintenanceManager);
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CustomerList(string searchString)
        {
            var customer = await userManager.GetUsersInRoleAsync("Customer");

            if (!String.IsNullOrEmpty(searchString))
            {
                customer = customer.Where(n => n.FullName.Contains(searchString)
                || n.Email.Contains(searchString)).ToList();
            }
            return View(customer);

        }

    }
}
