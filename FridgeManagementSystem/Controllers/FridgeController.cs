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
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FridgeController : Controller
    {
        private readonly FridgeManagementSystemContext _context;
        private readonly IWebHostEnvironment _environment;
        
        private readonly ILogger<RegisterEmployeeModel> _logger;

        public FridgeController(FridgeManagementSystemContext context, IWebHostEnvironment environment,
            ILogger<RegisterEmployeeModel> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // GET: Fridge
        public async Task<IActionResult> Index()
        {
            try
            {
                var fridges = await _context.Fridges
                    .Include(f => f.FridgeType)
                    .Include(f => f.Supplier)
                    .ThenInclude(s => s.User)
                    .Include(f => f.CreatedBy)
                    .Where(f => f.IsActive)
                    .OrderByDescending(f => f.PurchaseDate)
                    .ToListAsync();

                return View(fridges);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error loading fridges");

                // Return error view or redirect
                TempData["Error"] = "An error occurred while loading fridges.";
                return View(new List<Fridge>());
            }
            //var fridges = await _context.Fridges
            //    .Include(f => f.FridgeType)
            //    .Include(f => f.Supplier)
            //    .ThenInclude(s => s.User)
            //    .Include(f => f.CreatedBy)
            //    .Where(f => f.IsActive) // Only show active fridges
            //    .OrderByDescending(f => f.PurchaseDate)
            //    .ToListAsync();

            //return View(fridges);
        }

        // GET: Fridge/Inactive
        public async Task<IActionResult> Inactive()
        {
            var inactiveFridges = await _context.Fridges
                .Include(f => f.FridgeType)
                .Include(f => f.Supplier)
                .ThenInclude(s => s.User)
                .Include(f => f.CreatedBy)
                .Where(f => !f.IsActive) // Only show inactive fridges
                .OrderByDescending(f => f.PurchaseDate)
                .ToListAsync();

            return View(inactiveFridges);
        }

        // GET: Fridge/Details/5
        public async Task<IActionResult> FridgeDetails(int? id)
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
        public async Task<IActionResult> AddFridge()
        {
            try
            {
                await LoadViewData();
                var model = new FridgeViewModel
                {
                    AcquisitionDate = DateTime.Now,
                    Status = "Available" // Set default status
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading AddFridge page");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFridge(FridgeViewModel model)
        {
            // Manually validate the image file since it's an IFormFile
            if (model.ImageFileName == null || model.ImageFileName.Length == 0)
            {
                ModelState.AddModelError("ImageFileName", "Please upload an image");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get selected fridge type details
                    var selectedFridgeType = await _context.FridgeType
                        .FirstOrDefaultAsync(ft => ft.FridgeTypeId == model.FridgeTypeId);

                    if (selectedFridgeType == null)
                    {
                        ModelState.AddModelError("FridgeTypeId", "Selected fridge type is invalid");
                        await LoadViewData();
                        return View(model);
                    }

                    // Handle image upload
                    string newFileName = await SaveImageFile(model.ImageFileName);

                    var fridge = new Fridge
                    {
                        Price = model.Price,
                        Description = model.Description,
                        SerialNumber = model.SerialNumber,
                        AcquisitionDate = model.AcquisitionDate,
                        PurchaseDate = DateTime.Now,
                        Status = model.Status, // Use the status from view model
                        IsActive = true,
                        IsAvailable = model.Status == "Available", // Set based on status
                        FridgeTypeId = model.FridgeTypeId,
                        SupplierId = model.SupplierId,
                        CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        NextServiceDate = model.NextServiceDate,
                        ImageFile = newFileName,
                    };

                    _context.Fridges.Add(fridge);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Fridge added successfully! Type: {selectedFridgeType.Brand} {selectedFridgeType.Name} {selectedFridgeType.Model}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding fridge");
                    ModelState.AddModelError("", "An error occurred while creating the fridge. Please try again.");
                }
            }

            await LoadViewData();
            return View(model);
        }

        public async Task<IActionResult> EditFridge(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Fridge");
            }

            var fridge = await _context.Fridges
                .Include(f => f.FridgeType)
                .FirstOrDefaultAsync(f => f.FridgeId == id);

            if (fridge == null)
            {
                return RedirectToAction("Index", "Fridge");
            }

            var model = new FridgeViewModel
            {
                
                Price = fridge.Price,
                Description = fridge.Description,
                SerialNumber = fridge.SerialNumber,
                AcquisitionDate = fridge.AcquisitionDate,
                Status = fridge.Status,
                FridgeTypeId = (int)fridge.FridgeTypeId,
                SupplierId = (int)fridge.SupplierId,
                NextServiceDate = fridge.NextServiceDate,
                ServiceDate = fridge.ServiceDate,
                
                SelectedFridgeTypeDisplay = $"{fridge.FridgeType?.Brand} - {fridge.FridgeType?.Name} - {fridge.FridgeType?.Model}"
            };

            ViewData["FridgeId"] = fridge.FridgeId;
            ViewData["ImageFileName"] = fridge.ImageFile;
            ViewData["CreatedAt"] = fridge.PurchaseDate.ToString("MM/dd/yyyy");
            await LoadViewData();
            return View(model);
        }

        // POST: Fridge/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFridge(int id, FridgeViewModel model, IFormFile imageFile)
        {
            if (id != model.FridgeId)
            {
                return RedirectToAction("Index", "Fridge");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var fridge = await _context.Fridges.FindAsync(id);
                    if (fridge == null)
                    {
                        return RedirectToAction("Index", "Fridge");
                    }

                    ViewData["FridgeId"] = fridge.FridgeId;
                    ViewData["ImageFileName"] = fridge.ImageFile;
                    ViewData["CreatedAt"] = fridge.PurchaseDate.ToString("MM/dd/yyyy");

                    // Update properties
                    fridge.Price = model.Price;
                    fridge.Description = model.Description;
                    fridge.SerialNumber = model.SerialNumber;
                    fridge.AcquisitionDate = model.AcquisitionDate;
                    fridge.Status = model.Status;
                    fridge.IsAvailable = model.Status == "Available";
                    fridge.FridgeTypeId = model.FridgeTypeId;
                    fridge.SupplierId = model.SupplierId;
                    fridge.NextServiceDate = model.NextServiceDate;
                    fridge.ServiceDate = model.ServiceDate;

                    // Handle image upload
                    // update the image file if we have a new image file
                    string newFileName = fridge.ImageFile;

                    if (fridge.ImageFile != null)
                    {
                        newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        newFileName += Path.GetExtension(model.ImageFileName.FileName);

                        string imageFullPath = _environment.WebRootPath + "Images/Fridges/" + newFileName;
                        using (var stream = System.IO.File.Create(imageFullPath))
                        {
                            model.ImageFileName.CopyTo(stream);
                        }

                        // delete the old image
                        string oldImageFullPath = _environment.WebRootPath + "Images/Fridges/" + fridge.ImageFile;
                        System.IO.File.Delete(oldImageFullPath);
                    }
                    //if (imageFile != null && imageFile.Length > 0)
                    //{
                    //    // Delete old image if exists
                    //    if (!string.IsNullOrEmpty(fridge.ImageFile))
                    //    {
                    //        DeleteImage(fridge.ImageFile);
                    //    }
                    //    fridge.ImageFile = await SaveImage(imageFile);
                    //}

                    _context.Update(fridge);
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
            }

            await LoadViewData();
            return View(model);
        }

        // GET: Fridge/Delete/5
        public async Task<IActionResult> DeleteFridge(int? id)
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
                .FirstOrDefaultAsync(m => m.FridgeId == id);

            if (fridge == null)
            {
                return NotFound();
            }

            return View(fridge);
        }

        // POST: Fridge/Delete/5
        [HttpPost, ActionName("DeleteFridge")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fridge = await _context.Fridges.FindAsync(id);
            if (fridge != null)
            {
                // Soft delete - set IsActive to false
                fridge.IsActive = false;
                fridge.IsAvailable = false;
                fridge.Status = "Inactive";

                _context.Fridges.Update(fridge);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Fridge deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Fridge not found!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Fridge/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var fridge = await _context.Fridges.FindAsync(id);
            if (fridge != null)
            {
                // Restore - set IsActive to true
                fridge.IsActive = true;
                fridge.IsAvailable = true;
                fridge.Status = "Available";

                _context.Fridges.Update(fridge);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Fridge restored successfully!";
            }
            else
            {
                TempData["Error"] = "Fridge not found!";
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
                    .ToListAsync(),
                "SupplierId", // Make sure this matches your Supplier model's ID property name
                "User.FullName"
            );

            ViewData["StatusList"] = new SelectList(new[]
            {
                 new { Value = "Available", Text = "Available" },
                 new { Value = "Allocated", Text = "Allocated" },
                 new { Value = "InService", Text = "In-Service" },
                 new { Value = "Scrapped", Text = "Scrapped" },
                 new { Value = "Maintenance", Text = "Under Maintenance" },
            }, "Value", "Text");
        }

        private async Task<string> SaveImageFile(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "Images", "Fridges");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        //private void DeleteImage(string imagePath)
        //{
        //    if (!string.IsNullOrEmpty(imagePath))
        //    {
        //        var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
        //        if (System.IO.File.Exists(fullPath))
        //        {
        //            System.IO.File.Delete(fullPath);
        //        }
        //    }
        //}

        private async Task<string> GenerateSerialNumber()
        {
            var today = DateTime.Now.ToString("yyyyMMdd");
            var count = await _context.Fridges
                .Where(f => f.SerialNumber.StartsWith($"FRG{today}"))
                .CountAsync();

            return $"FRG{today}{(count + 1).ToString("D3")}";
        }
    }
}
