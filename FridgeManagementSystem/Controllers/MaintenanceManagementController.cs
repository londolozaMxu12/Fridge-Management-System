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
    }
}
