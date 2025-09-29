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

namespace FridgeManagementSystem.Areas.Identity.Pages.Account
{
    //[Authorize(Roles = "Administrator")]
    public class RegisterSupplierModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterSupplierModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly FridgeManagementSystemContext _context;
        
        public RegisterSupplierModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterSupplierModel> logger,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            FridgeManagementSystemContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            //_emailStore = GetEmailStore();
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public List<SelectListItem> SupplierTypes { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "Fridge", Text = "Fridge Supplier" },
            
            new SelectListItem { Value = "Parts", Text = "Parts Supplier" },
            new SelectListItem { Value = "Other", Text = "Other" }
        };

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required]
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

            // Supplier-specific properties
            [Required]
            [Display(Name = "Company Name")]
            public string CompanyName { get; set; }

            [Required]
            [Display(Name = "Supplier Type")]
            public string SupplierType { get; set; }

        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FullName = Input.FullName,
                    ContactNo = Input.ContactNo,
                    Address = Input.Address,
                    City = Input.City,
                    Suburb = Input.Suburb,
                    PostalCode = Input.PostalCode,
                    ApprovalStatus = "Approved", // Suppliers are auto-approved
                    ApprovedById = _userManager.GetUserId(User),
                    ApprovedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // ensure Supplier role exist
                    if (!await _roleManager.RoleExistsAsync("Supplier"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Supplier"));
                    }
                    // Add to Supplier role
                    await _userManager.AddToRoleAsync(user, "Supplier");

                    // Create Supplier record
                    var supplier = new Supplier
                    {
                        UserId = user.Id,
                        CompanyName = Input.CompanyName,
                        
                        SupplierType = Input.SupplierType,
                        
                        CreatedById = _userManager.GetUserId(User)
                    };

                    _context.Suppliers.Add(supplier);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Supplier account created for {FullName}", Input.FullName);

                    //return RedirectToPage("RegisterSupplierConfirmation", new { email = Input.Email });

                    // Auto-confirm email for admin-created users
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _userManager.ConfirmEmailAsync(user, token);

                    _logger.LogInformation("Admin created new user account for {Email}", Input.Email);
                    TempData["SuccessMessage"] = $"Supplier {Input.FullName} as been registered successfully!!!.";
                    return RedirectToPage();
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}