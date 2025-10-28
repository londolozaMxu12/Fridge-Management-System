using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class DeliveryNote
    {
        [Key]
        public int DeliveryNoteId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Delivery Note Number")]
        public string DeliveryNoteNumber { get; set; } = GenerateDeliveryNoteNumber();

        [Required]
        public int PurchaseOrderId { get; set; }

        [Required]
        [Display(Name = "Shipping Date")]
        public DateTime ShippingDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string Carrier { get; set; }

        [StringLength(100)]
        [Display(Name = "Tracking Number")]
        public string TrackingNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Shipped Quantity")]
        public int ShippedQuantity { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual PurchasingOrder PurchasingOrder { get; set; }

        private static string GenerateDeliveryNoteNumber()
        {
            return $"DN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }
    }
}
