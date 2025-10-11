using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Controllers
{
    public class ScheduleMaintenanceController : Controller
    {
        private readonly FridgeManagementSystemContext _context;

        public ScheduleMaintenanceController(FridgeManagementSystemContext context)
        {
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

                return RedirectToAction("Index"); // Go back to list page
            }

            return View(model);
        }
    }
}
