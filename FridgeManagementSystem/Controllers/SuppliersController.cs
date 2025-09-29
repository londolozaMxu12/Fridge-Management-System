// Controllers/SuppliersController.cs
using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Areas.Identity.Pages.Account;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,PurchasingManager")]
    public class SuppliersController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterSupplierModel> _logger;

        public SuppliersController(FridgeManagementSystemContext context,
            UserManager<ApplicationUser> userManager, ILogger<RegisterSupplierModel> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
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
        public async Task<IActionResult> Edit(int id)
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

            var viewModel = new SupplierEditViewModel
            {
                Id = supplier.Id,
                Email = supplier.User.Email,
                FullName = supplier.User.FullName,
                ContactNo = supplier.User.ContactNo,
                Address = supplier.User.Address,
                City = supplier.User.City,
                Suburb = supplier.User.Suburb,
                PostalCode = supplier.User.PostalCode,
                CompanyName = supplier.CompanyName,
                SupplierType = supplier.SupplierType,
                CreatedAt = supplier.CreatedAt,
                IsActive = supplier.User.IsActive,
            };

            ViewBag.SupplierTypes = await GetSupplierTypes(); // Populate dropdown

            ViewData["SupplierId"] = supplier.Id;
            ViewData["CreatedAt"] = supplier.CreatedAt.ToString("MM/dd/yyyy");
            ViewData["IsActiveStatus"] = (supplier.User.IsActive) ? "Active" : "Inactive";

            return View(viewModel);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingSupplier = await _context.Suppliers
                        .Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (existingSupplier == null)
                    {
                        return NotFound();
                    }

                    // Update supplier properties
                    existingSupplier.CompanyName = model.CompanyName;
                    existingSupplier.SupplierType = model.SupplierType;
                    

                    // Update user properties
                    existingSupplier.User.FullName = model.FullName;
                    existingSupplier.User.ContactNo = model.ContactNo;
                    existingSupplier.User.Address = model.Address;
                    existingSupplier.User.City = model.City;
                    existingSupplier.User.Suburb = model.Suburb;
                    existingSupplier.User.PostalCode = model.PostalCode;
                    

                    // Only update email if it's changed
                    if (existingSupplier.User.Email != model.Email)
                    {
                        existingSupplier.User.Email = model.Email;
                        existingSupplier.User.NormalizedEmail = model.Email.ToUpper();
                        existingSupplier.User.UserName = model.Email;
                        existingSupplier.User.NormalizedUserName = model.Email.ToUpper();
                    }

                    _context.Update(existingSupplier);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Supplier updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.SupplierTypes = await GetSupplierTypes(); // Re-populate dropdown on error
            return View(model);
        }

        // GET: Suppliers/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Supplier ID was not provided.";
                return RedirectToAction(nameof(Index));
            }

            var supplier = await _context.Suppliers
                .Include(s => s.User)
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id && m.User.IsActive);

            if (supplier == null)
            {
                TempData["ErrorMessage"] = "Supplier not found or has already been deleted.";
                return RedirectToAction(nameof(Index));
            }

            //// Check if supplier has fridges
            //var hasFridges = await _context.Fridges.AnyAsync(f => f.SupplierId == id && f.IsActive);
            //if (hasFridges)
            //{
            //    TempData["ErrorMessage"] = "Cannot delete this supplier because they have supplied fridges.";
            //    return RedirectToAction(nameof(Index));
            //}

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers
                 .Include(s => s.User)
                 .FirstOrDefaultAsync(s => s.Id == id && s.User.IsActive);

            if (supplier != null)
            {
                // Soft delete by deactivating the user
                supplier.User.IsActive = false;
                await _userManager.UpdateAsync(supplier.User);

                TempData["SuccessMessage"] = $"Supplier '{supplier.CompanyName}' has been successfully deleted.";

            }

            return RedirectToAction(nameof(Index));
            //var hasActiveFridges = await _context.Fridges
            //.AnyAsync(f => f.SupplierId == id && f.IsActive);

            //if (hasActiveFridges)
            //{
            //    TempData["ErrorMessage"] = "Cannot delete this supplier because they have supplied active fridges. Please reassign or deactivate the fridges first.";
            //    return RedirectToAction(nameof(Index));
            //}

        }

        private async Task<List<SelectListItem>> GetSupplierTypes()
        {
            // Return your supplier types - adjust based on your data source
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Fridge", Text = "Fridge Supplier" },

                new SelectListItem { Value = "Parts", Text = "Parts Supplier" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };
              
        }

        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.Id == id && e.User.IsActive);
        }
    }
}