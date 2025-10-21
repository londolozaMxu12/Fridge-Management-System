using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Controllers
{
    public class ScheduleMaintenanceController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ScheduleMaintenanceController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager, FridgeManagementSystemContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CreateMaintenanceSchedule()
        {
            return View();
        }
        [HttpPost]
        public IActionResult CreateMaintenanceSchedule(ScheduleMaintenance model)
        {
            if (ModelState.IsValid)
            {
                // Set CreatedDate automatically if needed
               // model.CreatedDate = DateTime.Now;

                _context.ScheduleMaintenances.Add(model);
                _context.SaveChanges();

                return RedirectToAction("Success"); // Go back to list page
            }

            return View(model);
        }
    }
}
