using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class PurchasingOrderDetails
    {
        [Key]
        public int PurchasingOrderDetailsId { get; set; }
        
        [Required, StringLength(200)]
        public string Details { get; set; }

        public decimal UnitPrice { get; set; }

        // Foreign Key
        public int FridgeId { get; set; }
        public Fridge Fridge { get; set; }
        public int PurchasingOrderId { get; set; }
        public PurchasingOrder PurchasingOrder { get; set; }
    }
}
