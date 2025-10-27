using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public ApplicationUser User { get; set; } = null!;

        [Required]
        [Display(Name = "Business Name")]
        [StringLength(100)]
        public string BusinessName { get; set; } = "";

        [Required]
        [Display(Name = "Customer Type")]
        public string CustomerType { get; set; } = "Spaza Shop";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedByFullName { get; set; } = "";
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }

        // Navigation properties
        public ICollection<Fridge> Fridges { get; set; } = new List<Fridge>();
        public ICollection<Fault> ReportedFaults { get; set; } = new List<Fault>();
        public ICollection<FridgeRequest> FridgeRequests { get; set; } = new List<FridgeRequest>();
        public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
        public ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<ScheduleMaintenance> ScheduleMaintenances { get; set; }
    }
}