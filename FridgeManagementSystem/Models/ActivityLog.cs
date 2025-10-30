using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } // "Created", "Updated", "Deleted", "Approved", "Rejected"

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(50)]
        public string EntityType { get; set; } // "PurchaseRequest", "RFQ", "Quotation", "PurchaseOrder", "Supplier"

        public int? EntityId { get; set; } // ID of the affected entity

        // Foreign Keys
        public string UserId { get; set; } // Who performed the action
        public int? PurchaseRequestId { get; set; }
        public int? RFQId { get; set; }
        public int? PurchaseOrderId { get; set; }
        public int? QuotationId { get; set; }
        public int? SupplierId { get; set; }

        // For tracking changes
        public string OldValues { get; set; } // JSON serialized old values
        public string NewValues { get; set; } // JSON serialized new values

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string IPAddress { get; set; }

        // Navigation Properties
      //  public virtual ApplicationUser User { get; set; }
        public virtual PurchaseRequest PurchaseRequest { get; set; }
        public virtual RFQ RFQ { get; set; }
        public virtual PurchasingOrder PurchasingOrder { get; set; }
        public virtual Quotation Quotation { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}
