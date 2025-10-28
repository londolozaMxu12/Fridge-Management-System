using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class QuotationViewModel
    {
        public int RFQId { get; set; }
        public int SupplierId { get; set; }

        [Required]
        [Display(Name = "Item Description")]
        public string ItemDescription { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Shipping cost cannot be negative")]
        [DataType(DataType.Currency)]
        [Display(Name = "Shipping Cost")]
        public decimal ShippingCost { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Quote Date")]
        public DateTime QuoteDate { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Valid Until")]
        public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddDays(30);

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Proposed Delivery Date")]
        public DateTime ProposedDeliveryDate { get; set; } = DateTime.UtcNow.AddDays(21);

        [StringLength(1000)]
        [Display(Name = "Additional Notes")]
        public string Notes { get; set; }

        [StringLength(500)]
        [Display(Name = "Terms and Conditions")]
        public string TermsAndConditions { get; set; }

        // Computed properties
        [Display(Name = "Total Price")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice => UnitPrice * Quantity;

        [Display(Name = "Grand Total")]
        [DataType(DataType.Currency)]
        public decimal GrandTotal => TotalPrice + ShippingCost;
    }
}
