using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Controllers
{
    public class CustomerManagementController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FridgeManagementSystemContext _context;
        private readonly ILogger<CustomerManagementController> _logger;

        public CustomerManagementController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager, FridgeManagementSystemContext context, ILogger<CustomerManagementController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }
        public async Task<IActionResult> ListCustomerManagement()
        {

            //var user = userManager.Users.ToList();
            var customerManager = await _userManager.GetUsersInRoleAsync("Customer Management");

            return View(customerManager);
        }
        public async  Task<IActionResult> Index()
        {
            ViewBag.totalCustomers = await _context.Customers
            .Include(c => c.User)
                .Where(c => c.User.ApprovalStatus == "Approved")
                .CountAsync();

            ViewBag.activeCustomers = await _context.Customers
                .Include(c => c.User)
                    .Where(c => c.IsActive && c.User.IsActive && c.User.ApprovalStatus == "Approved")
                    .CountAsync();

            ViewBag.inactiveCustomers = await _context.Customers
                .Include(c => c.User)
                    .Where(c => (!c.IsActive || !c.User.IsActive) && c.User.ApprovalStatus == "Approved")
                    .CountAsync();

            ViewBag.pendingApprovals = await _userManager.Users
                .Where(u => u.ApprovalStatus == "Pending" && u.Customers != null)
                    .CountAsync();

            return View();

        }


        //public async Task<IActionResult> GetCustomerStats()
        //{

        //    ViewBag.totalCustomers = await _context.Customers
        //        .Include(c => c.User)
        //        .Where(c => c.User.ApprovalStatus == "Approved")
        //        .CountAsync();

        //    ViewBag.activeCustomers = await _context.Customers
        //        .Include(c => c.User)
        //        .Where(c => c.IsActive && c.User.IsActive && c.User.ApprovalStatus == "Approved")
        //        .CountAsync();

        //    ViewBag.inactiveCustomers = await _context.Customers
        //        .Include(c => c.User)
        //        .Where(c => (!c.IsActive || !c.User.IsActive) && c.User.ApprovalStatus == "Approved")
        //        .CountAsync();

        //    ViewBag.pendingApprovals = await _userManager.Users
        //        .Where(u => u.ApprovalStatus == "Pending" && u.Customers != null)
        //        .CountAsync();

        //    var customers = new
        //    {
        //        totalCustomers,
        //        activeCustomers,
        //        inactiveCustomers,
        //        pendingApprovals
        //    };

        //    return Json(customers);
        //}
    }
}
