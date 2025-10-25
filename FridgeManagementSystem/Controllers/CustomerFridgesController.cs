using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
    [Authorize]
    public class CustomerFridgesController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CustomerFridgesController> _logger;

        public CustomerFridgesController(
            FridgeManagementSystemContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CustomerFridgesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: My Fridges - Show all allocated fridges for the current customer
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get customer's allocated fridges
                var allocatedFridges = await _context.Fridges
                    .Include(f => f.FridgeType)
                    .Include(f => f.Customer)
                        .ThenInclude(c => c.User)
                    .Where(f => f.Customer.UserId == userId && f.Status == "Allocated")
                    .OrderByDescending(f => f.AllocationDate)
                    .Take(4)
                    .ToListAsync();

                // Also get fridges from allocations (for historical records)
                var allocationFridges = await _context.Allocations
                    .Include(a => a.Fridge)
                        .ThenInclude(f => f.FridgeType)
                    .Include(a => a.Order)
                    .Where(a => a.Customer.UserId == userId)
                    .OrderByDescending(a => a.AllocationDate)
                    .Take(6)
                    .ToListAsync();

                ViewBag.AllocationHistory = allocationFridges;
                return View(allocatedFridges);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer fridges");
                TempData["ErrorMessage"] = "An error occurred while loading your fridges.";
                return View(new List<Fridge>());
            }
        }

        // GET: Fridge Details
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var fridge = await _context.Fridges
                    .Include(f => f.FridgeType)
                    .Include(f => f.Customer)
                        .ThenInclude(c => c.User)
                    .Include(f => f.Allocations)
                        .ThenInclude(a => a.Order)
                    .Include(f => f.MaintenanceRecords)
                    .Include(f => f.ReportedFaults)
                    .FirstOrDefaultAsync(f => f.FridgeId == id && f.Customer.UserId == userId);

                if (fridge == null)
                {
                    TempData["ErrorMessage"] = "Fridge not found or you don't have access to it.";
                    return RedirectToAction(nameof(Index));
                }

                // Get maintenance schedule for this fridge
                //var maintenanceSchedule = await _context.ScheduleMaintenances
                //    .Include(s => s.MaintenanceRecord)
                //    .Where(s => s.FridgeId == id && s.ScheduledDate >= DateTime.UtcNow)
                //    .OrderBy(s => s.ScheduledDate)
                //    .ToListAsync();

                //ViewBag.MaintenanceSchedule = maintenanceSchedule;
                return View(fridge);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading fridge details for fridge {id}");
                TempData["ErrorMessage"] = "An error occurred while loading fridge details.";
                return RedirectToAction(nameof(Index));
            }
        }

    }
}