//using FridgeManagementSystem.Areas.Identity.Data;
//using FridgeManagementSystem.Data;
//using FridgeManagementSystem.Models;
//using FridgeManagementSystem.ViewModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//[Authorize(Roles = "Admin,CustomerLiaison")]
//public class AllocationController : Controller
//{
//    private readonly FridgeManagementSystemContext _context;
//    private readonly UserManager<ApplicationUser> _userManager;
//    private readonly INotificationService _notificationService;

//    public AllocationController(FridgeManagementSystemContext context, UserManager<ApplicationUser> userManager,
//                              INotificationService notificationService)

//    {
//        _context = context;
//        _userManager = userManager;
//        _notificationService = notificationService;
//    }

//    // GET: Allocation/Index - Show allocated fridges
//    public async Task<IActionResult> Index()
//    {
//        var allocations = await _context.Allocations
//            .Include(a => a.Customer)
//            .ThenInclude(c => c.User)
//            .Include(a => a.Fridge)
//            .ThenInclude(f => f.FridgeType)
//            .Include(a => a.AllocatedBy)
//            .Where(a => a.IsActive)
//            .OrderByDescending(a => a.AllocationDate)
//            .ToListAsync();

//        return View(allocations);
//    }

//    GET: Allocation/PendingOrders - Show orders waiting for allocation
//    public async Task<IActionResult> PendingOrders()
//    {
//        var pendingOrders = await _context.CustomerOrders
//            .Include(o => o.User)
//            .Include(o => o.OrderItems)
//            .ThenInclude(oi => oi.Fridge)
//            .ThenInclude(f => f.FridgeType)
//            .Where(o => o.Status == "Processing" && o.PaymentStatus == "Paid")
//            .OrderBy(o => o.OrderDate)
//            .ToListAsync();

//        var viewModel = pendingOrders.Select(order => new PendingAllocationViewModel
//        {
//            CustomerOrderId = order.CustomerOrderId,
//            CustomerName = order.User.FullName,
//            CustomerEmail = order.User.Email,
//            CustomerPhone = order.User.PhoneNumber,
//            OrderDate = order.OrderDate,
//            TotalAmount = order.TotalAmount,
//            OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
//            {
//                FridgeId = oi.FridgeId,
//                SerialNumber = oi.Fridge.SerialNumber,
//                Description = oi.Fridge.Description,
//                Price = oi.UnitPrice,
//                FridgeType = $"{oi.Fridge.FridgeType.Brand} {oi.Fridge.FridgeType.Name}",
//                IsAllocated = oi.Fridge.Status == "Allocated"
//            }).ToList()
//        }).ToList();

//        return View(viewModel);
//    }

//    // GET: Allocation/Allocate/{orderId}
//    public async Task<IActionResult> Allocate(int orderId)
//    {
//        var order = await _context.CustomerOrders
//            .Include(o => o.User)
//            .Include(o => o.OrderItems)
//            .ThenInclude(oi => oi.Fridge)
//            .ThenInclude(f => f.FridgeType)
//            .FirstOrDefaultAsync(o => o.CustomerOrderId == orderId);

//        if (order == null)
//        {
//            TempData["Error"] = "Order not found";
//            return RedirectToAction(nameof(PendingOrders));
//        }

//        // Get or create customer
//        var customer = await _context.Customers
//            .FirstOrDefaultAsync(c => c.UserId == order.UserId);

//        if (customer == null)
//        {
//            // Create customer record if it doesn't exist
//            customer = new Customer
//            {
//                UserId = order.UserId,
//                BusinessName = $"{order.User.FullName}'s Business",
//                CustomerType = "Other",
//                IsActive = true,
//                CreatedAt = DateTime.UtcNow,
//                CreatedByFullName = User.Identity.Name,
//                CreatedById = User.FindFirstValue(ClaimTypes.NameIdentifier)
//            };
//            _context.Customers.Add(customer);
//            await _context.SaveChangesAsync();
//        }

