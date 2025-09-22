#nullable disable

using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

//[Authorize(Roles = "Admin")]
public class RegisterEmployeeModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterEmployeeModel> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;

    [BindProperty]
    public InputModel Input { get; set; }

    public List<SelectListItem> Roles { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Contact Number")]
        [Required(ErrorMessage = "Contact number is required")]
        [RegularExpression(@"^0[6-8][0-9]{8}$", ErrorMessage = "Enter a valid South African contact number and should be 10 digits")]
        public string? ContactNo { get; set; }
        [Required]
        [Display(Name = "Address")]
        public string? Address { get; set; }
        [Required]
        [Display(Name = "City")]
        public string? City { get; set; }
        [Required]
        [Display(Name = "Suburb")]
        public string? Suburb { get; set; }

        [Required(ErrorMessage = "Postal Code is required")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Postal Code must be exactly 4 digits")]
        public string? PostalCode { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string SelectedRole { get; set; } = string.Empty;
    }

    public RegisterEmployeeModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        ILogger<RegisterEmployeeModel> logger,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _logger = logger;
        _roleManager = roleManager;
    }

    public async Task OnGetAsync()
    {
        // Check if 'Admin' role exists, create it if not
        var adminRole = await _roleManager.FindByNameAsync("Admin");
        if (adminRole == null)
        {
            _logger.LogWarning("Admin role does not exist. Creating Admin role.");
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Ensure the user is logged in and has the 'Admin' role
        if (!User.Identity.IsAuthenticated)
        {
            _logger.LogWarning("User is not authenticated.");
            RedirectToPage("/Account/Login");
            return;
        }

        if (!User.IsInRole("Admin"))
        {
            _logger.LogWarning("User is not an Admin.");
            RedirectToPage("/Account/AccessDenied");
            return;
        }

        // Get all available roles EXCEPT Customer role
        Roles = await _roleManager.Roles
            .Where(r => r.Name != "Customer") // Exclude Customer role
            .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                FullName = Input.FullName,
                UserName = Input.Email,
                Email = Input.Email,
                ContactNo = Input.ContactNo,
                Address = Input.Address,
                City = Input.City,
                Suburb = Input.Suburb,
                PostalCode = Input.PostalCode,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // Ensure the selected role exists
                if (!await _roleManager.RoleExistsAsync(Input.SelectedRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(Input.SelectedRole));
                }

                // Add the user to the selected role
                await _userManager.AddToRoleAsync(user, Input.SelectedRole);

                // Auto-confirm email for admin-created users
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.ConfirmEmailAsync(user, token);

                _logger.LogInformation("Admin created new user account for {Email}", Input.Email);
                TempData["SuccessMessage"] = $"Employee {Input.FullName} as been registered successfully with role {Input.SelectedRole}.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Reload roles if validation fails (excluding Customer role)
        Roles = await _roleManager.Roles
            .Where(r => r.Name != "Customer") // Exclude Customer role
            .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
            .ToListAsync();

        return Page();
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}