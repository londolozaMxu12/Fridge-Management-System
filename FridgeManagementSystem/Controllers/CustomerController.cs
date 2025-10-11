using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.Repositories;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FridgeManagementSystemContext _context;
        private readonly ILogger<CustomerController> _logger;
        private readonly IHomeRepository _homeRepository;

        public CustomerController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager, FridgeManagementSystemContext context, IHomeRepository homeRepository, ILogger<CustomerController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _homeRepository = homeRepository;
        }
        public IActionResult index()
        {
            return View();
        }

        public async Task<IActionResult> Home(string searchTerm = "", int fridgeTypeId = 0)
        {
            IEnumerable<Fridge> fridges = await _homeRepository.GetFridges(searchTerm, fridgeTypeId);
            IEnumerable<FridgeType> fridgeTypes = await _homeRepository.FridgeTypes();
            FridgeDisplayModel FridgeModel = new FridgeDisplayModel
            {
                Fridges = fridges,
                FridgeTypes = fridgeTypes,
                searchTerm = searchTerm,
                FridgeTypeId = fridgeTypeId
            };

            return View(FridgeModel);

        }

        public async Task<IActionResult> MyFridges()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var fridges = await _context.Fridges
                //.Include(f => f.Location)
                .Include(f => f.Supplier)
                .Where(f => f.Customer.UserId == userId && f.IsActive && f.Status == "Assigned")
                .ToListAsync();

            return View(fridges);
        }
        // GET: Customer/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var customer = await _context.Customers
                .Include(c => c.User)
                //.Include(c => c.Location)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }
        public async Task<IActionResult> ListCustomers(string searchString)
        {

            //var user = userManager.Users.ToList();
            var customer = await _userManager.GetUsersInRoleAsync("Customer");
            if (!String.IsNullOrEmpty(searchString))
            {
                customer = customer.Where(n => n.FullName.Contains(searchString)
                || n.Email.Contains(searchString)).ToList();
            }
            return View(customer);
        }

        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> CustomerManagement(int pageNumber = 1, int pageSize = 5, string sortBy = "CreatedAt",
           string sortOrder = "desc",
           string searchString = "",
           string customerTypeFilter = "",
           string statusFilter = "active",
           string approvalFilter = "")
        {
            try
            {
                // Build base query with includes
                var query = _context.Customers
                    .Include(c => c.User)
                    .AsQueryable();

                // Apply status filter
                if (statusFilter == "active")
                {
                    query = query.Where(c => c.IsActive && c.User.IsActive);
                }
                else if (statusFilter == "inactive")
                {
                    query = query.Where(c => !c.IsActive || !c.User.IsActive);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(c =>
                        c.BusinessName.Contains(searchString) ||
                        c.User.FullName.Contains(searchString) ||
                        c.User.Email.Contains(searchString) ||
                        c.User.ContactNo.Contains(searchString) ||
                        c.CustomerType.Contains(searchString));
                }

                // Apply customer type filter
                if (!string.IsNullOrEmpty(customerTypeFilter))
                {
                    query = query.Where(c => c.CustomerType == customerTypeFilter);
                }

                // Apply approval filter
                if (!string.IsNullOrEmpty(approvalFilter))
                {
                    query = query.Where(c => c.User.ApprovalStatus == approvalFilter);
                }

                // Apply sorting
                query = sortBy.ToLower() switch
                {
                    "fullname" => sortOrder == "desc"
                        ? query.OrderByDescending(c => c.User.FullName)
                        : query.OrderBy(c => c.User.FullName),
                    "email" => sortOrder == "desc"
                        ? query.OrderByDescending(c => c.User.Email)
                        : query.OrderBy(c => c.User.Email),
                    "businessname" => sortOrder == "desc"
                        ? query.OrderByDescending(c => c.BusinessName)
                        : query.OrderBy(c => c.BusinessName),
                    "customertype" => sortOrder == "desc"
                        ? query.OrderByDescending(c => c.CustomerType)
                        : query.OrderBy(c => c.CustomerType),
                    "status" => sortOrder == "desc"
                        ? query.OrderByDescending(c => c.IsActive)
                        : query.OrderBy(c => c.IsActive),
                    _ => sortOrder == "desc"
                        ? query.OrderByDescending(c => c.CreatedAt)
                        : query.OrderBy(c => c.CreatedAt)
                };

                // Get total count after all filtering
                var totalCount = await query.CountAsync();

                // Apply pagination
                var customers = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Convert to ViewModels
                var customerViewModels = customers.Select(c => new CustomerViewModel
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    FullName = c.User.FullName,
                    Email = c.User.Email,
                    ContactNo = c.User.ContactNo,
                    City = c.User.City,
                    Suburb = c.User.Suburb,
                    BusinessName = c.BusinessName,
                    CustomerType = c.CustomerType,
                    IsActive = c.IsActive && c.User.IsActive,
                    CreatedAt = c.CreatedAt,
                    ApprovalStatus = c.User.ApprovalStatus,
                }).ToList();

                // Get customer types for filter dropdown
                var customerTypes = await _context.Customers
                    .Select(c => c.CustomerType)
                    .Distinct()
                    .OrderBy(ct => ct)
                    .ToListAsync();

                // Get approval statuses for filter dropdown
                var approvalStatuses = new List<string> { "Pending", "Approved", "Rejected" };

                var viewModel = new CustomerManagementViewModel
                {
                    Customers = customerViewModels,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    SearchString = searchString,
                    CustomerTypeFilter = customerTypeFilter,
                    StatusFilter = statusFilter,
                    ApprovalFilter = approvalFilter
                };

                ViewBag.CustomerTypes = customerTypes;
                ViewBag.ApprovalStatuses = approvalStatuses;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customers for management");
                TempData["Error"] = "An error occurred while loading customers.";
                return View(new CustomerManagementViewModel { Customers = new List<CustomerViewModel>() });
            }
        }
        // GET: Admin/CustomerDetails/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CustomerDetails(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .Include(c => c.CreatedBy)
                    .Include(c => c.Fridges)
                    .Include(c => c.FridgeRequests)
                    .Include(c => c.Faults)
                    .Include(c => c.Quotations)
                    .Include(c => c.Allocations)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(CustomerManagement));
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer details for ID: {CustomerId}", id);
                TempData["Error"] = "An error occurred while loading customer details.";
                return RedirectToAction(nameof(CustomerManagement));
            }
        }
        [Authorize(Roles = "Admin")]
