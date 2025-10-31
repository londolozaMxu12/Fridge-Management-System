using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace FridgeManagementSystem.Controllers
{
    public class FaultReportController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private static readonly List<FaultReport> schedules = new List<FaultReport>();
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;


        public FaultReportController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, FridgeManagementSystemContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }
        [HttpGet]
        public IActionResult Create(string customerId)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Id == customerId);
            var fridges = _context.Fridges.AsQueryable();
            if (customer == null)
                return NotFound();

            var model = new FaultReport { CustomerId = customer.Id };
            ViewBag.FridgeId = new SelectList(fridges, "Id", "SerialNumber");
            return View(model);
            // Logged in user
            //var user = _userManager.GetUserAsync(User);
            //var customer = _context.Customers.FirstOrDefault(c => c.Id == customerId);
            //var fridges = _context.Fridges.AsQueryable();
            //if (customer == null)
            //{
            //    // Only show fridges belonging to this customer
            //    fridges = fridges.Where(f=> f.Customer.Id == customer.Id);
            //}
            //var model = new FaultReport { CustomerId = customer.Id };
            // If admin/technician
           
          //  ViewBag.FridgeId = new SelectList(_context.Fridges, "Id", "SerialNumber");
            //return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FaultReport model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.FridgeId = new SelectList(_context.Fridges, "Id", "SerialNumber", model.FridgeId);
                return View(model);
            }
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == user.Id);



            if (customer == null)
            {
                ModelState.AddModelError("", "Invalid customer");
                ViewBag.FridgeId = new SelectList(_context.Fridges, "Id", "SerialNumber", model.FridgeId);
                return View(model);
            }
            var fault = new FaultReport
            {
                CustomerId = customer.Id,
                FridgeId = model.FridgeId,
                Description = model.Description,
                ReporteDate = DateTime.Now,
                Status = "Pending"
            };
              

                _context.FaultReports.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            

        }

        public async Task<IActionResult> Index()
        {
            var reports = _context.FaultReports
                .Include(f => f.Customer)
                .Include(f => f.Fridge);
               
            return View(await reports.ToListAsync());
        }
    }
}
