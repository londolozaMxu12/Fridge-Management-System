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

[Authorize(Roles = "Admin")]
public class RegisterEmployeeModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterEmployeeModel> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly FridgeManagementSystemContext _context;
    [BindProperty]
    public InputModel Input { get; set; }

    //public List<SelectListItem> Roles { get; set; }
    public List<SelectListItem> EmployeeTypes { get; set; } = new List<SelectListItem>();
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
        [Display(Name = "Employee Type")]
        public int EmployeeTypeId { get; set; }

        [Required]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }

        [Required]
        [Display(Name = "Date Employed")]
        [DataType(DataType.Date)]
        public DateTime DateEmployed { get; set; } = DateTime.Today;

    }

    public RegisterEmployeeModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        ILogger<RegisterEmployeeModel> logger,
        RoleManager<IdentityRole> roleManager,
        FridgeManagementSystemContext context)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _logger = logger;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task OnGetAsync()
    {
        await LoadEmployeeTypes();
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
                ApprovalStatus = "Approved", // Employees are auto-approved

                ApprovedById = _userManager.GetUserId(User),
                ApprovedAt = DateTime.UtcNow,
                IsActive = true              
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // ensure Employee role exist
                if (!await _roleManager.RoleExistsAsync("Employee"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Employee"));
                }

                // Add to Employee role
                await _userManager.AddToRoleAsync(user, "Employee");

                // Create Employee record
                var employee = new Employee
                {
                    UserId = user.Id,
                    EmployeeTypeId = Input.EmployeeTypeId,
                    JobTitle = Input.JobTitle,
                    DateEmployed = Input.DateEmployed,
                    CreatedById = _userManager.GetUserId(User)
                };
                // The EmployeeNumber will be auto-generated when saving
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Employee account created for {Email} by {Admin}", Input.Email, User.Identity.Name);

                 //return RedirectToPage("RegisterEmployeeConfirmation", new { email = Input.Email });

                // Auto-confirm email for admin-created users
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.ConfirmEmailAsync(user, token);

                _logger.LogInformation("Admin created new user account for {Email}", Input.Email);
                TempData["SuccessMessage"] = $"Employee {Input.FullName} as been registered successfully!!!.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        await LoadEmployeeTypes();
        return Page();
    }
    private async Task LoadEmployeeTypes()
    {
        EmployeeTypes = await _context.EmployeeTypes
            .Where(et => et.IsActive)
            .Select(et => new SelectListItem
            {
                Value = et.Id.ToString(),
                Text = et.Name
            })
            .ToListAsync();
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