using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class RepairScheduleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
