using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FridgeManagementSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FridgeManagementSystemContext _context;
        private readonly ILogger<AdminController> _logger;

        public UserController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager, FridgeManagementSystemContext context, ILogger<AdminController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5, string sortBy = "CreatedAt",
           string sortOrder = "desc",
           string searchString = "",
           string roleFilter = "",
           string statusFilter = "",
           string approvalFilter = "")
        {
            try
            {
                // Get all users with their roles efficiently using a join
                var userQuery = from user in _userManager.Users.Include(u => u.ApprovedBy)
                                join userRole in _context.UserRoles on user.Id equals userRole.UserId into userRoles
                                from ur in userRoles.DefaultIfEmpty()
                                join role in _context.Roles on ur.RoleId equals role.Id into roles
                                from r in roles.DefaultIfEmpty()
                                select new
                                {
                                    User = user,
                                    RoleName = r.Name
                                };

                // Apply search filter
                if (!string.IsNullOrEmpty(searchString))
                {
                    userQuery = userQuery.Where(x =>
                        x.User.FullName.Contains(searchString) ||
                        x.User.Email.Contains(searchString) ||
                        x.User.UserName.Contains(searchString) ||
                        x.User.ContactNo.Contains(searchString) ||
                        x.User.City.Contains(searchString) ||
                        x.User.Suburb.Contains(searchString));
                }

                // Apply status filter
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    if (statusFilter == "active")
                        userQuery = userQuery.Where(x => x.User.IsActive);
                    else if (statusFilter == "inactive")
                        userQuery = userQuery.Where(x => !x.User.IsActive);
                }

                // Apply approval filter
                if (!string.IsNullOrEmpty(approvalFilter))
                {
                    userQuery = userQuery.Where(x => x.User.ApprovalStatus == approvalFilter);
                }

                // Apply role filter
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    userQuery = userQuery.Where(x => x.RoleName == roleFilter);
                }

                // Get distinct users after all filtering
                var filteredUsersQuery = userQuery
                    .Select(x => x.User)
                    .Distinct();

                // Apply sorting
                var sortedUsersQuery = sortBy.ToLower() switch
                {
                    "fullname" => sortOrder == "desc"
                        ? filteredUsersQuery.OrderByDescending(u => u.FullName)
                        : filteredUsersQuery.OrderBy(u => u.FullName),
                    "email" => sortOrder == "desc"
                        ? filteredUsersQuery.OrderByDescending(u => u.Email)
                        : filteredUsersQuery.OrderBy(u => u.Email),

                    _ => sortOrder == "desc"
                        ? filteredUsersQuery.OrderByDescending(u => u.CreatedAt)
                        : filteredUsersQuery.OrderBy(u => u.CreatedAt)
                };

                // Get total count after all filtering
                var totalCount = await sortedUsersQuery.CountAsync();

                // Apply pagination
                var pagedUsers = await sortedUsersQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Convert to ViewModels and get roles for each user
                var userViewModels = new List<UserViewModel>();
                var allRoles = await _context.Roles.Select(r => r.Name).ToListAsync();

                foreach (var user in pagedUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    var userViewModel = new UserViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        ContactNo = user.ContactNo,
                        Address = user.Address,
                        City = user.City,
                        Suburb = user.Suburb,
                        PostalCode = user.PostalCode,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt,
                        ApprovalStatus = user.ApprovalStatus,
                        ApprovedById = user.ApprovedById,
                        ApprovedBy = user.ApprovedBy?.FullName,
                        ApprovedAt = user.ApprovedAt,
                        Roles = roles.ToList(),
                        RoleNames = string.Join(", ", roles)
                    };

                    userViewModels.Add(userViewModel);
                }

                var viewModel = new UserManagementViewModel
                {
                    Users = userViewModels,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    SearchString = searchString,
                    RoleFilter = roleFilter,
                    StatusFilter = statusFilter,
                    ApprovalFilter = approvalFilter
                };

                ViewBag.Roles = allRoles;
                ViewBag.Statuses = new List<string> { "active", "inactive" };
                ViewBag.ApprovalStatuses = new List<string> { "Pending", "Approved", "Rejected" };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["Error"] = "An error occurred while loading users.";
                return View(new UserManagementViewModel { Users = new List<UserViewModel>() });
            }
        }
        // GET: User/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManager.Users
                    .Include(u => u.ApprovedBy)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);

                var viewModel = new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    ContactNo = user.ContactNo,
                    Address = user.Address,
                    City = user.City,
                    Suburb = user.Suburb,
                    PostalCode = user.PostalCode,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    ApprovalStatus = user.ApprovalStatus,
                    ApprovedById = user.ApprovedById,
                    ApprovedBy = user.ApprovedBy?.FullName,
                    ApprovedAt = user.ApprovedAt,
                    Roles = roles.ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for {UserId}", id);
                TempData["Error"] = "An error occurred while loading user details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManager.Users
                    .Include(u => u.ApprovedBy)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                var viewModel = new UserEditViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    ContactNo = user.ContactNo,
                    Address = user.Address,
                    City = user.City,
                    Suburb = user.Suburb,
                    PostalCode = user.PostalCode,
                    IsActive = user.IsActive,
                    ApprovalStatus = user.ApprovalStatus
                };

                ViewData["UserId"] = user.Id;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user for edit {UserId}", id);
                TempData["Error"] = "An error occurred while loading user for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    // Update user properties
                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.FullName = model.FullName;
                    user.ContactNo = model.ContactNo;
                    user.Address = model.Address;
                    user.City = model.City;
                    user.Suburb = model.Suburb;
                    user.PostalCode = model.PostalCode;
                    user.IsActive = model.IsActive;
                    user.ApprovalStatus = model.ApprovalStatus;

                    if (model.ApprovalStatus == "Approved" && user.ApprovalStatus != "Approved")
                    {
                        user.ApprovedById = _userManager.GetUserId(User);
                        user.ApprovedAt = DateTime.UtcNow;
                    }

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["Success"] = "User updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user {UserId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the user.");
                }
            }

            return View(model);
        }

        // POST: User/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.IsActive = false;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["Success"] = "User deactivated successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to deactivate user.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", id);
                TempData["Error"] = "An error occurred while deactivating the user.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.IsActive = true;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    TempData["Success"] = "User activated successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to activate user.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}", id);
                TempData["Error"] = "An error occurred while activating the user.";
            }

            return RedirectToAction(nameof(Index));
        }

    }
}
