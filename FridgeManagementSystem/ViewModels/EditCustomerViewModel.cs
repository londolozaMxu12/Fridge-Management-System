using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class EditCustomerViewModel
    {
        public string Id { get; set; }
        //public string UserId { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Contact Number")]
        public string ContactNo { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Required]
        [Display(Name = "Business Name")]
        [StringLength(100)]
        public string BusinessName { get; set; }

        [Required]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; }

        [Required]
        [Display(Name = "Approval Status")]
        public string ApprovalStatus { get; set; }
    }
}