public async Task<IActionResult> EditCustomer(int id)
{
    try
    {
        var customer = await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            TempData["Error"] = "Customer not found.";
            return RedirectToAction(nameof(CustomerManagement));
        }

        var model = new EditCustomerViewModel
        {
            Id = customer.Id,
            UserId = customer.UserId,
            FullName = customer.User.FullName,
            Email = customer.User.Email,
            ContactNo = customer.User.ContactNo,
            Address = customer.User.Address,
            City = customer.User.City,
            Suburb = customer.User.Suburb,
            PostalCode = customer.User.PostalCode,
            BusinessName = customer.BusinessName,
            CustomerType = customer.CustomerType,
            IsActive = customer.IsActive && customer.User.IsActive,
            ApprovalStatus = customer.User.ApprovalStatus
        };

        ViewBag.CustomerTypes = new List<SelectListItem>
        {
            new SelectListItem { Value = "SpazaShop", Text = "Spaza Shop" },
            new SelectListItem { Value = "Liquor", Text = "Liquor" },
            new SelectListItem { Value = "Other", Text = "Other" }
        };

        ViewBag.ApprovalStatuses = new List<SelectListItem>
        {
            new SelectListItem { Value = "Pending", Text = "Pending" },
            new SelectListItem { Value = "Approved", Text = "Approved" },
            new SelectListItem { Value = "Rejected", Text = "Rejected" }
        };

        return View(model);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading customer for edit. CustomerId: {CustomerId}", id);
        TempData["Error"] = "An error occurred while loading the customer for editing.";
        return RedirectToAction(nameof(CustomerManagement));
    }
}

