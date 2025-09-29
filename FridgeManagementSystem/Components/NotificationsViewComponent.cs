
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FridgeManagementSystem.Data;
using Microsoft.AspNetCore.Identity;
using FridgeManagementSystem.Areas.Identity.Data;
using System.Security.Claims;

namespace FridgeManagementSystem.Components
{
    public class NotificationsViewComponent : ViewComponent
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsViewComponent(FridgeManagementSystemContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync((ClaimsPrincipal)User);
                if (user != null)
                {
                    var unreadCount = await _context.Notifications
                        .Where(n => n.UserId == user.Id && !n.IsRead)
                        .CountAsync();

                    return View(unreadCount);
                }
            }
            return View(0);
        }
    }
}
