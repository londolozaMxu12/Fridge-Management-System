using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using FridgeManagementSystem.Areas.Identity.Data;
using System.Security.Claims;

namespace FridgeManagementSystem.Helpers
{

    public class CustomerLiaisonAuthorizationAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userManager = context.HttpContext.RequestServices.GetService<UserManager<ApplicationUser>>();
            var currentUser = await userManager.GetUserAsync(context.HttpContext.User);

            if (currentUser == null)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // Check if user is Admin
            var isAdmin = context.HttpContext.User.IsInRole("Admin");

            // Check if user is Employee with CustomerLiaison type
            var isCustomerLiaison = currentUser.Employees?.EmployeeType?.Name == "CustomerLiaison";

            if (!isAdmin && !isCustomerLiaison)
            {
                context.Result = new ForbidResult();
            }
        }
    }

}
