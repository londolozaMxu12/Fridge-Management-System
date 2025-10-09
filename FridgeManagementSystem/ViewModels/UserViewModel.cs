using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ContactNo { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Suburb { get; set; }
        public string PostalCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovedById { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Role properties
        public List<string> Roles { get; set; }
        public string RoleNames { get; set; }

        // Display properties
        [Display(Name = "Status")]
        public string StatusDisplay => IsActive ? "Active" : "Inactive";

        [Display(Name = "Created Date")]
        public string CreatedAtDisplay => CreatedAt.ToString("MMM dd, yyyy");

        [Display(Name = "Approved Date")]
        public string ApprovedAtDisplay => ApprovedAt?.ToString("MMM dd, yyyy") ?? "N/A";

        [Display(Name = "Roles")]
        public string RolesDisplay => string.Join(", ", Roles) ?? "No Roles";
    }
}
