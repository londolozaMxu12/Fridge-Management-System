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
            try
            {
                await LoadViewData();
                var model = new CreateFridgeViewModel
                {
                    AcquisitionDate = DateTime.Now,
                    Status = "Available"
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Create view");
                TempData["Error"] = "An error occurred while loading the create form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Fridge/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFridgeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadViewData();
                return View(model);
            }

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

                // Handle image upload with enhanced error handling
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    _logger.LogInformation("Processing image upload for new fridge");

                    // Validate image file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(model.ImageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFile", "Only image files (JPG, JPEG, PNG) are allowed.");
                        await LoadViewData();
                        return View(model);
                    }

                    // Check file size (5MB limit)
                    if (model.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "Image file size must be less than 5MB.");
                        await LoadViewData();
                        return View(model);
                    }

                    try
                    {
                        var savedImagePath = await SaveImage(model.ImageFile);
                        fridge.ImageFileName = savedImagePath;

                        // Verify the image was saved successfully
                        if (!ImageExists(savedImagePath) && savedImagePath != "default-fridge.jpg")
                        {
                            _logger.LogWarning("Image was not saved properly for new fridge, using default image");
                            fridge.ImageFileName = "default-fridge.jpg";
                            ModelState.AddModelError("", "Warning: Image was uploaded but could not be saved properly. Using default image.");
                        }
                        else
                        {
                            _logger.LogInformation("Image saved successfully for new fridge: {ImageFileName}", savedImagePath);
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        _logger.LogError(ex, "Permission denied while saving image for new fridge");
                        ModelState.AddModelError("ImageFile", "Permission denied while saving image. Please contact administrator.");
                        await LoadViewData();
                        return View(model);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "IO error while saving image for new fridge");
                        ModelState.AddModelError("ImageFile", "Error saving image file. Please try again.");
                        await LoadViewData();
                        return View(model);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error while saving image for new fridge");
                        ModelState.AddModelError("ImageFile", "An unexpected error occurred while saving the image. Using default image.");
                        fridge.ImageFileName = "default-fridge.jpg";
                    }
                }
                else
                {
                    fridge.ImageFileName = "default-fridge.jpg";
                    _logger.LogInformation("No image provided for new fridge, using default image");
                }

                // Generate serial number if not provided
                if (string.IsNullOrEmpty(fridge.SerialNumber))
                {
                    fridge.SerialNumber = await GenerateSerialNumber();
                    _logger.LogInformation("Generated serial number: {SerialNumber}", fridge.SerialNumber);
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

                // Save the fridge to database
                _context.Fridges.Add(fridge);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Fridge created successfully. ID: {FridgeId}, Type: {FridgeType}",
                    fridge.FridgeId, selectedFridgeType.Name);

                TempData["Success"] = $"Fridge created successfully! Type: {selectedFridgeType.Name}, {selectedFridgeType.Brand}, {selectedFridgeType.Model}";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while creating fridge");
                ModelState.AddModelError("", "A database error occurred while creating the fridge. Please try again.");

                // Clean up any uploaded image if database save failed
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    try
                    {
                        // This would need the filename, but we don't have it yet since save failed
                        _logger.LogWarning("Database save failed, but image may have been uploaded. Manual cleanup may be required.");
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(cleanupEx, "Error during cleanup after database failure");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating fridge");
                ModelState.AddModelError("", "An unexpected error occurred while creating the fridge: " + ex.Message);
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

                // Verify existing image
                if (!string.IsNullOrEmpty(model.ExistingImagePath) && model.ExistingImagePath != "default-fridge.jpg")
                {
                    if (!ImageExists(model.ExistingImagePath))
                    {
                        _logger.LogWarning("Existing image not found for fridge {FridgeId}: {ImageFileName}",
                            fridge.FridgeId, model.ExistingImagePath);
                        model.ExistingImagePath = "default-fridge.jpg";
                    }
                }

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
                TempData["Error"] = "Fridge ID mismatch";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await LoadViewData();
                return View(model);
            }

            Fridge fridge = null;
            string oldImageFileName = null;

            try
            {
                fridge = await _context.Fridges.FindAsync(id);
                if (fridge == null)
                {
                    TempData["Error"] = "Fridge not found";
                    return RedirectToAction(nameof(Index));
                }

                // Store old image filename for potential cleanup
                oldImageFileName = fridge.ImageFileName;

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

                // Handle image upload with enhanced error handling
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    _logger.LogInformation("Processing image upload for fridge edit. FridgeId: {FridgeId}", id);

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

                    try
                    {
                        // Save new image first
                        var savedImagePath = await SaveImage(model.ImageFile);

                        // Verify new image was saved successfully
                        if (!ImageExists(savedImagePath))
                        {
                            _logger.LogWarning("New image was not saved properly for fridge {FridgeId}, keeping old image", id);
                            ModelState.AddModelError("ImageFile", "New image could not be saved properly. Keeping existing image.");
                        }
                        else
                        {
                            // New image saved successfully, delete old image
                            if (!string.IsNullOrEmpty(oldImageFileName) && oldImageFileName != "default-fridge.jpg")
                            {
                                try
                                {
                                    DeleteImage(oldImageFileName);
                                    _logger.LogInformation("Old image deleted successfully: {OldImageFileName}", oldImageFileName);
                                }
                                catch (Exception deleteEx)
                                {
                                    _logger.LogWarning(deleteEx, "Failed to delete old image: {OldImageFileName}", oldImageFileName);
                                    // Continue with update even if old image deletion fails
                                }
                            }

                            fridge.ImageFileName = savedImagePath;
                            _logger.LogInformation("New image saved successfully for fridge {FridgeId}: {ImageFileName}", id, savedImagePath);
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        _logger.LogError(ex, "Permission denied while saving new image for fridge {FridgeId}", id);
                        ModelState.AddModelError("ImageFile", "Permission denied while saving image. Please contact administrator.");
                        await LoadViewData();
                        return View(model);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogError(ex, "IO error while saving new image for fridge {FridgeId}", id);
                        ModelState.AddModelError("ImageFile", "Error saving image file. Please try again.");
                        await LoadViewData();
                        return View(model);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error while saving new image for fridge {FridgeId}", id);
                        ModelState.AddModelError("ImageFile", "An unexpected error occurred while saving the image. Keeping existing image.");
                        // Keep the existing image filename
                    }
                }

                // Update the fridge in database
                _context.Fridges.Update(fridge);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Fridge updated successfully. ID: {FridgeId}", id);
                TempData["Success"] = "Fridge updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FridgeExists(model.FridgeId))
                {
                    TempData["Error"] = "Fridge not found";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogError("Concurrency error updating fridge {FridgeId}", id);
                    ModelState.AddModelError("", "The fridge was modified by another user. Please refresh and try again.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while updating fridge {FridgeId}", id);
                ModelState.AddModelError("", "A database error occurred while updating the fridge. Please try again.");

                // If we saved a new image but database update failed, try to clean up the new image
                if (model.ImageFile != null && model.ImageFile.Length > 0 && fridge?.ImageFileName != oldImageFileName)
                {
                    try
                    {
                        DeleteImage(fridge?.ImageFileName);
                        _logger.LogInformation("Cleaned up new image after database failure");
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(cleanupEx, "Error cleaning up new image after database failure");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating fridge {FridgeId}", id);
                ModelState.AddModelError("", "An unexpected error occurred while updating the fridge: " + ex.Message);
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
            if (imageFile == null || imageFile.Length == 0)
            {
                _logger.LogWarning("SaveImage called with null or empty file");
                return "default-fridge.jpg";
            }

            try
            {
                // Validate WebRootPath
                if (string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    _logger.LogError("WebRootPath is null or empty. Environment: {Environment}", _environment.EnvironmentName);
                    throw new InvalidOperationException("WebRootPath is not configured properly");
                }

                _logger.LogInformation("WebRootPath: {WebRootPath}", _environment.WebRootPath);
                _logger.LogInformation("Environment: {Environment}", _environment.EnvironmentName);
                _logger.LogInformation("Original filename: {FileName}", imageFile.FileName);
                _logger.LogInformation("File size: {FileSize} bytes", imageFile.Length);

                // Define uploads folder path
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "Images", "Fridges");
                _logger.LogInformation("Target upload folder: {UploadsFolder}", uploadsFolder);

                // Ensure directory exists with proper permissions
                try
                {
                    if (!Directory.Exists(uploadsFolder))
                    {
                        _logger.LogInformation("Creating directory: {UploadsFolder}", uploadsFolder);
                        Directory.CreateDirectory(uploadsFolder);

                        // Verify directory was created
                        if (!Directory.Exists(uploadsFolder))
                        {
                            throw new InvalidOperationException($"Failed to create directory: {uploadsFolder}");
                        }
                        _logger.LogInformation("Directory created successfully");
                    }
                    else
                    {
                        _logger.LogInformation("Directory already exists");
                    }

                    // Check write permissions - FIXED: Use System.IO.File explicitly
                    var testFile = Path.Combine(uploadsFolder, "test_permission.txt");
                    await System.IO.File.WriteAllTextAsync(testFile, "test"); // FIXED
                    System.IO.File.Delete(testFile); // FIXED
                    _logger.LogInformation("Write permissions verified");
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "No write permission to directory: {UploadsFolder}", uploadsFolder);
                    throw new UnauthorizedAccessException($"No write permission to directory: {uploadsFolder}", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating or accessing directory: {UploadsFolder}", uploadsFolder);
                    throw new InvalidOperationException($"Cannot access directory: {uploadsFolder}", ex);
                }

                // Generate safe filename
                var originalFileName = Path.GetFileName(imageFile.FileName);
                var safeFileName = Path.GetInvalidFileNameChars()
                    .Aggregate(originalFileName, (current, c) => current.Replace(c, '_'));

                var uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                _logger.LogInformation("Generated unique filename: {UniqueFileName}", uniqueFileName);
                _logger.LogInformation("Full file path: {FilePath}", filePath);

                // Validate file doesn't already exist (unlikely with GUID, but safe) - FIXED
                if (System.IO.File.Exists(filePath)) // FIXED
                {
                    _logger.LogWarning("File already exists, generating new name: {FilePath}", filePath);
                    uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
                    filePath = Path.Combine(uploadsFolder, uniqueFileName);
                }

                // Save the file with retry logic
                const int maxRetries = 3;
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        _logger.LogInformation("Saving file (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);

                        using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            await imageFile.CopyToAsync(fileStream);
                            await fileStream.FlushAsync();
                        }

                        _logger.LogInformation("File saved successfully: {FilePath}", filePath);

                        // Verify file was written - FIXED
                        if (!System.IO.File.Exists(filePath)) // FIXED
                        {
                            throw new IOException("File was not created after write operation");
                        }

                        var fileInfo = new FileInfo(filePath);
                        _logger.LogInformation("File verification - Size: {FileSize} bytes, Exists: {Exists}",
                            fileInfo.Length, System.IO.File.Exists(filePath)); // FIXED

                        break; // Success, exit retry loop
                    }
                    catch (IOException ex) when (attempt < maxRetries)
                    {
                        _logger.LogWarning(ex, "Attempt {Attempt} failed for file: {FilePath}", attempt, filePath);

                        // Wait before retry (exponential backoff)
                        await Task.Delay(100 * attempt);

                        // Generate new filename for retry
                        uniqueFileName = $"{Guid.NewGuid()}_{safeFileName}";
                        filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        _logger.LogInformation("Retrying with new filename: {UniqueFileName}", uniqueFileName);
                    }
                }

                return uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error saving image file. WebRootPath: {WebRootPath}", _environment.WebRootPath);

                // Return default image instead of throwing to prevent complete failure
                return "default-fridge.jpg";
            }
        }

        private void DeleteImage(string imageFileName)
        {
            if (string.IsNullOrEmpty(imageFileName) || imageFileName == "default-fridge.jpg")
            {
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    _logger.LogWarning("WebRootPath is null during image deletion");
                    return;
                }

                var fullPath = Path.Combine(_environment.WebRootPath, "Images", "Fridges", imageFileName);

                _logger.LogInformation("Attempting to delete image: {FullPath}", fullPath);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    _logger.LogInformation("Image deleted successfully: {ImageFileName}", imageFileName);

                    // Verify deletion
                    if (System.IO.File.Exists(fullPath))
                    {
                        _logger.LogWarning("File still exists after deletion: {FullPath}", fullPath);
                    }
                }
                else
                {
                    _logger.LogWarning("Image file not found for deletion: {FullPath}", fullPath);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "No permission to delete image: {ImageFileName}", imageFileName);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IO error deleting image: {ImageFileName}", imageFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting image: {ImageFileName}", imageFileName);
            }
        }

        private bool ImageExists(string imageFileName)
        {
            if (string.IsNullOrEmpty(imageFileName) || imageFileName == "default-fridge.jpg")
            {
                return true; // Default image is assumed to exist
            }

            try
            {
                if (string.IsNullOrEmpty(_environment.WebRootPath))
                {
                    _logger.LogWarning("WebRootPath is null during image existence check");
                    return false;
                }

                var fullPath = Path.Combine(_environment.WebRootPath, "Images", "Fridges", imageFileName);
                var exists = System.IO.File.Exists(fullPath);

                if (!exists)
                {
                    _logger.LogWarning("Image file not found: {FullPath}", fullPath);
                }

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking image existence: {ImageFileName}", imageFileName);
                return false;
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