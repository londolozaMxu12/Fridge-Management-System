using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IActionResult Create(string customerId)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Id == customerId);
            if (customer == null)
                return NotFound();

            var model = new ScheduleMaintenance { CustomerId = customer.Id };
            ViewBag.CustomerName = customer.BusinessName;
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "BusinessName", customer.Id);
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleMaintenance model)
        {
            if (ModelState.IsValid)
            {
                ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "BusinessName", model.CustomerId);
                return View(model);
            }

            // get the logged-in technician
            var userId = _userManager.GetUserId(User);
            var technician = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);

            if (technician == null)
            {
                ModelState.AddModelError("", "Technician record not found.");
                ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "BusinessName", model.CustomerId);
                return View(model);
            }

            var schedule = new ScheduleMaintenance
            {
                CustomerId = model.CustomerId,             // ✅ comes from form (the selected customer)
                MaintenanceTechnicianId = technician.Id,   // ✅ logged-in technician
                Description = model.Description,
                ScheduledDate = model.ScheduledDate
            };

            _context.ScheduleMaintenances.Add(schedule);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            var schedules = _context.ScheduleMaintenances.OrderBy(s => s.ScheduledDate).ToList();
            return View(schedules);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var schedule = await _context.ScheduleMaintenances
                .Include(s => s.Customer)
                .Include(s => s.MaintenanceTechnician)
                .FirstOrDefaultAsync(s => s.scheduleMaintenanceId == id);

            if (schedule == null)
                return NotFound();

            return View(schedule);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var schedule = await _context.ScheduleMaintenances
                .Include(s => s.Customer)
                .Include(s => s.MaintenanceTechnician)
                .FirstOrDefaultAsync(s => s.scheduleMaintenanceId == id);

            if (schedule == null)
                return NotFound();

            // Populate dropdown for customers (optional: if you want technician to change the customer)
            ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "BusinessName", schedule.CustomerId);

            return View(schedule);
        }

        // POST: ScheduleMaintenance/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ScheduleMaintenance model)
        {
            if (id != model.scheduleMaintenanceId)
                return BadRequest();

            if (ModelState.IsValid)
            {
                // Re-populate dropdown for customers
                ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "BusinessName", model.CustomerId);
                return View(model);
            }

            try
            {
                // Optional: ensure technician is still the logged-in user
                var userId = _userManager.GetUserId(User);
                var technician = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);

                if (technician == null)
                {
                    ModelState.AddModelError("", "Technician record not found.");
                    ViewBag.CustomerId = new SelectList(_context.Customers, "Id", "BusinessName", model.CustomerId);
                    return View(model);
                }

                // Update fields
                var schedule = await _context.ScheduleMaintenances.FindAsync(id);
                if (schedule == null)
                    return NotFound();

                schedule.CustomerId = model.CustomerId;
                schedule.Description = model.Description;
                schedule.ScheduledDate = model.ScheduledDate;
                schedule.MaintenanceTechnicianId = technician.Id;

                _context.Update(schedule);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ScheduleMaintenances.Any(e => e.scheduleMaintenanceId == id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
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
//public IActionResult Create()
//{
//    var userId = _userManager.GetUserId(User); // or User.Identity.GetUserId() in old MVC
//    var customer = _context.Customers.FirstOrDefault(c => c.Id == userId);

//    if (customer == null)
//    {
//        ModelState.AddModelError("", "Customer not found for this user.");
//        return View();
//    }
//    return View();
//}



//public IActionResult Create(ScheduleMaintenance schedule)
//{




//    // 1️⃣ Get the logged-in user's ID
//    //var currentUser = _userManager.GetUserId(User);
//     var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//    // 3️⃣ Get the technician linked to this user


//    // Get the corresponding Customer record



//    //var customer = _userManager.GetUsersInRoleAsync("Customer");

//    //  // Optional: fetch their corresponding Customer records
//    //var customer = _context.Customers
//    //  .Where(c => customersInRole.Select(u => u.Id).Contains(c.UserId))
//    //   .ToList();
//    //  // 2️⃣ Get the customer linked to this user
//    //var customer = User.IsInRole("Customer");

//    //var customerdf = _context.Users
//    //   .Where(u => u.Customers.UserId.Any(r => r.RoleId == context.Roles.FirstOrDefault(role => role.Name == "Customer").Id))
//    //   .ToList();
//    // var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId);
//    //if (customer == null)
//    //{
//    //    ModelState.AddModelError("", "No customer record linked to your account. Please contact support.");
//    //    return View(schedule);
//    //}
//    var technician = _context.Employees.FirstOrDefault(e => e.UserId == userId);
//    if (technician == null)
//    {
//        ModelState.AddModelError("", "No technician record linked to your account. Please contact support.");
//        return View(schedule);
//    }

//    //var customer = _context.Customers.FirstOrDefault(e => e.Id == currentUser);
//    //if (customer == null)
//    //{
//    //    ModelState.AddModelError("", "No customer record linked to your account. Please contact support.");
//    //    return View(schedule);
//    //}

//    if (!ModelState.IsValid)
//    {

//        // 4️⃣ Assign CustomerId and TechnicianId to the schedule
//        // schedule.CustomerId = customer.User.Id;
//        schedule.MaintenanceTechnicianId = technician.Id;

//        // 5️⃣ Save to database
//        _context.ScheduleMaintenances.Add(schedule);
//        _context.SaveChanges();

//        return RedirectToAction("Index");
//    }
//    return View(schedule);
//    //  var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//    //  var username = User.FindFirstValue(ClaimTypes.Name);
//    //  //string userId = User.Identity.GetUserId(); // current logged-in ASP.NET Identity user
//    //  var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
//    //  var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId);

//    //  if (!ModelState.IsValid)
//    //  {
//    //      if (employee != null)
//    //      {
//    //          schedule.MaintenanceTechnicianId = employee.Id;
//    //          schedule.CustomerId = customer.Id;
//    //          _context.ScheduleMaintenances.Add(schedule);
//    //          _context.SaveChanges();
//    //          return RedirectToAction("Index");
//    //      }

//    //      ModelState.AddModelError("", "Could not find the logged-in technician."); // Go back to list page
//    //  }
//    ////  ViewBag.CustomerId = new SelectList(new[] { customer }, "Id", "Name", customer.Id);
//    // return View(schedule);
//}