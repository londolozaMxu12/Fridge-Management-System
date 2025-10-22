using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;


namespace FridgeManagementSystem.Controllers
{
    public class ScheduleMaintenanceController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private static readonly List<ScheduleMaintenance> schedules = new List<ScheduleMaintenance>();
        //private readonly RoleManager<IdentityRole> _roleManager;
        //private readonly UserManager<ApplicationUser> _userManager;

        public ScheduleMaintenanceController( FridgeManagementSystemContext context)
        {
           // _roleManager = roleManager;
           // _userManager = userManager;
            _context = context;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ScheduleMaintenance schedule)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //string userId = User.Identity.GetUserId(); // current logged-in ASP.NET Identity user
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            if (!ModelState.IsValid)
            {
                if (employee != null)
                {
                    schedule.MaintenanceTechnicianId = employee.Id;
                    _context.ScheduleMaintenances.Add(schedule);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "Could not find the logged-in technician."); // Go back to list page
            }

            return View(schedule);
        }
        public IActionResult Index()
        {
            var schedules = _context.ScheduleMaintenances.OrderBy(s => s.ScheduledDate).ToList();
            return View(schedules);
        }
        public ActionResult Details(int id)
        {
            var schedule = schedules.FirstOrDefault(s => s.scheduleMaintenanceId == id);
            if (schedule == null)
                return HttpNotFound();

            return View(schedule);
        }

        private ActionResult HttpNotFound()
        {
            throw new NotImplementedException();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
