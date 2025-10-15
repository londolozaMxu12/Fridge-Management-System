using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers.F.Technician
{
    //[Authorize(Roles = "FaultTechnician,Admin")]
    public class FaultTechnicianController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFaultNotificationRepository _notification;
       
        public FaultTechnicianController(FridgeManagementSystemContext context,
                                       UserManager<ApplicationUser> userManager,
                                       IFaultNotificationRepository notification)
        {
            _context = context;
            _userManager = userManager;
            _notification = notification;
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
        

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 4, string status = "all", string? search = null, string? priority = null, string? sort = null)
        {
            var query = _context.Faults
                .Include(f => f.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Include(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .Include(f => f.FaultTechnician)
                .ThenInclude(t => t.User)
                .AsQueryable();

            // Search functionality - search in Title or Description
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(f => f.Title.Contains(search) || f.Description.Contains(search));
            }

            // Filter by Priority
            if (!string.IsNullOrEmpty(priority) && priority == "urgent")
            {
                query = query.Where(f => f.Priority == FaultPriority.Critical || f.Priority == FaultPriority.High);
            }
            else if (!string.IsNullOrEmpty(priority) && priority != "all")
            {
                if (Enum.TryParse<FaultPriority>(priority, out var faultPriority))
                {
                    query = query.Where(f => f.Priority == faultPriority);
                }
            }

            // Filter by Status
            if (status != "all")
            {
                if (Enum.TryParse<FaultStatus>(status, out var faultStatus))
                {
                    query = query.Where(f => f.Status == faultStatus);
                }
            }

            // Check if current user is a fault technician and filter accordingly
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            var isFaultTechnician = currentTechnician != null;
            var currentTechnicianId = currentTechnician?.Id;

            if (isFaultTechnician)
            {
                query = query.Where(f => f.FaultTechnicianId == null || f.FaultTechnicianId == currentTechnicianId);
            }

            // Sort functionality (AFTER all filters)
            if (sort == "priority_asc")
            {
                query = query.OrderBy(f => f.Priority);
            }
            else if (sort == "priority_desc")
            {
                query = query.OrderByDescending(f => f.Priority);
            }
            else if (sort == "date_asc")
            {
                query = query.OrderBy(f => f.ReportedDate);
            }
            else if (sort == "date_desc")
            {
                query = query.OrderByDescending(f => f.ReportedDate);
            }
            else
            {
                // Default sort: highest priority first, then newest
                query = query.OrderByDescending(f => f.Priority)
                            .ThenByDescending(f => f.ReportedDate);
            }

            // Get counts for dashboard - Use the FILTERED query, not baseQuery
            var totalFaultsCount = await query.CountAsync(); // This is the key fix!
            ViewBag.TotalFaultsCount = totalFaultsCount;
            ViewBag.PendingFaultsCount = await query.CountAsync(f => f.Status == FaultStatus.Reported);
            ViewBag.UrgentFaultsCount = await query.CountAsync(f => f.Priority == FaultPriority.Critical || f.Priority == FaultPriority.High);
            ViewBag.InProgressFaultsCount = await query.CountAsync(f => f.Status == FaultStatus.InProgress);
            ViewBag.CompletedFaultsCount = await query.CountAsync(f => f.Status == FaultStatus.Completed);

            // Apply pagination
            var faults = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pass data to view
            ViewBag.IsFaultTechnician = isFaultTechnician;
            ViewBag.CurrentTechnicianId = currentTechnicianId;
            ViewBag.CurrentPriority = priority;
            ViewBag.CurrentStatus = status;

            // Create search model for form persistence
            var faultSearchViewModel = new FaultSearchViewModel()
            {
                Faults = faults,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Search = search,
                Priority = priority,
                Status = status,
                Sort = sort
            };

            return View(faultSearchViewModel);
        }
        // GET: FaultTechnician/Details/5
        public async Task<IActionResult> Details(int id)
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
                TempData["Error"] = "Fault not found";
                return RedirectToAction(nameof(Index));
            }

            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            ViewBag.CurrentTechnicianId = currentTechnician?.Id;
            ViewBag.IsFaultTechnician = currentTechnician != null;

            return View(fault);
        }

        // POST: FaultTechnician/AttendFault/5 - Technician assigns themselves to a fault
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttendFault(int id)
        {
            // Check if user is a fault technician
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                TempData["Error"] = "Access denied. Only fault technicians can attend faults.";
                return RedirectToAction(nameof(Index));
            }

            var fault = await _context.Faults
                .Include(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(f => f.FaultId == id);

            if (fault == null)
            {
                TempData["Error"] = "Fault not found";
                return RedirectToAction(nameof(Index));
            }

            // Check if fault is already attended
            if (fault.FaultTechnicianId.HasValue)
            {
                TempData["Error"] = "This fault is already being attended by another technician";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Update fault with attending technician
            var oldStatus = fault.Status.ToString();
            fault.FaultTechnicianId = currentTechnician.Id;
            fault.Status = FaultStatus.InProgress;
            fault.UpdatedAt = DateTime.UtcNow;

            // Create an initial repair schedule
            var repairSchedule = new RepairSchedule
            {
                FaultId = fault.FaultId,
                FaultTechnicianId = currentTechnician.Id,
                ScheduledDate = DateTime.Now.AddDays(2), // Default to tomorrow
                EstimatedHours = 2, // Default 2 hours
                Notes = "Fault attended by technician. Schedule to be confirmed.",
                Status = ScheduleStatus.Scheduled,
                CreatedById = currentTechnician.UserId,
                CreatedAt = DateTime.Now
            };

            _context.Faults.Update(fault);
            _context.RepairSchedules.Add(repairSchedule);
            await _context.SaveChangesAsync();

            // Send notifications to other technicians and customer
            await _notification.NotifyFaultAttendedAsync(fault, currentTechnician);

            //TempData["Success"] = "You have successfully attended this fault. Other technicians have been notified.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: FaultTechnician/Update/5
        public async Task<IActionResult> Update(int id)
        {
            var fault = await _context.Faults
                .Include(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(f => f.FaultId == id);

            if (fault == null)
            {
                TempData["Error"] = "Fault not found";
                return RedirectToAction(nameof(Index));
            }

            // Check if current user is assigned technician
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null || fault.FaultTechnicianId != currentTechnician.Id)
            {
                TempData["Error"] = "You are not assigned to this fault";
                return RedirectToAction(nameof(Index));
            }

            var model = new UpdateFaultViewModel
            {
                FaultId = fault.FaultId,
                Title = fault.Title,
                Status = fault.Status,
                ResolutionNotes = fault.ResolutionNotes
            };

            return View(model);
        }

        // POST: FaultTechnician/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(UpdateFaultViewModel model)
        {
            if (ModelState.IsValid)
            {
                var fault = await _context.Faults
                    .Include(f => f.ReportedBy)
                    .ThenInclude(c => c.User)
                    .Include(f => f.Fridge)
                    .FirstOrDefaultAsync(f => f.FaultId == model.FaultId);

                if (fault == null)
                {
                    TempData["Error"] = "Fault not found";
                    return RedirectToAction(nameof(Index));
                }

                // Check if current user is assigned technician
                var currentTechnician = await GetCurrentFaultTechnicianAsync();
                if (currentTechnician == null)
                {
                    TempData["Error"] = "Access denied. Only fault technicians can update faults.";
                    return RedirectToAction(nameof(Index));
                }

                if (fault.FaultTechnicianId != currentTechnician.Id)
                {
                    TempData["Error"] = "You are not assigned to this fault";
                    return RedirectToAction(nameof(Index));
                }

                var oldStatus = fault.Status.ToString();
                fault.ResolutionNotes = model.ResolutionNotes;
                fault.Status = model.Status;
                fault.UpdatedAt = DateTime.Now;


                // Update fridge status if fault is completed
                if (model.MarkCompleted && fault.FridgeId.HasValue)
                {
                    var fridge = await _context.Fridges.FindAsync(fault.FridgeId.Value);
                    if (fridge != null)
                    {
                        fridge.Status = "Allocated"; // Return to allocated status
                        fridge.NextServiceDate = DateTime.Now.AddMonths(3); // Schedule next service
                        _context.Fridges.Update(fridge);
                    }
                }

                _context.Faults.Update(fault);
                await _context.SaveChangesAsync();

                // Send notification for status change
                await _notification.NotifyFaultStatusUpdateAsync(fault, oldStatus);

                TempData["Success"] = "Fault updated successfully";
                return RedirectToAction(nameof(Details), new { id = fault.FaultId });
            }

            return View(model);
        }

        // GET: FaultTechnician/MySchedule
        public async Task<IActionResult> MySchedule()
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                TempData["Error"] = "Access denied. Only fault technicians can view schedules.";
                return RedirectToAction("Index", "FaultTechnician");
            }

            var schedules = await _context.RepairSchedules
                .Include(rs => rs.Fault)
                .ThenInclude(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .Include(rs => rs.Fault)
                .ThenInclude(f => f.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Where(rs => rs.FaultTechnicianId == currentTechnician.Id &&
                            rs.ScheduledDate >= DateTime.Today)
                .OrderBy(rs => rs.ScheduledDate)
                .ToListAsync();

            return View(schedules);
        }

        // GET: FaultTechnician/UpdateSchedule/5
        public async Task<IActionResult> UpdateSchedule(int id)
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                TempData["Error"] = "Access denied. Only fault technicians can update schedules.";
                return RedirectToAction(nameof(MySchedule));
            }

            var schedule = await _context.RepairSchedules
                .Include(rs => rs.Fault)
                .ThenInclude(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .Include(rs => rs.Fault)
                .ThenInclude(f => f.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Include(rs => rs.FaultTechnician)
                .FirstOrDefaultAsync(rs => rs.RepairScheduleId == id);

            if (schedule == null)
            {
                TempData["Error"] = "Repair schedule not found";
                return RedirectToAction(nameof(MySchedule));
            }

            // Check if current user is assigned technician
            if (schedule.FaultTechnicianId != currentTechnician.Id)
            {
                TempData["Error"] = "You are not assigned to this repair schedule";
                return RedirectToAction(nameof(MySchedule));
            }

            var viewModel = new UpdateRepairScheduleViewModel
            {
                RepairScheduleId = schedule.RepairScheduleId,
                ScheduledDate = schedule.ScheduledDate,
                EstimatedHours = schedule.EstimatedHours,
                Notes = schedule.Notes,
                Status = schedule.Status
            };

            // Store the original schedule in ViewBag for display
            ViewBag.OriginalSchedule = schedule;

            return View(viewModel);
        }

        // POST: FaultTechnician/UpdateSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSchedule(int id, UpdateRepairScheduleViewModel model)
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                TempData["Error"] = "Access denied. Only fault technicians can update schedules.";
                return RedirectToAction(nameof(MySchedule));
            }

            var schedule = await _context.RepairSchedules
                .Include(rs => rs.Fault)
                .ThenInclude(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .Include(rs => rs.FaultTechnician)
                .FirstOrDefaultAsync(rs => rs.RepairScheduleId == id);

            if (schedule == null)
            {
                TempData["Error"] = "Repair schedule not found";
                return RedirectToAction(nameof(MySchedule));
            }

            // Check if current user is assigned technician
            if (schedule.FaultTechnicianId != currentTechnician.Id)
            {
                TempData["Error"] = "You are not assigned to this repair schedule";
                return RedirectToAction(nameof(MySchedule));
            }

            if (ModelState.IsValid)
            {
                schedule.ScheduledDate = model.ScheduledDate;
                schedule.EstimatedHours = model.EstimatedHours;
                schedule.Notes = model.Notes;
                schedule.Status = model.Status;
                schedule.UpdatedAt = DateTime.Now;

                _context.RepairSchedules.Update(schedule);
                await _context.SaveChangesAsync();

                // Notify customer if schedule is updated
                if (schedule.Status == ScheduleStatus.Scheduled || schedule.Status == ScheduleStatus.Rescheduled)
                {
                    await _notification.NotifyRepairScheduledAsync(schedule);
                }

                TempData["Success"] = "Repair schedule updated successfully";
                return RedirectToAction(nameof(MySchedule));
            }

            // Reload the original schedule for display if validation fails
            ViewBag.OriginalSchedule = schedule;
            return View(model);
        }

        // POST: FaultTechnician/UpdateScheduleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateScheduleStatus(int id, ScheduleStatus status)
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                TempData["Error"] = "Access denied. Only fault technicians can update schedules.";
                return RedirectToAction(nameof(MySchedule));
            }

            var schedule = await _context.RepairSchedules
                .Include(rs => rs.Fault)
                .ThenInclude(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(rs => rs.RepairScheduleId == id);

            if (schedule == null)
            {
                TempData["Error"] = "Repair schedule not found";
                return RedirectToAction(nameof(MySchedule));
            }

            // Check if current user is assigned technician
            if (schedule.FaultTechnicianId != currentTechnician.Id)
            {
                TempData["Error"] = "You are not assigned to this repair schedule";
                return RedirectToAction(nameof(MySchedule));
            }

            var oldStatus = schedule.Status;
            schedule.Status = status;
            schedule.UpdatedAt = DateTime.Now;

            _context.RepairSchedules.Update(schedule);
            await _context.SaveChangesAsync();



            TempData["Success"] = $"Repair schedule status updated from {oldStatus} to {status}";
            return RedirectToAction(nameof(MySchedule));
        }
        
    }
}
