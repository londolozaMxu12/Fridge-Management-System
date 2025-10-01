//using FridgeManagementSystem.Data;
//using FridgeManagementSystem.Models;
//using FridgeManagementSystem.ViewModels;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using System.Drawing.Drawing2D;
//using System.Security.Claims;

//namespace FridgeManagementSystem.Controllers
//{
//    public class FridgeController : Controller
//    {
//        private readonly FridgeManagementSystemContext _context;
//        private readonly IWebHostEnvironment _environment;

//        public FridgeController(FridgeManagementSystemContext context, IWebHostEnvironment environment)
//        {
//            _context = context;
//           _environment = environment;
//        }
//        public async Task<IActionResult> Index()
//        {
//            var fridges= _context.Fridges
//                .Include(f => f.Customer)
//                .ThenInclude(c => c.User)
//                .Include(f => f.Supplier)
//                //.Include(f => f.CreatedBy)
//                .Where(f => f.IsActive)
//                .OrderBy(f => f.Status)
//                .ThenBy(f => f.SerialNumber)
//                .ToListAsync();

//            return View(fridges);
//        }

//        // GET: Fridges/Details/5
//        public async Task<IActionResult> Details(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var fridge = await _context.Fridges
                
//                .Include(f => f.Customer)
//                .ThenInclude(c => c.User)
//                .Include(f => f.Supplier)
//                //.Include(f => f.CreatedBy)
//                .FirstOrDefaultAsync(m => m.FridgeId == id && m.IsActive);

//            if (fridge == null)
//            {
//                return NotFound();
//            }

//            Load service history
//            ViewBag.ServiceRecords = await _context.ServiceRecords
//                .Include(sr => sr.ServiceTechnician)
//                .Include(sr => sr.ServiceCheckResults)
//                .ThenInclude(scr => scr.CheckType)
//                .Where(sr => sr.FridgeId == id)
//                .OrderByDescending(sr => sr.ServiceDate)
//                .ToListAsync();

//            return View(fridge);
//        }
//        // GET: Fridges/Create
//        public async Task<IActionResult> Create()
//        {
//            await LoadViewData();
//            return View();
//        }
//        // POST: Fridges/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("SerialNumber,Model,Name, Brand, Price, Description, ImageFile, PurchaseDate,Status,SupplierId")] Fridge fridge)
//        {
//            if (ModelState.IsValid)
//            {
                
//                fridge.CreatedAt = DateTime.UtcNow;
//                fridge.IsActive = true;

//                _context.Add(fridge);
//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Fridge created successfully.";
//                return RedirectToAction(nameof(Index));
//            }

//            await LoadViewData();
//            return View(fridge);
//        }
//        // GET: Fridges/Edit/5
//        public async Task<IActionResult> Edit(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var fridge = await _context.Fridges.FindAsync(id);
//            if (fridge == null || !fridge.IsActive)
//            {
//                return NotFound();
//            }

//            await LoadViewData();
//            return View(fridge);
//        }
//        // POST: Fridges/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, [Bind("FridgeId,SerialNumber, Model, Name, Brand, Price, Description, ImageFile, PurchaseDate, Status, SupplierId,LastServiceDate,NextServiceDate, IsActive")] Fridge fridge)
//        {
//            if (id != fridge.FridgeId)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {

//                try
//                {
//                    _context.Update(fridge);
//                    await _context.SaveChangesAsync();

//                    TempData["SuccessMessage"] = "Fridge updated successfully.";
//                }
//                catch (DbUpdateConcurrencyException)
//                {

//                    if (!FridgeExists(fridge.FridgeId))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }

//            await LoadViewData();
//            return View(fridge);
//        }

//        // GET: Fridges/Delete/5
//        public async Task<IActionResult> Delete(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var fridge = await _context.Fridges
                
//                .Include(f => f.Customer)
//                .ThenInclude(c => c.User)
//                .Include(f => f.Supplier)
//                .FirstOrDefaultAsync(m => m.FridgeId == id && m.IsActive);

//            if (fridge == null)
//            {
//                return NotFound();
//            }

//            return View(fridge);
//        }
//        // POST: Fridges/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var fridge = await _context.Fridges.FindAsync(id);
//            if (fridge != null)
//            {
//                fridge.IsActive = false;
//                _context.Update(fridge);
//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Fridge deleted successfully.";
//            }
//            return RedirectToAction(nameof(Index));
//        }

//        private bool FridgeExists(int id)
//        {
//            return _context.Fridges.Any(e => e.FridgeId == id && e.IsActive);
//        }

//        private async Task LoadViewData()
//        {
            

//            ViewData["SupplierId"] = await _context.Suppliers
//                .Where(s => s.User.IsActive)
//                .Select(s => new SelectListItem
//                {
//                    Value = s.Id.ToString(),
//                    Text = s.CompanyName
//                })
//                .ToListAsync();

//            ViewData["CustomerId"] = await _context.Customers
//                .Where(c => c.User.IsActive && c.User.ApprovalStatus == "Approved")
//                .Select(c => new SelectListItem
//                {
//                    Value = c.Id.ToString(),
//                    Text = c.BusinessName
//                })
//                .ToListAsync();
//        }
//    }
//}
