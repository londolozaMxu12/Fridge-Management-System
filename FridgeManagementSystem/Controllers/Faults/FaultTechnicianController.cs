using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Controllers.Faults;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Drawing; 
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers.F.Technician
{
    
    public class FaultTechnicianController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFaultNotificationRepository _notification;
        private readonly ITechnicianReportRepository _technician;
        private readonly ILogger<FaultTechnicianController> _logger;

        public FaultTechnicianController(FridgeManagementSystemContext context, UserManager<ApplicationUser> userManager,
                                       IFaultNotificationRepository notification, ITechnicianReportRepository technician,
                                       ILogger<FaultTechnicianController> logger)
        {
            _context = context;
            _userManager = userManager;
            _notification = notification;
            _technician = technician;
            _logger = logger;
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

        private async Task SetNavigationViewBagProperties()
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
                        
            var baseQuery = _context.Faults.AsQueryable();

            ViewBag.TotalFaultsCount = await baseQuery.CountAsync();
            ViewBag.PendingFaultsCount = await baseQuery.CountAsync(f => f.Status == FaultStatus.Reported);
            ViewBag.UrgentFaultsCount = await baseQuery.CountAsync(f => f.Priority == FaultPriority.Critical || f.Priority == FaultPriority.High);
            ViewBag.InProgressFaultsCount = await baseQuery.CountAsync(f => f.Status == FaultStatus.InProgress);
            ViewBag.CompletedFaultsCount = await baseQuery.CountAsync(f => f.Status == FaultStatus.Completed);

            // faults attended by other technicians
            if (currentTechnician != null)
            {
                ViewBag.AttendedByOthersCount = await baseQuery.CountAsync(f =>
                    f.FaultTechnicianId != null && f.FaultTechnicianId != currentTechnician.Id);
                ViewBag.AttendedByMeCount = await baseQuery.CountAsync(f =>
                    f.FaultTechnicianId == currentTechnician.Id);
                ViewBag.UnattendedCount = await baseQuery.CountAsync(f =>
                    f.FaultTechnicianId == null);
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            var isFaultTechnician = currentTechnician != null;
            var currentTechnicianId = currentTechnician?.Id;

            await SetNavigationViewBagProperties();

            // Get statistics for dashboard cards
            var totalFaults = await _context.Faults.CountAsync();
            var unattendedFaults = await _context.Faults.CountAsync(f => f.FaultTechnicianId == null);
            var urgentFaults = await _context.Faults.CountAsync(f =>
                f.FaultTechnicianId == null &&
                (f.Priority == FaultPriority.Critical || f.Priority == FaultPriority.High));

            var myActiveFaults = currentTechnicianId.HasValue ?
                await _context.Faults.CountAsync(f => f.FaultTechnicianId == currentTechnicianId &&
                                                    (f.Status == FaultStatus.InProgress || f.Status == FaultStatus.Scheduled)) : 0;

            var myCompletedFaults = currentTechnicianId.HasValue ?
                await _context.Faults.CountAsync(f => f.FaultTechnicianId == currentTechnicianId &&
                                                    f.Status == FaultStatus.Completed) : 0;

            // Get upcoming schedules for list view (next 7 days) - limited to 5
            var upcomingSchedules = currentTechnicianId.HasValue ?
                await _context.RepairSchedules
                    .Include(rs => rs.Fault)
                        .ThenInclude(f => f.ReportedBy)
                            .ThenInclude(c => c.User)
                    .Include(rs => rs.Fault)
                        .ThenInclude(f => f.Fridge)
                            .ThenInclude(f => f.FridgeType)
                    .Where(rs => rs.FaultTechnicianId == currentTechnicianId &&
                                rs.ScheduledDate >= DateTime.Today &&
                                rs.ScheduledDate <= DateTime.Today.AddDays(7))
                    .OrderBy(rs => rs.ScheduledDate)
                    .Take(5)
                    .ToListAsync() : new List<RepairSchedule>();

            // Get ALL upcoming schedules for calendar view (next 7 days) - no limit
            var allUpcomingSchedules = currentTechnicianId.HasValue ?
                await _context.RepairSchedules
                    .Include(rs => rs.Fault)
                        .ThenInclude(f => f.ReportedBy)
                            .ThenInclude(c => c.User)
                    .Include(rs => rs.Fault)
                        .ThenInclude(f => f.Fridge)
                            .ThenInclude(f => f.FridgeType)
                    .Where(rs => rs.FaultTechnicianId == currentTechnicianId &&
                                rs.ScheduledDate >= DateTime.Today &&
                                rs.ScheduledDate <= DateTime.Today.AddDays(7))
                    .OrderBy(rs => rs.ScheduledDate)
                    .ToListAsync() : new List<RepairSchedule>();

            // Get recent unattended faults (for quick action)
            var recentUnattendedFaults = await _context.Faults
                .Include(f => f.Fridge)
                    .ThenInclude(f => f.FridgeType)
                .Include(f => f.ReportedBy)
                    .ThenInclude(c => c.User)
                .Where(f => f.FaultTechnicianId == null)
                .OrderByDescending(f => f.Priority)
                .ThenByDescending(f => f.ReportedDate)
                .Take(5)
                .ToListAsync();

            // Get my recent faults
            var myRecentFaults = currentTechnicianId.HasValue ?
                await _context.Faults
                    .Include(f => f.Fridge)
                        .ThenInclude(f => f.FridgeType)
                    .Include(f => f.ReportedBy)
                        .ThenInclude(c => c.User)
                    .Where(f => f.FaultTechnicianId == currentTechnicianId)
                    .OrderByDescending(f => f.UpdatedAt)
                    .Take(5)
                    .ToListAsync() : new List<Fault>();

            // Calculate completion rate
            var completionRate = (myActiveFaults + myCompletedFaults) > 0 ?
                (double)myCompletedFaults / (myActiveFaults + myCompletedFaults) * 100 : 0;

            var dashboardView = new TechnicianDashboardViewModel
            {
                // Statistics
                TotalFaults = totalFaults,
                UnattendedFaults = unattendedFaults,
                UrgentFaults = urgentFaults,
                MyActiveFaults = myActiveFaults,
                MyCompletedFaults = myCompletedFaults,
                CompletionRate = completionRate,

                // Lists
                UpcomingSchedules = upcomingSchedules,
                AllUpcomingSchedules = allUpcomingSchedules, 
                RecentUnattendedFaults = recentUnattendedFaults,
                MyRecentFaults = myRecentFaults,

                // Technician info
                IsFaultTechnician = isFaultTechnician,
                CurrentTechnicianId = currentTechnicianId,
                TechnicianName = currentTechnician?.User?.FullName
            };

            // Pass data to view
            ViewBag.IsFaultTechnician = isFaultTechnician;
            ViewBag.CurrentTechnicianId = currentTechnicianId;

            return View(dashboardView);
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

            // Sort functionality (AFTER all filters)
            query = sort switch
            {
                "priority_asc" => query.OrderBy(f => f.Priority),
                "priority_desc" => query.OrderByDescending(f => f.Priority),
                "date_asc" => query.OrderBy(f => f.ReportedDate),
                "date_desc" => query.OrderByDescending(f => f.ReportedDate),
                _ => query.OrderByDescending(f => f.Priority).ThenByDescending(f => f.ReportedDate)
            };

            await SetNavigationViewBagProperties();

            // Apply pagination
            var totalCount = await query.CountAsync();
            var faults = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pass data to view
            ViewBag.IsFaultTechnician = await GetCurrentFaultTechnicianAsync() != null;
            ViewBag.CurrentTechnicianId = (await GetCurrentFaultTechnicianAsync())?.Id;
            ViewBag.CurrentPriority = priority;
            ViewBag.CurrentStatus = status;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

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
            await SetNavigationViewBagProperties();

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

            // Check if user is admin (should not be able to attend faults)
            if (User.IsInRole("Admin"))
            {
                TempData["Error"] = "Admins cannot assign faults. Please contact a fault technician.";
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
                if (fault.FaultTechnicianId == currentTechnician.Id)
                {
                    TempData["Info"] = "You have already attended this fault by yourself.";
                }
                else
                {
                    TempData["Error"] = "This fault is already being attended by another technician, look for an unattendend fault.";
                }
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                // Update fault with attending technician
                var oldStatus = fault.Status.ToString();
                fault.FaultTechnicianId = currentTechnician.Id;
                fault.Status = FaultStatus.InProgress;
                fault.UpdatedAt = DateTime.Now;

                // Create an initial repair schedule
                var repairSchedule = new RepairSchedule
                {
                    FaultId = fault.FaultId,
                    FaultTechnicianId = currentTechnician.Id,
                    ScheduledDate = DateTime.Now.AddDays(1), // Default to tomorrow
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

                TempData["Success"] = "You have successfully attended this fault. Other technicians have been notified.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while attending the fault. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: FaultTechnician/Update/5
        public async Task<IActionResult> Update(int id)
        {
            await SetNavigationViewBagProperties();

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
        public async Task<IActionResult> MySchedule(string? search, string? column, string? orderBy, string status = "all")
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                TempData["Error"] = "Access denied. Only fault technicians can view schedules.";
                return RedirectToAction("Index", "FaultTechnician");
            }

            // Start with base query including all necessary relationships
            IQueryable<RepairSchedule> query = _context.RepairSchedules
                .Include(rs => rs.Fault)
                    .ThenInclude(f => f.ReportedBy)
                        .ThenInclude(c => c.User)
                .Include(rs => rs.Fault)
                    .ThenInclude(f => f.Fridge)
                        .ThenInclude(f => f.FridgeType)
                .Where(rs => rs.FaultTechnicianId == currentTechnician.Id &&
                            rs.ScheduledDate >= DateTime.Today.AddDays(-60));

            // Search functionality - search in Fault Title or Customer FullName
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(rs =>
                    rs.Fault.Title.Contains(search) ||
                    rs.Fault.ReportedBy.User.FullName.Contains(search)
                );
            }

            // Filter by Status
            if (status != "all")
            {
                if (Enum.TryParse<ScheduleStatus>(status, out var scheduleStatus))
                {
                    query = query.Where(f => f.Status == scheduleStatus);
                }
            }

            // Sort functionality
            string[] validColumns = { "RepairScheduleId", "ScheduledDate" };
            string[] validOrderBy = { "desc", "asc" };

            if (!validColumns.Contains(column))
            {
                column = "ScheduledDate";
            }

            if (!validOrderBy.Contains(orderBy))
            {
                orderBy = "asc";
            }

            // Apply sorting
            query = (column, orderBy) switch
            {
                ("RepairScheduleId", "asc") => query.OrderBy(rs => rs.RepairScheduleId),
                ("RepairScheduleId", "desc") => query.OrderByDescending(rs => rs.RepairScheduleId),
                ("ScheduledDate", "asc") => query.OrderBy(rs => rs.ScheduledDate),
                ("ScheduledDate", "desc") => query.OrderByDescending(rs => rs.ScheduledDate),
                _ => query.OrderBy(rs => rs.ScheduledDate)
            };

            // Execute the query
            var schedules = await query.ToListAsync();

            // Set ViewBag values for the view
            ViewBag.Search = search;
            ViewBag.Column = column;
            ViewBag.OrderBy = orderBy;
            ViewBag.CurrentStatus = status;

            await SetNavigationViewBagProperties();

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

            await SetNavigationViewBagProperties();

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
                // Store the original status to check if it changed to cancelled
                var originalStatus = schedule.Status;

                schedule.ScheduledDate = model.ScheduledDate;
                schedule.EstimatedHours = model.EstimatedHours;
                schedule.Notes = model.Notes;
                schedule.Status = model.Status;
                schedule.UpdatedAt = DateTime.UtcNow;

                // If status changed to Cancelled, update the fault status to Reported AND unassign technician
                if (model.Status == ScheduleStatus.Cancelled && originalStatus != ScheduleStatus.Cancelled)
                {
                    schedule.Fault.Status = FaultStatus.Reported;
                    schedule.Fault.FaultTechnicianId = null; // Unassign the technician
                    schedule.Fault.UpdatedAt = DateTime.Now;

                    // Also update the repair schedule's technician to null
                    schedule.FaultTechnicianId = null;
                }
                // If status changed to Completed, update the fault status to Completed
                else if (model.Status == ScheduleStatus.Completed && originalStatus != ScheduleStatus.Cancelled)
                {
                    schedule.Fault.Status = FaultStatus.Completed;
                }
                else if ((model.Status == ScheduleStatus.Scheduled || model.Status == ScheduleStatus.Rescheduled) && originalStatus != ScheduleStatus.Cancelled)
                {
                    schedule.Fault.Status = FaultStatus.Scheduled;
                }

                _context.RepairSchedules.Update(schedule);
                _context.Faults.Update(schedule.Fault); // Ensure fault changes are tracked
                await _context.SaveChangesAsync();

                // Notify customer if schedule is updated
                if (schedule.Status == ScheduleStatus.Scheduled || schedule.Status == ScheduleStatus.Rescheduled
                    || schedule.Status == ScheduleStatus.Cancelled)
                {
                    await _notification.NotifyRepairScheduledAsync(schedule);
                }

                // Send notification about fault being unassigned if cancelled
                if (model.Status == ScheduleStatus.Cancelled && originalStatus != ScheduleStatus.Cancelled)
                {
                    await _notification.NotifyFaultUnassignedAsync(schedule.Fault, currentTechnician);
                }

                TempData["Success"] = "Repair schedule updated successfully";
                return RedirectToAction(nameof(MySchedule));
            }

            // Reload the original schedule for display if validation fails
            ViewBag.OriginalSchedule = schedule;
            return View(model);
        }

        // GET: FaultTechnician/RepairHistory
        public async Task<IActionResult> RepairHistory(string? search, string? status, DateTime? fromDate, DateTime? toDate, int pageNumber = 1, int pageSize = 5)
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                TempData["Error"] = "Access denied. Only fault technicians can view repair history.";
                return RedirectToAction(nameof(Index));
            }

            // Base query for technician's repair schedules
            var query = _context.RepairSchedules
                .Include(rs => rs.Fault)
                    .ThenInclude(f => f.ReportedBy)
                        .ThenInclude(c => c.User)
                .Include(rs => rs.Fault)
                    .ThenInclude(f => f.Fridge)
                        .ThenInclude(f => f.FridgeType)
                .Where(rs => rs.FaultTechnicianId == currentTechnician.Id)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(rs =>
                    rs.Fault.Title.Contains(search) ||
                    rs.Fault.ReportedBy.User.FullName.Contains(search) ||
                    rs.Fault.Fridge.SerialNumber.Contains(search)
                );
            }

            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                if (Enum.TryParse<ScheduleStatus>(status, out var scheduleStatus))
                {
                    query = query.Where(rs => rs.Status == scheduleStatus);
                }
            }

            if (fromDate.HasValue)
            {
                query = query.Where(rs => rs.ScheduledDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(rs => rs.ScheduledDate <= toDate.Value);
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering (most recent first)
            var repairSchedules = await query
                .OrderByDescending(rs => rs.ScheduledDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate statistics
            var allTechnicianSchedules = await _context.RepairSchedules
                .Where(rs => rs.FaultTechnicianId == currentTechnician.Id)
                .ToListAsync();

            var completedRepairs = allTechnicianSchedules.Count(rs => rs.Status == ScheduleStatus.Completed);
            var totalRepairs = allTechnicianSchedules.Count;
            var completionRate = totalRepairs > 0 ? (double)completedRepairs / totalRepairs * 100 : 0;

            // Pass data to view
            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate?.ToString("dd-MM-yyyy");
            ViewBag.ToDate = toDate?.ToString("dd-MM-yyyy");
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.CompletedRepairs = completedRepairs;
            ViewBag.TotalRepairs = totalRepairs;
            ViewBag.CompletionRate = completionRate;

            await SetNavigationViewBagProperties();
            return View(repairSchedules);
        }
        [Authorize(Roles = "Admin")]
        // GET: FaultTechnician/TechnicianReport
        public async Task<IActionResult> TechnicianReport()
        {
            await SetNavigationViewBagProperties();

            var technicians = await _technician.GetActiveTechniciansAsync();
            ViewBag.Technicians = new SelectList(technicians, "Id", "User.FullName");

            var model = new TechnicianReportViewModel
            {
                EndDate = DateTime.Today,
                StartDate = DateTime.Today.AddMonths(-3)
            };

            return View(model);
        }
        [Authorize(Roles = "Admin")]
        // POST: FaultTechnician/TechnicianReport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TechnicianReport(TechnicianReportViewModel model)
        {
            if (ModelState.IsValid && model.TechnicianId.HasValue)
            {
                try
                {
                    var report = await _technician.GenerateTechnicianReportAsync(model);

                    await SetNavigationViewBagProperties();

                    var technicians = await _technician.GetActiveTechniciansAsync();
                    ViewBag.Technicians = new SelectList(technicians, "Id", "User.FullName");

                    return View(report);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating technician report for technician {TechnicianId}", model.TechnicianId);
                    TempData["Error"] = "An error occurred while generating the report. Please try again.";
                }
            }
            else if (!model.TechnicianId.HasValue)
            {
                ModelState.AddModelError("TechnicianId", "Please select a technician.");
            }

            await SetNavigationViewBagProperties();
            var techniciansList = await _technician.GetActiveTechniciansAsync();
            ViewBag.Technicians = new SelectList(techniciansList, "Id", "User.FullName");

            return View(model);
        }
        [Authorize(Roles = "Admin")]
        // GET: FaultTechnician/ExportTechnicianReport
        public async Task<IActionResult> ExportTechnicianReport(
            int technicianId,
            DateTime startDate,
            DateTime endDate,
            string reportType,
            string format)
        {
            try
            {
                var parameters = new TechnicianReportViewModel
                {
                    TechnicianId = technicianId,
                    StartDate = startDate,
                    EndDate = endDate,
                    ReportType = reportType
                };

                var report = await _technician.GenerateTechnicianReportAsync(parameters);

                return format.ToLower() switch
                {
                    "pdf" => await ExportTechnicianPdf(report),
                    "excel" => await ExportTechnicianExcel(report),
                    _ => BadRequest("Unsupported export format")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting technician report for technician {TechnicianId}", technicianId);
                TempData["Error"] = "An error occurred while exporting the report.";
                return RedirectToAction(nameof(TechnicianReport));
            }
        }

        private async Task<IActionResult> ExportTechnicianPdf(TechnicianReportViewModel report)
        {
            try
            {
                var pdfBytes = await _technician.GenerateTechnicianPdfReportAsync(report);
                var fileName = $"{report.TechnicianName.Replace(" ", "_")}_Performance_Report ({DateTime.Now:dd/MM/yyyy}).pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF report");
                TempData["Error"] = "Failed to generate PDF report. Please try again.";
                return RedirectToAction(nameof(TechnicianReport));
            }
        }

        private async Task<IActionResult> ExportTechnicianExcel(TechnicianReportViewModel report)
        {
            try
            {
                var excelBytes = await _technician.GenerateTechnicianExcelReportAsync(report);
                var fileName = $"{report.TechnicianName.Replace(" ", "_")}_Performance_Report ({DateTime.Now:dd/MM/yyyy}).xlsx";
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                TempData["Error"] = "Failed to generate Excel report. Please try again.";
                return RedirectToAction(nameof(TechnicianReport));
            }
        }

        // AJAX endpoint for technician details
        [HttpGet]
        public async Task<IActionResult> GetTechnicianReportData(int technicianId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var parameters = new TechnicianReportViewModel
                {
                    TechnicianId = technicianId,
                    StartDate = startDate,
                    EndDate = endDate,
                    IncludeCharts = true
                };

                var report = await _technician.GenerateTechnicianReportAsync(parameters);

                var chartData = new
                {
                    statusDistribution = report.FaultsByStatus,
                    priorityDistribution = report.FaultsByPriority,
                    monthlyTrends = report.MonthlyCompletionRate
                };

                return Json(chartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting technician report data for technician {TechnicianId}", technicianId);
                return BadRequest("Error loading chart data");
            }
        }
    }
}