// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Data;
using FridgeManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace FridgeManagementSystem.Areas.Identity.Pages.Account
{
    //[AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly FridgeManagementSystemContext _context;
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            IHttpContextAccessor httpContextAccessor,
            FridgeManagementSystemContext context)

        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }
        // Helper method to generate absolute URLs
        private string GenerateAbsoluteUrl(string relativePath)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return relativePath;

            // Build absolute URL
            var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
            return $"{baseUrl}{relativePath}";
        }
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// 

        public List<SelectListItem> CustomerTypes { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "SpazaShop", Text = "Spaza Shop" },
            new SelectListItem { Value = "Liquor", Text = "Bottle Store" },
            
            new SelectListItem { Value = "Other", Text = "Other" }
        };
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /// 
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }
           
            [Required(ErrorMessage = "Please Enter Contact Number"), Phone]
            [Display(Name = "Contact Number")]
            public string ContactNo { get; set; }
            [Required]
            public string Address { get; set; }
            [Required]
            public string City { get; set; }
            [Required]
            public string Suburb { get; set; }
            [Required]
            [Display(Name = "Postal Code")]
            public string PostalCode { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            // Customer specific fields
            [Required]
            [Display(Name = "Business Name")]
            public string BusinessName { get; set; }
            [Required]
            [Display(Name = "Customer Type")]
            public string CustomerType { get; set; } /*= "Spaza Shop";*/

            //public string Role { get; set;}
            //public IEnumerable<SelectListItem> RoleList { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect("/");
            }
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            //Input = new InputModel
            //{
            //    RoleList = _roleManager.Roles.Select(x => x.Name).Select(i => new SelectListItem
            //    {
            //        Text = i,
            //        Value = i
            //    })
            //};
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.FullName = Input.FullName;
                user.ContactNo = Input.ContactNo;
                user.Address = Input.Address;
                user.City = Input.City;
                user.Suburb = Input.Suburb;
                user.PostalCode = Input.PostalCode;
                user.ApprovalStatus = "Pending"; // Customers need approval
                user.IsActive = true;
                user.CreatedAt = DateTime.UtcNow;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // Ensure Customer role exists
                    if (!await _roleManager.RoleExistsAsync("Customer"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Customer"));
                    }

                    // Automatically assign Customer role
                    await _userManager.AddToRoleAsync(user, "Customer");

                    // Create customer record - ONLY SET THE ID ONCE
                    var customer = new Customer
                    {
                        Id = user.Id, // This is correct - use the newly created user's ID
                        BusinessName = Input.BusinessName,
                        CustomerType = Input.CustomerType,
                        CreatedByFullName = user.FullName,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = null // Use user.Id here too, not the current authenticated user
                    };

                    _context.Customers.Add(customer);

                    // Create notifications for administrators and customer liaisons
                    await CreateApprovalNotifications(user);

                    await _context.SaveChangesAsync();

                    // Redirect to custom confirmation page with login details
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
        private async Task CreateApprovalNotifications(ApplicationUser user)
        {
            try
            {
                // Get all admin users
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

                // Get customer liaison employees by querying the Employee and EmployeeType tables
                var liaisonUsers = await _context.Users
                    .Where(u => u.Employees != null &&
                               u.Employees.EmployeeType != null &&
                               u.Employees.EmployeeType.Name == "CustomerLiaison" &&
                               u.Employees.IsActive)
                    .ToListAsync();

                // Combine both lists and remove duplicates
                var usersToNotify = adminUsers
                    .Union(liaisonUsers)
                    .GroupBy(u => u.Id)
                    .Select(g => g.First())
                    .ToList();

                _logger.LogInformation("Sending approval notifications to {Count} users", usersToNotify.Count);

                foreach (var notifyUser in usersToNotify)
                {
                    var notification = new Notification
                    {
                        UserId = notifyUser.Id,
                        Title = "New Customer Registration Requires Approval",
                        Message = $"Customer {user.FullName} ({user.Email}) from {user.City}, {user.Suburb} has been registered and requires approval.",
                        Link = GenerateAbsoluteUrl($"/Admin/ApproveCustomer/{user.Id}"),
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };

                    _context.Notifications.Add(notification);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating approval notifications for user {UserId}", user.Id);
                // Don't throw here - we don't want registration to fail just because notifications failed
            }
        }
    }
}
    

