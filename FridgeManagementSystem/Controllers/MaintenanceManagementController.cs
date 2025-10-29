using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Controllers
{
    public class MaintenanceManagementController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public MaintenanceManagementController(FridgeManagementSystemContext context, RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this._context = context;

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
            var customers = _context.Customers
                 .Include(c => c.Fridges)
                  .ThenInclude(f => f.FridgeType)
                    .Include(c => c.Allocations)// Optional: load fridges for display
                 .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c =>
                    c.BusinessName.Contains(searchString) ||
                    c.CustomerType.Contains(searchString) 
               ); // ✅ works if linked to ApplicationUser
            }

            // Execute the query
            var result = await customers.ToListAsync();

            return View(result);
        }
        //var customer = await userManager.GetUsersInRoleAsync("Customer");

        //if (!String.IsNullOrEmpty(searchString))
        //{
        //    customer = customer.Where(n => n.FullName.Contains(searchString)
        //    || n.Email.Contains(searchString)).ToList();
        //}
        //return View(customer);

    }

    
}
