using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.ViewModels
{
    public class CreateDirectPOViewModel
    {
        public int PurchaseRequestId { get; set; }

        [Required(ErrorMessage = "Item description is required")]
        [StringLength(200, ErrorMessage = "Item description cannot exceed 200 characters")]
        [Display(Name = "Item Description")]
        public string ItemDescription { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        [DataType(DataType.Currency)]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Expected delivery date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Expected Delivery Date")]
        public DateTime ExpectedDeliveryDate { get; set; } = DateTime.UtcNow.AddDays(14);

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Additional Notes")]
        public string Notes { get; set; }

        [Display(Name = "Shipping Cost")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Shipping cost cannot be negative")]
        public decimal ShippingCost { get; set; }

        [Display(Name = "Tax Amount")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Tax amount cannot be negative")]
        public decimal TaxAmount { get; set; }

        // For dropdown population
        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();

        // Computed properties
        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        public decimal Subtotal => UnitPrice * Quantity;

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount => Subtotal + ShippingCost + TaxAmount;
    }
}
