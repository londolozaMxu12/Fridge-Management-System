using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class RepairScheduleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Index(string selectedDate)
        {
            DateTime dt = Convert.ToDateTime(selectedDate);
            ViewBag.Message = "Selected Date: " + dt.ToShortDateString();
            return View();
        }
    }
}
