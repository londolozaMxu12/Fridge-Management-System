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
            var availableFridges = await _context.Fridges
                .Include(f => f.FridgeType)
                .Where(f => f.IsAvailable && f.IsActive && f.Status == "Available")
                .Select(f => new FridgeViewModel
                {
                    FridgeId = f.FridgeId,
                    Price = f.Price,
                    Description = f.Description,
                    ImageFile = f.ImageFile,
                    SerialNumber = f.SerialNumber,
                    Status = f.Status,
                    FridgeTypeName = f.FridgeType.Name,
                    Brand = f.FridgeType.Brand,
                    Model = f.FridgeType.Model,
                    IsAvailable = f.IsAvailable
                })
                .ToListAsync();

            return View(availableFridges);
        }

        public async Task<IActionResult> FridgeDetails(int id)
        {
            var fridge = await _context.Fridges
                .Include(f => f.FridgeType)
                .Where(f => f.IsAvailable && f.IsActive)
                .Select(f => new FridgeViewModel
                {
                    FridgeId = f.FridgeId,
                    Price = f.Price,
                    Description = f.Description,
                    ImageFile = f.ImageFile,
                    SerialNumber = f.SerialNumber,
                    Status = f.Status,
                    FridgeTypeName = f.FridgeType.Name,
                    Brand = f.FridgeType.Brand,
                    Model = f.FridgeType.Model,
                    IsAvailable = f.IsAvailable
                })
                .FirstOrDefaultAsync(f => f.FridgeId == id);

            if (fridge == null)
            {
                return NotFound();
            }

            return View(fridge);
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