//        var availableFridges = order.OrderItems
//            .Where(oi => oi.Fridge.Status == "Available" || oi.Fridge.Status == "Processing")
//            .Select(oi => oi.Fridge)
//            .ToList();

//        if (!availableFridges.Any())
//        {
//            TempData["Error"] = "No available fridges found in this order";
//            return RedirectToAction(nameof(PendingOrders));
//        }

//        ViewBag.OrderId = orderId;
//        ViewBag.CustomerId = customer.Id;
//        ViewBag.CustomerName = order.User.FullName;
//        ViewBag.CustomerBusiness = customer.BusinessName;

//        var fridgeList = availableFridges.Select(f => new SelectListItem
//        {
//            Value = f.FridgeId.ToString(),
//            Text = $"{f.FridgeType.Brand} {f.FridgeType.Name} - {f.SerialNumber} - {f.Price:C}"
//        }).ToList();

//        ViewBag.FridgeList = new SelectList(fridgeList, "Value", "Text");

//        var model = new AllocationViewModel
//        {
//            CustomerId = customer.Id,
//            CustomerName = order.User.FullName,
//            CustomerBusiness = customer.BusinessName
//        };

//        return View(model);
//    }

//    // POST: Allocation/Allocate
//    [HttpPost]
//    [ValidateAntiForgeryToken]
//    public async Task<IActionResult> Allocate(AllocationViewModel model)
//    {
//        if (ModelState.IsValid)
//        {
//            try
//            {
//                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//                var currentUser = await _userManager.FindByIdAsync(currentUserId);

//                // Verify customer exists and is active
//                var customer = await _context.Customers
//                    .Include(c => c.User)
//                    .FirstOrDefaultAsync(c => c.Id == model.CustomerId && c.IsActive);

//                if (customer == null)
//                {
//                    ModelState.AddModelError("CustomerId", "Customer not found or inactive");
//                    return View(model);
//                }

//                // Verify fridge exists and is available
//                var fridge = await _context.Fridges
//                    .Include(f => f.FridgeType)
//                    .FirstOrDefaultAsync(f => f.FridgeId == model.FridgeId &&
//                                            f.IsActive &&
//                                            (f.Status == "Available" || f.Status == "Processing"));

//                if (fridge == null)
//                {
//                    ModelState.AddModelError("FridgeId", "Fridge not found, inactive, or already allocated");
//                    return View(model);
//                }

//                // Create allocation
//                var allocation = new Allocation
//                {
//                    CustomerId = model.CustomerId,
//                    FridgeId = model.FridgeId,
//                    AllocatedById = currentUserId,
//                    AllocationDate = DateTime.Now,
//                    ServiceDate = model.ServiceDate,
//                    IsActive = true
//                };

//                _context.Allocations.Add(allocation);

//                // Update fridge status
//                fridge.Status = "Allocated";
//                fridge.IsAvailable = false;
//                fridge.AllocationDate = DateTime.Now;
//                fridge.CustomerId = model.CustomerId;

//                _context.Fridges.Update(fridge);

//                // Update order status if all items are allocated
//                var order = await _context.CustomerOrders
//                    .Include(o => o.OrderItems)
//                    .FirstOrDefaultAsync(o => o.UserId == customer.UserId &&
//                                            o.Status == "Processing" &&
//                                            o.OrderItems.Any(oi => oi.FridgeId == model.FridgeId));

//                if (order != null)
//                {
//                    var unallocatedItems = order.OrderItems
//                        .Where(oi => oi.Fridge.Status != "Allocated")
//                        .Count();

//                    if (unallocatedItems == 0)
//                    {
//                        order.Status = "Completed";
//                        _context.CustomerOrders.Update(order);
//                    }
//                }

//                await _context.SaveChangesAsync();

//                // Send notifications
//                await _notificationService.NotifyFridgeAllocationAsync(allocation, currentUser.FullName);

