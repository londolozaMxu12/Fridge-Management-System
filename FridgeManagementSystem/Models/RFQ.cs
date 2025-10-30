using FridgeManagementSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class RFQ
    {
        [Key]
        public int RFQId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "RFQ Number")]
        public string RFQNumber { get; set; } = GenerateRFQNumber();

        [Required]
        public int PurchaseRequestId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Item Description")]
        public string ItemDescription { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [StringLength(1000)]
        public string Specifications { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Deadline")]
        public DateTime Deadline { get; set; }

        [Display(Name = "Status")]
        public RFQStatus Status { get; set; } = RFQStatus.Draft;

        [StringLength(450)]
        public string CreatedById { get; set; }

        // Navigation properties
        public PurchaseRequest PurchaseRequest { get; set; }
        public ApplicationUser CreatedBy { get; set; }
        public ICollection<RFQSupplier> RFQSuppliers { get; set; } = new List<RFQSupplier>();
        public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

        private static string GenerateRFQNumber()
        {
            return $"RFQ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }
    }

    public enum RFQStatus
    {
        Draft,
        Sent,
        QuotesReceived,
        UnderEvaluation,
        ConvertedToPO,
        Expired,
        Cancelled
    }
}

