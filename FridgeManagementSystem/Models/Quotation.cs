using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class Quotation
    {
        [Key]
        public int QuotationId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required, StringLength(200)]
        public string Details { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Quote Number")]
        public string QuoteNumber { get; set; } = GenerateQuoteNumber();
        [Required]
        public int RFQId { get; set; }


        // Foreign Key
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Total Price")]
        public decimal TotalPrice { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Shipping Cost")]
        public decimal ShippingCost { get; set; } = 0;

        [Display(Name = "Quote Date")]
        public DateTime QuoteDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Valid Until")]
        public DateTime ValidUntil { get; set; }

        [Required]
        [Display(Name = "Proposed Delivery Date")]
        public DateTime ProposedDeliveryDate { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [StringLength(500)]
        public string TermsAndConditions { get; set; }

        public QuoteStatus Status { get; set; } = QuoteStatus.Submitted;

        // Navigation properties
        public virtual RFQ RFQ { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual PurchasingOrder PurchasingOrder { get; set; }
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

        private static string GenerateQuoteNumber()
        {
            return $"QUOTE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }
    }

    public enum QuoteStatus
    {
        Draft,
        Submitted,
        UnderReview,
        Accepted,
        Rejected,
        Expired
    }


}
