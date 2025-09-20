using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class EmployeeRegisterViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Contact Number")]
        [Required(ErrorMessage = "Contact number is required")]
        [RegularExpression(@"^0[6-8][0-9]{8}$", ErrorMessage = "Enter a valid South African contact number")]
        public string? ContactNo { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Suburb { get; set; }

        [Required(ErrorMessage = "Postal Code is required")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Postal Code must be exactly 4 digits")]
        public string? PostalCode { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public string SelectedRole { get; set; }
    }
}
