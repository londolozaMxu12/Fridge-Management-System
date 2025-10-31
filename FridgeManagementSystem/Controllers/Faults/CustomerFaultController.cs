using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers.Faults
{
    [Authorize(Roles = "Customer")]
    public class CustomerFaultController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFaultNotificationRepository _notification;
        private readonly ILogger<CustomerFaultController> _logger;

        public CustomerFaultController(FridgeManagementSystemContext context, UserManager<ApplicationUser> userManager,
                                     IFaultNotificationRepository notification,
                                     ILogger<CustomerFaultController> logger)
        {
            _context = context;
            _userManager = userManager;
            _notification = notification;
            _logger = logger;
        }

        // GET: CustomerFault/Index - Customer's fault reports
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (customer == null)
            {
                TempData["Error"] = "Customer not found";
                return RedirectToAction("Index", "CustomerFault");
            }

            var faults = await _context.Faults
                .Include(f => f.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Include(f => f.FaultTechnician)
                .ThenInclude(t => t.User)
                .Where(f => f.ReportedById == customer.Id)
                .OrderByDescending(f => f.ReportedDate)
                .ToListAsync();

            return View(faults);
        }

        // GET: CustomerFault/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.Fridges)
                .ThenInclude(f => f.FridgeType)
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (customer == null)
            {
                TempData["Error"] = "Customer not found";
                return RedirectToAction("Index", "CustomerFault");
            }

            var model = new CreateFaultViewModel
            {
                CustomerFridges = customer.Fridges
                    .Where(f => f.IsActive && f.Status == "Allocated")
                    .Select(f => new CustomerFridgeViewModel
                    {
                        FridgeId = f.FridgeId,
                        SerialNumber = f.SerialNumber,
                        Description = f.Description,
                        FridgeType = $"{f.FridgeType.Name} {f.FridgeType.Brand}",
                        Status = f.Status
                    }).ToList()
            };

            return View(model);
        }

        // POST: CustomerFault/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFaultViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (customer == null)
            {
                TempData["Error"] = "Customer not found";
                return RedirectToAction("Index", "CustomerFault");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Create the fault
                    var fault = new Fault
                    {
                        Title = model.Title,
                        Description = model.Description,
                        Priority = model.Priority,
                        Status = FaultStatus.Reported,
                        ReportedDate = DateTime.UtcNow,
                        ReportedById = customer.Id,
                        FridgeId = model.FridgeId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Faults.Add(fault);
                    await _context.SaveChangesAsync();

                    // Update fridge status if delivered
                    if (model.FridgeId.HasValue)
                    {
                        var fridge = await _context.Fridges
                            .FirstOrDefaultAsync(f => f.FridgeId == model.FridgeId.Value);

                        if (fridge != null)
                        {
                            fridge.Status = "UnderRepair";
                            
                            _context.Fridges.Update(fridge);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Send notifications to all fault technicians and admins
                    await _notification.NotifyFaultReportedAsync(fault);

                    TempData["Success"] = "Fault reported successfully. A technician will contact you soon for repair schedule.";
                    return RedirectToAction(nameof(Details), new { id = fault.FaultId });
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync();

                    // Log the detailed error
                    var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                    _logger.LogError(dbEx, "Database error while creating fault: {Error}", innerException);

                    ModelState.AddModelError("", $"A database error occurred: {innerException}");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error while creating fault");
                    ModelState.AddModelError("", $"An error occurred while saving the fault: {ex.Message}");
                }
            }

            // Reload customer fridges if validation fails
            var customerFridges = await _context.Customers
                .Include(c => c.Fridges)
                .ThenInclude(f => f.FridgeType)
                .Where(c => c.Id == userId)
                .SelectMany(c => c.Fridges)
                .Where(f => f.IsActive && f.Status == "Allocated")
                .ToListAsync();

            model.CustomerFridges = customerFridges.Select(f => new CustomerFridgeViewModel
            {
                FridgeId = f.FridgeId,
                SerialNumber = f.SerialNumber,
                Description = f.Description,
                FridgeType = $"{f.FridgeType?.Name} {f.FridgeType?.Brand} ",
                Status = f.Status
            }).ToList();

            return View(model);
        }

        // GET: CustomerFault/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (customer == null)
            {
                TempData["Error"] = "Customer not found";
                return RedirectToAction("Index", "CustomerFault");
            }

            var fault = await _context.Faults
                .Include(f => f.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Include(f => f.FaultTechnician)
                .ThenInclude(t => t.User)
                .Include(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .Include(f => f.RepairSchedules)
                .ThenInclude(rs => rs.FaultTechnician)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(f => f.FaultId == id && f.ReportedById == customer.Id);

            if (fault == null)
            {
                TempData["Error"] = "Fault not found";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new FaultDetailsViewModel
            {
                FaultId = fault.FaultId,
                Title = fault.Title,
                Description = fault.Description,
                Priority = fault.Priority,
                Status = fault.Status,
                ReportedDate = fault.ReportedDate,
                ScheduledDate = fault.ScheduledDate,
                ResolutionNotes = fault.ResolutionNotes,
                FaultTechnician = fault.FaultTechnician?.User?.FullName ?? "Not assigned",
                ReportedBy = fault.ReportedBy.User.FullName,
                BusinessName = fault.ReportedBy.BusinessName,
                FridgeInfo = fault.Fridge != null ?
                    $"{fault.Fridge.FridgeType.Brand} {fault.Fridge.FridgeType.Name} - {fault.Fridge.SerialNumber}" :
                    "No specific fridge",
                RepairSchedules = fault.RepairSchedules.Select(rs => new RepairScheduleViewModel
                {
                    RepairScheduleId = rs.RepairScheduleId,
                    ScheduledDate = rs.ScheduledDate,
                    Technician = rs.FaultTechnician.User.FullName,
                    Status = rs.Status,
                    Notes = rs.Notes,
                    EstimatedHours = rs.EstimatedHours
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: CustomerFault/MyCalendar - Full Screen Calendar View
        public async Task<IActionResult> MyCalendar()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (customer == null)
            {
                TempData["Error"] = "Customer not found";
                return RedirectToAction("Index", "CustomerFault");
            }

            // Get all upcoming schedules for this customer (next 90 days for better calendar view)
            var upcomingSchedules = await _context.RepairSchedules
                .Include(rs => rs.Fault)
                    .ThenInclude(f => f.Fridge)
                        .ThenInclude(f => f.FridgeType)
                .Include(rs => rs.Fault)
                    .ThenInclude(f => f.ReportedBy)
                .Include(rs => rs.FaultTechnician)
                    .ThenInclude(t => t.User)
                .Where(rs => rs.Fault.ReportedById == customer.Id && // Only schedules for a specific customer's faults
                             rs.ScheduledDate >= DateTime.Today.AddDays(-7) && // Include some past schedules for context
                             rs.ScheduledDate <= DateTime.Today.AddDays(90) && // Extended view for better planning
                             rs.Status != ScheduleStatus.Cancelled)
                .OrderBy(rs => rs.ScheduledDate)
                .ToListAsync();

            var viewModel = new CustomerCalendarViewModel
            {
                CustomerName = customer.User.FullName,
                BusinessName = customer.BusinessName,
                UpcomingSchedules = upcomingSchedules
            };

            return View(viewModel);
        }
    }
}
