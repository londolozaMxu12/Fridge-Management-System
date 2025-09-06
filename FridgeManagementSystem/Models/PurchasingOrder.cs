using System.ComponentModel.DataAnnotations;

namespace FridgeManagementSystem.Models
{
    public class PurchasingOrder
    {
        [Key]
        public int PurchasingOrderId { get; set; }
        [Required]
        public DateTime Date {  get; set; }
        [Required, StringLength(200)]
        public string Details { get; set; }
        //[Required]
        //public OrderStatus Status { get; set; } = FaultStatus.Pending;


        // Foreign Key
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
        public int PurchasingManagerId { get; set; }
        public PurchasingManager PurchasingManager { get; set; }
        public ICollection<PurchasingOrderDetails> OrderDetails { get; set; }
    }
}
