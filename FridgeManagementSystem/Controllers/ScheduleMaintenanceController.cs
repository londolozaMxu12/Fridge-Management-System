using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ScheduleMaintenanceController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, FridgeManagementSystemContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
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
            //var usersInCustomerRole = from user in _context.Users
            //                          join userRole in _context.UserRoles on user.Id equals userRole.UserId
            //                          join role in _context.Roles on userRole.RoleId equals role.Id
            //                          where role.Name == "Customer"
            //                          select user;

            //foreach (var user in usersInCustomerRole)
            //{
            //    var customer = _context.Customers.FirstOrDefault(c => c.UserId == user.Id.ToString());
            //    if (customer != null && string.IsNullOrEmpty(customer.UserId))
            //    {
            //        customer.UserId = user.Id; // assign Identity user ID
            //    }
            //}

            // 1️⃣ Get the logged-in user's ID
            var currentUser = _userManager.GetUserId(User);
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // 3️⃣ Get the technician linked to this user
            var technician = _context.Employees.FirstOrDefault(e => e.UserId == currentUser);
            if (technician == null)
            {
                ModelState.AddModelError("", "No technician record linked to your account. Please contact support.");
                return View(schedule);
            }

            // Get the corresponding Customer record



            //var customer = _userManager.GetUsersInRoleAsync("Customer");

            //  // Optional: fetch their corresponding Customer records
            //var customer = _context.Customers
            //  .Where(c => customersInRole.Select(u => u.Id).Contains(c.UserId))
            //   .ToList();
            //  // 2️⃣ Get the customer linked to this user
            //var customer = User.IsInRole("Customer");

            //var customerdf = _context.Users
            //   .Where(u => u.Customers.UserId.Any(r => r.RoleId == context.Roles.FirstOrDefault(role => role.Name == "Customer").Id))
            //   .ToList();
            // var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId);
            //if (customer == null)
            //{
            //    ModelState.AddModelError("", "No customer record linked to your account. Please contact support.");
            //    return View(schedule);
            //}

            if (!ModelState.IsValid)
            {

                // 4️⃣ Assign CustomerId and TechnicianId to the schedule
            //    schedule.CustomerId = customer.Id;
                schedule.MaintenanceTechnicianId = technician.Id;

                // 5️⃣ Save to database
                _context.ScheduleMaintenances.Add(schedule);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(schedule);
            //  var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //  var username = User.FindFirstValue(ClaimTypes.Name);
            //  //string userId = User.Identity.GetUserId(); // current logged-in ASP.NET Identity user
            //  var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            //  var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId);

            //  if (!ModelState.IsValid)
            //  {
            //      if (employee != null)
            //      {
            //          schedule.MaintenanceTechnicianId = employee.Id;
            //          schedule.CustomerId = customer.Id;
            //          _context.ScheduleMaintenances.Add(schedule);
            //          _context.SaveChanges();
            //          return RedirectToAction("Index");
            //      }

            //      ModelState.AddModelError("", "Could not find the logged-in technician."); // Go back to list page
            //  }
            ////  ViewBag.CustomerId = new SelectList(new[] { customer }, "Id", "Name", customer.Id);
            // return View(schedule);
        }
        public IActionResult Index()
        {
            var schedules = _context.ScheduleMaintenances.OrderBy(s => s.ScheduledDate).ToList();
            return View(schedules);
        }
        public ActionResult Details(int id)
        {
            var schedule = _context.ScheduleMaintenances
              .Include(s => s.Customer)
              .Include(s => s.MaintenanceTechnician)
              .FirstOrDefault(s => s.MaintenanceTechnicianId == id);

            if (schedule == null)
            {
                ViewBag.Message = "Schedule not found.";
                return View("Error");
            }
            return View(schedule);
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
