using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FridgeController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FridgeController> _logger;

        public FridgeController(FridgeManagementSystemContext context, IWebHostEnvironment environment, ILogger<FridgeController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // GET: Fridge
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5, string sortBy = "AcquisitionDate",
            string sortOrder = "desc",
            string searchString = "",
            string statusFilter = "",
            string brandFilter = "",
            string supplierFilter = "",
            decimal? minPrice = null,
            decimal? maxPrice = null)
        {
            var query = _context.Fridges
                .Include(f => f.FridgeType)
                .Include(f => f.Supplier)
                .ThenInclude(s => s.User)
                .Include(f => f.CreatedBy)
                .Where(f => f.IsActive)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(f =>
                    f.SerialNumber.Contains(searchString) ||
                    f.Description.Contains(searchString) ||
                    f.FridgeType.Brand.Contains(searchString) ||
                    f.FridgeType.Name.Contains(searchString) ||
                    f.FridgeType.Model.Contains(searchString) ||
                    (f.Supplier.User.FullName != null && f.Supplier.User.FullName.Contains(searchString)) ||
                    (f.Supplier.CompanyName != null && f.Supplier.CompanyName.Contains(searchString)));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(f => f.Status == statusFilter);
            }

            // Apply brand filter
            if (!string.IsNullOrEmpty(brandFilter))
            {
                query = query.Where(f => f.FridgeType.Brand == brandFilter);
            }

            // Apply supplier filter
            if (!string.IsNullOrEmpty(supplierFilter))
            {
                query = query.Where(f => f.Supplier.User.FullName == supplierFilter || f.Supplier.CompanyName == supplierFilter);
            }

            // Apply price range filter
            if (minPrice.HasValue)
            {
                query = query.Where(f => f.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(f => f.Price <= maxPrice.Value);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "serialnumber" => sortOrder == "desc" ? query.OrderByDescending(f => f.SerialNumber) : query.OrderBy(f => f.SerialNumber),
                "price" => sortOrder == "desc" ? query.OrderByDescending(f => f.Price) : query.OrderBy(f => f.Price),
                "brand" => sortOrder == "desc" ? query.OrderByDescending(f => f.FridgeType.Brand) : query.OrderBy(f => f.FridgeType.Brand),
                "status" => sortOrder == "desc" ? query.OrderByDescending(f => f.Status) : query.OrderBy(f => f.Status),
                "supplier" => sortOrder == "desc" ? query.OrderByDescending(f => f.Supplier.User.FullName ?? f.Supplier.CompanyName) : query.OrderBy(f => f.Supplier.User.FullName ?? f.Supplier.CompanyName),
                "acquisitiondate" => sortOrder == "desc" ? query.OrderByDescending(f => f.AcquisitionDate) : query.OrderBy(f => f.AcquisitionDate),
                _ => sortOrder == "desc" ? query.OrderByDescending(f => f.AcquisitionDate) : query.OrderBy(f => f.AcquisitionDate)
            };

            var totalCount = await query.CountAsync();

            // Apply pagination
            var fridges = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get filter options for dropdowns
            var brands = await _context.FridgeType
                .Where(ft => ft.IsActive)
                .Select(ft => ft.Brand)
                .Distinct()
                .OrderBy(b => b)
                .ToListAsync();

            var suppliers = await _context.Suppliers
                .Include(s => s.User)
                .Where(s => s.User.IsActive)
                .Select(s => s.User.FullName ?? s.CompanyName)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            var statuses = new List<string> { "Available", "Allocated", "InService", "Scrapped" };

            var viewModel = new FridgeManagementViewModel
            {
                Fridges = fridges,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                SortBy = sortBy,
                SortOrder = sortOrder,
                SearchString = searchString,
                StatusFilter = statusFilter,
                BrandFilter = brandFilter,
                SupplierFilter = supplierFilter,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            ViewBag.Brands = brands;
            ViewBag.Suppliers = suppliers;
            ViewBag.Statuses = statuses;

            return View(viewModel);
        }

        // GET: Fridge/Inactive
        public async Task<IActionResult> Inactive()
        {
            var inactiveFridges = await _context.Fridges
                .Include(f => f.FridgeType)
                .Include(f => f.Supplier)
                .ThenInclude(s => s.User)
                .Include(f => f.CreatedBy)
                .Where(f => !f.IsActive)
                .OrderByDescending(f => f.AcquisitionDate)
                .ToListAsync();

            return View(inactiveFridges);
        }

        // GET: Fridge/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fridge = await _context.Fridges
                .Include(f => f.FridgeType)
                .Include(f => f.Supplier)
                .ThenInclude(s => s.User)
                .Include(f => f.Customer)
                .ThenInclude(c => c.User)
                .Include(f => f.CreatedBy)
                .Include(f => f.MaintenanceRecords)
                .ThenInclude(m => m.MaintenanceTechnician)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(m => m.FridgeId == id);

            if (fridge == null)
            {
                return NotFound();
            }

            return View(fridge);
        }

        // GET: Fridge/Create
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            var model = new CreateFridgeViewModel
            {
                AcquisitionDate = DateTime.Now,
                Status = "Available"
            };
            return View(model);
        }

        // POST: Fridge/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFridgeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get selected fridge type details
                    var selectedFridgeType = await _context.FridgeType
                        .FirstOrDefaultAsync(ft => ft.FridgeTypeId == model.FridgeTypeId && ft.IsActive);

                    if (selectedFridgeType == null)
                    {
                        ModelState.AddModelError("FridgeTypeId", "Selected fridge type is invalid or inactive");
                        await LoadViewData();
                        return View(model);
                    }

                    // Check if supplier exists and is active
                    var supplier = await _context.Suppliers
                        .Include(s => s.User)
                        .FirstOrDefaultAsync(s => s.Id == model.SupplierId && s.User.IsActive);

                    if (supplier == null)
                    {
                        ModelState.AddModelError("SupplierId", "Selected supplier is invalid or inactive");
                        await LoadViewData();
                        return View(model);
                    }

                    var fridge = new Fridge
                    {
                        Price = model.Price,
                        Description = model.Description,
                        SerialNumber = model.SerialNumber?.Trim(),
                        AcquisitionDate = model.AcquisitionDate,
                        Status = model.Status,
                        IsActive = true,
                        FridgeTypeId = model.FridgeTypeId,
                        SupplierId = model.SupplierId,
                        CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        NextServiceDate = model.NextServiceDate,
                        Category = model.Category ?? "Refrigerator",
                        CreatedAt = DateTime.Now
                    };

                    // Handle image upload
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        // Validate image file
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png"};
                        var fileExtension = Path.GetExtension(model.ImageFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageFile", "Only image files (JPG, JPEG, PNG) are allowed.");
                            await LoadViewData();
                            return View(model);
                        }

                        // Check file size (e.g., 5MB limit)
                        if (model.ImageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ImageFile", "Image file size must be less than 5MB.");
                            await LoadViewData();
                            return View(model);
                        }

                        fridge.ImageFileName = await SaveImage(model.ImageFile);
                    }
                    else
                    {
                        fridge.ImageFileName = "default-fridge.jpg";
                    }

                    // Generate serial number if not provided
                    if (string.IsNullOrEmpty(fridge.SerialNumber))
                    {
                        fridge.SerialNumber = await GenerateSerialNumber();
                    }

                    // Check if serial number already exists
                    var existingSerial = await _context.Fridges
                        .AnyAsync(f => f.SerialNumber == fridge.SerialNumber && f.IsActive);

                    if (existingSerial)
                    {
                        ModelState.AddModelError("SerialNumber", "Serial number already exists");
                        await LoadViewData();
                        return View(model);
                    }

                    _context.Fridges.Add(fridge);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Fridge created successfully! Type: {selectedFridgeType.Name}, {selectedFridgeType.Brand}, {selectedFridgeType.Model}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating fridge");
                    ModelState.AddModelError("", "An error occurred while creating the fridge: " + ex.Message);
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: Fridge/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Fridge ID is required";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var fridge = await _context.Fridges
                    .Include(f => f.FridgeType)
                    .Include(f => f.Supplier)
                    .ThenInclude(s => s.User)
                    .FirstOrDefaultAsync(f => f.FridgeId == id && f.IsActive);

                if (fridge == null)
                {
                    TempData["Error"] = "Fridge not found or is inactive";
                    return RedirectToAction(nameof(Index));
                }

                // Validate required fields
                if (fridge.FridgeTypeId == 0 || fridge.SupplierId == 0)
                {
                    TempData["Error"] = "Fridge is missing required type or supplier information";
                    return RedirectToAction(nameof(Index));
                }

                var model = new EditFridgeViewModel
                {
                    FridgeId = fridge.FridgeId,
                    Price = fridge.Price,
                    Description = fridge.Description ?? string.Empty,
                    SerialNumber = fridge.SerialNumber ?? string.Empty,
                    AcquisitionDate = fridge.AcquisitionDate,
                    Status = fridge.Status ?? "Available",
                    FridgeTypeId = fridge.FridgeTypeId,
                    SupplierId = fridge.SupplierId,
                    NextServiceDate = fridge.NextServiceDate,
                    Category = fridge.Category ?? "Refrigerator",
                    ExistingImagePath = fridge.ImageFileName ?? string.Empty,
                    SelectedFridgeTypeDisplay = fridge.FridgeType != null ?
                        $" {fridge.FridgeType.Name} - {fridge.FridgeType.Brand} - {fridge.FridgeType.Model}" : "Not Set"
                };

                await LoadViewData();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fridge for edit. FridgeId: {FridgeId}", id);
                TempData["Error"] = "An error occurred while loading the fridge for editing. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Fridge/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditFridgeViewModel model)
        {
            if (id != model.FridgeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var fridge = await _context.Fridges.FindAsync(id);
                    if (fridge == null)
                    {
                        return NotFound();
                    }

                    // Check if serial number already exists (excluding current fridge)
                    if (!string.IsNullOrEmpty(model.SerialNumber))
                    {
                        var existingSerial = await _context.Fridges
                            .AnyAsync(f => f.SerialNumber == model.SerialNumber.Trim() &&
                                         f.FridgeId != id &&
                                         f.IsActive);

                        if (existingSerial)
                        {
                            ModelState.AddModelError("SerialNumber", "Serial number already exists");
                            await LoadViewData();
                            return View(model);
                        }
                    }

                    // Update properties
                    fridge.Price = model.Price;
                    fridge.Description = model.Description;
                    fridge.SerialNumber = model.SerialNumber?.Trim();
                    fridge.AcquisitionDate = model.AcquisitionDate;
                    fridge.Status = model.Status;
                    fridge.FridgeTypeId = model.FridgeTypeId;
                    fridge.SupplierId = model.SupplierId;
                    fridge.NextServiceDate = model.NextServiceDate;
                    fridge.Category = model.Category ?? "Refrigerator";

                    // Handle image upload
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        // Validate image file
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var fileExtension = Path.GetExtension(model.ImageFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageFile", "Only image files (JPG, JPEG, PNG) are allowed.");
                            await LoadViewData();
                            return View(model);
                        }

                        if (model.ImageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ImageFile", "Image file size must be less than 5MB.");
                            await LoadViewData();
                            return View(model);
                        }

                        // Delete old image if exists and not default
                        if (!string.IsNullOrEmpty(fridge.ImageFileName) && fridge.ImageFileName != "default-fridge.jpg")
                        {
                            DeleteImage(fridge.ImageFileName);
                        }
                        fridge.ImageFileName = await SaveImage(model.ImageFile);
                    }

                    _context.Fridges.Update(fridge);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Fridge updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FridgeExists(model.FridgeId))
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
                    _logger.LogError(ex, "Error updating fridge. FridgeId: {FridgeId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the fridge: " + ex.Message);
                }
            }

            await LoadViewData();
            return View(model);
        }

        // GET: Fridge/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fridge = await _context.Fridges
                .Include(f => f.FridgeType)
                .Include(f => f.Supplier)
                .ThenInclude(s => s.User)
                .Include(f => f.CreatedBy)
                .FirstOrDefaultAsync(m => m.FridgeId == id && m.IsActive);

            if (fridge == null)
            {
                return NotFound();
            }

            return View(fridge);
        }

        // POST: Fridge/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var fridge = await _context.Fridges.FindAsync(id);
                if (fridge != null)
                {
                    // Soft delete - set IsActive to false
                    fridge.IsActive = false;
                    fridge.Status = "Inactive";

                    _context.Fridges.Update(fridge);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Fridge deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Fridge not found!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting fridge. FridgeId: {FridgeId}", id);
                TempData["Error"] = "An error occurred while deleting the fridge.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Fridge/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var fridge = await _context.Fridges.FindAsync(id);
                if (fridge != null)
                {
                    // Restore - set IsActive to true and status to Available
                    fridge.IsActive = true;
                    fridge.Status = "Available";

                    _context.Fridges.Update(fridge);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Fridge restored successfully!";
                }
                else
                {
                    TempData["Error"] = "Fridge not found!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring fridge. FridgeId: {FridgeId}", id);
                TempData["Error"] = "An error occurred while restoring the fridge.";
            }

            return RedirectToAction(nameof(Inactive));
        }

        private bool FridgeExists(int id)
        {
            return _context.Fridges.Any(e => e.FridgeId == id);
        }

        private async Task LoadViewData()
        {
            // Get active fridge types with display format
            var fridgeTypes = await _context.FridgeType
                .Where(ft => ft.IsActive)
                .OrderBy(ft => ft.Brand)
                .ThenBy(ft => ft.Name)
                .ThenBy(ft => ft.Model)
                .Select(ft => new
                {
                    FridgeTypeId = ft.FridgeTypeId,
                    DisplayText = $"{ft.Brand} - {ft.Name} - {ft.Model}"
                })
                .ToListAsync();

            ViewData["FridgeTypeId"] = new SelectList(fridgeTypes, "FridgeTypeId", "DisplayText");

            ViewData["SupplierId"] = new SelectList(
                await _context.Suppliers
                    .Include(s => s.User)
                    .Where(s => s.User.IsActive)
                    .Select(s => new
                    {
                        Id = s.Id,
                        Name = s.User.FullName ?? s.CompanyName
                    })
                    .ToListAsync(),
                "Id",
                "Name"
            );

            ViewData["StatusList"] = new SelectList(new[]
            {
                new { Value = "Available", Text = "Available" },
                new { Value = "Allocated", Text = "Allocated" },
                new { Value = "InService", Text = "In-Service" },
                new { Value = "Scrapped", Text = "Scrapped" }
                
            }, "Value", "Text");

            ViewData["Categories"] = new SelectList(new[]
            {
                "Refrigerator",
                "Freezer",
                "Wine Cooler",
                "Commercial Fridge"
                
            });
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "Images/Fridges");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        private void DeleteImage(string imageFileName)
        {
            if (!string.IsNullOrEmpty(imageFileName) && imageFileName != "default-fridge.jpg")
            {
                var fullPath = Path.Combine(_environment.WebRootPath, "Images/Fridges", imageFileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
        }

        private async Task<string> GenerateSerialNumber()
        {
            var today = DateTime.Now.ToString("ddMMyyyy");
            var count = await _context.Fridges
                .Where(f => f.SerialNumber.StartsWith($"SN{today}"))
                .CountAsync();

            return $"SN{today}{(count + 1).ToString("D3")}";
        }
    }
}