// POST: Admin/EditCustomer/5
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> EditCustomer(int id, EditCustomerViewModel model)
{
    if (id != model.Id)
    {
        TempData["Error"] = "Invalid customer ID.";
        return RedirectToAction(nameof(CustomerManagement));
    }

    if (ModelState.IsValid)
    {
        try
        {
            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction(nameof(CustomerManagement));
            }

            // Update ApplicationUser properties
            customer.User.FullName = model.FullName;
            customer.User.Email = model.Email;
            customer.User.ContactNo = model.ContactNo;
            customer.User.Address = model.Address;
            customer.User.City = model.City;
            customer.User.Suburb = model.Suburb;
            customer.User.PostalCode = model.PostalCode;
            customer.User.ApprovalStatus = model.ApprovalStatus;

            // Update Customer properties
            customer.BusinessName = model.BusinessName;
            customer.CustomerType = model.CustomerType;

            // Update active status for both customer and user
            customer.IsActive = model.IsActive;
            customer.User.IsActive = model.IsActive;

            // If approving the customer, set approved by and timestamp
            if (model.ApprovalStatus == "Approved" && customer.User.ApprovalStatus != "Approved")
            {
                customer.User.ApprovedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                customer.User.ApprovedAt = DateTime.UtcNow;
            }

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer updated successfully!";
            return RedirectToAction(nameof(CustomerManagement));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer. CustomerId: {CustomerId}", id);
            TempData["Error"] = "An error occurred while updating the customer.";
        }
    }

    // If we got this far, something failed; redisplay form
    ViewBag.CustomerTypes = new List<SelectListItem>
    {
        new SelectListItem { Value = "SpazaShop", Text = "Spaza Shop" },
        new SelectListItem { Value = "Liquor", Text = "Liquor" },
        new SelectListItem { Value = "Other", Text = "Other" }
    };

    ViewBag.ApprovalStatuses = new List<SelectListItem>
    {
        new SelectListItem { Value = "Pending", Text = "Pending" },
        new SelectListItem { Value = "Approved", Text = "Approved" },
        new SelectListItem { Value = "Rejected", Text = "Rejected" }
    };

    return View(model);
}

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(CustomerManagement));
                }

                customer.IsActive = false;
                customer.User.IsActive = false;

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Customer {customer.User.FullName} has been deactivated successfully.";
                return RedirectToAction(nameof(CustomerManagement));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating customer. CustomerId: {CustomerId}", id);
                TempData["Error"] = "An error occurred while deactivating the customer.";
                return RedirectToAction(nameof(CustomerManagement));
            }
        }

        // POST: Admin/ActivateCustomer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateCustomer(int id)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(CustomerManagement));
                }

                customer.IsActive = true;
                customer.User.IsActive = true;

                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Customer {customer.User.FullName} has been activated successfully.";
                return RedirectToAction(nameof(CustomerManagement));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating customer. CustomerId: {CustomerId}", id);
                TempData["Error"] = "An error occurred while activating the customer.";
                return RedirectToAction(nameof(CustomerManagement));
            }
        }
        public IActionResult AllocatedFridge()
        {
            return View();
        }


        //public async Task<IActionResult> ListUsers()
        //{
        //    var users = userManager.Users.ToList();
        //    var userRoles = new List<object>();
        //    foreach (var user in users)
        //    {
        //        var roles = await userManager.GetRolesAsync(user);
        //        userRoles.Add(new
        //        {
        //            user.UserName,
        //            user.Email,
        //            Roles = roles
        //        });
        //    }

        //    return Json(userRoles);
        //public CustomerController(FridgeManagementSystemContext db) { _db = db; }

        //public async Task<IActionResult> Index() => View(await _db.Customers.Include(c => c.Allocations).ToListAsync());
        ////public IActionResult Index()
        ////{
        ////    return View();
        ////}
        //public IActionResult Create() => View();

        //[HttpPost]
        //public async Task<IActionResult> Create(Customer customer)
        //{
        //    if (!ModelState.IsValid) return View(customer);
        //    _db.Add(customer);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var customer = await _db.Customers.FindAsync(id);
        //    if (customer == null) return NotFound();
        //    return View(customer);
        //}

        //[HttpPost]
        //public async Task<IActionResult> Edit(Customer customer)
        //{
        //    if (!ModelState.IsValid) return View(customer);
        //    _db.Update(customer);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var customer = await _db.Customers.FindAsync(id);
        //    if (customer == null) return NotFound();
        //    _db.Customers.Remove(customer);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        //public async Task<IActionResult> Details(int id)
        //{
        //    var customer = await _db.Customers
        //        .Include(c => c.Allocations)
        //        .ThenInclude(a => a.Fridge)
        //        .FirstOrDefaultAsync(c => c.CustomerId == id);
        //    if (customer == null) return NotFound();
        //    return View(customer);
        //}
    }
}
