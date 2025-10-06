using FridgeManagementSystem.Data;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Controllers
{
    
    public class FridgesController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly IWebHostEnvironment _environment;

        public FridgesController(FridgeManagementSystemContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
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
