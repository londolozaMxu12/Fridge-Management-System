using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Controllers
{
    public class InventoryLiaisonController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FridgeManagementSystemContext _context;
        private readonly ILogger<CustomerManagementController> _logger;

        public InventoryLiaisonController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager, FridgeManagementSystemContext context, ILogger<CustomerManagementController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
