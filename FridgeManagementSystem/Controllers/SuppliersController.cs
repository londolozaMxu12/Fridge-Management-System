
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
    [Authorize(Roles = "Admin,Employee")]
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

        // Helper method to check if current user is Purchasing Manager
        private async Task<bool> IsPurchasingManager()
        {
            if (User.IsInRole("Employee"))
            {
                var userId = _userManager.GetUserId(User);
                var employee = await _context.Employees
                    .Include(e => e.EmployeeType)
                    .FirstOrDefaultAsync(e => e.UserId == userId);

                return employee?.EmployeeType?.Name == "PurchasingManager";
            }
            return false;
        }

        // Helper method to set ViewBag for employee type
        private async Task SetEmployeeViewBag()
        {
            if (User.IsInRole("Employee"))
            {
                var userId = _userManager.GetUserId(User);
                var employee = await _context.Employees
                    .Include(e => e.EmployeeType)
                    .FirstOrDefaultAsync(e => e.UserId == userId);

                ViewBag.EmployeeType = employee?.EmployeeType?.Name;
                ViewBag.IsPurchasingManager = employee?.EmployeeType?.Name == "PurchasingManager";

                //// Add procurement metrics for Purchasing Manager
                //if (ViewBag.IsPurchasingManager == true)
                //{
                //    ViewBag.ProcurementMetrics = await GetProcurementMetrics();
                //}
            }
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            await SetEmployeeViewBag();

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

            await SetEmployeeViewBag();

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
            //if (ViewBag.IsPurchasingManager == true)
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

            await SetEmployeeViewBag();

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

            ViewBag.SupplierTypes = await GetSupplierTypes();

            //// Add procurement metrics for Purchasing Manager
            //if (ViewBag.IsPurchasingManager == true)
            //{
            //    ViewBag.ReliabilityRating = await CalculateReliabilityRating(supplier.Id);
            //    ViewBag.ProcurementStats = await GetSupplierProcurementStats(supplier.Id);
            //}

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

            // Check if user has permission to edit
            var isPurchasingManager = await IsPurchasingManager();
            if (!User.IsInRole("Admin") && !isPurchasingManager)
            {
                TempData["ErrorMessage"] = "You do not have permission to edit suppliers.";
                return RedirectToAction(nameof(Index));
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
                            existingSupplier.User.ApprovedAt = DateTime.Now;
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
            await SetEmployeeViewBag(); // Reset ViewBag on error

            //if (ViewBag.IsPurchasingManager == true)
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

            await SetEmployeeViewBag();

            var supplier = await _context.Suppliers
                .Include(s => s.User)
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id && m.User.IsActive);

            if (supplier == null)
            {
                TempData["ErrorMessage"] = "Supplier not found or has already been deleted.";
                return RedirectToAction(nameof(Index));
            }

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

        // Helper Methods
        private async Task<List<SelectListItem>> GetSupplierTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Fridge", Text = "Fridge Supplier" },
                new SelectListItem { Value = "Parts", Text = "Parts Supplier" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };
        }

        //private async Task<Dictionary<string, int>> GetProcurementMetrics()
        //{
        //    Mock data -replace with actual database queries
        //    return new Dictionary<string, int>
        //    {
        //        { "PendingRFQs", 12 },
        //        { "QuotationsReceived", 8 },
        //        { "PendingPurchaseRequests", 5 },
        //        { "ActiveSuppliers", await _context.Suppliers.CountAsync(s => s.User.IsActive && s.User.ApprovalStatus == "Approved") }
        //    };
        //}

        //private async Task<Dictionary<string, int>> GetSupplierProcurementStats(int supplierId)
        //{
        //    Mock data -replace with actual database queries
        //    return new Dictionary<string, int>
        //    {
        //        { "RFQsSent", 12 },
        //        { "QuotesReceived", 8 },
        //        { "ActiveOrders", 5 }
        //    };
        //}

        //private async Task<int> CalculateReliabilityRating(int supplierId)
        //{
        //    Mock calculation -replace with actual logic
        //    return 4; // Out of 5
        //}

        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.Id == id && e.User.IsActive);
        }
    }
}