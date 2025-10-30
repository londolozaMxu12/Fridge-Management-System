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

        // Add to CustomerManagementController.cs
        public async Task<JsonResult> GetCustomerAnalysisData()
        {
            try
            {
                var analysisData = new
                {
                    // Current Status Counts
                    statusCounts = new
                    {
                        active = await _context.Customers
                            .Include(c => c.User)
                            .Where(c => c.IsActive && c.User.IsActive && c.User.ApprovalStatus == "Approved")
                            .CountAsync(),
                        inactive = await _context.Customers
                            .Include(c => c.User)
                            .Where(c => (!c.IsActive || !c.User.IsActive) && c.User.ApprovalStatus == "Approved")
                            .CountAsync(),
                        pending = await _userManager.Users
                            .Where(u => u.ApprovalStatus == "Pending" && u.Customers != null)
                            .CountAsync()
                    },

                    // Customer Type Distribution
                    typeDistribution = await _context.Customers
                        .Include(c => c.User)
                        .Where(c => c.CustomerType != null && c.User.ApprovalStatus == "Approved")
                        .GroupBy(c => c.CustomerType)
                        .Select(g => new
                        {
                            type = g.Key,
                            active = g.Count(c => c.IsActive && c.User.IsActive),
                            inactive = g.Count(c => !c.IsActive || !c.User.IsActive),
                            total = g.Count()
                        })
                        .OrderByDescending(x => x.total)
                        .ToListAsync(),

                    // Monthly Registration Trends (Last 6 months)
                    monthlyTrends = await GetMonthlyRegistrationTrends(),

                    // Activation/Deactivation Rate
                    activationStats = new
                    {
                        totalApproved = await _context.Customers
                            .Include(c => c.User)
                            .Where(c => c.User.ApprovalStatus == "Approved")
                            .CountAsync(),
                        activeRate = await _context.Customers
                            .Include(c => c.User)
                            .Where(c => c.User.ApprovalStatus == "Approved")
                            .CountAsync(c => c.IsActive && c.User.IsActive)
                    }
                };

                return Json(analysisData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer analysis data");
                return Json(new { error = "Failed to load analysis data" });
            }
        }

        private async Task<List<object>> GetMonthlyRegistrationTrends()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var trends = new List<object>();

            for (int i = 0; i < 6; i++)
            {
                var monthStart = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1).AddMonths(i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var monthData = await _context.Customers
                    .Include(c => c.User)
                    .Where(c => c.CreatedAt >= monthStart && c.CreatedAt <= monthEnd)
                    .GroupBy(c => 1)
                    .Select(g => new
                    {
                        period = monthStart.ToString("MMM yyyy"),
                        newRegistrations = g.Count(),
                        approved = g.Count(c => c.User.ApprovalStatus == "Approved"),
                        activated = g.Count(c => c.IsActive && c.User.IsActive)
                    })
                    .FirstOrDefaultAsync();

                trends.Add(monthData ?? new
                {
                    period = monthStart.ToString("MMM yyyy"),
                    newRegistrations = 0,
                    approved = 0,
                    activated = 0
                });
            }

            return trends;
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
