using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class SupplierEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

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

        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        public string ApprovalStatus { get; set; }

        [Required]
        [Display(Name = "Supplier Type")]
        public string SupplierType { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
