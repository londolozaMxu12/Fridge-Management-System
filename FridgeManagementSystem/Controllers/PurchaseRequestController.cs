using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Controllers
{
    public class PurchaseRequestController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        private readonly FridgeManagementSystemContext _db;
        public PurchaseRequestController(FridgeManagementSystemContext db) { _db = db; }

        public async Task<IActionResult> Index() => View(await _db.PurchaseRequests.ToListAsync());

        public async Task<IActionResult> Approve(int id)
        {
            var req = await _db.PurchaseRequests.FindAsync(id);
            if (req == null) return NotFound();
            req.Status = RequestStatus.Approved;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Reject(int id)
        {
            var req = await _db.PurchaseRequests.FindAsync(id);
            if (req == null) return NotFound();
            req.Status = RequestStatus.Rejected;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
