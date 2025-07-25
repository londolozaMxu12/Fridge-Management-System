using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class MaintenanceManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
