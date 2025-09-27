// Controllers/SuppliersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace FridgeManagementSystem.Controllers
{
    //[Authorize(Roles = "Administrator,PurchasingManager")]
    public class SuppliersController : Controller
    {
        private readonly FridgeManagementSystemContext _context;

        public SuppliersController(FridgeManagementSystemContext context)
        {
            _context = context;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.User)
                .Include(s => s.CreatedBy)
                .Where(s => s.User.IsActive)
                .OrderBy(s => s.CompanyName)
                .ToListAsync();

            return View(suppliers);
        }

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .Include(s => s.User)
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id && m.User.IsActive);

            if (supplier == null)
            {
                return NotFound();
            }

            // Load supplied fridges
            //ViewBag.SuppliedFridges = await _context.Fridges
            //    .Where(f => f.SupplierId == id && f.IsActive)
            //    .Include(f => f.Customer)
            //    .ThenInclude(c => c.User)
            //    .ToListAsync();

            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.User.IsActive);

            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,SupplierType")] Supplier supplier)
        {
            if (id != supplier.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingSupplier = await _context.Suppliers.FindAsync(id);
                    if (existingSupplier == null)
                    {
                        return NotFound();
                    }

                    existingSupplier.CompanyName = supplier.CompanyName;
                    
                    existingSupplier.SupplierType = supplier.SupplierType;
                    

                    _context.Update(existingSupplier);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Supplier updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(supplier.Id))
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
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .Include(s => s.User)
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id && m.User.IsActive);

            if (supplier == null)
            {
                return NotFound();
            }

            // Check if supplier has fridges
            var hasFridges = await _context.Fridges.AnyAsync(f => f.SupplierId == id && f.IsActive);
            if (hasFridges)
            {
                TempData["ErrorMessage"] = "Cannot delete this supplier because they have supplied fridges.";
                return RedirectToAction(nameof(Index));
            }

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier != null)
            {
                // Soft delete by deactivating the user
                supplier.User.IsActive = false;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Supplier deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.Id == id && e.User.IsActive);
        }
    }
}