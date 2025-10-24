using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;

namespace FridgeManagementSystem.Controllers
{
    
    [Authorize(Roles = "Customer")]
    public class CustomerStoreController : Controller
    {
        private readonly FridgeManagementSystemContext _context;

        public CustomerStoreController(FridgeManagementSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var availableFridges = await _context.Fridges
                .Where(f => f.IsActive && f.Status == "Available")
                .Include(f => f.FridgeType)
                .OrderByDescending(f => f.CreatedAt)
                .Take(12)
                .ToListAsync();

            return View(availableFridges);
        }

        public async Task<IActionResult> Details(int id)
        {
            var fridge = await _context.Fridges
                .Include(f => f.FridgeType)
                .Include(f => f.Supplier)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(f => f.FridgeId == id && f.IsActive && f.Status == "Available");

            if (fridge == null)
            {
                return NotFound();
            }

            return View(fridge);
        }
    }
}