using FridgeManagementSystem.Areas.Identity.Data;
using FridgeManagementSystem.Controllers;
using NuGet.Protocol.Plugins;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [Display(Name = "Business Name")]
        [StringLength(100)]
        public string BusinessName { get; set; }
        [Required]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; } // Spaza Shop, Liquor, Other
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByFullName { get; set; }
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
        public ICollection<Fridge> Fridges { get; set; }
        public ICollection<Fault> Faults { get; set; }
        public ICollection<FridgeRequest> FridgeRequests { get; set; }
        public ICollection<Quotation> Quotations { get; set; }
        public ICollection<Allocation> Allocations { get; set; }
    }
}
