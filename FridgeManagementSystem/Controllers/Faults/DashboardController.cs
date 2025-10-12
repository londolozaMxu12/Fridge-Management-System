using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers.F.Technician
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
