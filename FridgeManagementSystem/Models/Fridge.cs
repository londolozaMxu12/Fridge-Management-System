using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FridgeManagementSystem.Models
{
    public class Fridge
    {
        [Key]
        public int FridgeId { get; set; }

        [Required]
        [Precision(16, 2)]
        public decimal Price { get; set; }

        [StringLength(255)]
        public string ImageFileName { get; set; } = "default-fridge.jpg";

        //[Required]
        //public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "Refrigerator";

        public string Description { get; set; } = "";

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string SerialNumber { get; set; } = "";

        [Required]
        [Display(Name = "Acquisition Date")]
        [DataType(DataType.Date)]
        public DateTime AcquisitionDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Allocation Date")]
        public DateTime? AllocationDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Available"; // Available, Reserved, Allocated

        [Display(Name = "Next Service Date")]
        [DataType(DataType.Date)]
        public DateTime? NextServiceDate { get; set; }

        [Display(Name = "Customer")]
        public string CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; } = null!;

        [Required]
        [Display(Name = "Fridge Type")]
        public int FridgeTypeId { get; set; }
        public FridgeType FridgeType { get; set; } = null!;

        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }

        public Allocation? CurrentAllocation { get; set; }
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Fault> ReportedFaults { get; set; } = new List<Fault>();
       // public Order? Order { get; set; }

        [NotMapped]
        public string Name => FridgeType?.Name ?? "";

        [NotMapped]
        public string Brand => FridgeType?.Brand ?? "";

        [NotMapped]
        public string Model => FridgeType?.Model ?? "";

        [NotMapped]
        public string DisplayName => $"{Brand} {Name} - {Model}";
    }
}