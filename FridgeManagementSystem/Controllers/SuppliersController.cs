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
    //[Authorize(Roles = "Admin,PurchasingManager")]
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

            //// Add procurement metrics for Purchasing Manager view
            //if (User.IsInRole("PurchasingManager"))
            //{
            //    ViewBag.ProcurementMetrics = await GetProcurementMetrics();
            //}

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
                .Include(s => s.User.ApprovedBy)
                .FirstOrDefaultAsync(m => m.Id == id && m.User.IsActive);

            if (supplier == null)
            {
                return NotFound();
            }

            //// Add procurement data for Purchasing Manager
            //if (User.IsInRole("PurchasingManager"))
            //{
            //    ViewBag.ProcurementStats = await GetSupplierProcurementStats(id.Value);
            //}

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
                ApprovalStatus = supplier.User.ApprovalStatus
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

                    // Only Admin can update status fields
                    if (User.IsInRole("Admin"))
                    {
                        existingSupplier.User.IsActive = model.IsActive;
                        existingSupplier.User.ApprovalStatus = model.ApprovalStatus;

                        // If status changed to Approved, set approval details
                        if (model.ApprovalStatus == "Approved" && existingSupplier.User.ApprovalStatus != "Approved")
                        {
                            existingSupplier.User.ApprovedById = _userManager.GetUserId(User);
                            existingSupplier.User.ApprovedAt = DateTime.UtcNow;
                        }
                    }

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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating supplier {SupplierId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the supplier.");
                }
            }

            // Re-populate dropdown and metrics on error
            ViewBag.SupplierTypes = await GetSupplierTypes();

            //if (User.IsInRole("PurchasingManager"))
            //{
            //    ViewBag.ReliabilityRating = await CalculateReliabilityRating(id);
            //    ViewBag.ProcurementStats = await GetSupplierProcurementStats(id);
            //}

            return View(model);
        }
        // GET: Suppliers/Delete/5
        [Authorize(Roles = "Admin")] // Only Admin can delete
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

            //// Check for active purchase orders or RFQs
            //var hasActiveProcurements = await HasActiveProcurements(id);
            //if (hasActiveProcurements)
            //{
            //    TempData["ErrorMessage"] = "Cannot delete this supplier because they have active purchase orders or RFQs.";
            //    return RedirectToAction(nameof(Index));
            //}

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Only Admin can delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers
                 .Include(s => s.User)
                 .FirstOrDefaultAsync(s => s.Id == id && s.User.IsActive);

            if (supplier != null)
            {
                //// Check for active procurements one more time
                //var hasActiveProcurements = await HasActiveProcurements(id);
                //if (hasActiveProcurements)
                //{
                //    TempData["ErrorMessage"] = "Cannot delete this supplier because they have active purchase orders or RFQs.";
                //    return RedirectToAction(nameof(Index));
                //}

                // Soft delete by deactivating the user
                supplier.User.IsActive = false;
                supplier.User.ApprovalStatus = "Rejected";
                await _userManager.UpdateAsync(supplier.User);

                TempData["SuccessMessage"] = $"Supplier '{supplier.CompanyName}' has been successfully deleted.";
            }
            else
            {
                TempData["ErrorMessage"] = "Supplier not found or has already been deleted.";
            }

            return RedirectToAction(nameof(Index));
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