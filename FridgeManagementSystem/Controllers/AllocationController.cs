//using FridgeManagementSystem.Data;
//using FridgeManagementSystem.Models;
//using FridgeManagementSystem.ViewModels;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//namespace FridgeManagementSystem.Controllers
//{
//    public class AllocationController : Controller
//    {
//        private readonly FridgeManagementSystemContext _context;
//        public AllocationController(FridgeManagementSystemContext context)
//        { 
//            _context = context;
//        }
//        // GET: FridgeAllocations
//        public async Task<IActionResult> Index()
//        {
//            var allocations = await _context.Allocations
//                .Include(f => f.Fridge)
//                .Include(f => f.Customer)
//                .ThenInclude(c => c.User)
//                .Include(f => f.AllocatedBy)
//                .Where(f => f.IsActive)
//                .OrderByDescending(f => f.AllocationDate)
//                .ToListAsync();

//            return View(allocations);
//        }

//        // GET: FridgeAllocations/Allocate/5
//        public async Task<IActionResult> Allocate(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var fridge = await _context.Fridges
//                .Include(f => f.Customer)
//                .ThenInclude(c => c.User)
//                .FirstOrDefaultAsync(f => f.FridgeId == id && f.IsActive);

//            if (fridge == null)
//            {
//                return NotFound();
//            }

//            if (fridge.Status == "Assigned")
//            {
//                TempData["ErrorMessage"] = "This fridge is already allocated to a customer.";
//                return RedirectToAction(nameof(Index));
//            }

//            ViewBag.Customers = await _context.Customers
//                .Where(c => c.User.IsActive && c.User.ApprovalStatus == "Approved")
//                .Select(c => new SelectListItem
//                {
//                    Value = c.Id.ToString(),
//                    Text = $"{c.BusinessName} - {c.User.FullName}"
//                })
//                .ToListAsync();

//            var model = new FridgeAllocationViewModel
//            {
//                FridgeId = fridge.FridgeId,
//                FridgeSerialNumber = fridge.SerialNumber,
//                FridgeModel = fridge.Model
//            };

//            return View(model);
//        }

//        // POST: FridgeAllocations/Allocate
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Allocate(FridgeAllocationViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                var fridge = await _context.Fridges.FindAsync(model.FridgeId);
//                if (fridge == null || !fridge.IsActive)
//                {
//                    return NotFound();
//                }

//                // Update fridge status and allocation info
//                fridge.Status = "Assigned";
//                fridge.CustomerId = model.CustomerId;
//                fridge.AllocationDate = DateTime.UtcNow;
//                fridge.ServiceDate = model.ServiceDate;

//                // Create allocation record
//                var allocation = new Allocation
//                {
//                    FridgeId = model.FridgeId,
//                    CustomerId = model.CustomerId.Value,
//                    AllocatedById = User.FindFirstValue(ClaimTypes.NameIdentifier),
//                    AllocationDate = DateTime.UtcNow,
//                    ServiceDate = model.ServiceDate,
                    
//                    IsActive = true
//                };

//                _context.Allocations.Add(allocation);
//                _context.Fridges.Update(fridge);

//                // Create notification for the customer
//                var customer = await _context.Customers
//                    .Include(c => c.User)
//                    .FirstOrDefaultAsync(c => c.Id == model.CustomerId);

//                if (customer != null)
//                {
//                    var notification = new Notification
//                    {
//                        UserId = customer.UserId,
//                        Title = "Fridge Allocated to Your Business",
//                        Message = $"A fridge (Serial: {fridge.SerialNumber}, Model: {fridge.Model}) has been allocated to your business. Service date: {model.ServiceDate:yyyy-MM-dd}",
//                        Link = $"/Customer/MyFridges"
//                    };

//                    _context.Notifications.Add(notification);
//                }

//                await _context.SaveChangesAsync();

//                TempData["SuccessMessage"] = "Fridge allocated successfully. Customer has been notified.";
//                return RedirectToAction(nameof(Index));
//            }

//            ViewBag.Customers = await _context.Customers
//                .Where(c => c.User.IsActive && c.User.ApprovalStatus == "Approved")
//                .Select(c => new SelectListItem
//                {
//                    Value = c.Id.ToString(),
//                    Text = $"{c.BusinessName} - {c.User.FullName}"
//                })
//                .ToListAsync();

//            return View(model);
//        }

//        // GET: FridgeAllocations/Deallocate/5
//        public async Task<IActionResult> Deallocate(int? id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var allocation = await _context.Allocations
//                .Include(f => f.Fridge)
//                .Include(f => f.Customer)
//                .ThenInclude(c => c.User)
//                .FirstOrDefaultAsync(f => f.FridgeId == id && f.IsActive);

//            if (allocation == null)
//            {
//                return NotFound();
//            }

//            return View(allocation);
//        }

//        // POST: FridgeAllocations/Deallocate/5
//        [HttpPost, ActionName("Deallocate")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeallocateConfirmed(int id)
//        {
//            var allocation = await _context.Allocations.FindAsync(id);
//            if (allocation == null)
//            {
//                return NotFound();
//            }

//            allocation.IsActive = false;

//            // Update fridge status
//            var fridge = await _context.Fridges.FindAsync(allocation.FridgeId);
//            if (fridge != null)
//            {
//                fridge.Status = "Available";
//                fridge.CustomerId = null;
//                fridge.AllocationDate = null;
//                fridge.ServiceDate = null;

//                _context.Fridges.Update(fridge);
//            }

//            await _context.SaveChangesAsync();

//            TempData["SuccessMessage"] = "Fridge deallocated successfully.";
//            return RedirectToAction(nameof(Index));
//        }
//        //public async Task<IActionResult> Create()
//        //{
//        //    ViewBag.Customers = await _db.Customers.ToListAsync();
//        //    ViewBag.Fridges = await _db.Fridges.Where(f => f.Status == Status.Available).ToListAsync();
//        //    return View();
//        //}

//        //[HttpPost]
//        //public async Task<IActionResult> Create(int customerId, int fridgeId)
//        //{
//        //    var fridge = await _db.Fridges.FindAsync(fridgeId);
//        //    if (fridge == null || fridge.Status != FridgeStatus.Available) return BadRequest("Fridge not available");

//        //    var allocation = new Allocation { CustomerId = customerId, FridgeId = fridgeId, StartDate = DateTime.UtcNow, Status = AllocationStatus.Active };
//        //    fridge.Status = FridgeStatus.Allocated;

//        //    _db.Allocations.Add(allocation);
//        //    _db.Fridges.Update(fridge);
//        //    await _db.SaveChangesAsync();
//        //    return RedirectToAction("Details", "Customers", new { id = customerId });
//        //}

//        //[HttpPost]
//        //public async Task<IActionResult> Return(int allocationId)
//        //{
//        //    var allocation = await _db.Allocations.Include(a => a.Fridge).FirstOrDefaultAsync(a => a.AllocationId == allocationId);
//        //    if (allocation == null) return NotFound();

//        //    allocation.EndDate = DateTime.UtcNow;
//        //    allocation.Status = AllocationStatus.Returned;
//        //    allocation.Fridge.Status = FridgeStatus.Available;

//        //    _db.Update(allocation);
//        //    _db.Update(allocation.Fridge);
//        //    await _db.SaveChangesAsync();

//        //    return RedirectToAction("Details", "Customers", new { id = allocation.CustomerId });
//        //}

//    }
//}
