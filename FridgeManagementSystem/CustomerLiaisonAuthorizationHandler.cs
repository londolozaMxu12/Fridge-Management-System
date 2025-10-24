using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;

namespace FridgeManagementSystem.Authorization
{
    public class CustomerLiaisonAuthorizationHandler : AuthorizationHandler<IAuthorizationRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FridgeManagementSystemContext _dbContext;

        public CustomerLiaisonAuthorizationHandler(
            UserManager<ApplicationUser> userManager,
            FridgeManagementSystemContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IAuthorizationRequirement requirement)
        {
            // Admin always has access
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return;
            }

            // Check if user is an Employee
            if (context.User.IsInRole("Employee"))
            {
                var user = await _userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    // Load the user with EmployeeType from database
                    var userWithDetails = await _dbContext.Users
                        .Include(u => u.Employees)
                        .ThenInclude(e => e.EmployeeType)
                        .FirstOrDefaultAsync(u => u.Id == user.Id);

                    if (userWithDetails?.Employees?.EmployeeType?.Name == "CustomerLiaison")
                    {
                        context.Succeed(requirement);
                    }
                }
            }
        }
    }
}
