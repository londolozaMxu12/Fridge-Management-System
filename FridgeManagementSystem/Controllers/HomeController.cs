using System.Diagnostics;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace FridgeManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FridgeManagementSystemContext _context;

        public HomeController(ILogger<HomeController> logger, FridgeManagementSystemContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var newestFridges = await _context.Fridges
                .Where(f => f.IsActive && f.Status == "Available")
                .Include(f => f.FridgeType)
                .OrderByDescending(f => f.CreatedAt)
                .Take(4)
                .ToListAsync();

            ViewData["Title"] = "Home Page";
            ViewData["HomePage"] = true;

            return View(newestFridges);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult AboutUs()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
