using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
