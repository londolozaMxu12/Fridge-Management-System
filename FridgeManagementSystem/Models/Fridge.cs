using FridgeManagementSystem.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FridgeManagementSystem.Models
{
    
    public class Fridge
    {
        [Key]
        public int FridgeId { get; set; }

        [Required, Precision(16, 2)]
        public Decimal Price { get; set; }

        [Required]
        public string Description { get; set; }

        [Required, MaxLength(255)]
        public string ImageFile { get; set; }

        [Required, StringLength(100)]
        public string SerialNumber { get; set; }

        [Required]
        [Display(Name = "Acquisition Date")]
        [DataType(DataType.Date)]
        public DateTime AcquisitionDate { get; set; }

        [Display(Name = "Allocation Date")]
        public DateTime? AllocationDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // Available, Allocated, InService, scrapped

        [Display(Name = "Active")]
        public bool IsActive { get; set; } =true;

        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        public bool IsAvailable { get; set; } = true;

        public string CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public ApplicationUser CreatedBy { get; set; }

        [Display(Name = "Next Service Date")]
        [DataType(DataType.Date)]
        public DateTime? NextServiceDate { get; set; }

        [Display(Name = "Service Date")]
        [DataType(DataType.Date)]
        public DateTime? ServiceDate { get; set; }

        // Foreign Key
        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        [ForeignKey("SupplierId")]
        public Supplier Supplier { get; set; }

        [Display(Name = "FridgeType")]
        public int? FridgeTypeId { get; set; }
        public FridgeType FridgeType { get; set; }

        public Allocation CurrentAllocation { get; set; }

        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; }

        public ICollection<CartDetails> CartDetails { get; set; }

        public ICollection<PurchasingOrderDetails> OrderDetails { get; set; }
    }
}