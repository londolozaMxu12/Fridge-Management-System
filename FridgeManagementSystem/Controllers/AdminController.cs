using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using FridgeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace FridgeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly FridgeManagementSystemContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager, FridgeManagementSystemContext context, ILogger<AdminController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // GET: Admin/PendingApprovals
        public async Task<IActionResult> PendingApprovals()
        {
            var pendingUsers = await _userManager.Users
                .Where(u => u.ApprovalStatus == "Pending" && u.Customers != null)
                .Include(u => u.Customers)
                .ToListAsync();


            return View(pendingUsers);
        }

        // GET: Admin/ApproveCustomer/5
        public async Task<IActionResult> ApproveCustomer(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .Include(u => u.Customers)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            if (user.ApprovalStatus != "Pending")
            {
                TempData["ErrorMessage"] = "This customer has already been processed.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            return View(user);
        }

        // POST: Admin/ApproveCustomer/5
        [HttpPost, ActionName("ApproveCustomer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCustomerConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.ApprovalStatus != "Pending")
            {
                TempData["ErrorMessage"] = "This customer has already been processed.";
                return RedirectToAction(nameof(PendingApprovals));
            }


            user.ApprovalStatus = "Approved";
            user.ApprovedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
            user.ApprovedAt = DateTime.UtcNow;
            user.IsActive = true;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Create notification for the customer
                var notification = new Notification
                {
                    UserId = user.Id,
                    Title = "Account Approved",
                    Message = "Your account has been approved. You can now access all the features.",
                    Link = $"/Customers/Index/{user.Customers?.Id}"
                };

                _context.Notifications.Add(notification);

                // Create notification for customer liaisons
                var liaisons = await _userManager.GetUsersInRoleAsync("CustomerLiaison");
                foreach (var liaison in liaisons.Where(l => l.IsActive))
                {
                    var liaisonNotification = new Notification
                    {
                        UserId = liaison.Id,
                        Title = "New Customer Approved",
                        Message = $"Customer {user.FullName} ({user.Customers?.BusinessName}) has been approved and is now active in the system.",
                        Link = $"/Customers/Details/{user.Customers?.Id}"
                    };
                    _context.Notifications.Add(liaisonNotification);
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Customer {user.FullName} has been approved successfully.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

        // POST: Admin/RejectCustomer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCustomer(string id, string reason)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.ApprovalStatus != "Pending")
            {
                TempData["ErrorMessage"] = "This customer has already been processed.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            user.ApprovalStatus = "Rejected";
            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // notification for the customer
                var notification = new Notification
                {
                    UserId = user.Id,
                    Title = "Account Registration Rejected",
                    Message = $"Dear {user.FullName}, your registration has been reviewed. Unfortunately, we cannot approve your account. Reason: {reason}",
                    Link = "/"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Customer {user.FullName} has been rejected.";
                return RedirectToAction(nameof(PendingApprovals));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("ApproveCustomer", user);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EmployeeManagement(int pageNumber = 1, int pageSize = 5, string sortBy = "EmployeeNo",
            string sortOrder = "asc",
            string searchString = "",
            string employeeTypeFilter = "",
            string statusFilter = "active")
        {
            var query = _userManager.Users
                .Where(u => u.Employees != null)
                .Include(u => u.Employees)
                .ThenInclude(e => e.EmployeeType)
                .AsQueryable();

            // Apply status filter
            if (statusFilter == "active")
            {
                query = query.Where(u => u.IsActive);
            }
            else if (statusFilter == "inactive")
            {
                query = query.Where(u => !u.IsActive);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(u =>
                    u.FullName.Contains(searchString) ||
                    u.Employees.EmployeeNo.Contains(searchString) ||
                    u.Email.Contains(searchString) ||
                    u.ContactNo.Contains(searchString) ||
                    u.Employees.JobTitle.Contains(searchString) ||
                    u.Employees.EmployeeType.Name.Contains(searchString));
            }

            // Apply employee type filter
            if (!string.IsNullOrEmpty(employeeTypeFilter))
            {
                query = query.Where(u => u.Employees.EmployeeType.Name == employeeTypeFilter);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "name" => sortOrder == "desc" ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
                "email" => sortOrder == "desc" ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "contactno" => sortOrder == "desc" ? query.OrderByDescending(u => u.ContactNo) : query.OrderBy(u => u.ContactNo),
                "employeetype" => sortOrder == "desc" ? query.OrderByDescending(u => u.Employees.EmployeeType.Name) : query.OrderBy(u => u.Employees.EmployeeType.Name),
                "status" => sortOrder == "desc" ? query.OrderByDescending(u => u.ApprovalStatus) : query.OrderBy(u => u.ApprovalStatus),
                _ => sortOrder == "desc" ? query.OrderByDescending(u => u.Employees.EmployeeNo) : query.OrderBy(u => u.Employees.EmployeeNo)
            };

            var totalCount = await query.CountAsync();

            // Get employee types for filter dropdown
            var employeeTypes = await _context.EmployeeTypes
                .Where(et => et.IsActive)
                .Select(et => et.Name)
                .Distinct()
                .ToListAsync();

            // Apply pagination
            var employees = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new EmployeeManagementViewModel
            {
                Employees = employees,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                SortBy = sortBy,
                SortOrder = sortOrder,
                SearchString = searchString,
                EmployeeTypeFilter = employeeTypeFilter,
                StatusFilter = statusFilter
            };

            ViewBag.EmployeeTypes = employeeTypes;
            return View(viewModel);
        }

        // GET: Admin/EmployeeDetails/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EmployeeDetails(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _userManager.Users
                .Include(u => u.Employees)
                .ThenInclude(e => e.EmployeeType)
                .Include(u => u.ApprovedBy)
                .FirstOrDefaultAsync(u => u.Id == id && u.Employees != null);

            if (employee == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(employee);
            ViewBag.UserRoles = userRoles;

            return View(employee);
        }

        // GET: Admin/EditEmployee/5
        //[Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditEmployee(string id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Admin");

            }

            var employee = await _userManager.Users
                .Include(u => u.Employees)
                .FirstOrDefaultAsync(u => u.Id == id && u.Employees != null);

            if (employee == null)
            {
                return RedirectToAction("Index", "Admin");
            }

            //var roles = await _userManager.GetRolesAsync(employee);

            var model = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Email = employee.Email,
                FullName = employee.FullName,
                ContactNo = employee.ContactNo,
                Address = employee.Address,
                City = employee.City,
                Suburb = employee.Suburb,
                PostalCode = employee.PostalCode,
                EmployeeNo = employee.Employees.EmployeeNo,
                EmployeeTypeId = employee.Employees.EmployeeTypeId,
                JobTitle = employee.Employees.JobTitle,
                //SelectedRole = selectedRoles.ToList(),
                DateEmployed = employee.Employees.DateEmployed,
                //EmployeeRole = roles.FirstOrDefault(),
                
                IsActive = employee.IsActive,
                
            };

            ViewBag.EmployeeTypes = await _context.EmployeeTypes
                .Where(et => et.IsActive)
                .Select(et => new SelectListItem
                {
                    Value = et.Id.ToString(),
                    Text = et.Name
                })
                .ToListAsync();

            ViewData["EmployeeId"] = employee.Id;
            ViewData["EmployeeNo"] = employee.Employees.EmployeeNo;
            
            ViewData["IsActiveStatus"] = (employee.IsActive) ? "Active" : "Inactive";
            ViewData["CreatedAt"] = employee.CreatedAt.ToString("MM/dd/yyyy");

            var currentRoles = await _userManager.GetRolesAsync(employee);
            ViewBag.CurrentRole = currentRoles.FirstOrDefault();
            ViewBag.Roles = await _context.Roles
                .Where(r => r.Name != "Customer")
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToListAsync();

            return View(model);
        }

        // POST: Admin/EditEmployee/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditEmployee(string id, EmployeeEditViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Index", "Admin");
            }

            if (ModelState.IsValid)
            {
                var employee = await _userManager.Users
                    .Include(u => u.Employees)
                    .FirstOrDefaultAsync(u => u.Id == id && u.Employees != null);

                if (employee == null)
                {
                    return RedirectToAction("Index", "Admin"); 
                }

                //Check if user is a Customer (prevent editing)

                var currentRoles = await _userManager.GetRolesAsync(employee);
                if (currentRoles.Contains("Customer"))
                {
                    return RedirectToAction("Index", "EmployeeManagement");
                }

                // Update ApplicationUser properties
                employee.Email = model.Email;
                employee.FullName = model.FullName;
                employee.ContactNo = model.ContactNo;
                employee.Address = model.Address;
                employee.City = model.City;
                employee.Suburb = model.Suburb;
                employee.PostalCode = model.PostalCode;

                var userResult = await _userManager.UpdateAsync(employee);
                if (userResult.Succeeded)
                {
                    // Update Employee properties
                    //employee.Employees.EmployeeNo = model.EmployeeNo;
                    employee.Employees.EmployeeTypeId = model.EmployeeTypeId;
                    employee.Employees.JobTitle = model.JobTitle;
                    
                    employee.Employees.DateEmployed = model.DateEmployed;
                    

                    _context.Employees.Update(employee.Employees);

                    // Remove existing roles and add new one (excluding Customer role)
                    await _userManager.RemoveFromRolesAsync(employee, currentRoles);

                    //Update role if changed
                    if (!string.IsNullOrEmpty(model.EmployeeRole) && model.EmployeeRole != "Customer")
                    {
                       //var currentRoles = await _userManager.GetRolesAsync(employee);
                       await _userManager.RemoveFromRolesAsync(employee, currentRoles);
                       await _userManager.AddToRoleAsync(employee, model.EmployeeRole);
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Employee updated successfully.";
                    return RedirectToAction(nameof(EmployeeManagement));
                }

                foreach (var error in userResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.EmployeeTypes = await _context.EmployeeTypes
                .Where(et => et.IsActive)
                .Select(et => new SelectListItem
                {
                    Value = et.Id.ToString(),
                    Text = et.Name
                })
                .ToListAsync();

            ViewBag.Roles = _roleManager.Roles.Where(r => r.Name != "Customer")
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToListAsync();

            return View(model);
        }

        // GET: Admin/DeactivateEmployee/5
        //[Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeactivateEmployee(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _userManager.Users
                .Include(u => u.Employees)
                .FirstOrDefaultAsync(u => u.Id == id && u.Employees != null && u.IsActive);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Admin/DeactivateEmployee/5
        [HttpPost, ActionName("DeactivateEmployee")]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeactivateEmployeeConfirmed(string id)
        {
            var employee = await _userManager.FindByIdAsync(id);
            if (employee != null)
            {
                employee.IsActive = false;
                await _userManager.UpdateAsync(employee);

                TempData["SuccessMessage"] = $"Employee {employee.FullName} has been deactivated successfully.";
            }

            return RedirectToAction(nameof(EmployeeManagement));
        }

        // GET: Admin/ActivateEmployee/5
       // [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ActivateEmployee(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _userManager.Users
                .Include(u => u.Employees)
                .FirstOrDefaultAsync(u => u.Id == id && u.Employees != null && !u.IsActive);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Admin/ActivateEmployee/5
        [HttpPost, ActionName("ActivateEmployee")]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ActivateEmployeeConfirmed(string id)
        {
            var employee = await _userManager.FindByIdAsync(id);
            if (employee != null)
            {
                employee.IsActive = true;
                await _userManager.UpdateAsync(employee);

                TempData["SuccessMessage"] = $"Employee {employee.FullName} has been activated successfully.";
            }

            return RedirectToAction(nameof(EmployeeManagement));
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

        //}
        //public async Task<IActionResult> DeleteUser(int id)
        //{
        //    var user = await userManager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
        //        return View("NotFound");
        //    }
        //    else
        //    {
        //        var result = await userManager.DeleteAsync(user);
        //        if (result.Succeeded)
        //        {
        //            return RedirectToAction("Index", "Home");
        //        }
        //    }
        //}
        public IActionResult Index()
        {
            return View();
        }
    }
}
