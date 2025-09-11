using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class Fridges : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        public IActionResult Fridge()
        {
            return View();
        }
        public IActionResult Cart()
        {
            return View();
        }
    }
}
