using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class CustomerManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
