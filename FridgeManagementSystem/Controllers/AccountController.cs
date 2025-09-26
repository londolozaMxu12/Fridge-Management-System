//using FridgeManagementSystem.Areas.Identity.Data;
//using FridgeManagementSystem.ViewModels;
//using Microsoft.AspNetCore.Http.HttpResults;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;

//namespace FridgeManagementSystem.Controllers
//{
//    public class AccountController : Controller
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly RoleManager<IdentityRole> _roleManager;

//        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _roleManager = roleManager;
//        }
//        // GET: Employees (Active employees only, excluding Customers)
//        public async Task<IActionResult> Index()
//        {
//            // Get all users first, then filter for active users on the client side
//            var allUsers = await _userManager.Users.ToListAsync();

//            // Filter for active users only (IsActive = true or null treated as active)
//            var activeUsers = allUsers.Where(u => u.IsActive).ToList();

//            var employeeViewModels = new List<EmployeeViewModel>();
//            foreach (var user in activeUsers) 
//            {
//                var roles = await _userManager.GetRolesAsync(user);
//                // Exclude users with Customer role
//                if (!roles.Contains("Customer"))
//                {
//                    employeeViewModels.Add(new EmployeeViewModel
//                    {
//                        Id = user.Id,
//                        Email = user.Email,
//                        FullName = user.FullName,
//                        ContactNo = user.ContactNo,
//                        Address = user.Address,
//                        City = user.City,
//                        Suburb = user.Suburb,
//                        PostalCode = user.PostalCode,
//                        IsActive = user.IsActive,
//                        CreatedAt = user.CreatedAt ?? DateTime.UtcNow,
//                        Roles = roles.ToList(),
//                    });
//                }
//            }

//            return View(employeeViewModels);
//        }

//        // GET: Employees/Edit/5
//        // GET: Employees/Edit/5
//        public async Task<IActionResult> EditEmployee(string id)
//        {
//            var user = await _userManager.FindByIdAsync(id);
//            if (user == null)
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            var roles = await _userManager.GetRolesAsync(user);

//            var model = new EmployeeEditViewModel
//            {
//                Id = user.Id,
//                Email = user.Email,
//                FullName = user.FullName,
//                ContactNo = user.ContactNo,
//                Address = user.Address,
//                City = user.City,
//                Suburb = user.Suburb,
//                PostalCode = user.PostalCode,
//                //IsActive = (bool)user.IsActive,
//                IsActive = user.IsActive,
//                SelectedRole = roles.FirstOrDefault(),
//            };

//            ViewData["EmployeeId"] = user.Id;
//            ViewData["CreatedAt"] = user.CreatedAt?.ToString("MM/dd/yyyy");
//            ViewData["IsActiveStatus"] = (user.IsActive) ? "Active" : "Inactive";
//            //ViewData["IsActiveStatus"] = (bool)user.IsActive? "Active" : "Inactive";

//            ViewBag.Roles = _roleManager.Roles
//                .Where(r => r.Name != "Customer")
//                .Select(r => new SelectListItem
//                {
//                    Value = r.Name,
//                    Text = r.Name
//                }).ToList();

//            return View(model);
//        }
//        // POST: Employees/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> EditEmployee(string id, EmployeeEditViewModel model)
//        {
//            if (id != model.Id)
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            if (ModelState.IsValid)
//            {
//                var user = await _userManager.FindByIdAsync(id);
//                if (user == null)
//                {
//                    return RedirectToAction("Index", "Account");
//                }

//                // Check if user is a Customer (prevent editing)
//                var currentRoles = await _userManager.GetRolesAsync(user);
//                if (currentRoles.Contains("Customer"))
//                {
//                    return RedirectToAction("Index", "Account");
//                }

//                user.Email = model.Email;
//                user.UserName = model.Email;
//                user.FullName = model.FullName;
//                user.ContactNo = model.ContactNo;
//                user.Address = model.Address;
//                user.City = model.City;
//                user.Suburb = model.Suburb;
//                user.PostalCode = model.PostalCode;
//                //user.IsActive = model.IsActive;

//                var result = await _userManager.UpdateAsync(user);

