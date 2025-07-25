using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class Fridges : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
