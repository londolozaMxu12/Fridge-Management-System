using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Helpers;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.Repositories;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.IO;


namespace FridgeManagementSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FridgeManagementSystemContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            FridgeManagementSystemContext context,
            ILogger<CustomerController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        private async Task<bool> CheckAndSetAccess()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            var isCustomerLiaison = currentUser?.Employees?.EmployeeType?.Name == "CustomerLiaison";
            var isCustomer = User.IsInRole("Customer");

            ViewBag.IsCustomerLiaison = isCustomerLiaison && !isAdmin;
            ViewBag.UserRole = isAdmin ? "Admin" : (isCustomerLiaison ? "CustomerLiaison" : (isCustomer ? "Customer" : "Unauthorized"));

            return isAdmin || isCustomerLiaison || isCustomer;
        }

        public async Task<IActionResult> MyFridges()
        {
            if (!await CheckAndSetAccess())
                return Forbid();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // FIXED: Use f.CustomerId instead of f.Customer.Id since CustomerId is now a string
            var fridges = await _context.Fridges
                .Include(f => f.Supplier)
                .Include(f => f.Customer) // Include customer to access properties
                .Where(f => f.CustomerId == userId && f.IsActive && f.Status == "Assigned")
                .ToListAsync();

            return View(fridges);
        }

        // GET: Customer/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            if (!await CheckAndSetAccess())
                return Forbid();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var customer = await _context.Customers
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == userId); // FIXED: Now comparing string to string

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> GetCustomerStats()
        {
            var totalCustomers = await _context.Customers
                .Include(c => c.User)
                .Where(c => c.User.ApprovalStatus == "Approved")
                .CountAsync();

            var activeCustomers = await _context.Customers
                .Include(c => c.User)
                .Where(c => c.IsActive && c.User.IsActive && c.User.ApprovalStatus == "Approved")
                .CountAsync();

            var inactiveCustomers = await _context.Customers
                .Include(c => c.User)
                .Where(c => (!c.IsActive || !c.User.IsActive) && c.User.ApprovalStatus == "Approved")
                .CountAsync();

            var pendingApprovals = await _userManager.Users
                .Where(u => u.ApprovalStatus == "Pending" && u.Customers != null)
                .CountAsync();

            var customers = new
            {
                totalCustomers,
                activeCustomers,
                inactiveCustomers,
                pendingApprovals
            };

            return Json(customers);
        }

        public async Task<IActionResult> ListCustomers(string searchString)
        {
            if (!await CheckAndSetAccess())
                return Forbid();

            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            if (!String.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(n =>
                    (n.FullName != null && n.FullName.Contains(searchString)) ||
                    (n.Email != null && n.Email.Contains(searchString))).ToList();
            }
            return View(customers);
        }

        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> CustomerManagement(int pageNumber = 1, int pageSize = 5, string sortBy = "CreatedAt",
    string sortOrder = "desc",
    string searchString = "",
    string customerTypeFilter = "",
    string statusFilter = "active",
    string approvalFilter = "")
        {
            // Initialize ViewBag properties first to ensure they're never null
            ViewBag.CustomerTypes = new List<string>();
            ViewBag.ApprovalStatuses = new List<string> { "Pending", "Approved", "Rejected" };

            try
            {
                // Set ViewBag for layout detection
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    TempData["Error"] = "User not found.";
                    return View(new CustomerManagementViewModel { Customers = new List<CustomerViewModel>() });
                }

                var userWithDetails = await _context.Users
                    .Include(u => u.Employees)
                    .ThenInclude(e => e.EmployeeType)
                    .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

                var isCustomerLiaison = userWithDetails?.Employees?.EmployeeType?.Name == "CustomerLiaison";
                ViewBag.IsCustomerLiaison = isCustomerLiaison && !User.IsInRole("Admin");
                ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : "CustomerLiaison";

                // SIMPLIFIED QUERY APPROACH to avoid SuburbId issue
                var customersQuery = _context.Customers.AsQueryable();
                var usersQuery = _context.Users.AsQueryable();

                // Join manually to avoid the SuburbId issue
                var query = from customer in customersQuery
                            join user in usersQuery on customer.Id equals user.Id
                            select new { customer, user };

                // Apply status filter
                if (statusFilter == "active")
                {
                    query = query.Where(x => x.customer.IsActive && x.user.IsActive);
                }
                else if (statusFilter == "inactive")
                {
                    query = query.Where(x => !x.customer.IsActive || !x.user.IsActive);
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(searchString))
                {
                    query = query.Where(x =>
                        (x.customer.BusinessName != null && x.customer.BusinessName.Contains(searchString)) ||
                        (x.user.FullName != null && x.user.FullName.Contains(searchString)) ||
                        (x.user.Email != null && x.user.Email.Contains(searchString)) ||
                        (x.user.ContactNo != null && x.user.ContactNo.Contains(searchString)) ||
                        (x.customer.CustomerType != null && x.customer.CustomerType.Contains(searchString)));
                }

                // Apply customer type filter
                if (!string.IsNullOrEmpty(customerTypeFilter))
                {
                    query = query.Where(x => x.customer.CustomerType == customerTypeFilter);
                }

                // Apply approval filter
                if (!string.IsNullOrEmpty(approvalFilter))
                {
                    query = query.Where(x => x.user.ApprovalStatus == approvalFilter);
                }

                // Apply sorting
                var orderedQuery = sortBy.ToLower() switch
                {
                    "fullname" => sortOrder == "desc"
                        ? query.OrderByDescending(x => x.user.FullName)
                        : query.OrderBy(x => x.user.FullName),
                    "email" => sortOrder == "desc"
                        ? query.OrderByDescending(x => x.user.Email)
                        : query.OrderBy(x => x.user.Email),
                    "businessname" => sortOrder == "desc"
                        ? query.OrderByDescending(x => x.customer.BusinessName)
                        : query.OrderBy(x => x.customer.BusinessName),
                    "customertype" => sortOrder == "desc"
                        ? query.OrderByDescending(x => x.customer.CustomerType)
                        : query.OrderBy(x => x.customer.CustomerType),
                    "status" => sortOrder == "desc"
                        ? query.OrderByDescending(x => x.customer.IsActive)
                        : query.OrderBy(x => x.customer.IsActive),
                    _ => sortOrder == "desc"
                        ? query.OrderByDescending(x => x.customer.CreatedAt)
                        : query.OrderBy(x => x.customer.CreatedAt)
                };

                // Get total count
                var totalCount = await orderedQuery.CountAsync();

                // Apply pagination
                var results = await orderedQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Convert to ViewModels
                var customerViewModels = results.Select(x => new CustomerViewModel
                {
                    Id = x.customer.Id,
                    FullName = x.user.FullName ?? "N/A",
                    Email = x.user.Email ?? "N/A",
                    ContactNo = x.user.ContactNo ?? "N/A",
                    City = x.user.City ?? "N/A",
                    Suburb = x.user.Suburb ?? "N/A",
                    BusinessName = x.customer.BusinessName ?? "N/A",
                    CustomerType = x.customer.CustomerType ?? "N/A",
                    IsActive = x.customer.IsActive && x.user.IsActive,
                    CreatedAt = x.customer.CreatedAt,
                    ApprovalStatus = x.user.ApprovalStatus ?? "Pending",
                }).ToList();

                // Get customer types for filter dropdown
                var customerTypes = await _context.Customers
                    .Where(c => c.CustomerType != null)
                    .Select(c => c.CustomerType)
                    .Distinct()
                    .OrderBy(ct => ct)
                    .ToListAsync();

                ViewBag.CustomerTypes = customerTypes;

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

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customers for management");
                TempData["Error"] = "An error occurred while loading customers. Please check the database connection and relationships.";

                return View(new CustomerManagementViewModel
                {
                    Customers = new List<CustomerViewModel>(),
                    PageNumber = 1,
                    PageSize = pageSize,
                    TotalCount = 0
                });
            }
        }

        [Authorize(Policy = "CustomerLiaisonAccess")]
        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> CustomerDetails(string id)
        {
            // Set ViewBag for layout detection 
            var currentUser = await _userManager.GetUserAsync(User);
            var userWithDetails = await _context.Users
                .Include(u => u.Employees)
                .ThenInclude(e => e.EmployeeType)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

            var isCustomerLiaison = userWithDetails?.Employees?.EmployeeType?.Name == "CustomerLiaison";
            ViewBag.IsCustomerLiaison = isCustomerLiaison && !User.IsInRole("Admin");
            ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : "CustomerLiaison";

            try
            {
                // Load customer with essential data
                var customer = await _context.Customers
                    .Include(c => c.User)
                        .ThenInclude(u => u.ApprovedBy)
                    .Include(c => c.CreatedBy)
                    .Include(c => c.Fridges)
                        .ThenInclude(f => f.FridgeType)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(CustomerManagement));
                }

                // Load counts separately to avoid complex queries
                ViewBag.FridgeCount = await _context.Fridges.CountAsync(f => f.CustomerId == id);
                ViewBag.FaultCount = await _context.Faults.CountAsync(f => f.ReportedById == id);
                
                ViewBag.AllocationCount = await _context.Allocations.CountAsync(a => a.CustomerId == id);

                // Order counts
                var orders = await _context.Orders.Where(o => o.CustomerId == id).ToListAsync();
                var completedStatuses = new[] { "Sipped", "Cancelled", "Delivered" };

                ViewBag.ActiveOrdersCount = orders.Count(o => !completedStatuses.Contains(o.OrderStatus));
                ViewBag.TotalOrdersCount = orders.Count;
                ViewBag.CompletedOrdersCount = orders.Count(o => completedStatuses.Contains(o.OrderStatus));
                ViewBag.PendingOrdersCount = orders.Count(o => o.OrderStatus == "Received");
                ViewBag.ProcessingOrdersCount = orders.Count(o => o.OrderStatus == "Processing");

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading customer details for ID: {CustomerId}", id);
                TempData["Error"] = "An error occurred while loading customer details.";
                return RedirectToAction(nameof(CustomerManagement));
            }
        }

        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> EditCustomer(string id)
        {
            // Set ViewBag for layout detection
            var currentUser = await _userManager.GetUserAsync(User);
            var userWithDetails = await _context.Users
                .Include(u => u.Employees)
                .ThenInclude(e => e.EmployeeType)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

            var isCustomerLiaison = userWithDetails?.Employees?.EmployeeType?.Name == "CustomerLiaison";
            ViewBag.IsCustomerLiaison = isCustomerLiaison && !User.IsInRole("Admin");
            ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : "CustomerLiaison";

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
        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> EditCustomer(string id, EditCustomerViewModel model)
        {
            // Set ViewBag for layout detection
            var currentUser = await _userManager.GetUserAsync(User);
            var userWithDetails = await _context.Users
                .Include(u => u.Employees)
                .ThenInclude(e => e.EmployeeType)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

            var isCustomerLiaison = userWithDetails?.Employees?.EmployeeType?.Name == "CustomerLiaison";
            ViewBag.IsCustomerLiaison = isCustomerLiaison && !User.IsInRole("Admin");
            ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : "CustomerLiaison";

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
        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> DeactivateCustomer(string id)
        {
            // Set ViewBag for layout detection
            var currentUser = await _userManager.GetUserAsync(User);
            var userWithDetails = await _context.Users
                .Include(u => u.Employees)
                .ThenInclude(e => e.EmployeeType)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

            var isCustomerLiaison = userWithDetails?.Employees?.EmployeeType?.Name == "CustomerLiaison";
            ViewBag.IsCustomerLiaison = isCustomerLiaison && !User.IsInRole("Admin");
            ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : "CustomerLiaison";

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
        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> ActivateCustomer(string id)
        {
            // Set ViewBag for layout detection
            var currentUser = await _userManager.GetUserAsync(User);
            var userWithDetails = await _context.Users
                .Include(u => u.Employees)
                .ThenInclude(e => e.EmployeeType)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

            var isCustomerLiaison = userWithDetails?.Employees?.EmployeeType?.Name == "CustomerLiaison";
            ViewBag.IsCustomerLiaison = isCustomerLiaison && !User.IsInRole("Admin");
            ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : "CustomerLiaison";

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

        // GET: Customer/ActiveCustomers
        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> ActiveCustomers(int pageNumber = 1, int pageSize = 10, string sortBy = "FullName",
            string sortOrder = "asc", string searchString = "", string customerTypeFilter = "")
        {
            // Set ViewBag for layout detection
            var currentUser = await _userManager.GetUserAsync(User);
            var userWithDetails = await _context.Users
                .Include(u => u.Employees)
                .ThenInclude(e => e.EmployeeType)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

            ViewBag.IsCustomerLiaison = userWithDetails?.Employees?.EmployeeType?.Name == "CustomerLiaison" && !User.IsInRole("Admin");
            ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : "CustomerLiaison";

            // Call existing CustomerManagement with active filter
            return await CustomerManagement(pageNumber, pageSize, sortBy, sortOrder, searchString,
                customerTypeFilter, "active", "");
        }

        // GET: Customer/InactiveCustomers
        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> InactiveCustomers(int pageNumber = 1, int pageSize = 10, string sortBy = "FullName",
            string sortOrder = "asc", string searchString = "", string customerTypeFilter = "", string approvalFilter = "")
        {
            // Set ViewBag for layout detection
            var currentUser = await _userManager.GetUserAsync(User);
            var userWithDetails = await _context.Users
                .Include(u => u.Employees)
                .ThenInclude(e => e.EmployeeType)
                .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

            ViewBag.IsCustomerLiaison = userWithDetails?.Employees?.EmployeeType?.Name == "CustomerLiaison" && !User.IsInRole("Admin");
            ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : "CustomerLiaison";

            // Call existing CustomerManagement with inactive filter
            return await CustomerManagement(pageNumber, pageSize, sortBy, sortOrder, searchString,
                customerTypeFilter, "inactive", approvalFilter);
        }

        public IActionResult AllocatedFridge()
        {
            return View();
        }

        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> ExportAllCustomers()
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .OrderBy(c => c.User.FullName)
                .ToListAsync();

            return GenerateCustomerCsv(customers, "All_Customers.csv");
        }

        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> ExportActiveCustomers()
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .Where(c => c.IsActive && c.User.IsActive)
                .OrderBy(c => c.User.FullName)
                .ToListAsync();

            return GenerateCustomerCsv(customers, "Active_Customers.csv");
        }

        [Authorize(Policy = "CustomerLiaisonAccess")]
        public async Task<IActionResult> ExportInactiveCustomers()
        {
            var customers = await _context.Customers
                .Include(c => c.User)
                .Where(c => !c.IsActive || !c.User.IsActive)
                .OrderBy(c => c.User.FullName)
                .ToListAsync();

            return GenerateCustomerCsv(customers, "Inactive_Customers.csv");
        }

        private FileContentResult GenerateCustomerCsv(IEnumerable<Customer> customers, string fileName)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Full Name,Email,Contact No,Business Name,Customer Type,City,Suburb,Active,Approval Status,Created At");

            foreach (var c in customers)
            {
                csv.AppendLine($"\"{c.User?.FullName}\",\"{c.User?.Email}\",\"{c.User?.ContactNo}\",\"{c.BusinessName}\",\"{c.CustomerType}\",\"{c.User?.City}\",\"{c.User?.Suburb}\",\"{(c.IsActive && c.User?.IsActive == true ? "Active" : "Inactive")}\",\"{c.User?.ApprovalStatus}\",\"{c.CreatedAt:yyyy-MM-dd}\"");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }
    }
}