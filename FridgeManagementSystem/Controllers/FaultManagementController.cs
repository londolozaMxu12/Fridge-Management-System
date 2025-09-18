using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class FaultManagementController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public FaultManagementController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;

        }
        public async Task<IActionResult> ListFaultManagement()
        {

            //var user = userManager.Users.ToList();
            var faultManager = await userManager.GetUsersInRoleAsync("Fault Management");

            return View(faultManager);
        }

    
        public IActionResult Index()
        {
            return View();
        }
    }
}

   

