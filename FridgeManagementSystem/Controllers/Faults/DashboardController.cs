using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FridgeManagementSystem.Controllers.F.Technician
{
    public class DashboardController : Controller
    {
        private readonly FridgeManagementSystemContext _context;

        public DashboardController(FridgeManagementSystemContext context)
        {
            _context = context;
        }
        // Helper method to get current fault technician
        private async Task<Employee?> GetCurrentFaultTechnicianAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _context.Employees
                .Include(e => e.User)
                .Include(e => e.EmployeeType)
                .FirstOrDefaultAsync(e => e.UserId == userId &&
                                         e.EmployeeType.Name == "FaultTechnician" &&
                                         e.IsActive);
        }
        public async Task<IActionResult> GetFaultDetailsPartial(int id)
        {
            var fault = await _context.Faults
                .Include(f => f.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Include(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .Include(f => f.FaultTechnician)
                .ThenInclude(t => t.User)
                .Include(f => f.RepairSchedules)
                .ThenInclude(rs => rs.FaultTechnician)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(f => f.FaultId == id);

            if (fault == null)
            {
                return Content("<div class='alert alert-danger'>Fault not found</div>");
            }

            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            ViewBag.CurrentTechnicianId = currentTechnician?.Id;
            ViewBag.IsFaultTechnician = currentTechnician != null;

            return PartialView("_FaultDetailsPartial", fault);
        }

        public async Task<IActionResult> Index()
        {
            // Check if current user is a fault technician and filter accordingly
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            var isFaultTechnician = currentTechnician != null;
            var currentTechnicianId = currentTechnician?.Id;

            // Get only unattended faults (FaultTechnicianId == null)
            var faults = await _context.Faults
                .Include(f => f.Fridge) 
                .ThenInclude(f => f.FridgeType)
                .Include(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .Include(f => f.FaultTechnician)
                .ThenInclude(t => t.User)
                .Where(f => f.FaultTechnicianId == null)  // Only unattended faults
                .OrderByDescending(f => f.Priority)
                .ThenByDescending(f => f.ReportedDate)
                .Take(2)
                .ToListAsync();

            // Get schedules for the stat cards
            var schedules = await _context.RepairSchedules.Where(rs => rs.FaultTechnicianId == currentTechnicianId)
                .ToListAsync();

            var dashboardView = new TechnicianDashboardViewModel
            {
                Faults = faults,
                Schedules = schedules,
                IsFaultTechnician = isFaultTechnician,
                CurrentTechnicianId = currentTechnicianId

            };

            // Pass data to view
            ViewBag.IsFaultTechnician = isFaultTechnician;
            ViewBag.CurrentTechnicianId = currentTechnicianId;

            return View(dashboardView); 
        }
    }
}
