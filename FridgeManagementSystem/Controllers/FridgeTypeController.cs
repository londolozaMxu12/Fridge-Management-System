using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FridgeTypeController : Controller
    {
        
        private readonly FridgeManagementSystemContext _context;

        public FridgeTypeController(FridgeManagementSystemContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
     
        }
        public async Task<IActionResult> Index()
        {
            try
            {
                var fridgeTypes = await _context.FridgeType
                    .Where(ft => ft.IsActive)
                    .OrderBy(ft => ft.Brand)
                    .ThenBy(ft => ft.Name)
                    .ToListAsync();

                return View(fridgeTypes);
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["Error"] = "An error occurred while loading fridge types.";
                return View(new List<FridgeType>());
            }

            //var fridgeTypes = await _context.FridgeType
            //    .Where(ft => ft.IsActive)
            //    .OrderBy(ft => ft.Brand)
            //    .ThenBy(ft => ft.Name)
            //    .ToListAsync();
            //return View(fridgeTypes);
        }

        public IActionResult AddFridgeType()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFridgeType([Bind("Name,Brand,Model,IsActive")] FridgeType fridgeType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    fridgeType.IsActive = true;

                    _context.Add(fridgeType);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Fridge type added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                }
            }

            return View(fridgeType);
        }

        public async Task<IActionResult> EditFridgeType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fridgeType = await _context.FridgeType.FindAsync(id);
            if (fridgeType == null)
            {
                return NotFound();
            }
            return View(fridgeType);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFridgeType(int id, FridgeType fridgeType)
        {
            if (id != fridgeType.FridgeTypeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fridgeType);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Fridge type updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FridgeTypeExists(fridgeType.FridgeTypeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(fridgeType);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFridgeType(int id)
        {
            var fridgeType = await _context.FridgeType.FindAsync(id);
            if (fridgeType != null)
            {
                // Check if fridge type is in use
                var isInUse = await _context.Fridges.AnyAsync(f => f.FridgeTypeId == id && f.IsActive);
                if (isInUse)
                {
                    TempData["Error"] = "Cannot delete this fridge type. It is currently assigned to a fridge.";
                    return RedirectToAction(nameof(Index));
                }

                // Soft delete
                fridgeType.IsActive = false;
                _context.Update(fridgeType);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Fridge type deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FridgeTypeExists(int id)
        {
            return _context.FridgeType.Any(e => e.FridgeTypeId == id);
        }
    }
}
