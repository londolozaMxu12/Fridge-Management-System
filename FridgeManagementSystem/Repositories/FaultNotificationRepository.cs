using FridgeManagementSystem.Areas.Identity.Data;

namespace FridgeManagementSystem.Repositories
{
    public class FaultNotificationRepository: IFaultNotificationRepository
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FaultNotificationRepository(FridgeManagementSystemContext context, UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper method to generate absolute URLs
        private string GenerateAbsoluteUrl(string relativePath)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return relativePath;

            // Build absolute URL
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return $"{baseUrl}{relativePath}";
        }

        public async Task NotifyFaultAttendedAsync(Fault fault, Employee attendingTechnician)
        {
            // Get all active Fault Technicians (excluding the one who attended)
            var otherTechnicians = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.EmployeeType)
                .Where(e => e.EmployeeType.Name == "FaultTechnician" &&
                           e.IsActive &&
                           e.Id != attendingTechnician.Id)
                .ToListAsync();

            // Notify other technicians that the fault has been attended
            foreach (var technician in otherTechnicians)
            {
                var notification = new Notification
                {
                    UserId = technician.UserId,
                    Title = "Fault Already Attended",
                    Message = $"Fault '{fault.Title}' has been attended by {attendingTechnician.User.FullName}. Look for new unattended faults",
                    Link = GenerateAbsoluteUrl($"/FaultTechnician/Index"),
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);
            }

            // Notify customer that their fault is being attended
            var customerNotification = new Notification
            {
                UserId = fault.ReportedBy.Id,
                Title = "Fault Being Attended",
                Message = $"Your fault '{fault.Title}' is being attended by technician {attendingTechnician.User.FullName}",
                Link = GenerateAbsoluteUrl($"/CustomerFault/Details/{fault.FaultId}"),
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(customerNotification);
            await _context.SaveChangesAsync();
        }

        public async Task NotifyFaultReportedAsync(Fault fault)
        {
            // Get all active Fault Technicians
            var faultTechnicians = await _context.Employees
                .Include(e => e.User)
                .Include(e => e.EmployeeType)
                .Where(e => e.EmployeeType.Name == "FaultTechnician" && e.IsActive)
                .ToListAsync();

            // Get Admins as well
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

            // Combine recipients
            var technicianUsers = faultTechnicians.Select(ft => ft.User).ToList();
            var allRecipients = technicianUsers.Union(adminUsers).Distinct();

            foreach (var user in allRecipients)
            {
                var notification = new Notification
                {
                    UserId = user.Id,
                    Title = "New Fault Reported",
                    Message = $"Customer {fault.ReportedBy.User.FullName} reported a fault: {fault.Title}. Priority: {fault.Priority}",
                    Link = GenerateAbsoluteUrl($"/FaultTechnician/Details/{fault.FaultId}"),
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();
        }

        public async Task NotifyFaultStatusUpdateAsync(Fault fault, string oldStatus)
        {
            var customerNotification = new Notification
            {
                UserId = fault.ReportedBy.Id,
                Title = "Fault Status Updated",
                Message = $"Your fault '{fault.Title}' status changed from {oldStatus} to {fault.Status}",
                Link = GenerateAbsoluteUrl($"/CustomerFault/Details/{fault.FaultId}"),
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(customerNotification);
            await _context.SaveChangesAsync();
        }

        //public async Task NotifyRepairScheduledAsync(RepairSchedule schedule)
        //{
        //    var customerNotification = new Notification
        //    {
        //        UserId = schedule.Fault.ReportedBy.Id,
        //        Title = "Repair Scheduled",
        //        Message = $"Repair for your fault '{schedule.Fault.Title}' has been scheduled for {schedule.ScheduledDate:yyyy-MM-dd HH:mm}, make sure you available at this date and time. " +
        //        $"" +
        //        $"Thank you, have a good day! ",
        //        Link = GenerateAbsoluteUrl($"/CustomerFault/Details/{schedule.FaultId}"),
        //        CreatedAt = DateTime.Now
        //    };

        //    _context.Notifications.Add(customerNotification);
        //    await _context.SaveChangesAsync();
        //}

        public async Task NotifyRepairScheduledAsync(RepairSchedule schedule)
        {
            // Check if the schedule status requires notification
            if (schedule.Status == ScheduleStatus.Scheduled)
            {
                var customerNotification = new Notification
                {
                    UserId = schedule.Fault.ReportedBy.Id,
                    Title = "Repair Scheduled",
                    Message = $"Repair for your fault '{schedule.Fault.Title}' has been scheduled for {schedule.ScheduledDate: dd-MM-yyyy HH:mm}, make sure you available at this date and time. " +
                    $"" +
                    $"Thank you, have a good day! ",
                    Link = GenerateAbsoluteUrl($"/CustomerFault/Details/{schedule.FaultId}"),
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(customerNotification);
                await _context.SaveChangesAsync();
            }
            else if (schedule.Status == ScheduleStatus.Rescheduled)
            {
                var customerNotification = new Notification
                {
                    UserId = schedule.Fault.ReportedBy.Id,
                    Title = "Repair Rescheduled",
                    Message = $"Repair for your fault '{schedule.Fault.Title}' has been rescheduled for {schedule.ScheduledDate: dd-MM-yyyy HH:mm}, make sure you available at this date and time. " +
                    $"" +
                    $"Thank you, have a good day! ",
                    Link = GenerateAbsoluteUrl($"/CustomerFault/Details/{schedule.FaultId}"),
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(customerNotification);
                await _context.SaveChangesAsync();
            }
            else if(schedule.Status == ScheduleStatus.Cancelled) 
            {
                var customerNotification = new Notification
                {
                    UserId = schedule.Fault.ReportedBy.Id,
                    Title = "Repair Scheduled Cancelled",
                    Message = $"Repair for your fault '{schedule.Fault.Title}' has been Cancelled, technician is not available",
                    Link = GenerateAbsoluteUrl($"/CustomerFault/Details/{schedule.FaultId}"),
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(customerNotification);
                await _context.SaveChangesAsync();
            }


            
        }

        public async Task NotifyFaultUnassignedAsync(Fault fault, Employee technician)
        {
            try
            {
                // Notify other technicians that the fault is now available
                var otherTechnicians = await _context.Employees
                    .Include(e => e.User)
                    .Where(e => e.EmployeeType.Name == "FaultTechnician" &&
                               e.Id != technician.Id &&
                               e.IsActive)
                    .ToListAsync();

                foreach (var tech in otherTechnicians)
                {
                    var notification = new Notification
                    {
                        UserId = tech.UserId,
                        Title = "Fault Available",
                        Message = $"Fault '{fault.Title}' has been unassigned and is now un attended.",
                        IsRead = false,
                        CreatedAt = DateTime.Now,
                        Link = GenerateAbsoluteUrl($"/FaultTechnician/Details/{fault.FaultId}") // Link to fault details
                    };
                    _context.Notifications.Add(notification);
                }

                // Notify the customer that the repair has been cancelled
                if (fault.ReportedBy?.Id != null)
                {
                    var customerNotification = new Notification
                    {
                        UserId = fault.ReportedBy.Id,
                        Title = "Repair Schedule Cancelled",
                        Message = $"The repair schedule for your fault '{fault.Title}' has been cancelled. The fault will be attended by other technician.",
                        IsRead = false,
                        CreatedAt = DateTime.Now,
                        Link = GenerateAbsoluteUrl($"/CustomerFault/Details/{fault.FaultId}")// Link to customer fault details
                    };
                    _context.Notifications.Add(customerNotification);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
