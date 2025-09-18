using FridgeManagementSystem.Models;
using FridgeManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace FridgeManagementSystem.Controllers
{
    public class InventoryController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public InventoryController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;

        }
        public async Task<IActionResult> ListInventoryLiaison()
        {

            //var user = userManager.Users.ToList();
            var inventoryLiaison = await userManager.GetUsersInRoleAsync(" Inventory Liaison");

            return View(inventoryLiaison);
        }
    }


}

        //    private readonly FridgeManagementSystemContext _db;
        //    private const int ReorderThreshold = 5; // example threshold

        //    public InventoryController(FridgeManagementSystemContext db) { _db = db; }

        //    public async Task<IActionResult> Index() => View(await _db.Fridges.Include(f => f.Supplier).ToListAsync());

        //    public IActionResult Create() => View();

        //    [HttpPost]
        //    public async Task<IActionResult> Create(Fridge fridge)
        //    {
        //        if (!ModelState.IsValid) return View(fridge);
        //        fridge.Status = FridgeStatus.Available;
        //        fridge.PurchaseDate = DateTime.UtcNow;
        //        _db.Add(fridge);
        //        await _db.SaveChangesAsync();
        //        await CheckReorderAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    [HttpPost]
        //    public async Task<IActionResult> Scrap(int id, string reason)
        //    {
        //        var fridge = await _db.Fridges.FindAsync(id);
        //        if (fridge == null) return NotFound();
        //        fridge.Status = FridgeStatus.Scrapped;
        //        fridge.ScrapDate = DateTime.UtcNow;
        //        _db.Update(fridge);
        //        await _db.SaveChangesAsync();
        //        await CheckReorderAsync();
        //        return RedirectToAction(nameof(Index));
        //    }

        //    private async Task CheckReorderAsync()
        //    {
        //        var availableCount = await _db.Fridges.CountAsync(f => f.Status == FridgeStatus.Available);
        //        if (availableCount <= ReorderThreshold)
        //        {
        //            var req = new PurchaseRequest { DateRequested = DateTime.UtcNow, Quantity = ReorderThreshold * 2, Status = RequestStatus.Pending, Notes = "Auto-generated low stock" };
        //            _db.PurchaseRequests.Add(req);
        //            await _db.SaveChangesAsync();
        //        }
        //    }
        //    //public IActionResult Index()
        //    //{
        //    //    return View();
        //    //}
        //}
    