//                TempData["Success"] = $"Fridge {fridge.SerialNumber} successfully allocated to {customer.User.FullName}";
//                return RedirectToAction(nameof(Details), new { id = allocation.AllocationId });
//            }
//            catch (Exception ex)
//            {
//                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
//            }
//        }

//        // Reload view data if validation fails
//        await LoadAllocationViewData(model.CustomerId, model.FridgeId);
//        return View(model);
//    }

//    // GET: Allocation/Details/5
//    public async Task<IActionResult> Details(int id)
//    {
//        var allocation = await _context.Allocations
//            .Include(a => a.Customer)
//            .ThenInclude(c => c.User)
//            .Include(a => a.Fridge)
//            .ThenInclude(f => f.FridgeType)
//            .Include(a => a.AllocatedBy)
//            .FirstOrDefaultAsync(a => a.AllocationId == id);

//        if (allocation == null)
//        {
//            TempData["Error"] = "Allocation not found";
//            return RedirectToAction(nameof(Index));
//        }

//        return View(allocation);
//    }

//    // GET: Allocation/Deallocate/5
//    [Authorize(Roles = "Admin")]
//    public async Task<IActionResult> Deallocate(int id)
//    {
//        var allocation = await _context.Allocations
//            .Include(a => a.Customer)
//            .ThenInclude(c => c.User)
//            .Include(a => a.Fridge)
//            .ThenInclude(f => f.FridgeType)
//            .FirstOrDefaultAsync(a => a.AllocationId == id && a.IsActive);

//        if (allocation == null)
//        {
//            TempData["Error"] = "Allocation not found or already deallocated";
//            return RedirectToAction(nameof(Index));
//        }

//        return View(allocation);
//    }

//    // POST: Allocation/Deallocate/5
//    [HttpPost, ActionName("Deallocate")]
//    [ValidateAntiForgeryToken]
//    [Authorize(Roles = "Admin")]
//    public async Task<IActionResult> DeallocateConfirmed(int id)
//    {
//        var allocation = await _context.Allocations
//            .Include(a => a.Fridge)
//            .Include(a => a.Customer)
//            .ThenInclude(c => c.User)
//            .FirstOrDefaultAsync(a => a.AllocationId == id && a.IsActive);

//        if (allocation != null)
//        {
//            // Deactivate allocation
//            allocation.IsActive = false;

//            // Reset fridge status
//            var fridge = allocation.Fridge;
//            fridge.Status = "Available";
//            fridge.IsAvailable = true;
//            fridge.AllocationDate = null;
//            fridge.CustomerId = null;

//            _context.Allocations.Update(allocation);
//            _context.Fridges.Update(fridge);

//            await _context.SaveChangesAsync();

//            // Send notification
//            await _notificationService.CreateNotificationAsync(
//                allocation.Customer.UserId,
//                "Fridge Deallocated",
//                $"Your fridge {fridge.SerialNumber} has been deallocated from your business.",
//                "/Customer/MyFridges"
//            );

//            TempData["Success"] = $"Fridge {fridge.SerialNumber} deallocated from {allocation.Customer.User.FullName}";
//        }

//        return RedirectToAction(nameof(Index));
//    }

//    private async Task LoadAllocationViewData(int customerId, int fridgeId)
//    {
//        var customer = await _context.Customers
//            .Include(c => c.User)
//            .FirstOrDefaultAsync(c => c.Id == customerId);

//        var fridge = await _context.Fridges
//            .Include(f => f.FridgeType)
//            .FirstOrDefaultAsync(f => f.FridgeId == fridgeId);

//        if (customer != null)
//        {
//            ViewBag.CustomerName = customer.User.FullName;
//            ViewBag.CustomerBusiness = customer.BusinessName;
//        }

//        if (fridge != null)
//        {
//            ViewBag.FridgeDescription = fridge.Description;
//            ViewBag.FridgeSerialNumber = fridge.SerialNumber;
//            ViewBag.FridgePrice = fridge.Price;
//        }
//    }
//}