//                if (result.Succeeded)
//                {
//                    // Remove existing roles and add new one (excluding Customer role)
//                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

//                    if (!string.IsNullOrEmpty(model.SelectedRole) && model.SelectedRole != "Customer")
//                    {
//                        await _userManager.AddToRoleAsync(user, model.SelectedRole);
//                    }
                    
//                    TempData["SuccessMessage"] = "Updated successfully!.";
//                    return RedirectToAction(nameof(Index));
//                }

//                foreach (var error in result.Errors)
//                {
//                    ModelState.AddModelError(string.Empty, error.Description);
//                }
//            }

//            // Include all roles except Customer for selection
//            ViewBag.Roles = _roleManager.Roles.Where(r => r.Name != "Customer").Select(r => new SelectListItem
//            {
//                Value = r.Name,
//                Text = r.Name
//            }).ToList();
//            return View(model);

//        }
//        public async Task<IActionResult> EmployeeDetails(string id)
//        {
//            if (id == null)
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            var user = await _userManager.FindByIdAsync(id);
//            if (user == null)
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            var roles = await _userManager.GetRolesAsync(user);

//            // Prevent viewing Customer users
//            if (roles.Contains("Customer"))
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            var model = new EmployeeViewModel
//            {
//                Id = user.Id,
//                Email = user.Email,
//                FullName = user.FullName,
//                ContactNo = user.ContactNo,
//                Address = user.Address,
//                City = user.City,
//                Suburb = user.Suburb,
//                PostalCode = user.PostalCode,
//                IsActive = user.IsActive,
//                CreatedAt = user.CreatedAt ?? DateTime.UtcNow,
//                Roles = roles.ToList()
//            };

//            return View(model);
//        }
//        // GET: Employees/Deactivate/5
//        public async Task<IActionResult> DeactivateAccount(string id)
//        {
//            if (id == null)
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            var user = await _userManager.FindByIdAsync(id);
//            if (user == null || !(user.IsActive))  // Check if already inactive
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            var roles = await _userManager.GetRolesAsync(user);

//            // Prevent deactivating Customer users
//            if (roles.Contains("Customer"))
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            var model = new EmployeeViewModel
//            {
//                Id = user.Id,
//                Email = user.Email,
//                FullName = user.FullName,
//                ContactNo = user.ContactNo,
//                Address = user.Address,
//                City = user.City,
//                Suburb = user.Suburb,
//                PostalCode = user.PostalCode,
//                IsActive = user.IsActive,
//                CreatedAt = (DateTime)user.CreatedAt,
//                Roles = roles.ToList(),
                
//            };

//            return View(model);
//        }
//        // POST: Employees/Deactivate/5
//        [HttpPost, ActionName("DeactivateAccount")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeactivateAccountConfirmed(string id)
//        {
//            if (id == null)
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            var user = await _userManager.FindByIdAsync(id);
//            if (user == null || !(user.IsActive))  // Check if already inactive
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            var roles = await _userManager.GetRolesAsync(user);

//            // Prevent deactivating Customer users
//            if (roles.Contains("Customer"))
//            {
//                return RedirectToAction("Index", "Account");
//            }

//            // Soft delete: set IsActive to false (0)
//            user.IsActive = false;

//            var result = await _userManager.UpdateAsync(user);

//            if (result.Succeeded)
//            {
//                TempData["SuccessMessage"] = $" Employee {user.FullName} has been deactivated successfully.";
//                return RedirectToAction(nameof(Index));
//            }

//            // If deactivation failed
//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError(string.Empty, error.Description);
//            }

//            // Return to the deactivate view with errors
//            var model = new EmployeeViewModel
//            {
//                Id = user.Id,
//                Email = user.Email,
//                FullName = user.FullName,
//                ContactNo = user.ContactNo,
//                Address = user.Address,
//                City = user.City,
//                Suburb = user.Suburb,
//                PostalCode = user.PostalCode,
//                IsActive = user.IsActive,
//                CreatedAt = (DateTime)user.CreatedAt,
//                Roles = roles.ToList(),
                
//            };

