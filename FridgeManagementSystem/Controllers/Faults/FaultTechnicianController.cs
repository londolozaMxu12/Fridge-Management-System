using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OfficeOpenXml;
using System.Drawing; // For Color in EPPlus
using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FridgeManagementSystem.Models;
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

        private async Task SetNavigationViewBagProperties()
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            var baseQuery = _context.Faults.AsQueryable();

            if (currentTechnician != null)
            {
                // For technicians, only show counts for their assigned faults + unassigned
                baseQuery = baseQuery.Where(f => f.FaultTechnicianId == null || f.FaultTechnicianId == currentTechnician.Id);
            }

            ViewBag.TotalFaultsCount = await baseQuery.CountAsync();
            ViewBag.PendingFaultsCount = await baseQuery.CountAsync(f => f.Status == FaultStatus.Reported);
            ViewBag.UrgentFaultsCount = await baseQuery.CountAsync(f => f.Priority == FaultPriority.Critical || f.Priority == FaultPriority.High);
            ViewBag.InProgressFaultsCount = await baseQuery.CountAsync(f => f.Status == FaultStatus.InProgress);
            ViewBag.CompletedFaultsCount = await baseQuery.CountAsync(f => f.Status == FaultStatus.Completed);
        }

        public async Task<IActionResult> Dashboard()
        {
            // Check if current user is a fault technician and filter accordingly
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            var isFaultTechnician = currentTechnician != null;
            var currentTechnicianId = currentTechnician?.Id;

            await SetNavigationViewBagProperties();

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
            var schedules = currentTechnicianId.HasValue ?
                await _context.RepairSchedules.Where(rs => rs.FaultTechnicianId == currentTechnicianId).ToListAsync()
                : new List<RepairSchedule>();

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
            ViewBag.IsFaultTechnician = isFaultTechnician;
            ViewBag.CurrentTechnicianId = currentTechnicianId;
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
                fault.UpdatedAt = DateTime.UtcNow;

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
        // GET: FaultTechnician/MyPerformance
        // GET: FaultTechnician/MyPerformance
        public async Task<IActionResult> MyPerformance(DateTime? dateFrom, DateTime? dateTo, string reportType = "detailed")
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                TempData["Error"] = "Access denied. Only fault technicians can view performance reports.";
                return RedirectToAction(nameof(Index));
            }

            // Set default date range if not provided
            dateFrom ??= DateTime.Now.AddMonths(-3);
            dateTo ??= DateTime.Now;

            // Get fault data for the date range
            var technicianFaults = await _context.Faults
                .Include(f => f.RepairSchedules)
                .Include(f => f.Fridge)
                .ThenInclude(f => f.FridgeType)
                .Include(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .Where(f => f.FaultTechnicianId == currentTechnician.Id &&
                           f.ReportedDate >= dateFrom && f.ReportedDate <= dateTo)
                .ToListAsync();

            // Create and populate the ViewModel with all required properties
            var model = new TechnicianPerformanceViewModel
            {
                Technician = currentTechnician,
                ReportDateFrom = dateFrom,
                ReportDateTo = dateTo,
                

                // Initialize collections
                MonthlyPerformance = new List<MonthlyPerformance>(),
                PriorityMetrics = new List<PriorityMetrics>(),
                PerformanceTrends = new List<PerformanceTrend>(),
                TechnicianComparisons = new List<TechnicianComparison>()
            };

            // Calculate comprehensive metrics
            CalculatePerformanceMetrics(model, technicianFaults);
            GeneratePerformanceChartsData(model, technicianFaults);

            ViewBag.ReportType = reportType;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            await SetNavigationViewBagProperties();
            return View(model);
        }

        // GET: FaultTechnician/FaultAnalysis
        public async Task<IActionResult> FaultAnalysis(DateTime? dateFrom, DateTime? dateTo, string analysisType = "comprehensive")
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                TempData["Error"] = "Access denied. Only fault technicians can view analysis reports.";
                return RedirectToAction(nameof(Index));
            } 

            // Set default date range if not provided
            dateFrom ??= DateTime.Now.AddMonths(-6);
            dateTo ??= DateTime.Now;

            var model = new FaultAnalysisViewModel
            {
                ReportDateFrom = dateFrom,
                ReportDateTo = dateTo,
                AnalysisType = analysisType
            };

            // Get comprehensive fault data
            var allFaults = await _context.Faults
                .Include(f => f.FaultTechnician)
                .ThenInclude(t => t.User)
                .Include(f => f.Fridge)
                .ThenInclude(fr => fr.FridgeType)
                .Include(f => f.ReportedBy)
                .ThenInclude(c => c.User)
                .Include(f => f.RepairSchedules)
                .Where(f => f.ReportedDate >= dateFrom && f.ReportedDate <= dateTo)
                .ToListAsync();

            CalculateComprehensiveFaultAnalysis(model, allFaults);
            
            ViewBag.AnalysisType = analysisType;
            ViewBag.DateFrom = dateFrom?.ToString("dd-MM-yyyy");
            ViewBag.DateTo = dateTo?.ToString("dd-MM-yyyy");

            await SetNavigationViewBagProperties();
            return View(model);
        }
        // POST: FaultTechnician/ExportPerformanceReport
        // POST: FaultTechnician/ExportPerformanceReport
        [HttpPost]
        public async Task<IActionResult> ExportPerformanceReport(DateTime? dateFrom, DateTime? dateTo, string format = "pdf")
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                return Json(new { success = false, message = "Access denied" });
            }

            dateFrom ??= DateTime.Now.AddMonths(-3);
            dateTo ??= DateTime.Now;

            var model = new TechnicianPerformanceViewModel
            {
                Technician = currentTechnician,
                ReportDateFrom = dateFrom,
                ReportDateTo = dateTo
            };

            // Generate report data
            var technicianFaults = await _context.Faults
                .Include(f => f.RepairSchedules)
                .Where(f => f.FaultTechnicianId == currentTechnician.Id &&
                           f.ReportedDate >= dateFrom && f.ReportedDate <= dateTo)
                .ToListAsync();

            CalculatePerformanceMetrics(model, technicianFaults);

            // Generate PDF report
            if (format.ToLower() == "pdf")
            {
                var pdfBytes = GeneratePerformancePdfReport(model);
                return File(pdfBytes, "application/pdf",
                    $"Performance_Report_{currentTechnician.User.FullName}_{DateTime.Now:ddMMyyyy}.pdf");
            }

            // Generate Excel report - FIXED: Correct content type and extension
            if (format.ToLower() == "excel")
            {
                var excelBytes = GeneratePerformanceExcelReport(model);

                // FIX: Ensure we have valid data before returning
                if (excelBytes == null || excelBytes.Length == 0)
                {
                    TempData["Error"] = "Failed to generate Excel report. Please try again.";
                    return RedirectToAction(nameof(MyPerformance));
                }

                // FIX: Correct content type and file extension
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // Correct MIME type
                    $"Performance_Report_{currentTechnician.User.FullName}_{DateTime.Now:ddMMyyyy}.xlsx"); // Correct extension
            }

            return Json(new { success = false, message = "Unsupported format" });
        }

        // POST: FaultTechnician/ExportFaultAnalysisReport
        [HttpPost]
        public async Task<IActionResult> ExportFaultAnalysisReport(DateTime? dateFrom, DateTime? dateTo, string format = "pdf")
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                return Json(new { success = false, message = "Access denied" });
            }

            dateFrom ??= DateTime.Now.AddMonths(-6);
            dateTo ??= DateTime.Now;

            var model = new FaultAnalysisViewModel
            {
                ReportDateFrom = dateFrom,
                ReportDateTo = dateTo
            };

            var allFaults = await _context.Faults
                .Include(f => f.FaultTechnician)
                .ThenInclude(t => t.User)
                .Include(f => f.Fridge)
                .ThenInclude(fr => fr.FridgeType)
                .Where(f => f.ReportedDate >= dateFrom && f.ReportedDate <= dateTo)
                .ToListAsync();

            CalculateComprehensiveFaultAnalysis(model, allFaults);

            if (format.ToLower() == "pdf")
            {
                var pdfBytes = GenerateFaultAnalysisPdfReport(model);
                return File(pdfBytes, "application/pdf",
                    $"Fault_Analysis_Report_{DateTime.Now:ddMMyyyy}.pdf");
            }

            if (format.ToLower() == "excel")
            {
                var excelBytes = GenerateFaultAnalysisExcelReport(model);
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Fault_Analysis_Report_{DateTime.Now:ddMMyyyy}.xlsx");
            }

            return Json(new { success = false, message = "Unsupported format" });
        }

        // AJAX endpoint for real-time chart data
        [HttpGet]
        public async Task<JsonResult> GetPerformanceChartData(DateTime? dateFrom, DateTime? dateTo)
        {
            var currentTechnician = await GetCurrentFaultTechnicianAsync();
            if (currentTechnician == null)
            {
                return Json(new { success = false });
            }

            dateFrom ??= DateTime.Now.AddMonths(-6);
            dateTo ??= DateTime.Now;

            var faults = await _context.Faults
                .Where(f => f.FaultTechnicianId == currentTechnician.Id &&
                           f.ReportedDate >= dateFrom && f.ReportedDate <= dateTo)
                .ToListAsync();

            var chartData = new
            {
                monthlyTrends = GenerateMonthlyTrends(faults, dateFrom.Value, dateTo.Value),
                priorityDistribution = GeneratePriorityDistribution(faults),
                efficiencyTrends = GenerateEfficiencyTrends(faults)
            };

            return Json(new { success = true, data = chartData });
        }
        private static object GeneratePriorityDistribution(List<Fault> faults)
        {
            var priorityGroups = faults
        .GroupBy(f => f.Priority)
        .Select(g => new
        {
            Priority = g.Key.ToString(),
            Count = g.Count(),
            Completed = g.Count(f => f.Status == FaultStatus.Completed),
            CompletionRate = g.Count() > 0 ? (double)g.Count(f => f.Status == FaultStatus.Completed) / g.Count() * 100 : 0
        })
        .OrderByDescending(p => p.Priority) // Critical, High, Medium, Low
        .ToList();

            return new
            {
                labels = priorityGroups.Select(p => p.Priority).ToArray(),
                counts = priorityGroups.Select(p => p.Count).ToArray(),
                completed = priorityGroups.Select(p => p.Completed).ToArray(),
                rates = priorityGroups.Select(p => p.CompletionRate).ToArray()
            };
        }
        private static object GenerateMonthlyTrends(List<Fault> faults, DateTime dateFrom, DateTime dateTo)
        {
            var monthlyData = faults
                .Where(f => f.ReportedDate >= dateFrom && f.ReportedDate <= dateTo)
                .GroupBy(f => new { f.ReportedDate.Year, f.ReportedDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Period = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Reported = g.Count(),
                    Resolved = g.Count(f => f.Status == FaultStatus.Completed),
                    Critical = g.Count(f => f.Priority == FaultPriority.Critical),
                    AvgResolutionTime = g.Where(f => f.Status == FaultStatus.Completed && f.UpdatedAt.HasValue)
                                       .Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours)
                })
                .ToList();

            return new
            {
                labels = monthlyData.Select(m => m.Period).ToArray(),
                reported = monthlyData.Select(m => m.Reported).ToArray(),
                resolved = monthlyData.Select(m => m.Resolved).ToArray(),
                critical = monthlyData.Select(m => m.Critical).ToArray(),
                avgTimes = monthlyData.Select(m => m.AvgResolutionTime).ToArray()
            };
        }
        private static object GenerateEfficiencyTrends(List<Fault> faults)
        {
            if (!faults.Any())
            {
                return new
                {
                    labels = new string[0],
                    efficiencies = new double[0]
                };
            }

            var weeklyGroups = faults
                .GroupBy(f => new
                {
                    f.ReportedDate.Year,
                    Week = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        f.ReportedDate,
                        System.Globalization.CalendarWeekRule.FirstDay,
                        DayOfWeek.Monday)
                })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Week)
                .TakeLast(8) // Last 8 weeks
                .Select(g => new
                {
                    Period = $"W{g.Key.Week}",
                    Efficiency = CalculateWeeklyEfficiency(g.ToList())
                })
                .ToList();

            return new
            {
                labels = weeklyGroups.Select(w => w.Period).ToArray(),
                efficiencies = weeklyGroups.Select(w => w.Efficiency).ToArray()
            };
        }

        private static double CalculateWeeklyEfficiency(List<Fault> weeklyFaults)
        {
            if (!weeklyFaults.Any()) return 0;

            var completed = weeklyFaults.Count(f => f.Status == FaultStatus.Completed);
            var completionRate = (double)completed / weeklyFaults.Count * 100;

            var completedFaults = weeklyFaults.Where(f => f.Status == FaultStatus.Completed && f.UpdatedAt.HasValue);
            if (completedFaults.Any())
            {
                var avgTime = completedFaults.Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours);
                var timeScore = Math.Max(0, 100 - (avgTime / 48 * 100));
                return (completionRate * 0.6) + (timeScore * 0.4);
            }

            return completionRate * 0.6;
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
        // Private helper methods for calculations
        private static void CalculatePerformanceMetrics(TechnicianPerformanceViewModel model, List<Fault> faults)
        {
            var completedFaults = faults.Where(f => f.Status == FaultStatus.Completed).ToList();

            // Set the properties that the view expects
            model.TotalFaultsAssigned = faults.Count;
            model.CompletedFaults = completedFaults.Count;
            model.InProgressFaults = faults.Count(f => f.Status == FaultStatus.InProgress);
            model.OverdueFaults = faults.Count(f => f.Status != FaultStatus.Completed &&
                                                   (DateTime.Now - f.ReportedDate).TotalDays > 7);

            model.CompletionRate = model.TotalFaultsAssigned > 0 ?
                (double)model.CompletedFaults / model.TotalFaultsAssigned * 100 : 0;

            // Calculate average completion time
            var completedFaultsWithUpdate = completedFaults
                .Where(f => f.UpdatedAt.HasValue)
                .ToList();

            if (completedFaultsWithUpdate.Any())
            {
                model.AverageCompletionTimeHours = completedFaultsWithUpdate
                    .Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours);
            }
            else
            {
                model.AverageCompletionTimeHours = 0;
            }

            // Calculate efficiency score
            model.EfficiencyScore = CalculateEfficiencyScore(faults, completedFaults);
        }
        private static void GeneratePerformanceChartsData(TechnicianPerformanceViewModel model, List<Fault> faults)
        {
            // Clear any existing data
            model.MonthlyPerformance.Clear();
            model.PriorityMetrics.Clear();

            // Generate monthly performance data
            var monthlyData = faults
                .GroupBy(f => new { f.ReportedDate.Year, f.ReportedDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month);

            foreach (var monthGroup in monthlyData)
            {
                var monthDate = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1);
                var completed = monthGroup.Count(f => f.Status == FaultStatus.Completed);
                var total = monthGroup.Count();

                model.MonthlyPerformance.Add(new MonthlyPerformance
                {
                    Month = monthDate.ToString("MMM yyyy"),
                    TotalFaults = total,
                    CompletedFaults = completed
                });
            }

            // Generate priority metrics - ensure ALL priorities are included
            var allPriorities = Enum.GetValues(typeof(FaultPriority)).Cast<FaultPriority>();

            foreach (var priority in allPriorities)
            {
                var priorityFaults = faults.Where(f => f.Priority == priority).ToList();
                var completed = priorityFaults.Count(f => f.Status == FaultStatus.Completed);
                var total = priorityFaults.Count;
                var successRate = total > 0 ? (double)completed / total * 100 : 0;

                model.PriorityMetrics.Add(new PriorityMetrics
                {
                    Priority = (int)priority,
                    Attended = total,
                    Completed = completed,
                    SuccessRate = successRate
                });
            }

            // Sort by priority severity (Critical=4, High=3, Medium=2, Low=1)
            model.PriorityMetrics = model.PriorityMetrics
                .OrderByDescending(p => p.Priority)
                .ToList();
        }
        private async Task GenerateComparativeAnalysis(TechnicianPerformanceViewModel model, int technicianId, DateTime fromDate, DateTime toDate)
        {
            // Get data for all technicians for comparison
            var allTechnicians = await _context.Employees
                .Include(e => e.User)
                .Where(e => e.EmployeeType.Name == "FaultTechnician" && e.IsActive)
                .ToListAsync();

            var technicianFaults = await _context.Faults
                .Where(f => f.ReportedDate >= fromDate && f.ReportedDate <= toDate &&
                           f.FaultTechnicianId.HasValue)
                .ToListAsync();

            var comparisons = new List<TechnicianComparison>();

            foreach (var tech in allTechnicians)
            {
                var techFaults = technicianFaults.Where(f => f.FaultTechnicianId == tech.Id).ToList();
                var completed = techFaults.Count(f => f.Status == FaultStatus.Completed);
                var total = techFaults.Count;
                var completionRate = total > 0 ? (double)completed / total * 100 : 0;
                var avgTime = techFaults
                    .Where(f => f.Status == FaultStatus.Completed && f.UpdatedAt.HasValue)
                    .Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours);

                comparisons.Add(new TechnicianComparison
                {
                    TechnicianName = tech.User.FullName,
                    CompletedFaults = completed,
                    AvgCompletionTime = avgTime,
                    CompletionRate = completionRate,
                    
                });
            }

            // Rank technicians
            var rankedComparisons = comparisons
                .OrderByDescending(c => c.CompletionRate)
                .ThenBy(c => c.AvgCompletionTime)
                .Select((c, index) =>
                {
                    c.Rank = index + 1;
                    return c;
                })
                .ToList();

            model.TechnicianComparisons = rankedComparisons;
        }

        private static double CalculateEfficiencyScore(List<Fault> faults, List<Fault> completedFaults)
        {
            if (!completedFaults.Any()) return 0;

            var completionRate = (double)completedFaults.Count / faults.Count * 100;
            var avgTime = completedFaults
                .Where(f => f.UpdatedAt.HasValue)
                .Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours);

            // Normalize time (lower is better, max 48 hours for normalization)
            var timeScore = Math.Max(0, 100 - (avgTime / 48 * 100));

            // Simulate quality score (based on priority completion)
            var criticalCompleted = completedFaults.Count(f => f.Priority == FaultPriority.Critical);
            var criticalScore = criticalCompleted > 0 ? 100 : 80;

            // Weighted average
            return (completionRate * 0.4) + (timeScore * 0.4) + (criticalScore * 0.2);
        }
        private static void GeneratePerformanceTrends(TechnicianPerformanceViewModel model, List<Fault> faults)
        {
            var completedFaults = faults.Where(f => f.Status == FaultStatus.Completed).ToList();

            // Group by month and calculate trends
            var monthlyGroups = faults
                .GroupBy(f => new { f.ReportedDate.Year, f.ReportedDate.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToList();

            for (int i = 0; i < monthlyGroups.Count; i++)
            {
                var monthGroup = monthlyGroups[i];
                var monthDate = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1);
                var monthFaults = monthGroup.ToList();
                var monthCompleted = monthFaults.Count(f => f.Status == FaultStatus.Completed);

                // Calculate completion rate
                var completionRate = monthFaults.Count > 0 ?
                    (double)monthCompleted / monthFaults.Count * 100 : 0;

                // Calculate average completion time for this month - FIXED
                var completedFaultsWithUpdate = monthFaults
                    .Where(f => f.Status == FaultStatus.Completed && f.UpdatedAt.HasValue)
                    .ToList();

                double avgCompletionTime = 0;
                if (completedFaultsWithUpdate.Any())
                {
                    avgCompletionTime = completedFaultsWithUpdate
                        .Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours);
                }

                var trend = new PerformanceTrend
                {
                    Period = monthDate.ToString("MMM yyyy"),
                    CompletionRate = completionRate,
                    AvgCompletionTime = avgCompletionTime,
                    TotalFaults = monthFaults.Count,
                    CompletedFaults = monthCompleted
                };

                model.PerformanceTrends.Add(trend);
            }

            // Calculate trend direction (improving/declining)
            if (model.PerformanceTrends.Count >= 2)
            {
                var recent = model.PerformanceTrends.TakeLast(3).ToList();
                if (recent.Count >= 2)
                {
                    var current = recent[recent.Count - 1];
                    var previous = recent[recent.Count - 2];

                    // Simple trend calculation
                    var completionTrend = current.CompletionRate - previous.CompletionRate;
                    var timeTrend = previous.AvgCompletionTime - current.AvgCompletionTime; // Lower time is better

                    // Overall trend score (-1 to 1)
                    var overallTrend = (completionTrend / 100) + (timeTrend / 24);
                    model.EfficiencyScore = Math.Max(0, Math.Min(100, model.EfficiencyScore + (overallTrend * 10)));
                }
            }
        }
        private static void GeneratePriorityMetrics(TechnicianPerformanceViewModel model, List<Fault> faults)
        {
            var priorityGroups = faults.GroupBy(f => f.Priority);

            foreach (var priorityGroup in priorityGroups)
            {
                var priorityFaults = priorityGroup.ToList();
                var completedFaults = priorityFaults.Count(f => f.Status == FaultStatus.Completed);
                var totalFaults = priorityFaults.Count;

                // Calculate average completion time for this priority - FIXED with null check
                var completedFaultsWithUpdate = priorityFaults
                    .Where(f => f.Status == FaultStatus.Completed && f.UpdatedAt.HasValue)
                    .ToList();

                double avgCompletionTime = 0;
                if (completedFaultsWithUpdate.Any())
                {
                    avgCompletionTime = completedFaultsWithUpdate
                        .Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours);
                }

                var successRate = totalFaults > 0 ? (double)completedFaults / totalFaults * 100 : 0;

                model.PriorityMetrics.Add(new PriorityMetrics
                {
                    Priority = (int)priorityGroup.Key, // CAST TO INT
                    Attended = totalFaults,
                    Completed = completedFaults,
                    AvgCompletionTime = avgCompletionTime,
                    SuccessRate = successRate
                });
            }

            // Fill in missing priorities with zero values
            var allPriorities = Enum.GetValues(typeof(FaultPriority)).Cast<FaultPriority>();
            foreach (var priority in allPriorities)
            {
                if (!model.PriorityMetrics.Any(p => p.Priority == (int)priority)) // CAST TO INT FOR COMPARISON
                {
                    model.PriorityMetrics.Add(new PriorityMetrics
                    {
                        Priority = (int)priority, // CAST TO INT
                        Attended = 0,
                        Completed = 0,
                        AvgCompletionTime = 0,
                        SuccessRate = 0
                    });
                }
            }

            // Sort by priority severity (Critical, High, Medium, Low)
            model.PriorityMetrics = model.PriorityMetrics
                .OrderByDescending(p => p.Priority)
                .ToList();
        }
        private static void CalculateComprehensiveFaultAnalysis(FaultAnalysisViewModel model, List<Fault> allFaults)
        {
            var completedFaults = allFaults.Where(f => f.Status == FaultStatus.Completed).ToList();
            var resolvedFaults = allFaults.Where(f => f.Status == FaultStatus.Completed).ToList();

            // Executive Summary
            model.Summary.TotalFaults = allFaults.Count;
            model.Summary.ResolvedFaults = resolvedFaults.Count;
            model.Summary.ResolutionRate = allFaults.Count > 0 ? (double)resolvedFaults.Count / allFaults.Count * 100 : 0;

            // Calculate average resolution time
            if (completedFaults.Any(f => f.UpdatedAt.HasValue))
            {
                model.Summary.AvgResolutionTime = completedFaults
                    .Where(f => f.UpdatedAt.HasValue)
                    .Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours);
            }

            // Top performing technician
            var topTechnician = allFaults
                .Where(f => f.FaultTechnicianId.HasValue)
                .GroupBy(f => f.FaultTechnician)
                .Select(g => new
                {
                    Technician = g.Key,
                    Completed = g.Count(f => f.Status == FaultStatus.Completed),
                    Total = g.Count(),
                    Rate = g.Count() > 0 ? (double)g.Count(f => f.Status == FaultStatus.Completed) / g.Count() * 100 : 0
                })
                .OrderByDescending(x => x.Rate)
                .FirstOrDefault();

            model.Summary.TopPerformingTechnician = topTechnician?.Technician?.User?.FullName ?? "N/A";

            // Most common fault type
            var mostCommonFault = allFaults
                .Where(f => !string.IsNullOrEmpty(f.Title))
                .GroupBy(f => f.Title)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            model.Summary.MostCommonFaultType = mostCommonFault?.Key ?? "N/A";

            // Generate trends
            GenerateFaultTrends(model, allFaults);

            // Generate geographic distribution
            GenerateGeographicDistribution(model, allFaults);

            // Generate technician efficiency
            GenerateTechnicianEfficiency(model, allFaults);

            // Generate KPI metrics
            GenerateKpiMetrics(model, allFaults);
        }

        private static void GenerateFaultTrends(FaultAnalysisViewModel model, List<Fault> allFaults)
        {
            var monthlyGroups = allFaults
        .GroupBy(f => new { f.ReportedDate.Year, f.ReportedDate.Month })
        .OrderBy(g => g.Key.Year)
        .ThenBy(g => g.Key.Month)
        .ToList();

            foreach (var monthGroup in monthlyGroups)
            {
                var monthDate = new DateTime(monthGroup.Key.Year, monthGroup.Key.Month, 1);
                var monthFaults = monthGroup.ToList();
                var resolved = monthFaults.Count(f => f.Status == FaultStatus.Completed);
                var critical = monthFaults.Count(f => f.Priority == FaultPriority.Critical || f.Priority == FaultPriority.High);

                var resolutionRate = monthFaults.Count > 0 ? (double)resolved / monthFaults.Count * 100 : 0;

                var avgResolutionTime = monthFaults
                    .Where(f => f.Status == FaultStatus.Completed && f.UpdatedAt.HasValue)
                    .Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours);

                // Calculate trend direction (simple comparison with previous month)
                double trendDirection = 0;
                var currentIndex = monthlyGroups.IndexOf(monthGroup);
                if (currentIndex > 0)
                {
                    var previousMonth = monthlyGroups[currentIndex - 1];
                    var previousResolved = previousMonth.Count(f => f.Status == FaultStatus.Completed);
                    var previousTotal = previousMonth.Count();
                    var previousRate = previousTotal > 0 ? (double)previousResolved / previousTotal * 100 : 0;

                    trendDirection = resolutionRate > previousRate ? 1 : (resolutionRate < previousRate ? -1 : 0);
                }

                model.Trends.Add(new FaultTrend
                {
                    Period = monthDate.ToString("MMM yyyy"),
                    ReportedFaults = monthFaults.Count,
                    ResolvedFaults = resolved,
                    CriticalFaults = critical,
                    ResolutionRate = resolutionRate,
                    AvgResolutionTime = avgResolutionTime,
                    TrendDirection = trendDirection
                });
            }
        }

        private static void GenerateGeographicDistribution(FaultAnalysisViewModel model, List<Fault> allFaults)
        {
            // Get actual cities from customer addresses instead of hardcoded areas
            var customerCities = allFaults
                .Where(f => f.ReportedBy != null && f.ReportedBy.User != null && !string.IsNullOrEmpty(f.ReportedBy.User.City))
                .Select(f => f.ReportedBy.User.City)
                .Distinct()
                .ToList();

            var random = new Random();

            foreach (var city in customerCities)
            {
                // Use actual faults for this city instead of random simulation
                var cityFaults = allFaults.Where(f =>
                    f.ReportedBy != null &&
                    f.ReportedBy.User != null &&
                    f.ReportedBy.User.City == city).ToList();

                var criticalFaults = cityFaults.Count(f => f.Priority == FaultPriority.Critical);
                var resolved = cityFaults.Count(f => f.Status == FaultStatus.Completed);

                var resolutionRate = cityFaults.Count > 0 ? (double)resolved / cityFaults.Count * 100 : 0;

                // Simulate response time
                var avgResponseTime = cityFaults.Any(f => f.UpdatedAt.HasValue) ?
                       cityFaults.Where(f => f.UpdatedAt.HasValue).Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours) : 24;


                var hotspotLevel = criticalFaults > 10 ? "Critical" :
                                  criticalFaults > 5 ? "High" :
                                  criticalFaults > 2 ? "Medium" : "Low";

                model.GeographicData.Add(new GeographicDistribution
                {
                    City = city,
                    TotalFaults = cityFaults.Count,
                    CriticalFaults = criticalFaults,
                    ResolutionRate = resolutionRate,
                    AvgResponseTime = avgResponseTime,
                    HotspotLevel = hotspotLevel

                });
            }
        }

        private static void GenerateTechnicianEfficiency(FaultAnalysisViewModel model, List<Fault> allFaults)
        {
            var technicianGroups = allFaults
                .Where(f => f.FaultTechnicianId.HasValue)
                .GroupBy(f => f.FaultTechnician)
                .ToList();

            foreach (var techGroup in technicianGroups)
            {
                var techFaults = techGroup.ToList();
                var completed = techFaults.Count(f => f.Status == FaultStatus.Completed);
                var total = techFaults.Count;

                var completionRate = total > 0 ? (double)completed / total * 100 : 0;

                var avgResolutionTime = techFaults
                    .Where(f => f.Status == FaultStatus.Completed && f.UpdatedAt.HasValue)
                    .Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours);

                // Calculate efficiency score
                var efficiencyScore = CalculateTechnicianEfficiency(techFaults, completed);

                // Quality score (simulated)
                var qualityScore = 80 + (new Random().NextDouble() * 20); // 80-100

                // Cost effectiveness (simulated)
                var costEffectiveness = 75 + (new Random().NextDouble() * 25); // 75-100

                var performanceTier = efficiencyScore >= 90 ? "Expert" :
                                     efficiencyScore >= 80 ? "Advanced" :
                                     efficiencyScore >= 60 ? "Intermediate" : "Beginner";

                model.TechnicianEfficiency.Add(new TechnicianEfficiency
                {
                    TechnicianName = techGroup.Key?.User?.FullName ?? "Unknown",
                    CompletedFaults = completed,
                    EfficiencyScore = efficiencyScore,

                    PerformanceTier = performanceTier
                });
            }
        }
        private static double CalculateTechnicianEfficiency(List<Fault> faults, int completedCount)
        {
            if (!faults.Any()) return 0;

            var completionRate = (double)completedCount / faults.Count * 100;

            var avgTime = faults
                .Where(f => f.Status == FaultStatus.Completed && f.UpdatedAt.HasValue)
                .Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours);

            // Normalize time (lower is better)
            var timeScore = Math.Max(0, 100 - (avgTime / 48 * 100));

            // Priority weighting (completing high priority faults is better)
            var criticalCompleted = faults.Count(f => f.Status == FaultStatus.Completed && f.Priority == FaultPriority.Critical);
            var priorityScore = criticalCompleted > 0 ? 100 : 80;

            return (completionRate * 0.4) + (timeScore * 0.4) + (priorityScore * 0.2);
        }

        private static void GenerateKpiMetrics(FaultAnalysisViewModel model, List<Fault> allFaults)
        {
            var completedFaults = allFaults.Where(f => f.Status == FaultStatus.Completed).ToList();

            // Current values
            var currentResolutionRate = allFaults.Count > 0 ?
                (double)completedFaults.Count / allFaults.Count * 100 : 0;

            var currentAvgTime = completedFaults.Any(f => f.UpdatedAt.HasValue) ?
                completedFaults.Where(f => f.UpdatedAt.HasValue)
                    .Average(f => (f.UpdatedAt.Value - f.ReportedDate).TotalHours) : 0;

            // Previous period values (simulated)
            var previousResolutionRate = currentResolutionRate * 0.9; // 10% worse
            var previousAvgTime = currentAvgTime * 1.1; // 10% slower

            model.Kpis.AddRange(new[]
            {
                new KpiMetric
                {
            Name = "Resolution Rate",
            CurrentValue = currentResolutionRate,
            TargetValue = 85,
            PreviousValue = previousResolutionRate,
            Status = currentResolutionRate > previousResolutionRate ? "Improving" : "Declining",
            Variance = currentResolutionRate - previousResolutionRate
               },
               new KpiMetric
               {
            Name = "Average Resolution Time (hours)",
            CurrentValue = currentAvgTime,
            TargetValue = 24,
            PreviousValue = previousAvgTime,
            Status = currentAvgTime < previousAvgTime ? "Improving" : "Declining",
            Variance = previousAvgTime - currentAvgTime // Negative variance is good for time
               },

            });
        }
        // PDF Generation for Performance Report
        // PDF Generation for Performance Report
        private byte[] GeneratePerformancePdfReport(TechnicianPerformanceViewModel model)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        // Header
                        page.Header()
                            .AlignCenter()
                            .Text(model.ReportTitle)
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        page.Header()
                            .AlignCenter()
                            .Text($"Generated on: {model.GeneratedOn}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);

                        page.Header()
                            .AlignCenter()
                            .Text($"Period: {model.ReportPeriod}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);

                        // Content
                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Spacing(20);

                                // Technician Information
                                column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(techColumn =>
                                {
                                    techColumn.Spacing(5);
                                    techColumn.Item().Text("Technician Information").SemiBold().FontSize(16);
                                    techColumn.Item().Text($"Name: {model.Technician?.User?.FullName ?? "N/A"}");
                                    techColumn.Item().Text($"Employee No: {model.Technician?.EmployeeNo ?? "N/A"}");
                                });

                                // Performance Summary
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Metric").FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Value").FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Target").FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Status").FontColor(Colors.White);
                                    });

                                    AddPerformanceMetric(table, "Completion Rate", $"{model.CompletionRate:F1}%", "85%", model.CompletionRate >= 85);
                                    AddPerformanceMetric(table, "Efficiency Score", $"{model.EfficiencyScore:F1}/100", "80", model.EfficiencyScore >= 80);
                                    AddPerformanceMetric(table, "Avg. Resolution Time", $"{model.AverageCompletionTimeHours:F1}h", "24h", model.AverageCompletionTimeHours <= 24);
                                });

                                // Priority Performance - FIXED: Handle null or empty PriorityMetrics
                                if (model.PriorityMetrics != null && model.PriorityMetrics.Any())
                                {
                                    column.Item().Text("Priority Performance").SemiBold().FontSize(16);
                                    column.Item().Table(priorityTable =>
                                    {
                                        priorityTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                        });

                                        priorityTable.Header(header =>
                                        {
                                            header.Cell().Background(Colors.Green.Medium).Padding(5).Text("Priority").FontColor(Colors.White);
                                            header.Cell().Background(Colors.Green.Medium).Padding(5).Text("Attended").FontColor(Colors.White);
                                            header.Cell().Background(Colors.Green.Medium).Padding(5).Text("Completed").FontColor(Colors.White);
                                            header.Cell().Background(Colors.Green.Medium).Padding(5).Text("Success Rate").FontColor(Colors.White);
                                        });

                                        foreach (var priority in model.PriorityMetrics)
                                        {
                                            // FIXED: Convert priority number to readable name
                                            var priorityName = priority.Priority switch
                                            {
                                                4 => "Critical",
                                                3 => "High",
                                                2 => "Medium",
                                                1 => "Low",
                                                _ => $"Priority {priority.Priority}"
                                            };

                                            priorityTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(priorityName);
                                            priorityTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(priority.Attended.ToString());
                                            priorityTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(priority.Completed.ToString());
                                            priorityTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{priority.SuccessRate:F1}%");
                                        }
                                    });
                                }
                                else
                                {
                                    column.Item().Text("Priority Performance").SemiBold().FontSize(16);
                                    column.Item().Background(Colors.Grey.Lighten3).Padding(10).Text("No priority data available").Italic();
                                }

                                // Monthly Trends - FIXED: Handle null or empty MonthlyPerformance
                                if (model.MonthlyPerformance != null && model.MonthlyPerformance.Any())
                                {
                                    column.Item().Text("Monthly Performance Trends").SemiBold().FontSize(16);
                                    column.Item().Table(trendsTable =>
                                    {
                                        trendsTable.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                        });

                                        trendsTable.Header(header =>
                                        {
                                            header.Cell().Background(Colors.Orange.Medium).Padding(5).Text("Month").FontColor(Colors.White);
                                            header.Cell().Background(Colors.Orange.Medium).Padding(5).Text("Total Faults").FontColor(Colors.White);
                                            header.Cell().Background(Colors.Orange.Medium).Padding(5).Text("Completed").FontColor(Colors.White);
                                            header.Cell().Background(Colors.Orange.Medium).Padding(5).Text("Completion Rate").FontColor(Colors.White);
                                        });

                                        foreach (var month in model.MonthlyPerformance)
                                        {
                                            var completionRate = month.TotalFaults > 0 ?
                                                (double)month.CompletedFaults / month.TotalFaults * 100 : 0;

                                            trendsTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(month.Month ?? "N/A");
                                            trendsTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(month.TotalFaults.ToString());
                                            trendsTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(month.CompletedFaults.ToString());
                                            trendsTable.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{completionRate:F1}%");
                                        }
                                    });
                                }
                                else
                                {
                                    column.Item().Text("Monthly Performance Trends").SemiBold().FontSize(16);
                                    column.Item().Background(Colors.Grey.Lighten3).Padding(10).Text("No monthly performance data available").Italic();
                                }
                            });

                        // Footer
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" of ");
                                x.TotalPages();
                            });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"PDF Generation Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Fallback to simple PDF generation
                return GenerateSimplePerformancePdf(model);
            }
        }

        private static void AddPerformanceMetric(TableDescriptor table, string metric, string value, string target, bool isOnTarget)
        {
            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(metric);
            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(value);
            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(target);
            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(isOnTarget ? "On Target" : "Needs Improvement");
        }

        private byte[] GenerateSimplePerformancePdf(TechnicianPerformanceViewModel model)
        {
            var htmlContent = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 40px; }}
                .header {{ text-align: center; color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 20px; }}
                .section {{ margin: 30px 0; }}
                .section-title {{ background: #3498db; color: white; padding: 10px; border-radius: 5px; }}
                .metric {{ margin: 10px 0; padding: 10px; background: #f8f9fa; border-radius: 5px; }}
                table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
                th {{ background-color: #3498db; color: white; }}
                .on-target {{ color: #27ae60; font-weight: bold; }}
                .needs-improvement {{ color: #e74c3c; font-weight: bold; }}
            </style>
        </head>
        <body>
            <div class='header'>
                <h1>{model.ReportTitle}</h1>
                <p>Generated on: {model.GeneratedOn}</p>
                <p>Period: {model.ReportPeriod}</p>
            </div>

            <div class='section'>
                <h2 class='section-title'>Performance Summary</h2>
                <div class='metric'><strong>Total Faults Assigned:</strong> {model.TotalFaultsAssigned}</div>
                <div class='metric'><strong>Completed Faults:</strong> {model.CompletedFaults}</div>
                <div class='metric'><strong>Completion Rate:</strong> {model.CompletionRate:F1}%</div>
                <div class='metric'><strong>Average Resolution Time:</strong> {model.AverageCompletionTimeHours:F1} hours</div>
                <div class='metric'><strong>Efficiency Score:</strong> {model.EfficiencyScore:F1}/100</div>
                
            </div>

            <div class='section'>
                <h2 class='section-title'>Priority Performance</h2>
                <table>
                    <thead>
                        <tr>
                            <th>Priority</th>
                            <th>Attended</th>
                            <th>Completed</th>
                            <th>Success Rate</th>
                        </tr>
                    </thead>
                    <tbody>
                        {string.Join("", model.PriorityMetrics.Select(p => $@"
                        <tr>
                            <td>{p.Priority}</td>
                            <td>{p.Attended}</td>
                            <td>{p.Completed}</td>
                            <td>{p.SuccessRate:F1}%</td>
                        </tr>"))}
                    </tbody>
                </table>
            </div>

            <div class='section'>
                <h2 class='section-title'>Monthly Trends</h2>
                <table>
                    <thead>
                        <tr>
                            <th>Month</th>
                            <th>Total Faults</th>
                            <th>Completed</th>
                            <th>Completion Rate</th>
                        </tr>
                    </thead>
                    <tbody>
                        {string.Join("", model.MonthlyPerformance.Select(m => $@"
                        <tr>
                            <td>{m.Month}</td>
                            <td>{m.TotalFaults}</td>
                            <td>{m.CompletedFaults}</td>
                            <td>{((double)m.CompletedFaults / m.TotalFaults * 100):F1}%</td>
                        </tr>"))}
                    </tbody>
                </table>
            </div>
        </body>
        </html>";

            return System.Text.Encoding.UTF8.GetBytes(htmlContent);
        }

        // Excel Generation for Performance Report
        // Excel Generation for Performance Report
        private static byte[] GeneratePerformanceExcelReport(TechnicianPerformanceViewModel model)
        {
            try
            {
                // Set EPPlus license context (important for non-commercial use)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage();

                // Summary Worksheet
                var summarySheet = package.Workbook.Worksheets.Add("Performance Summary");

                // Set header
                summarySheet.Cells[1, 1].Value = model.ReportTitle ?? "Performance Report";
                summarySheet.Cells[1, 1].Style.Font.Bold = true;
                summarySheet.Cells[1, 1].Style.Font.Size = 16;

                summarySheet.Cells[2, 1].Value = "Generated on:";
                summarySheet.Cells[2, 2].Value = model.GeneratedOn ?? DateTime.Now.ToString("MMM dd, yyyy");
                summarySheet.Cells[3, 1].Value = "Period:";
                summarySheet.Cells[3, 2].Value = model.ReportPeriod ?? "N/A";

                // Performance Metrics
                int row = 5;
                summarySheet.Cells[row, 1].Value = "Performance Metric";
                summarySheet.Cells[row, 2].Value = "Value";
                summarySheet.Cells[row, 3].Value = "Target";
                summarySheet.Cells[row, 4].Value = "Status";

                var headerRange = summarySheet.Cells[row, 1, row, 4];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                row++;
                AddExcelPerformanceMetrics(summarySheet, row++, "Total Faults Assigned", model.TotalFaultsAssigned.ToString(), "N/A", "N/A");
                AddExcelPerformanceMetrics(summarySheet, row++, "Completed Faults", model.CompletedFaults.ToString(), "N/A", "N/A");
                AddExcelPerformanceMetrics(summarySheet, row++, "Completion Rate", model.CompletionRate, "85", model.CompletionRate >= 85 ? "On Target" : "Needs Improvement");
                AddExcelPerformanceMetrics(summarySheet, row++, "Average Resolution Time", model.AverageCompletionTimeHours, "24", model.AverageCompletionTimeHours <= 24 ? "On Target" : "Needs Improvement");
                AddExcelPerformanceMetrics(summarySheet, row++, "Efficiency Score", model.EfficiencyScore, "80", model.EfficiencyScore >= 80 ? "On Target" : "Needs Improvement");

                // Priority Performance Worksheet - FIXED: Handle null PriorityMetrics
                var prioritySheet = package.Workbook.Worksheets.Add("Priority Performance");
                prioritySheet.Cells[1, 1].Value = "Priority Performance Analysis";
                prioritySheet.Cells[1, 1].Style.Font.Bold = true;
                prioritySheet.Cells[1, 1].Style.Font.Size = 14;

                row = 3;
                prioritySheet.Cells[row, 1].Value = "Priority";
                prioritySheet.Cells[row, 2].Value = "Attended";
                prioritySheet.Cells[row, 3].Value = "Completed";
                prioritySheet.Cells[row, 4].Value = "Success Rate";
                prioritySheet.Cells[row, 5].Value = "Average Time";

                headerRange = prioritySheet.Cells[row, 1, row, 5];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

                row++;

                // FIXED: Safe handling of PriorityMetrics
                var priorityMetrics = model.PriorityMetrics?.Where(p => p != null).ToList() ?? new List<PriorityMetrics>();
                foreach (var priority in priorityMetrics)
                {
                    var priorityName = priority.Priority switch
                    {
                        4 => "Critical",
                        3 => "High",
                        2 => "Medium",
                        1 => "Low",
                        _ => $"Priority {priority.Priority}"
                    };

                    prioritySheet.Cells[row, 1].Value = priorityName;
                    prioritySheet.Cells[row, 2].Value = priority.Attended;
                    prioritySheet.Cells[row, 3].Value = priority.Completed;
                    prioritySheet.Cells[row, 4].Value = priority.SuccessRate / 100; // Convert to decimal for percentage
                    prioritySheet.Cells[row, 5].Value = priority.AvgCompletionTime;
                    row++;
                }

                // Format percentage column if we have data
                if (priorityMetrics.Any())
                {
                    var percentageRange = prioritySheet.Cells[4, 4, row - 1, 4];
                    percentageRange.Style.Numberformat.Format = "0.0%";
                }

                prioritySheet.Cells[3, 1, Math.Max(3, row - 1), 5].AutoFitColumns();

                // Monthly Trends Worksheet - FIXED: Handle null MonthlyPerformance
                var trendsSheet = package.Workbook.Worksheets.Add("Monthly Trends");
                trendsSheet.Cells[1, 1].Value = "Monthly Performance Trends";
                trendsSheet.Cells[1, 1].Style.Font.Bold = true;
                trendsSheet.Cells[1, 1].Style.Font.Size = 14;

                row = 3;
                trendsSheet.Cells[row, 1].Value = "Month";
                trendsSheet.Cells[row, 2].Value = "Total Faults";
                trendsSheet.Cells[row, 3].Value = "Completed Faults";
                trendsSheet.Cells[row, 4].Value = "Completion Rate";
                trendsSheet.Cells[row, 5].Value = "Average Time";

                headerRange = trendsSheet.Cells[row, 1, row, 5];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);

                row++;

                // FIXED: Safe handling of MonthlyPerformance
                var monthlyData = model.MonthlyPerformance?.Where(m => m != null).ToList() ?? new List<MonthlyPerformance>();
                foreach (var month in monthlyData)
                {
                    trendsSheet.Cells[row, 1].Value = month.Month ?? "Unknown";
                    trendsSheet.Cells[row, 2].Value = month.TotalFaults;
                    trendsSheet.Cells[row, 3].Value = month.CompletedFaults;

                    // Safe calculation of completion rate
                    var completionRate = month.TotalFaults > 0 ?
                        (double)month.CompletedFaults / month.TotalFaults : 0;
                    trendsSheet.Cells[row, 4].Value = completionRate;

                    trendsSheet.Cells[row, 5].Value = month.AverageCompletionTime;
                    row++;
                }

                // Format percentage column if we have data
                if (monthlyData.Any())
                {
                    var percentageRange = trendsSheet.Cells[4, 4, row - 1, 4];
                    percentageRange.Style.Numberformat.Format = "0.0%";
                }

                trendsSheet.Cells[3, 1, Math.Max(3, row - 1), 5].AutoFitColumns();

                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Excel Generation Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Return empty byte array or handle appropriately
                return Array.Empty<byte>();
            }
        }
        // PDF Generation for Fault Analysis Report
        private byte[] GenerateFaultAnalysisPdfReport(FaultAnalysisViewModel model)
        {
            try
            {
                // Using QuestPDF for PDF generation
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        // Header
                        page.Header()
                            .AlignCenter()
                            .Text("Comprehensive Fault Analysis Report")
                            .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                        page.Header()
                            .AlignCenter()
                            .Text($"Generated on: {model.GeneratedOn}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);

                        page.Header()
                            .AlignCenter()
                            .Text($"Period: {model.ReportPeriod}")
                            .FontSize(10).FontColor(Colors.Grey.Medium);

                        // Content
                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Spacing(20);

                                // Executive Summary
                                column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(summaryColumn =>
                                {
                                    summaryColumn.Spacing(5);
                                    summaryColumn.Item().Text("Executive Summary").SemiBold().FontSize(16);
                                    summaryColumn.Item().Text($"Total Faults: {model.Summary.TotalFaults}");
                                    summaryColumn.Item().Text($"Resolved Faults: {model.Summary.ResolvedFaults}");
                                    summaryColumn.Item().Text($"Resolution Rate: {model.Summary.ResolutionRate:F1}%");
                                    summaryColumn.Item().Text($"Average Resolution Time: {model.Summary.AvgResolutionTime:F1} hours");
                                    summaryColumn.Item().Text($"Top Performing Technician: {model.Summary.TopPerformingTechnician}");
                                });

                                // Key Metrics
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Metric").FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Value").FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Medium).Padding(5).Text("Status").FontColor(Colors.White);
                                    });

                                    foreach (var kpi in model.Kpis)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(kpi.Name);
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(kpi.CurrentValue.ToString("F1"));
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(kpi.Status);
                                    }
                                });

                            });

                        // Footer
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                                x.Span(" of ");
                                x.TotalPages();
                            });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                // Fallback to simple PDF generation if QuestPDF fails
                return GenerateSimpleFaultAnalysisPdf(model);
            }
        }
        private static void AddExcelPerformanceMetrics(ExcelWorksheet sheet, int row, string metric, object value, string target, string status)
        {
            sheet.Cells[row, 1].Value = metric;
            sheet.Cells[row, 2].Value = value;
            sheet.Cells[row, 3].Value = target;
            sheet.Cells[row, 4].Value = status;

            // Apply conditional formatting for status
            if (status == "On Target")
            {
                sheet.Cells[row, 4].Style.Font.Color.SetColor(System.Drawing.Color.Green);
            }
            else if (status == "Needs Improvement")
            {
                sheet.Cells[row, 4].Style.Font.Color.SetColor(System.Drawing.Color.Red);
            }
            else if (status == "Needs Work")
            {
                sheet.Cells[row, 4].Style.Font.Color.SetColor(System.Drawing.Color.Orange);
            }
        }
        // Simple PDF fallback method
        private byte[] GenerateSimpleFaultAnalysisPdf(FaultAnalysisViewModel model)
        {
            var htmlContent = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; margin: 40px; }}
                .header {{ text-align: center; color: #2c3e50; border-bottom: 2px solid #3498db; padding-bottom: 20px; }}
                .section {{ margin: 30px 0; }}
                .section-title {{ background: #3498db; color: white; padding: 10px; border-radius: 5px; }}
                .metric {{ margin: 10px 0; padding: 10px; background: #f8f9fa; border-radius: 5px; }}
                table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                th, td {{ border: 1px solid #ddd; padding: 12px; text-align: left; }}
                th {{ background-color: #3498db; color: white; }}
                .footer {{ text-align: center; margin-top: 40px; color: #7f8c8d; font-size: 12px; }}
            </style>
        </head>
        <body>
            <div class='header'>
                <h1>Comprehensive Fault Analysis Report</h1>
                <p>Generated on: {model.GeneratedOn}</p>
                <p>Period: {model.ReportPeriod}</p>
            </div>

            <div class='section'>
                <h2 class='section-title'>Executive Summary</h2>
                <div class='metric'><strong>Total Faults:</strong> {model.Summary.TotalFaults}</div>
                <div class='metric'><strong>Resolved Faults:</strong> {model.Summary.ResolvedFaults}</div>
                <div class='metric'><strong>Resolution Rate:</strong> {model.Summary.ResolutionRate:F1}%</div>
                <div class='metric'><strong>Average Resolution Time:</strong> {model.Summary.AvgResolutionTime:F1} hours</div>
                <div class='metric'><strong>Top Performing Technician:</strong> {model.Summary.TopPerformingTechnician}</div>
            </div>

            <div class='section'>
                <h2 class='section-title'>Key Performance Indicators</h2>
                <table>
                    <thead>
                        <tr>
                            <th>Metric</th>
                            <th>Current Value</th>
                            <th>Target</th>
                            <th>Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        {string.Join("", model.Kpis.Select(kpi => $@"
                        <tr>
                            <td>{kpi.Name}</td>
                            <td>{kpi.CurrentValue:F1}</td>
                            <td>{kpi.TargetValue:F1}</td>
                            <td>{kpi.Status}</td>
                        </tr>"))}
                    </tbody>
                </table>
            </div>

            <div class='footer'>
                <p>Report generated by NM Design Hub | Confidential</p>
            </div>
        </body>
        </html>";

            // Convert HTML to PDF using a simple approach
            // In production, you might want to use a proper HTML to PDF converter
            return System.Text.Encoding.UTF8.GetBytes(htmlContent);
        }

        // Excel Generation for Fault Analysis Report
        private byte[] GenerateFaultAnalysisExcelReport(FaultAnalysisViewModel model)
        {
            using var package = new OfficeOpenXml.ExcelPackage();

            // Executive Summary Worksheet
            var summarySheet = package.Workbook.Worksheets.Add("Executive Summary");
            summarySheet.Cells[1, 1].Value = "Comprehensive Fault Analysis Report";
            summarySheet.Cells[1, 1].Style.Font.Bold = true;
            summarySheet.Cells[1, 1].Style.Font.Size = 16;

            summarySheet.Cells[2, 1].Value = "Generated on:";
            summarySheet.Cells[2, 2].Value = model.GeneratedOn;
            summarySheet.Cells[3, 1].Value = "Report Period:";
            summarySheet.Cells[3, 2].Value = model.ReportPeriod;

            // Executive Summary Data
            int row = 5;
            summarySheet.Cells[row, 1].Value = "Metric";
            summarySheet.Cells[row, 2].Value = "Value";
            summarySheet.Cells[row, 1].Style.Font.Bold = true;
            summarySheet.Cells[row, 2].Style.Font.Bold = true;
            row++;

            summarySheet.Cells[row, 1].Value = "Total Faults";
            summarySheet.Cells[row, 2].Value = model.Summary.TotalFaults;
            row++;

            summarySheet.Cells[row, 1].Value = "Resolved Faults";
            summarySheet.Cells[row, 2].Value = model.Summary.ResolvedFaults;
            row++;

            summarySheet.Cells[row, 1].Value = "Resolution Rate";
            summarySheet.Cells[row, 2].Value = model.Summary.ResolutionRate;
            summarySheet.Cells[row, 2].Style.Numberformat.Format = "0.0%";
            row++;

            summarySheet.Cells[row, 1].Value = "Average Resolution Time";
            summarySheet.Cells[row, 2].Value = model.Summary.AvgResolutionTime;
            summarySheet.Cells[row, 2].Style.Numberformat.Format = "0.0\" hours\"";
            row++;

            summarySheet.Cells[row, 1].Value = "Top Performing Technician";
            summarySheet.Cells[row, 2].Value = model.Summary.TopPerformingTechnician;
            row++;

            // KPI Worksheet
            var kpiSheet = package.Workbook.Worksheets.Add("KPI Metrics");
            kpiSheet.Cells[1, 1].Value = "KPI Metrics";
            kpiSheet.Cells[1, 1].Style.Font.Bold = true;
            kpiSheet.Cells[1, 1].Style.Font.Size = 14;

            row = 3;
            kpiSheet.Cells[row, 1].Value = "Metric";
            kpiSheet.Cells[row, 2].Value = "Current Value";
            kpiSheet.Cells[row, 3].Value = "Target";
            kpiSheet.Cells[row, 4].Value = "Previous Value";
            kpiSheet.Cells[row, 5].Value = "Status";
            kpiSheet.Cells[row, 6].Value = "Variance";

            var headerRange = kpiSheet.Cells[row, 1, row, 6];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

            row++;
            foreach (var kpi in model.Kpis)
            {
                kpiSheet.Cells[row, 1].Value = kpi.Name;
                kpiSheet.Cells[row, 2].Value = kpi.CurrentValue;
                kpiSheet.Cells[row, 3].Value = kpi.TargetValue;
                kpiSheet.Cells[row, 4].Value = kpi.PreviousValue;
                kpiSheet.Cells[row, 5].Value = kpi.Status;
                kpiSheet.Cells[row, 6].Value = kpi.Variance;
                row++;
            }

            kpiSheet.Cells[3, 1, row - 1, 6].AutoFitColumns();

            // Trends Worksheet
            var trendsSheet = package.Workbook.Worksheets.Add("Trends");
            trendsSheet.Cells[1, 1].Value = "Fault Trends";
            trendsSheet.Cells[1, 1].Style.Font.Bold = true;
            trendsSheet.Cells[1, 1].Style.Font.Size = 14;

            row = 3;
            trendsSheet.Cells[row, 1].Value = "Period";
            trendsSheet.Cells[row, 2].Value = "Reported Faults";
            trendsSheet.Cells[row, 3].Value = "Resolved Faults";
            trendsSheet.Cells[row, 4].Value = "Critical Faults";
            trendsSheet.Cells[row, 5].Value = "Resolution Rate";
            trendsSheet.Cells[row, 6].Value = "Avg. Resolution Time";

            headerRange = trendsSheet.Cells[row, 1, row, 6];
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);

            row++;
            foreach (var trend in model.Trends)
            {
                trendsSheet.Cells[row, 1].Value = trend.Period;
                trendsSheet.Cells[row, 2].Value = trend.ReportedFaults;
                trendsSheet.Cells[row, 3].Value = trend.ResolvedFaults;
                trendsSheet.Cells[row, 4].Value = trend.CriticalFaults;
                trendsSheet.Cells[row, 5].Value = trend.ResolutionRate / 100; // Convert to decimal for percentage format
                trendsSheet.Cells[row, 6].Value = trend.AvgResolutionTime;
                row++;
            }

            // Format percentage column
            var percentageRange = trendsSheet.Cells[4, 5, row - 1, 5];
            percentageRange.Style.Numberformat.Format = "0.0%";

            trendsSheet.Cells[3, 1, row - 1, 6].AutoFitColumns();

            return package.GetAsByteArray();
        }

    }
}
