using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Controllers
{
    public class AllocationController : Controller
    {
        private readonly FridgeManagementSystemContext _db;
        public AllocationController(FridgeManagementSystemContext db) { _db = db; }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _db.Customers.ToListAsync();
            ViewBag.Fridges = await _db.Fridges.Where(f => f.Status == FridgeStatus.Available).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int customerId, int fridgeId)
        {
            var fridge = await _db.Fridges.FindAsync(fridgeId);
            if (fridge == null || fridge.Status != FridgeStatus.Available) return BadRequest("Fridge not available");

            var allocation = new Allocation { CustomerId = customerId, FridgeId = fridgeId, StartDate = DateTime.UtcNow, Status = AllocationStatus.Active };
            fridge.Status = FridgeStatus.Allocated;

            _db.Allocations.Add(allocation);
            _db.Fridges.Update(fridge);
            await _db.SaveChangesAsync();
            return RedirectToAction("Details", "Customers", new { id = customerId });
        }

        [HttpPost]
        public async Task<IActionResult> Return(int allocationId)
        {
            var allocation = await _db.Allocations.Include(a => a.Fridge).FirstOrDefaultAsync(a => a.AllocationId == allocationId);
            if (allocation == null) return NotFound();

            allocation.EndDate = DateTime.UtcNow;
            allocation.Status = AllocationStatus.Returned;
            allocation.Fridge.Status = FridgeStatus.Available;

            _db.Update(allocation);
            _db.Update(allocation.Fridge);
            await _db.SaveChangesAsync();

            return RedirectToAction("Details", "Customers", new { id = allocation.CustomerId });
        }
        
    }
}
