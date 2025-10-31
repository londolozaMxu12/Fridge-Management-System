using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "Full name cannot be longer than 100 characters.")]
        public string FullName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Contact Number")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string ContactNo { get; set; }

        [Display(Name = "Address")]
        [StringLength(200, ErrorMessage = "Address cannot be longer than 200 characters.")]
        public string Address { get; set; }

        [Display(Name = "City")]
        [StringLength(50, ErrorMessage = "City cannot be longer than 50 characters.")]
        public string City { get; set; }

        [Display(Name = "Suburb")]
        [StringLength(50, ErrorMessage = "Suburb cannot be longer than 50 characters.")]
        public string Suburb { get; set; }

        [Display(Name = "Postal Code")]
        [RegularExpression("^[0-9]{4}$", ErrorMessage = "Postal code must be 4 digits.")]
        [StringLength(4)]
        public string PostalCode { get; set; }

        // Read-only properties for display
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Account Created")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Status")]
        public string ApprovalStatus { get; set; }
    }
}