//            return View("DeactivateAccount", model);
//        }
//        // GET: Employees/Inactive
//        public async Task<IActionResult> InactiveAccounts()
//        {
//            // Get all inactive users excluding Customers
//            var allUsers = await _userManager.Users
//                .Where(u => !(u.IsActive))  // Only inactive users
//                .ToListAsync();

//            var employeeViewModels = new List<EmployeeViewModel>();
//            foreach (var user in allUsers)
//            {
//                var roles = await _userManager.GetRolesAsync(user);
//                // Exclude users with Customer role
//                if (!roles.Contains("Customer"))
//                {
//                    employeeViewModels.Add(new EmployeeViewModel
//                    {
//                        Id = user.Id,
//                        Email = user.Email,
//                        FullName = user.FullName,
//                        ContactNo = user.ContactNo,
//                        Address = user.Address,
//                        City = user.City,
//                        Suburb = user.Suburb,
//                        PostalCode = user.PostalCode,
//                        IsActive = user.IsActive,
//                        CreatedAt = (DateTime)user.CreatedAt,
//                        Roles = roles.ToList(),
                        
//                    });
//                }
//            }

//            return View(employeeViewModels);
//        }

//        // GET: Employees/ReactivateEmployeeAccount/5
//        public async Task<IActionResult> ReactivateEmployeeAccount(string id)
//        {
//            if (id == null)
//            {
//                return RedirectToAction("InactiveAccounts", "Account");
//            }

//            var user = await _userManager.FindByIdAsync(id);
//            if (user == null || (user.IsActive))  // Check if already active
//            {
//                return RedirectToAction("InactiveAccounts", "Account");
//            }

//            var roles = await _userManager.GetRolesAsync(user);

//            // Prevent reactivating Customer users
//            if (roles.Contains("Customer"))
//            {
//                return RedirectToAction("InactiveAccounts", "Account");
//            }

//            var model = new EmployeeViewModel
//            {
//                Id = user.Id,
//                Email = user.Email,
//                FullName = user.FullName,
//                ContactNo = user.ContactNo,
//                Address = user.Address,
//                City = user.City,
//                Suburb = user.Suburb,
//                PostalCode = user.PostalCode,
//                IsActive = user.IsActive,
//                CreatedAt = (DateTime)user.CreatedAt,  
//                Roles = roles.ToList(),
//            };

//            return View(model);
//        }
//        // POST: Employees/ReactivateEmployeeAccount/5
//        [HttpPost, ActionName("ReactivateEmployeeAccount")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ReactivateEmployeeAccounts(string id)  // FIX: Method name should match ActionName
//        {
//            if (id == null)
//            {
//                return RedirectToAction("InactiveAccounts", "Account");
//            }

//            var user = await _userManager.FindByIdAsync(id);
//            if (user == null || (user.IsActive))  // Check if already active
//            {
//                return RedirectToAction("InactiveAccounts", "Account");
//            }

//            var roles = await _userManager.GetRolesAsync(user);

//            // Prevent reactivating Customer users
//            if (roles.Contains("Customer"))
//            {
//                return RedirectToAction("InactiveAccounts", "Account");
//            }

//            // Reactivate the user: set IsActive to true (1)
//            user.IsActive = true;

//            var result = await _userManager.UpdateAsync(user);

//            if (result.Succeeded)
//            {
//                TempData["SuccessMessage"] = $"Employee {user.FullName} has been reactivated successfully.";
//                return RedirectToAction("InactiveAccounts", "Account"); 
//            }

//            // If reactivation failed
//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError(string.Empty, error.Description);
//            }

//            // Return to the reactivate view with errors
//            var model = new EmployeeViewModel
//            {
//                Id = user.Id,
//                Email = user.Email,
//                FullName = user.FullName,
//                ContactNo = user.ContactNo,
//                Address = user.Address,
//                City = user.City,
//                Suburb = user.Suburb,
//                PostalCode = user.PostalCode,
//                IsActive = user.IsActive,
//                CreatedAt = (DateTime)user.CreatedAt,
//                Roles = roles.ToList(),
//            };

//            return View(model);
//        }
//    }
//}
